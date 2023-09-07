import os
import astropy.io.fits as fits
import astropy.wcs as wcs
from astropy.stats import SigmaClip
from astropy.time import Time
from photutils.background import Background2D, MedianBackground
from photutils.aperture import aperture_photometry
from astropy.io import ascii

from Astrometry import Astrometry
from Photometry_Utils import *


path2data = r'D:\RoboPhot Data\Raw Images\2023_09 FLI\2023_09_06 GSC2314–0530\r\done'
Cat_R = 0.1  # catalog each cone radius
Cat_len = 400  # catalog max length
V_lim = 15  # catalog max depth
Bbox = 9  # centering/morphology box half-size (pixels)
Raper = 5
coords = '02:20:50.9 +33:20:46.6'
C = SkyCoord(coords, unit=(u.hourangle, u.deg), frame='icrs')  # , obstime='J2015.5'

# do astrometry
path2data = Astrometry(path2data, C)

# read directory and create list of fits-files
file_list = []
for f in os.listdir(path2data):
    if f.count('.fits') or f.count('.fit'):
        file_list.append(path2data + '/' + f)

# set paths and suffix for output files
Path2Save = path2data + '/Photometry'
if not os.path.exists(Path2Save):
    os.makedirs(Path2Save)

# clean old photometry
for f in os.listdir(Path2Save):
    if f.count('Phot'):
        os.remove(Path2Save+'/'+f)

print('Download GAIA')
Catalog = Get_GAIA(C.ra.degree, C.dec.degree, Cat_R, V_lim, Cat_len)
ascii.write(Catalog, Path2Save + '/Cat.txt', overwrite=True, delimiter='\t')

# open log file
df = open(Path2Save + '/Time.txt', 'w')
df.write('JD\tEXPTIME\t' +
         'Sky\tX\tY\tMax\n')
output = open(Path2Save + '/Phot' + '.txt', 'a')
counter = 0
for f in file_list:
    counter = counter + 1
    print(f'----------<Frame: {counter}/{len(file_list)}>-----------')
    print(f.split('/')[-1])
    hduList = fits.open(f)
    Header = hduList[0].header
    Data = hduList[0].data
    hduList.verify('fix')
    hduList.close()
    w = wcs.WCS(Header)

    jd = Time(Header['DATE-OBS'], scale='utc').jd
    df.write('{:.7f}'.format(jd) + '\t')  # JD
    # df.write('{:.7f}'.format(Header['jd']) + '\t')  # JD
    df.write('{:.1f}'.format(Header['EXPTIME']) + '\t')  # EXPTIME

    X, Y = w.all_world2pix(Catalog['Ra'], Catalog['Dec'], 0)
    Catalog.add_columns([X, Y], names=['X', 'Y'])

    # delete background
    sigmaclip = SigmaClip(sigma=3.)
    bkg_estimator = MedianBackground()
    bkg = Background2D(Data, (100, 100), filter_size=(9, 9),
                       sigma_clip=sigmaclip, bkg_estimator=bkg_estimator)
    Data_without_background = Data - bkg.background
    Sky = np.median(bkg.background)
    df.write('{:.1f}'.format(Sky) + '\t')  # Sky
    print('Sky={0:.1f}'.format(Sky))

    # centroiding and image properties
    Catalog_COM = get_com(Data_without_background, Catalog, Bbox)  # center of mass / fast and better!
    #     Catalog = get_1dg(Data, Catalog, Bbox) # Gauss 1d fit

    if counter == 1:
        DrawMap(Data, 256, Header, Catalog_COM, Path2Save + r'\field_com.pdf', Raper)

    df.write('{0:.3f}\t{1:.3f}\t{2:.1f}\n'.format(Catalog_COM['X'][0],
                                                  Catalog_COM['Y'][0],
                                                  Catalog_COM['Max'][0]))
    print('Max count={0:.1f}'.format(Catalog_COM['Max'][0]))

    Positions = np.transpose([Catalog_COM['X'], Catalog_COM['Y']])
    Stellar_aper = CircularAperture(Positions, r=Raper)
    Stellar_phot = aperture_photometry(Data_without_background, Stellar_aper, method='exact')

    # write to log
    Flux = Stellar_phot['aperture_sum'].value
    np.savetxt(output, Flux, fmt='%1.3f', delimiter='\t', newline='\t')
    output.write('\n')

    Catalog_COM.remove_columns(names=('X', 'Y', 'Max'))

output.close()
df.close()

path2phot = path2data + r'\Photometry'
flux = np.genfromtxt(path2phot + r'\Phot.txt')
cat = ascii.read(path2phot + r'\Cat.txt')
time = ascii.read(path2phot + r'\Time.txt')

Trend, Cat = GetTrend(flux, cat)
Flux_Corr = flux / Trend[:, np.newaxis]

tar_Flux = Flux_Corr[:, 0]

t = time['JD'] - int(np.max(time['JD']))
plt.clf()
plt.plot(t[200:], tar_Flux[200:], '.r')
plt.show()
plt.savefig(rf'{path2phot}\plot_crop.pdf')
