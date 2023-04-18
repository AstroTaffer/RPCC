import numpy as np
import os
import subprocess
from astropy.io import ascii
from astropy.stats import sigma_clipped_stats as scs


def Sex(Input_File, Output_File):
    cwd = os.getcwd()
    Sex = cwd + '\Sex\Extract.exe '
    dSex = ' -c ' + cwd + '\Sex\default.sex'
    dPar = ' -PARAMETERS_NAME ' + cwd + '\Sex\default.par'
    dFilt = ' -FILTER_NAME ' + cwd + '\Sex\\tophat_2.5_3x3.conv'
    NNW = ' -STARNNW_NAME ' + cwd + '\Sex\default.nnw'

    shell = Sex + Input_File + dSex + dPar + dFilt + NNW + \
            ' -CATALOG_NAME ' + Output_File
    print(shell)
    startupinfo = subprocess.STARTUPINFO()
    startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    child = subprocess.run(shell, timeout=60, startupinfo=startupinfo)

    if (child.returncode == 0 and os.path.isfile(Output_File)):
        print('Ok')
    else:
        print('Error')


def Mean_FWHM(Input_File, x, y, s):
    Tbl = ascii.read(Input_File)
    X = Tbl['X_IMAGE']
    Y = Tbl['Y_IMAGE']
    Z = Tbl['FWHM_IMAGE']

    Index = np.where((Z < 5) & (Z > 0))[0]
    X = X[Index]
    Y = Y[Index]
    Z = Z[Index]

    Index = np.where((X < (x + s)) & (X > (x - s)))[0]
    X = X[Index]
    Y = Y[Index]
    Z = Z[Index]
    Index = np.where((Y < (y + s)) & (Y > (y - s)))[0]
    # X = X[Index]
    # Y = Y[Index]
    Z = Z[Index]
    stat = scs(Z)
    print(stat)
    return (stat)


Path2Data = 'D:/Observations/Masha/check/FWHM'
file_list = []
dir_content = os.listdir(Path2Data)
for ii in range(0, len(dir_content)):
    if dir_content[ii].count('.fit') or dir_content[ii].count('.fits'):
        file_list.append(Path2Data + '/' + dir_content[ii])
res = []
for f in file_list:
    print(f)
    c = f.replace('fits', 'cat')
    Sex(f, c)
    stat = Mean_FWHM(c, 1500, 2200, 100)
    res.append(stat)

print(res)
