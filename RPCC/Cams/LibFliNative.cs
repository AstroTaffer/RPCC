//**************************************************************
// Finger Lakes Instrumentation Camera Interface library
//
// The NativeMethods class contains static methods which use interp services
// to properly invoke and marshal functions exported by libfli.dll.
//
// These method definitions MUST be consistent with the 
// version of libfli.dll referenced by the application.
// Specifically, the method definitions must match the function prototype
// declarations in libfli.h
//
// NOTE: These static methods are NOT MTSafe. they are intended to be
// invoked by a higher abstraction level layer providing the appropriate
// critical region protection.
//
// Refer to FLI Software Development Library documentation for 
// detailed descriptions of each function.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace RPCC.Cams
{
    internal static class NativeMethods
    {
        // for 2.0.0.0
        // used with 2.0.3 x86
        //**************************************************************

        #region Constants and Type definitions

        // Bit Depth modes
        public enum FLiBitDepth
        {
            MODE_8BIT = 0,
            MODE_16BIT = 1,
            MODE_12BIT = 2
        }

        public const int INVALID_ARGUMENT = -22;

        #endregion Constants and Type definitions

        //**************************************************************

        #region Library Functions

        //-------------------------------------------
        /// <summary>
        ///     Get the current library version string
        ///     Prior to call, ver capacity must be >= len
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "0")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetLibVersion(
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder ver,
            IntPtr len); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Set the library debug level
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "0")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetDebugLevel(
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int level);

        /// <summary>
        ///     Write a message to the debug log
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "1")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void FLIDebug(
            int level,
            [MarshalAs(UnmanagedType.LPStr)] string message);

        #endregion Library Functions

        //**************************************************************

        #region Device Enumeration, Open, Close

        //-------------------------------------------
        /// <summary>
        ///     Retrieve the list of available devices
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIList(
            int domain,
            out IntPtr names); // NOTE: char*** (pointer to array of strings) is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Free a previously generated device list
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIFreeList(
            IntPtr names); // NOTE char** (array of strings) - how many

        //-------------------------------------------
        /// <summary>
        ///     Open a connection to the specified device
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "1")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIOpen(
            out int dev,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            int domain);

        //-------------------------------------------
        /// <summary>
        ///     Close the handle to the specified FLI device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIClose(
            int dev);

        #endregion Device Enumeration, Open, Close

        //**************************************************************

        #region Get Camera Info

        //-------------------------------------------
        /// <summary>
        ///     Get the model of the specified device
        ///     Prior to call, model capacity must be >= len
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "1")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetModel(
            int dev,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder model,
            IntPtr len); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Get the serial string for the specified device
        ///     Prior to call, serial capacity must be >= len
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "1")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetSerialString(
            int dev,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder serial,
            IntPtr len); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Get the pixel dimensions for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetPixelSize(
            int dev,
            out double pixel_x,
            out double pixel_y);

        //-------------------------------------------
        /// <summary>
        ///     Get the hardware revision for the specified device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetHWRevision(
            int dev,
            out int hwrev);

        //-------------------------------------------
        /// <summary>
        ///     Get firmware revisiion for specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetFWRevision(
            int dev,
            out uint fwrev);

        //-------------------------------------------
        /// <summary>
        ///     Get the total area of the image array for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetArrayArea(
            int dev,
            out int ul_x,
            out int ul_y,
            out int lr_x,
            out int lr_y);

        //-------------------------------------------
        /// <summary>
        ///     Get the visible area of the image array for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetVisibleArea(
            int dev,
            out int ul_x,
            out int ul_y,
            out int lr_x,
            out int lr_y);

        //-------------------------------------------
        /// <summary>
        ///     Get the current cooler power for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetCoolerPower(
            int dev,
            out double power);

        //-------------------------------------------
        /// <summary>
        ///     Get the color pattern for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetCameraColorPattern(
            int dev,
            out int pattern);

        #endregion Get Camera Info

        //**************************************************************

        #region Camera Configuration

        //-------------------------------------------
        /// <summary>
        ///     Set the exposure time for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetExposureTime(
            int dev,
            int exptime);

        //-------------------------------------------
        /// <summary>
        ///     Set the active image area for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetImageArea(
            int dev,
            int ul_x,
            int ul_y,
            int lr_x,
            int lr_y);

        //-------------------------------------------
        /// <summary>
        ///     Set the horizontal bin factor for the specified camera
        ///     (valid range is 1-16)
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetHBin(
            int dev,
            int hbin);

        //-------------------------------------------
        /// <summary>
        ///     Set the vertical bin factor for the specified camera
        ///     (valid range is 1-16)
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetVBin(
            int dev,
            int vbin);

        //-------------------------------------------
        /// <summary>
        ///     Set the frame type for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetFrameType(
            int dev,
            int frametype);

        //-------------------------------------------
        /// <summary>
        ///     Set the target temperature (deg C) for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetTemperature(
            int dev,
            double temperature);

        //-------------------------------------------
        /// <summary>
        ///     Get the current temperature for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetTemperature(
            int dev,
            out double temperature);

        //-------------------------------------------
        /// <summary>
        ///     Retrieves the specified temperature from a device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIReadTemperature(
            int dev,
            int channel,
            out double temperature);

        //-------------------------------------------
        /// <summary>
        ///     Set the number of times the image array is flushed before exposing a frame
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetNFlushes(
            int dev,
            int nflushes);

        //-------------------------------------------
        /// <summary>
        ///     Set the gray-scale bit depth of the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetBitDepth(
            int dev,
            int bitdepth);

        //-------------------------------------------
        /// <summary>
        ///     Get the mode of the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetCameraMode(
            int dev,
            out int mode_index);

        //-------------------------------------------
        /// <summary>
        ///     Get the mode string for mode mode_index on the specified camera
        ///     Prior to call, mode_string capacity must be >= len
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments",
            MessageId = "2")]
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetCameraModeString(
            int dev,
            int mode_index,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder mode_string,
            IntPtr len); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Set the mode for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetCameraMode(
            int dev,
            int mode_index);

        //-------------------------------------------
        /// <summary>
        ///     Set the time delay and integration for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetTDI(
            int dev,
            int tdi_rate,
            int flags);

        //-------------------------------------------
        /// <summary>
        ///     Set the fan speed for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetFanSpeed(
            int dev,
            int fan_speed);

        //-------------------------------------------
        /// <summary>
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIEnableVerticalTable(
            int dev,
            int width,
            int offset,
            int flags);

        //-------------------------------------------
        /// <summary>
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetVerticalTableEntry(
            int dev,
            int index,
            int height,
            int bin,
            int mode);

        //-------------------------------------------
        /// <summary>
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetVerticalTableEntry(
            int dev,
            int index,
            out int height,
            out int bin,
            out int mode);

        /// <summary>
        ///     Retrieve the Thumbnail from the camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIReadThumbnail(
            int dev,
            long length,
            byte[] rbuf);

        /// <summary>
        ///     Write the Thumbnail to the camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIWriteThumbnail(
            int dev,
            long length,
            byte[] wbuf);

        #endregion Camera Configuration

        //**************************************************************

        #region Camera Control

        //-------------------------------------------
        /// <summary>
        ///     Locks the specified device for exclusive access
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLILockDevice(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Unlock the specified device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIUnlockDevice(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Cancel an exposure in progress by closing the shutter
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLICancelExposure(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Get remaining exposure time (mSec) for specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetExposureStatus(
            int dev,
            out int timeleft);

        //-------------------------------------------
        /// <summary>
        ///     Retrieve the next available image frame row from the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGrabRow(
            int dev,
            ushort[] buff,
            IntPtr width); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Expose a frame according to current settings
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIExposeFrame(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Flush row(s) on the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIFlushRow(
            int dev,
            int rows,
            int repeat);

        //-------------------------------------------
        /// <summary>
        ///     Control the camera shutter operation
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIControlShutter(
            int dev,
            int shutter);

        //-------------------------------------------
        /// <summary>
        ///     Enables background flushing on the image array
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIControlBackgroundFlush(
            int dev,
            int bgflush);

        //-------------------------------------------
        /// <summary>
        ///     Retrieve a full image frame from the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGrabFrame(
            int dev,
            IntPtr pBuff,
            IntPtr buffsize, // NOTE: size_t is marshalled as IntPtr
            out IntPtr bytesgrabbed); // NOTE: This is a pointer to size_t

        //-------------------------------------------
        /// <summary>
        ///     Start video mode for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIStartVideoMode(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Stop video mode on specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIStopVideoMode(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Retrieve a video frame from the specified camera
        ///     Prior to call, buff length be >= size
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGrabVideoFrame(
            int dev,
            ushort[] buff,
            IntPtr size); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     end the exposure on the specified camera and begin immediate image transfer
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIEndExposure(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Trigger an exposure that is awaiting an external trigger
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLITriggerExposure(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Get current status of specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetDeviceStatus(
            int dev,
            out int status);

        #endregion Camera Control

        //**************************************************************

        #region Camera IO Control

        //-------------------------------------------
        /// <summary>
        ///     Configures the camera I/O port
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIConfigureIOPort(
            int dev,
            int ioportset);

        //-------------------------------------------
        /// <summary>
        ///     Read the I/O port on the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIReadIOPort(
            int dev,
            out int ioportset);

        //-------------------------------------------
        /// <summary>
        ///     Write the I/O port on the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIWriteIOPort(
            int dev,
            int ioportset);

        //-------------------------------------------
        /// <summary>
        ///     Get the dimensions of the next exposure for the specified camera
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetReadoutDimensions(
            int dev,
            out int width,
            out int hoffset,
            out int hbin,
            out int height,
            out int voffset,
            out int vbin);

        #endregion Camera IO Control

        //**************************************************************

        #region Camera Low Level Access

        //-------------------------------------------
        /// <summary>
        ///     Send a Raw packet
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLICmdSendRaw(
            int dev,
            byte[] Data,
            int Length);

        //-------------------------------------------
        /// <summary>
        ///     Send a Rww packet, Receive response
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLICmdSendRecvRaw(
            int dev,
            byte[] TxData,
            uint TxLength,
            out byte[] RxData,
            uint RxLength);

        //-------------------------------------------
        /// <summary>
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetDAC(
            int dev,
            uint dacset);

        //-------------------------------------------
        /// <summary>
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIUsbBulkIO(
            int dev,
            int ep,
            byte[] buf,
            out int len);

        //-------------------------------------------
        /// <summary>
        ///     Read data from the camera EEPROM
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIReadUserEEPROM(
            int dev,
            int loc,
            int address,
            int length,
            out byte[] rbuf);

        //-------------------------------------------
        /// <summary>
        ///     Write data to the camera EEPROM
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIWriteUserEEPROM(
            int dev,
            int loc,
            int address,
            int length,
            byte[] wbuf);

        #endregion Camera Low Level Access

        //**************************************************************

        #region Filter Wheel and Focuser

        /// -------------------------------------------
        /// <summary>
        ///     Get name of the specified filter
        ///     Prior to call, name capacity must be >= len
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetFilterName(
            int dev,
            int filter,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder name,
            IntPtr len); // NOTE: size_t is marshalled as IntPtr

        //-------------------------------------------
        /// <summary>
        ///     Get the active filter wheel for the specified device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetActiveWheel(
            int dev,
            out int wheel);

        //-------------------------------------------
        /// <summary>
        ///     Sets the active filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetActiveWheel(
            int dev,
            int wheel);

        //-------------------------------------------
        /// <summary>
        ///     Set movement speed for the specified high speed filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetWheelSpeed(
            int dev,
            int speed);

        //-------------------------------------------
        /// <summary>
        ///     Resets the specified high speed filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIResetHSFilterWheel(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Get the sequence programming status for specifed filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetSequenceProgramming(
            int dev,
            out int enabled);

        //-------------------------------------------
        /// <summary>
        ///     Start sequence programming mode for the specified filter wheel device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIStartSequenceProgramming(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Ends sequence programming mode for the specified high speed filter wheel device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIEndSequenceProgramming(
            int dev);

        //-------------------------------------------
        /// <summary>
        ///     Set the filter position for the specified filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLISetFilterPos(
            int dev, int filter);

        //-------------------------------------------
        /// <summary>
        ///     Get the current position for the specified filter
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetFilterPos(
            int dev,
            out int filter);

        //-------------------------------------------
        /// <summary>
        ///     Get number of filters for specified filter wheel
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetFilterCount(
            int dev,
            out int filter);

        //-------------------------------------------
        /// <summary>
        ///     Move the specified focuser or filter wheel in blocking mode
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIStepMotor(
            int dev,
            int steps);

        //-------------------------------------------
        /// <summary>
        ///     Move the specified focuser or filter wheel in non-blocking mode
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIStepMotorAsync(
            int dev,
            int steps);

        //-------------------------------------------
        /// <summary>
        ///     Get the stepper motor position for the specified filter wheel or focuser device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetStepperPosition(
            int dev,
            out int position);

        //-------------------------------------------
        /// <summary>
        ///     Get the number of motor steps remaining for the specified filter wheel or focuser device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetStepsRemaining(
            int dev,
            out int steps);

        //-------------------------------------------
        /// <summary>
        ///     Get the maximum extent for the specified focuser
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIGetFocuserExtent(
            int dev,
            out int extent);

        //-------------------------------------------
        /// <summary>
        ///     Home the specified focuser or filter wheel device
        /// </summary>
        [DllImport("libfli.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int FLIHomeDevice(
            int dev);

        #endregion Filter Wheel and Focuser
    }
}