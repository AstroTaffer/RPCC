using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RPCC.Utils;

namespace RPCC.Comms
{
    internal static class SiTechExeSocket
    {
        // private readonly Logger Logger;
        // private readonly Settings Settings;
        // private  readonly MountDataCollector MountDataCollector;

        private static readonly Timer MountTimer;

        internal static bool IsConnected;
        private static IPEndPoint _endPoint;
        private static TcpClient _client;
        private static NetworkStream _stream;
        private static StreamReader _streamReader;
        private static StreamWriter _streamWriter;


        /**
         * "-//-" == default response string
         * 
         * Valid requests and their respective confirmation responses:
         * ReadScopeStatus --- -//-
         * Abort           --- -//-, message == "_Abort Command Successful"
         * MotorsToBlinky  --- -//-, message == "_ToBlinky"
         * MotorsToAuto    --- -//-, message == "_ToAuto"
         * Park            --- -//-, message == "_Park Command Successful"
         * UnPark          --- -//-, message == "_UnPark Command Successful"
         * GoToPark        --- -//-, message == "_GoToPark {parkLocNum} Command Successful"
         * GoTo            --- -//-, message == "_GoTo Accepted"
         * Jog             --- -//-, message == "__JogArcSeconds Accepted"
         * PulseGuide      --- -//-, message == "_PulseGuide Accepted"
         * SetTrackMode    --- -//-, message == "_SetTrackMode Command Successful"
         */
        static SiTechExeSocket()
        {
            // Logger = logger;
            // Settings = settings;
            // MountDataCollector = collector;

            MountTimer = new Timer(1000);
            MountTimer.Elapsed += OnMountTimedEvent;

            IsConnected = false;
        }

        #region General

        internal static async Task<bool> Connect()
        {
            if (IsConnected)
            {
                Logger.AddLogEntry("WARNING Already connected to SiTechExe");
                return true;
            }

            _client = new TcpClient();
            _endPoint = new IPEndPoint(IPAddress.Loopback, Settings.SiTechExeTcpIpPort);
            try
            {
                await _client.ConnectAsync(_endPoint.Address, _endPoint.Port);
                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _streamReader = new StreamReader(_stream, Encoding.ASCII);
                    _streamWriter = new StreamWriter(_stream, Encoding.ASCII);
                    _streamWriter.AutoFlush = true;
                    IsConnected = true;

                    // The very first sent command will never be recognized, so we send some nonsense, literally
                    ExchangeMessages("Nonsense");
                    //await ExchangeMessagesAsync("Nonsense");
                    ReadScopeStatus();
                    MountTimer.Start();

                    Logger.AddLogEntry($"Connected to SiTechExe {_endPoint}");
                }
                else
                {
                    MountTimer.Stop();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                MountTimer.Stop();
                Logger.AddLogEntry($"WARNING Unable to connect to SiTechExe: {ex.Message}");
            }

            return IsConnected;
        }

        internal static void Disconnect()
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Already disconnected from SiTechExe");
                return;
            }

            try
            {
                MountTimer.Stop();
                MountTimer.Close();

                _streamWriter.Close();
                _streamReader.Close();
                _stream.Close();
                _client.Close();

                Logger.AddLogEntry("Disconnected from SiTechExe");
                IsConnected = false;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                Logger.AddLogEntry($"WARNING Unable to disconnect from SiTechExe: {ex.Message}");
            }
        }

        internal static async Task<string[]> ExchangeMessagesAsync(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with SiTechExe: not connected");
                return null;
            }

            try
            {
                await _streamWriter.WriteLineAsync(request);
                var response = (await _streamReader.ReadLineAsync()).Split(';');
                MountDataCollector.ParseScopeStatus(response);
                return response;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                Logger.AddLogEntry($"WARNING Unable to exchange messages with SiTechExe: {ex.Message}");
                return null;
            }
        }

        private static string[] ExchangeMessages(string request)
        {
            if (!IsConnected)
            {
                Logger.AddLogEntry("WARNING Unable to exchange messages with SiTechExe: not connected");
                return null;
            }

            try
            {
                _streamWriter.WriteLine(request);
                var response = _streamReader.ReadLine()?.Split(';');
                MountDataCollector.ParseScopeStatus(response);
                return response;
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                Logger.AddLogEntry($"WARNING Unable to exchange messages with SiTechExe: {ex.Message}");
                return null;
            }
        }

        private static bool CheckResponse(string[] response, string request)
        {
            if (response is null)
            {
                Logger.AddLogEntry("WARNING Response is null: disconnecting from SiTechExe");
                Disconnect();
                return false;
            }

            if (response[response.Length - 1] == "_UnRecognized Command")
            {
                Logger.AddLogEntry($"WARNING Unable to perform {request} command: _UnRecognized Command");
                return false;
            }

            return true;
        }

        private static void OnMountTimedEvent(object sender, EventArgs e)
        {
            if (!IsConnected)
                MountTimer.Stop();
            else
                ReadScopeStatus();
        }

        #endregion

        #region SiTechExe Commands

        internal static void ReadScopeStatus()
        {
            var response = ExchangeMessages("ReadScopeStatus");
            //var response = await ExchangeMessagesAsync("ReadScopeStatus");
            if (CheckResponse(response, "ReadScopeStatus"))
            {
                var message = response[response.Length - 1];
                if (message == "_")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to read scope status: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        internal static void Abort()
        {
            var response = ExchangeMessages("Abort");
            //var response = await ExchangeMessagesAsync("Abort");
            if (CheckResponse(response, "Abort"))
            {
                var message = response[response.Length - 1];
                if (message == "_Abort Command Successful")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to abort scope movement: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        internal static void MotorsToBlinky()
        {
            var response = ExchangeMessages("MotorsToBlinky");
            //var response = await ExchangeMessagesAsync("MotorsToBlinky");
            if (CheckResponse(response, "MotorsToBlinky"))
            {
                var message = response[response.Length - 1];
                if (message == "_ToBlinky")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to force the motors to blinky mode: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        internal static void MotorsToAuto()
        {
            var response = ExchangeMessages("MotorsToAuto");
            //var response = await ExchangeMessagesAsync("MotorsToAuto");
            if (CheckResponse(response, "MotorsToAuto"))
            {
                var message = response[response.Length - 1];
                if (message == "_ToAuto")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to force the motors to auto mode: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        internal static void Park()
        {
            var response = ExchangeMessages("Park");
            //var response = await ExchangeMessagesAsync("Park");
            if (CheckResponse(response, "Park"))
            {
                var message = response[response.Length - 1];
                if (message == "_Park Command Successful")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to park scope: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        // HACK: Apparently, it takes ~5 seconds to Unpark the scope
        internal static void Unpark()
        {
            var response = ExchangeMessages("UnPark");
            System.Threading.Thread.Sleep(1000);
            //var response = await ExchangeMessagesAsync("UnPark");
            if (CheckResponse(response, "UnPark"))
            {
                var message = response[response.Length - 1];
                if (message == "_UnPark Command Successful")
                {
                    while (MountDataCollector.IsParking)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to unpark scope: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        // HACK: After completing GoToPark command the scope won't be "parked", but won't be tracking either
        internal static void GoToPark(int parkLocNum)
        {
            var response = ExchangeMessages($"GoToPark {parkLocNum}");
            //var response = await ExchangeMessagesAsync($"GoToPark {parkLocNum}");
            if (CheckResponse(response, $"GoToPark {parkLocNum}"))
            {
                var message = response[response.Length - 1];
                if (message == $"_GoToPark {parkLocNum} Command Successful")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to go to park {parkLocNum} location: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        // [RA] = h, [DEC] = deg
        internal static bool GoTo(double ra, double dec, bool isJ2K)
        {
            if (MountDataCollector.IsParking)
            {
                Logger.AddLogEntry("Can't goto while parking");
                return false;
            }
            if (MountDataCollector.IsSlewing)
            {
                Logger.AddLogEntry("Can't goto while slewing");
                return false;
            }
            var response = ExchangeMessages($"GoTo {ra} {dec}{(isJ2K ? " J2K" : "")}");
            //var response = await ExchangeMessagesAsync($"GoTo {ra} {dec}{(isJ2K ? " J2K" : "")}");
            if (CheckResponse(response, $"GoTo {ra} {dec}{(isJ2K ? " J2K" : "")}"))
            {
                var message = response[response.Length - 1];
                if (message == "_GoTo Accepted")
                {
                    // TODO: Do stuff if good
                    while (MountDataCollector.IsSlewing)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    System.Threading.Thread.Sleep(3000);
                    return true;
                }
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to go to {ra} {dec}{(isJ2K ? " J2K" : "")}: {message}");
                    // TODO: Do stuff if bad
                    
                    // return false;
                }
            }

            return false;
        }

        // Valid directions: N (to North Celestial Pole), S (to South Celestial Pole), W (clockwise), E (counterclockwise)
        // [distance] == arcseconds
        internal static void Jog(string direction, int distance)
        {
            if (direction == "N" || direction == "S" || direction == "E" || direction == "W")
            {
                var response = ExchangeMessages($"JogArcSeconds {direction} {distance}");
                //var response = await ExchangeMessagesAsync($"JogArcSeconds {direction} {distance}");
                if (CheckResponse(response, $"JogArcSeconds {direction} {distance}"))
                {
                    var message = response[response.Length - 1];
                    if (message == "__JogArcSeconds Accepted")
                    {
                        // TODO: Do stuff if good
                    }
                    else
                    {
                        Logger.AddLogEntry($"WARNING Unable to jog {direction} {distance}: {message}");
                        // TODO: Do stuff if bad
                    }
                }
            }
            else
            {
                Logger.AddLogEntry($"WARNING Unable to jog {direction} {distance}: invalid direction");
            }
        }

        // Valid directions: N (to North Celestial Pole), S (to South Celestial Pole), W (clockwise), E (counterclockwise)
        // [time] == msec
        internal static void PulseGuide(string direction, int time)
        {
            if (MountDataCollector.IsParked)
            {
                Logger.AddLogEntry("Can't PulseGuide when parked");
                return;
            }
            if (MountDataCollector.IsParking)
            {
                Logger.AddLogEntry("Can't PulseGuide while parking");
                return;
            }
            // if (MountDataCollector.IsSlewing)
            // {
            //     Logger.AddLogEntry("Can't PulseGuide while slewing");
            //     return;
            // }
            if (direction == "N" || direction == "S" || direction == "E" || direction == "W")
            {
                string[] response = null;
                switch (direction)
                {
                    case "N":
                        response = ExchangeMessages($"PulseGuide 0 {time}");
                        //response = await ExchangeMessagesAsync($"PulseGuide 0 {time}");
                        break;
                    case "S":
                        response = ExchangeMessages($"PulseGuide 1 {time}");
                        //response = await ExchangeMessagesAsync($"PulseGuide 1 {time}");
                        break;
                    case "E":
                        response = ExchangeMessages($"PulseGuide 2 {time}");
                        //response = await ExchangeMessagesAsync($"PulseGuide 2 {time}");
                        break;
                    case "W":
                        response = ExchangeMessages($"PulseGuide 3 {time}");
                        //response = await ExchangeMessagesAsync($"PulseGuide 3 {time}");
                        break;
                }

                if (CheckResponse(response, $"PulseGuide {direction} {time}"))
                {
                    var message = response[response.Length - 1];
                    if (message == "_PulseGuide Accepted")
                    {
                        // TODO: Do stuff if good
                        Logger.AddLogEntry($"PulseGuide {direction} {time} ms is ok");
                    }
                    else
                    {
                        Logger.AddLogEntry($"WARNING Unable to pulse guide {direction} {time} ms: {message}");
                        // TODO: Do stuff if bad
                    }
                }
            }
            else
            {
                Logger.AddLogEntry($"WARNING Unable to pulse guide {direction} {time}: invalid direction");
            }
        }

        // [raRate] = [decRate] = [arcsec/sec]
        internal static void SetTrackMode(bool shouldTrack, double raRate = 0.0, double decRate = 0.0)
        {
            var response = ExchangeMessages(
                $"SetTrackMode {(shouldTrack ? 1 : 0)} {(raRate == 0.0 && decRate == 0.0 ? 0 : 1)} {raRate} {decRate}");
            //var response = await ExchangeMessagesAsync(
            //    $"SetTrackMode {(shouldTrack ? 1 : 0)} {(raRate == 0.0 && decRate == 0.0 ? 0 : 1)} {raRate} {decRate}");
            if (CheckResponse(response,
                    $"SetTrackMode {(shouldTrack ? 1 : 0)} {(raRate == 0.0 && decRate == 0.0 ? 0 : 1)} {raRate} {decRate}"))
            {
                var message = response[response.Length - 1];
                if (message == "_SetTrackMode Command Successful")
                {
                    // TODO: Do stuff if good
                }
                else
                {
                    Logger.AddLogEntry(
                        $"WARNING Unable to set track mode {(shouldTrack ? 1 : 0)} {(raRate == 0.0 && decRate == 0.0 ? 0 : 1)} {raRate} {decRate}: {message}");
                    // TODO: Do stuff if bad
                }
            }
        }

        #endregion
    }

    internal static class MountDataCollector
    {
        internal static bool ParseScopeStatus(string[] data)
        {
            /* 0  - int    BoolParms
             * 1  - double RightAsc
             * 2  - double Declination
             * 3  - double Altitude
             * 4  - double Azimuth
             * 5  - // double Secondary Axis Angle
             * 6  - // double Primary Axis Angle
             * 7  - double SiderealTime
             * 8  - double JulianDate
             * 9  - double Time
             * 10 - // double AirMass
             * 11 - string Message
             *
             * Example: 128;3,2447519;0,024984;-32,938628;0,000614;0;0;15,24479;2460178,05422338;18,30121139;-1,86;_
             */

            var buffBoolData = int.Parse(data[0]);
            IsInit = (buffBoolData & 1) > 0;
            IsTracking = (buffBoolData & 2) > 0;
            IsSlewing = (buffBoolData & 4) > 0;
            IsParking = (buffBoolData & 8) > 0;
            IsParked = (buffBoolData & 16) > 0;
            IsLookingEast = (buffBoolData & 32) > 0;
            IsInBlinky = (buffBoolData & 64) > 0;
            IsCommFault = (buffBoolData & 128) > 0;
            IsPplsOn = (buffBoolData & 256) > 0;
            IsPmlsOn = (buffBoolData & 512) > 0;
            IsSplsOn = (buffBoolData & 1024) > 0;
            IsSmlsOn = (buffBoolData & 2048) > 0;
            IsPhsOn = (buffBoolData & 4096) > 0;
            IsShsOn = (buffBoolData & 8192) > 0;

            RightAsc = double.Parse(data[1]);
            Declination = double.Parse(data[2]);
            Altitude = double.Parse(data[3]);
            Azimuth = double.Parse(data[4]);
            SiderealTime = double.Parse(data[7]);
            JulianDate = double.Parse(data[8]);
            Time = double.Parse(data[9]);
            Airmass = double.Parse(data[10]);

            return true;
        }

        #region Bool parameters

        public static bool IsInit { get; set; } // Scope is initialized
        public static bool IsTracking { get; set; } // Scope is tracking
        public static bool IsSlewing { get; set; } // Scope is slewing
        public static bool IsParking { get; set; } // Scope is parking
        public static bool IsParked { get; set; } // Scope is parked
        public static bool IsLookingEast { get; set; } // Scope is "Looking East" (GEM)
        public static bool IsInBlinky { get; set; } // BrushlessController is in Blinky mode, one or both axis's
        public static bool IsCommFault { get; set; } // Communication fault between SiTechExe and BrushlessController
        public static bool IsPplsOn { get; set; } // Primary Plus Limit Switch activated
        public static bool IsPmlsOn { get; set; } // Primary Minus Limit Switch activated
        public static bool IsSplsOn { get; set; } // Secondary Plus Limit Switch activated
        public static bool IsSmlsOn { get; set; } // Secondary Minus Limit Switch activated
        public static bool IsPhsOn { get; set; } // Primary Homing Switch activated
        public static bool IsShsOn { get; set; } // Secondary Homing Switch activated

        #endregion

        #region Non-bool parameters

        public static double RightAsc { get; set; } // Scope Right Ascension [h]
        public static double Declination { get; set; } // Scope Declination [deg]
        public static double Altitude { get; set; } // Scope Altitude [deg]
        public static double Azimuth { get; set; } // Scope Azimuth [deg]
        public static double SiderealTime { get; set; } // Scope Sidereal Time [h]
        public static double JulianDate { get; set; } // Scope JD [d]   
        public static double Time { get; set; } // Scope Time [h]
        public static double Airmass { get; set; } // Airmass

        #endregion
    }
}