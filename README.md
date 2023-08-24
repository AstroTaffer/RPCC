# RPCC
RoboPhot Cameras Controls

This program is designed for control and adjustment of RoboPhot KAO UrFU telescope. It does the following:
- Observation planning
- Cameras and focuser control
- Mount control and guiding correction
- FITS images IO and basic photometric analysis
- Data exchange with [MeteoDome](https://github.com/NabatFasetochnii/MeteoDome) and [SiTechExe](https://www.siderealtechnology.com/)


Installation:
1. Make sure you have following software installed:
    - .NET Framework 4.6.1
    - [Python 3.X](https://www.python.org/) (latest version)
    - [NumPy](https://numpy.org/install/) (latest version)
    - [Donuts](https://pypi.org/project/donuts/) (latest version)
    - MeteoDome (optionally)
    - [ASCOM Platform](https://ascom-standards.org/index.htm) (latest version, optionally, required by SiTechExe)
    - SiTechExe (latest version, optionally)
2. Clone this repo and build executable files
3. Launch application (RPCC.exe)
