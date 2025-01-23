import datetime
import socket
import threading

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

time_last_message = None
time_wait_sec = 3
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


def write_to_fits(path, fwhm, ell, stars_num, b):
    with fits.open(path, memmap=False, mode='update') as hdulist:
        fwhm_card = fits.Card('FWHM', 'nan' if np.isnan(fwhm) else fwhm, 'Median FWHM [arcsec]')
        ell_card = fits.Card('ELL', 'nan' if np.isnan(ell) else ell, 'Median ellipticity')
        stars_card = fits.Card('NSTARS', 'nan' if np.isnan(stars_num) else stars_num, "Stars on frame")
        bkg_card = fits.Card('BKG', 'nan' if np.isnan(b) else b, "Median background")
        hdulist[0].header.append(fwhm_card)
        hdulist[0].header.append(ell_card)
        hdulist[0].header.append(stars_card)
        hdulist[0].header.append(bkg_card)


def calc_fwhm(path):
    with fits.open(path, memmap=False, mode='update') as hdulist:
        header = hdulist[0].header
        image = hdulist[0].data.copy()
        sigma_clip = SigmaClip(sigma=3.0)
        bkg_estimator = MedianBackground()
        bkg = Background2D(image, (32, 32), filter_size=(3, 3),
                           sigma_clip=sigma_clip, bkg_estimator=bkg_estimator)
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
        fwhm = np.round((np.nanmedian(np.asarray(FWHM))-2.2) * 0.65 * header['XBINNING'], 2)
        ell = np.round(np.nanmedian(np.asarray(ELL)), 2)
        stars_num = len(FWHM)
        b = np.round(bkg.background_median, 2)
    if np.isnan(fwhm):
        return 'fail'
    # return '~'.join([str(header['FOCUS']), str(fwhm), str(ell), str(stars_num), str(b)])
    return header['FOCUS'], fwhm, ell, stars_num, b


def calc_source_catalog(path):
    with fits.open(path, memmap=False, mode='update') as hdulist:
        header = hdulist[0].header
        image = hdulist[0].data.copy()
        sigmaclip = SigmaClip(sigma=3.)
        bkg_estimator = MedianBackground()
        # delete background
        bkg = Background2D(image, (32, 32), filter_size=(9, 9),
                           sigma_clip=sigmaclip, bkg_estimator=bkg_estimator)
        Data_without_background = image - bkg.background
        # Sky = bkg.background_median
        # print('Sky={0:.1f}'.format(Sky))
        s_sky = sigma_clip(Data_without_background, stdfunc=mad_std).filled(np.nan)
        s_sky = np.nanstd(s_sky)
        # print('S_Sky = ', s_sky)
        sigma = 9.0 * gaussian_fwhm_to_sigma  # FWHM = 3.
        kernel = Gaussian2DKernel(sigma, x_size=3, y_size=3)
        kernel.normalize()
        segm = detect_sources(convolve(Data_without_background, kernel), 50 * s_sky, npixels=5)
        cat = SourceCatalog(Data_without_background, segm)
        # cat.moments
        fwhm = np.round(np.median(cat.fwhm.value) * 0.65 * header['XBINNING'], 2)
        ell = np.round(np.median(cat.ellipticity.value), 2)
        b = np.round(bkg.background_median, 2)
        stars_num = len(cat.fwhm.value)
        if np.isnan(fwhm):
            return 'fail'
    # return '~'.join([str(header['FOCUS']), str(fwhm), str(ell), str(stars_num), str(b)])
    return header['FOCUS'], fwhm, ell, stars_num, b


def timer_loop():
    now = datetime.datetime.utcnow()
    if time_last_message is not None:
        if (now - time_last_message).total_seconds() > time_wait_sec:
            print('Donuts timeout, closing server')
            server.close()


def calc_don_shifts(data):
    donuts = Donuts(refimage=data[1], image_ext=0, overscan_width=24, prescan_width=24,
                    border=50, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
    hlist = fits.open(data[1])
    h = hlist[0].header
    shift_result = donuts.measure_shift(data[2])
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

    ans = f'{np.round(dx, 2)}~{np.round(dy, 2)}~{np.round(dalpha, 2)}~{np.round(ddelta, 2)}'
    print('ans = ' + ans)
    return ans


def pars_req(req: str) -> str:
    # if 'ping' in req:
    #     return 'pong'
    data = req.split('~')
    if len(data) < 2:
        return 'fail while split'
    if "\n" in data[-1]:
        data[-1] = data[-1][:-1]

    if 'don' in data[0]:
        try:
            return calc_don_shifts(data)
        except Exception as e:
            print(e)
            print('fail don')
            return f'fail: {e}'
    if 'fwhm' in data[0]:
        try:
            print(data)
            focus, fwhm, ell, stars_num, b = calc_source_catalog(data[1])
            if fwhm > 4.5:
                focus, fwhm, ell, stars_num, b = calc_fwhm(data[1])
            write_to_fits(data[1], fwhm, ell, stars_num, b)
        except Exception as e:
            print(e)
            print('fail fwhm')
            return f'fail: {e}'
        return '~'.join([str(focus), str(fwhm), str(ell), str(stars_num), str(b)])


def handle_client(reader, writer):
    t = threading.Timer(time_wait_sec, timer_loop)
    t.start()
    while True:
        request = reader.readline()
        time_last_message = datetime.datetime.utcnow()
        if 'quit' in request:
            break
        if (request is not None) and (request != '') and ('\ufeff' not in request):
            response = pars_req(request) + '\n'
            # if 'pong' in response:
            #     continue
            writer.writelines(response)
            writer.flush()
    writer.close()


def run_server():
    # server.bind((socket.gethostname(), 3030))
    server.bind(('127.0.0.1', 3030))
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # реюз порта
    server.listen(1)
    # while True:
    client_socket, _ = server.accept()
    # client_socket.settimeout(60)
    try:
        with client_socket:
            handle_client(client_socket.makefile('r'), client_socket.makefile('w'))
    except ConnectionAbortedError:
        pass


if __name__ == '__main__':
    run_server()
