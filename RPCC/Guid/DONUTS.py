import json
import os
import subprocess
import sys
import traceback
import warnings

from filelock import FileLock, Timeout
import numpy as np
from astropy.io import fits
from astropy.stats import sigma_clipped_stats
from donuts import Donuts
from photutils.background import MedianBackground, Background2D
from scipy import ndimage
from astropy.stats import gaussian_sigma_to_fwhm
from astropy import wcs
from astropy.stats import SigmaClip
import astropy.io.ascii as ascii
warnings.filterwarnings("ignore")


def write_to_fits(path, fwhm, ell, stars_num, b):
    try:
        with FileLock(f"{path}.lock").acquire(timeout=300):
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
        print("Файл не освободился за 60 секунд, не удалось записать данные")
        return 'fail'
    except Exception as ex:
        print(ex)
        return f'fail, {ex}'


def sex(input_file):
    cwd = 'C:\\'
    # cwd = os.getcwd() + '\\'
    Sex = cwd + 'Sex\Extract.exe '
    dSex = ' -c ' + cwd + 'Sex\pipeline.sex'
    dPar = ' -PARAMETERS_NAME ' + cwd + 'Sex\pipeline.par'
    dFilt = ' -FILTER_NAME ' + cwd + r'Sex\tophat_2.5_3x3.conv'
    NNW = ' -STARNNW_NAME ' + cwd + 'Sex\default.nnw'

    output_file = ".".join(input_file.split('.')[:-1]) + '.cat'
    # output_file = input_file.replace('fits.gz', 'cat')

    shell = Sex + "\"" + input_file + "\"" + dSex + dPar + dFilt + NNW + ' -CATALOG_NAME ' + "\"" + output_file + "\""
    print(shell)
    try:
        with FileLock(f"{input_file}.lock").acquire(timeout=300):
            with fits.open(input_file, memmap=False) as hdulist:
                header = hdulist[0].header.copy()
            # startupinfo = subprocess.STARTUPINFO()
            # startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            # child = subprocess.run(shell, timeout=60, startupinfo=startupinfo)
            child = subprocess.run(shell, timeout=60)

            if child.returncode == 0 and os.path.isfile(output_file):
                print('Ok')
            else:
                print('Error')
                return -1, -1, -1, -1, -1
    except Timeout:
        print("Файл не освободился за 300 секунды — пропускаем")
        return 'fail'
    tbl = ascii.read(output_file)
    os.remove(output_file)
    # indx = np.where((tbl['FWHM_IMAGE'] < 50) & (tbl['FWHM_IMAGE'] > 1))[0]
    indx = np.where((tbl['FWHM_IMAGE'] > 1) & (tbl['FLUX_ISOCOR']/tbl['FLUXERR_ISOCOR'] > 15) &
                    (tbl['FLUX_ISOCOR']/tbl['FLUXERR_ISOCOR'] < 1000) & (tbl['FLAGS'] == 0))[0]
    # & (tbl['FLAGS'] == 0) &
    # (tbl['FLUX_ISOCOR']/tbl['FLUXERR_ISOCOR'] > 15) &
    # (tbl['FLUX_ISOCOR']/tbl['FLUXERR_ISOCOR'] < 1000)
    if len(indx) < 10:
        print('Can\'t find stars')
        return 0, 0, 0, 0, 0
    med_fwhm = np.round(np.median(tbl['FWHM_IMAGE'][indx]), 2)
    med_ell = np.round(np.median(tbl['ELLIPTICITY'][indx]), 2)
    med_bkg = np.round(np.median(tbl['BACKGROUND'][indx]), 2)
    # med_zeropoi = np.round(np.median(tbl['ZEROPOI']), 2)

    return header['FOCUS'], med_fwhm, med_ell, len(indx), med_bkg, header['BINNING']


def star_hoover(path):
    try:
        with FileLock(f"{path}.lock").acquire(timeout=60):
            # print("Файл успешно захвачен")
            with fits.open(path, memmap=False) as hdulist:
                header = hdulist[0].header.copy()
                image = hdulist[0].data.copy()
    except Timeout:
        print("Файл не освободился за 5 секунды — пропускаем")
        return 'fail'
    SN = 3
    # median filter for supressing of hot pixels
    Data = ndimage.median_filter(image, size=3)

    # delete background
    mean, median, stddev = sigma_clipped_stats(Data)
    Data = Data - median

    NStars, FWHM, Ell = donuts_fwhm(Data, SN, stddev)

    if FWHM < 10:
        NStars, FWHM, Ell = buns(Data, SN)
    return header['FOCUS'], FWHM, Ell, NStars, median


def donuts_fwhm(Data, SN, stddev):
    # set threshold
    Image = Data - SN * stddev

    # low-pass filtering
    Image = ndimage.gaussian_filter(Image, 10)
    Image[Image < 0] = 0

    # edge detection
    Image = ndimage.gaussian_gradient_magnitude(Image, sigma=4)

    # detect objects
    XY_coo = []
    detected_peaks = Image > 0
    labeled, num_objects = ndimage.label(detected_peaks)
    slices = ndimage.find_objects(labeled)
    for dy, dx in slices:
        x_center = (dx.start + dx.stop - 1) / 2
        x_size = dx.stop - dx.start
        y_center = (dy.start + dy.stop - 1) / 2
        y_size = dy.stop - dy.start

        # check minsize and roundness
        if (x_size > 10 and y_size > 10) and (abs(1 - (x_size / y_size)) < 0.2):
            XY_coo.append([x_center, y_center, (x_size + y_size) / 2])

    XY_coo = np.asarray(XY_coo)
    if len(XY_coo) > 0:
        NStars, FWHM, Ell = get_FWHM(Data, XY_coo)
    else:
        NStars, FWHM, Ell = 0, np.nan, np.nan

    return NStars, FWHM, Ell


def buns(Data, SN):
    # low-pass filtering
    Image = ndimage.gaussian_filter(Data, 10)
    # calc simple statistics
    mean, median, stddev = sigma_clipped_stats(Image)

    # set threshold
    Image[Image < (median + SN * stddev)] = 0

    # detect objects
    XY_coo = []
    detected_peaks = Image > 0
    labeled, num_objects = ndimage.label(detected_peaks)
    slices = ndimage.find_objects(labeled)
    for dy, dx in slices:
        x_center = (dx.start + dx.stop - 1) / 2
        x_size = dx.stop - dx.start
        y_center = (dy.start + dy.stop - 1) / 2
        y_size = dy.stop - dy.start

        # check minsize and roundness
        if (x_size > 10 and y_size > 10) and (abs(1 - (x_size / y_size)) < 0.2):
            XY_coo.append([x_center, y_center, (x_size + y_size) / 2])

    XY_coo = np.asarray(XY_coo)
    if len(XY_coo) > 0:
        NStars, FWHM, Ell = get_FWHM(Data, XY_coo)
    else:
        NStars, FWHM, Ell = 0, np.nan, np.nan

    return NStars, FWHM, Ell


def get_moments(arr):
    y, x = np.mgrid[:arr.shape[0], :arr.shape[1]]
    #     arr = arr-np.min(arr)

    # https://www.jstor.org/stable/pdf/10.1086/506972.pdf?refreqid=excelsior%3A4d186793abd043f8fe0021eda2d7c5a1
    M00 = np.sum(arr)
    M01 = np.sum(arr * x)
    M10 = np.sum(arr * y)
    Xc = M01 / M00
    Yc = M10 / M00

    # central moments
    x = x - Xc
    y = y - Yc

    M11 = np.sum(arr * y * x) / M00
    M02 = np.sum(arr * x * x) / M00
    M20 = np.sum(arr * y * y) / M00

    Msum = M20 + M02
    Mdiff = M02 - M20
    fwhm = np.sqrt(Msum / 2)
    ell = np.sqrt(Mdiff ** 2 + 4.0 * M11 ** 2) / Msum

    return fwhm, ell


def get_FWHM(Data, XY_coo):
    FWHM = []
    Ell = []
    #  set size of subarray
    R = np.ceil(np.median(XY_coo[:, 2]) / 2)

    for Star in XY_coo:  # for every star from coo file

        if R < Star[0] < (Data.shape[1] - R) and R < Star[1] < (Data.shape[0] - R):  # check edge of frame
            ROI = np.copy(Data[int(Star[1] - R):int(Star[1] + R), int(Star[0] - R):int(Star[0] + R)])  # copy small area

            _fwhm, _ell = get_moments(ROI)  # search centroid, Gauss sigma and mean sky
            FWHM.append(_fwhm)
            Ell.append(_ell)

        else:
            FWHM.append(np.nan)
            Ell.append(np.nan)

    return np.count_nonzero(~np.isnan(Ell)), np.nanmedian(FWHM).round(2), np.nanmedian(Ell).round(2)


def calc_fwhm(path):
    try:
        with FileLock(f"{path}.lock").acquire(timeout=5):
            # print("Файл успешно захвачен")
            with fits.open(path, memmap=False) as hdulist:
                header = hdulist[0].header.copy()
                image = hdulist[0].data.copy()
    except Timeout:
        print("Файл не освободился за 5 секунды — пропускаем")
        return 'fail'

    sigma_clip = SigmaClip(sigma=3.0)
    bkg_estimator = MedianBackground()
    bkg = Background2D(image, (32, 32), filter_size=(5, 5),
                       sigma_clip=sigma_clip, bkg_estimator=bkg_estimator)
    b = np.round(bkg.background_median, 2)

    # фильтрация
    f_image = ndimage.median_filter(image, 3, mode='reflect')
    f_image = ndimage.gaussian_filter(f_image, 3, 0, mode='reflect')

    mean, median, stddev = sigma_clipped_stats(f_image, sigma=3, maxiters=3,
                                               cenfunc='median', stdfunc='mad_std')

    Peaks = f_image - (median + 5 * stddev)
    detected_peaks = Peaks > 0
    labeled_im, nb_labels = ndimage.label(detected_peaks)

    sizes = ndimage.sum(detected_peaks, labeled_im, range(nb_labels + 1))
    mask_size = sizes < 5
    remove_pixel = mask_size[labeled_im]
    labeled_im[remove_pixel] = 0
    labeled_im[labeled_im > 0] = 100

    labeled_im, nb_labels = ndimage.label(labeled_im)
    if nb_labels == 0:
        return header['FOCUS'], 0, 0, 0, b

    slices = ndimage.find_objects(labeled_im)
    FWHM = []
    ELL = []

    width, height = image.shape[1], image.shape[0]
    for s in slices:
        y0, y1 = s[0].start, s[0].stop
        x0, x1 = s[1].start, s[1].stop
        cx = (x0 + x1) / 2
        cy = (y0 + y1) / 2

        # фильтр: объект в центре
        if abs(cx - width / 2) > width / 4 or abs(cy - height / 2) > height / 4:
            continue

        sx = x1 - x0
        sy = y1 - y0
        x2y = sy / sx
        if x2y > 1.2 or x2y < 0.8:
            continue

        sub = image[s] - median
        if np.sum(sub) <= 0:
            continue

        Y_index = np.arange(0, sub.shape[0], dtype=np.float64)
        X_index = np.arange(0, sub.shape[1], dtype=np.float64)
        try:
            My = np.sum(sub * Y_index[:, None]) / np.sum(sub)
            Mx = np.sum(sub * X_index[None, :]) / np.sum(sub)
        except ZeroDivisionError:
            continue

        Y_index -= My
        X_index -= Mx
        Myy = np.sum(sub * Y_index[:, None] * Y_index[:, None]) / np.sum(sub)
        Mxx = np.sum(sub * X_index[None, :] * X_index[None, :]) / np.sum(sub)

        if Mxx <= 0 or Myy <= 0:
            continue

        _fwhm = np.round(np.sqrt(Mxx + Myy) * gaussian_sigma_to_fwhm - 4, 2)
        sn = np.sum(sub) / (np.sqrt(np.sum(sub)) + bkg.background_rms_median * np.sqrt(sub.size))

        if _fwhm < 1.6 or sn < 15 or sn > 1000:
            continue

        ell = 1 - np.sqrt(min(Mxx, Myy) / max(Mxx, Myy))
        FWHM.append(_fwhm)
        ELL.append(ell)
    if not FWHM:
        return header['FOCUS'], 0, 0, 0, b

    fwhm = np.round((np.nanmedian(FWHM)), 2)
    ell = np.round(np.nanmedian(ELL), 2)
    stars_num = len(FWHM)

    if np.isnan(fwhm):
        return 'fail'
    return header['FOCUS'], fwhm, ell, stars_num, b, header['BINNING']


# def calc_source_catalog(path):
#     try:
#         with FileLock(f"{path}.lock").acquire(timeout=5):
#             # print("Файл успешно захвачен")
#             with fits.open(path, memmap=False) as hdulist:
#                 header = hdulist[0].header.copy()
#                 image = hdulist[0].data.copy()
#     except Timeout:
#         print("Файл не освободился за 5 секунды — пропускаем")
#         return 'fail'
# 
#     sigmaclip = SigmaClip(sigma=3.)
#     bkg_estimator = MedianBackground()
#     # delete background
#     bkg = Background2D(image, (32, 32), filter_size=(9, 9),
#                        sigma_clip=sigmaclip, bkg_estimator=bkg_estimator)
#     Data_without_background = image - bkg.background
#     b = np.round(bkg.background_median, 2)
#     s_sky = sigma_clip(Data_without_background, stdfunc=mad_std).filled(np.nan)
#     s_sky = np.nanstd(s_sky)
#     sigma = 9.0 * gaussian_fwhm_to_sigma  # FWHM = 3.
#     kernel = Gaussian2DKernel(sigma, x_size=3, y_size=3)
#     kernel.normalize()
#     segm = detect_sources(convolve(Data_without_background, kernel), 50 * s_sky,
#                           npixels=np.round(10/header['BINNING']))
#     if not segm:
#         return calc_fwhm(header, image)
#         # return header['FOCUS'], 0, 0, 0, b
#     cat = SourceCatalog(Data_without_background, segm)
#     fwhm = np.round(np.median(cat.fwhm.value) * 0.65 * header['BINNING'], 2)
#     ell = np.round(np.median(cat.ellipticity.value), 2)
#     stars_num = len(cat.fwhm.value)
#     if np.isnan(fwhm):
#         return 'fail'
#     return header['FOCUS'], fwhm, ell, stars_num, b


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
    dalpha = (cRa - bRa) * 60 * 60
    ddelta = (cDec - bDec) * 60 * 60

    return np.round(dx, 2), np.round(dy, 2), np.round(dalpha, 2), np.round(ddelta, 2)


if __name__ == "__main__":
    # print(sys.argv)
    if len(sys.argv) >= 3 and sys.argv[1] == "fwhm":
        try:
            image_path = sys.argv[2]
            # print(image_path)
            if not os.path.exists(image_path):
                print(f"ERR~Файл не найден: {image_path}", file=sys.stderr)
                sys.exit(1)
            focus, fwhm, ell, stars_num, b, _bin = calc_fwhm(image_path)

            fwhm = np.round(fwhm * 0.65*_bin, 2)

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
