import json
import os
import sys
import traceback

from filelock import FileLock, Timeout
import numpy as np
from astropy.io import fits
from astropy.stats import sigma_clipped_stats
from donuts import Donuts
from photutils.background import MedianBackground, Background2D
from scipy import ndimage
from astropy.stats import gaussian_fwhm_to_sigma, gaussian_sigma_to_fwhm
from astropy.convolution import Gaussian2DKernel, convolve
from photutils.segmentation import detect_sources, SourceCatalog
from astropy import wcs
from astropy.stats import SigmaClip, mad_std, sigma_clip


def write_to_fits(path, fwhm, ell, stars_num, b):
    try:
        with FileLock(f"{path}.lock").acquire(timeout=5):
            with fits.open(path, memmap=False, mode='update') as hdulist:
                fwhm_card = fits.Card('FWHM', 'nan' if np.isnan(fwhm) else fwhm, 'Median FWHM [arcsec]')
                ell_card = fits.Card('ELL', 'nan' if np.isnan(ell) else ell, 'Median ellipticity')
                stars_card = fits.Card('NSTARS', 'nan' if np.isnan(stars_num) else stars_num, "Stars on frame")
                bkg_card = fits.Card('BKG', 'nan' if np.isnan(b) else b, "Median background")
                hdulist[0].header.append(fwhm_card)
                hdulist[0].header.append(ell_card)
                hdulist[0].header.append(stars_card)
                hdulist[0].header.append(bkg_card)
    except Timeout:
        print("Файл не освободился за 5 секунды, не удалось записать данные")
        return 'fail'


def calc_fwhm(header, image):
    sigma_clip = SigmaClip(sigma=3.0)
    bkg_estimator = MedianBackground()
    bkg = Background2D(image, (32, 32), filter_size=(3, 3),
                       sigma_clip=sigma_clip, bkg_estimator=bkg_estimator)
    b = np.round(bkg.background_median, 2)
    # apply filters
    f_image = ndimage.median_filter(image, 9, mode='reflect')
    f_image = ndimage.gaussian_filter(f_image, 3, 0, mode='reflect')
    # calc noise etc.
    mean, median, stddev = sigma_clipped_stats(f_image, sigma=3, maxiters=3,
                                               cenfunc='median', stdfunc='mad_std')
    # detect peaks
    Peaks = f_image - (median + 5 * stddev)
    detected_peaks = Peaks > 0
    labeled_im, nb_labels = ndimage.label(detected_peaks)
    # check labels size
    sizes = ndimage.sum(detected_peaks, labeled_im, range(nb_labels + 1))
    mask_size = sizes < 5
    remove_pixel = mask_size[labeled_im]
    labeled_im[remove_pixel] = 0
    labeled_im[labeled_im > 0] = 100
    # redetect features
    labeled_im, nb_labels = ndimage.label(labeled_im)
    if nb_labels == 0:
        return header['FOCUS'], 0, 0, 0, b
    slices = ndimage.find_objects(labeled_im)
    FWHM = []
    ELL = []
    for Slice in slices:
        # check roundness
        X2Y = (Slice[0].stop - Slice[0].start) / (Slice[1].stop - Slice[1].start)
        if (X2Y > 1.2) or (X2Y < 0.8):
            continue
            # copy small area of the image
        Slice = image[Slice] - median
        # index_array
        Y_index = np.arange(0, Slice.shape[0], 1)
        X_index = np.arange(0, Slice.shape[1], 1)
        # calc centroid
        My = np.sum(Slice * Y_index[:, None]) / np.sum(Slice)
        Mx = np.sum(Slice * X_index[None, :]) / np.sum(Slice)

        # calc second order moments
        Y_index = Y_index - My
        X_index = X_index - Mx
        Myy = np.sum(Slice * Y_index[:, None] * Y_index[:, None]) / np.sum(Slice)
        Mxx = np.sum(Slice * X_index[None, :] * X_index[None, :]) / np.sum(Slice)
        # calc FWHM
        M = Mxx + Myy
        _fwhm = np.sqrt(M) * gaussian_sigma_to_fwhm

        # sigmax = np.sqrt(Mxx)
        # sigmay = np.sqrt(Myy)
        ell = 1 - np.sqrt(min([Mxx, Myy]) / max([Mxx, Myy]))

        #     print('Centriod: ', Mx, My, '\t FWHM: ', _fwhm)
        FWHM.append(_fwhm)
        ELL.append(ell)
    fwhm = np.round((np.nanmedian(np.asarray(FWHM))) * 0.65 * header['XBINNING'], 2) # fwhm-2.2
    ell = np.round(np.nanmedian(np.asarray(ELL)), 2)
    stars_num = len(FWHM)
    if np.isnan(fwhm):
        return 'fail'
    return header['FOCUS'], fwhm, ell, stars_num, b


def calc_source_catalog(path):
    try:
        with FileLock(f"{path}.lock").acquire(timeout=5):
            # print("Файл успешно захвачен")
            with fits.open(path, memmap=False) as hdulist:
                header = hdulist[0].header.copy()
                image = hdulist[0].data.copy()
    except Timeout:
        print("Файл не освободился за 5 секунды — пропускаем")
        return 'fail'

    sigmaclip = SigmaClip(sigma=3.)
    bkg_estimator = MedianBackground()
    # delete background
    bkg = Background2D(image, (32, 32), filter_size=(9, 9),
                       sigma_clip=sigmaclip, bkg_estimator=bkg_estimator)
    Data_without_background = image - bkg.background
    b = np.round(bkg.background_median, 2)
    s_sky = sigma_clip(Data_without_background, stdfunc=mad_std).filled(np.nan)
    s_sky = np.nanstd(s_sky)
    sigma = 9.0 * gaussian_fwhm_to_sigma  # FWHM = 3.
    kernel = Gaussian2DKernel(sigma, x_size=3, y_size=3)
    kernel.normalize()
    segm = detect_sources(convolve(Data_without_background, kernel), 50 * s_sky,
                          npixels=np.round(10/header['XBINNING']))
    if not segm:
        return calc_fwhm(header, image)
        # return header['FOCUS'], 0, 0, 0, b
    cat = SourceCatalog(Data_without_background, segm)
    fwhm = np.round(np.median(cat.fwhm.value) * 0.65 * header['XBINNING'], 2)
    ell = np.round(np.median(cat.ellipticity.value), 2)
    stars_num = len(cat.fwhm.value)
    if np.isnan(fwhm):
        return 'fail'
    return header['FOCUS'], fwhm, ell, stars_num, b


def calc_don_shifts(path_start, path_end):
    donuts = Donuts(refimage=path_start, image_ext=0, overscan_width=24, prescan_width=24,
                    border=50, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
    hlist = fits.open(path_start)
    h = hlist[0].header
    shift_result = donuts.measure_shift(path_end)
    dx = - shift_result.x.value
    dy = - shift_result.y.value

    x_m = h['CRPIX1'] - shift_result.x.value
    y_m = h['CRPIX2'] - shift_result.y.value
    hlist.close()
    w = wcs.WCS(h)
    bRa, bDec = w.all_pix2world(x_m, y_m, 0)
    cRa, cDec = w.all_pix2world(h['CRPIX1'], h['CRPIX2'], 0)
    dalpha = (cRa - bRa)*60*60
    ddelta = (cDec - bDec)*60*60

    return np.round(dx, 2), np.round(dy, 2), np.round(dalpha, 2), np.round(ddelta, 2)


if __name__ == "__main__":
    if len(sys.argv) >= 3 and sys.argv[1] == "fwhm":
        try:
            image_path = sys.argv[2]
            if not os.path.exists(image_path):
                print(f"ERR~Файл не найден: {image_path}", file=sys.stderr)
                sys.exit(1)
            focus, fwhm, ell, stars_num, b = calc_source_catalog(image_path)
            write_to_fits(image_path, fwhm, ell, stars_num, b)
            response = {
                "focus": focus,
                "fwhm": fwhm,
                "ell": ell,
                "stars": stars_num,
                "bkg": b
            }
            print(json.dumps(response))
        except Exception as e:
            print(json.dumps({"error": str(e)}), file=sys.stderr)
            traceback.print_exc(file=sys.stderr)
            sys.exit(1)
    elif len(sys.argv) >= 4 and sys.argv[1] == "don":
        ref_path = sys.argv[2]
        new_path = sys.argv[3]
        try:
            if not os.path.exists(ref_path) or not os.path.exists(new_path):
                print(json.dumps({"error": "Один или оба FITS-файла не найдены"}), file=sys.stderr)
                sys.exit(1)
            don = calc_don_shifts(ref_path, new_path)
            response = {
                "dx": don[0],
                "dy": don[1],
                "dalpha": don[2],
                "ddelta": don[3]
            }
            print(json.dumps(response))
        except Exception as e:
            print(json.dumps({"error": str(e)}), file=sys.stderr)
            traceback.print_exc(file=sys.stderr)
            sys.exit(1)
    else:
        print("Usage: python DONUTS.py fwhm [don] <path_to_fits> [<path_to_fits>]", file=sys.stderr)
        sys.exit(1)
