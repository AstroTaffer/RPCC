using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Tasks;
using RPCC.Utils;

namespace RPCC.Focus
{
    public static class CameraFocus
    {
        /// <summary>
        ///     Функции фокусировки камеры.
        /// </summary>
        private const int MaxFocCycles = 5;
        private const int MaxFocBadFrames = 3;
        private const int MaxSumShifts = 1000;
        // public static double FwhmBest { get; set; }
        public static bool IsAutoFocus { get; set; }
        public static bool IsZenith { get; set; }
        public static int DeFocus { get; set; }
        private static short _focCycles; // focus cycles counter
        private static short _focBadFrames; // bad focus frames
        private static readonly List<GetDataFromFits> Frames = new();
        private static int _startFocusPos;
        // private const int N = 10; //количество точек для построения кривой фокусировки
        private static short _shift; //шаг по фокусу = -50
        private static short _sumShift;
        private static ObservationTask _taskForFocus;
        private static short _phase;
        private static double _oldFwhm;
        private const short FocusExp = 20;
        public static double Seeing = 1;
        public static bool IsFocusing;

        public static void StartAutoFocus(ObservationTask observationTask)
        {
            Logger.AddLogEntry("FOCUS: phase 1: defocusing");
            IsFocusing = true;
            _taskForFocus = observationTask.Copy();
            _taskForFocus.FrameType = StringHolder.Focus;
            if (Head.currentTask.Exp > FocusExp)
            {
                _taskForFocus.Exp = FocusExp;
            }
            
            _focCycles = 0; //
            _focBadFrames = 0; //
            Frames.Clear();
            _startFocusPos = SerialFocus.CurrentPosition;
            _shift = -150;
            _sumShift = 0;
            _phase = 0;
            // _frameCounter = 0;
            
            //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
            
            GetImForFocus(_shift);
        }
        
        public static void CamFocusCallback()
        {
            if (!IsAutoFocus)
            {
                Logger.AddLogEntry("FOCUS: Autofocus disable, exit");
                ReturnFocusAndExit();
                return;
            }

            if (!WeatherDataCollector.Obs)
            {
                Logger.AddLogEntry($"Weather is bad, pause task #{_taskForFocus.TaskNumber}");
                Head.isOnPause = true;
            }

            if (Math.Abs(_sumShift) > MaxSumShifts)
            {
                Logger.AddLogEntry($"FOCUS: Math.Abs(_sumShift) > {MaxSumShifts}, return focus and exit");
                ReturnFocusAndExit();
                return;
            }
            // Head.Guiding();
            var focusImPath = CameraControl.cams.Last().LatestImageFilename;
            if (string.IsNullOrEmpty(focusImPath))
            {
                Logger.AddLogEntry("FOCUS: no data available, stop obs");
                ReturnFocusAndExit();
                return;
            }

            if (Head.currentTask.Status != 1) 
            {
                Logger.AddLogEntry("FOCUS: task ended, return focus and exit");
                ReturnFocusAndExit();
                return;
            }
            switch (_phase)
            {
                case 0: 
                    PhaseOne(focusImPath);
                    break;
                case 1:
                    PhaseTwo(focusImPath);
                    break;
                case 2:
                    PhaseThree(focusImPath);
                    break;
            }
        }

        #region Phases
        private static void PhaseOne(string focusImPath)
        {
            // if (IsZenith)
            // {
            //     Repoint4Focus(); //Перенаводимся в зенит для фокусировки
            // }
            
            //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
            if (_focBadFrames == 0)
            {
                _focCycles++;
            }
            
            Frames.Add(new GetDataFromFits(focusImPath));
            Logger.LogFrameInfo(Frames.Last(), CameraControl.cams.Last().Filter);
            
            if (!Frames.Last().Status)
            {
                Logger.AddLogEntry("FOCUS: FWHM information is not available, return focus and exit");
                ReturnFocusAndExit();
                return;
            }
            
            //проверяем валидность измерений (кол-во звезд, вытянутость), счетчик плохих кадров + или 0.
            if (!Frames.Last().Quality)
            {
                _focBadFrames++;
                //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит и рестарт процедуры, но без первого сдвига.
                if (_focBadFrames >= MaxFocBadFrames)
                {
                    // if (IsZenith)
                    // {
                    Logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                    ReturnFocusAndExit();
                    return;
                    // }
                    // Logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                    // Repoint4Focus();
                    // _focBadFrames = 0;
                    // _focCycles = 0;
                }
                else
                {
                    Logger.AddLogEntry("FOCUS: Bad frames, another cycle");
                }
                GetImForFocus(_shift); //еще раз к началу цикла
                return;
            }
            
            //много плохих кадров
            if (_focBadFrames >= MaxFocBadFrames)
            {
                Logger.AddLogEntry("FOCUS: can't defocus, bad frames limit, return focus and exit");
                ReturnFocusAndExit();
                return;
            }

            //после 5 попыток дефокуса
            if (_focCycles >= MaxFocCycles)
            {
                Logger.AddLogEntry($"FOCUS: can't defocus after {MaxFocCycles} iterations, return focus and exit");
                ReturnFocusAndExit();
                return; //что-то сломалось, выходим
            }

            if (Frames.Last().Fwhm < 3)
            {
                Logger.AddLogEntry("FOCUS: FWHM < 3");
                Logger.AddLogEntry("FOCUS: image is focused");
                FocusingDone(Frames.Last().Fwhm);
                return;
            }
            
            //проверяем FWHM, если больше 6, то рассчитываем сдвиг в ту же сторону еще на 90*(6-FWHM)
            if (Frames.Last().Fwhm < 6)
            {
                _shift = BigShift(Frames.Last().Fwhm, _shift);
                GetImForFocus(_shift);
                return;
            }

            _phase = 1;

            //теперь точно в дефокусе и точно знаем где фокус
            //начинаем движение в сторону фокуса
            Logger.AddLogEntry("FOCUS: phase 2, focusing");
            _focCycles = 0;
            _focBadFrames = 0;
            _oldFwhm = Frames.Last().Fwhm;
            //сдвиг считаем как 22*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
            _shift = SmallShift(_oldFwhm, _shift);
            GetImForFocus(_shift);
        }

        private static void PhaseTwo(string focusImPath)
        {
            Logger.AddLogEntry("FOCUS: Focus cycle #" + _focCycles);
            Frames.Add(new GetDataFromFits(focusImPath));
            Logger.LogFrameInfo(Frames.Last(), CameraControl.cams.Last().Filter);
            var fwhm = Frames.Last().Fwhm;
            
            if (!Frames.Last().Status)
            {
                Logger.AddLogEntry("FOCUS: FWHM information is not available, return focus and exit");
                ReturnFocusAndExit();
                return;
            }

            //проверяем валидность измерений (кол-во звезд, вытянутость в двух трубах), счетчик плохих кадров+ или 0.
            if (!Frames.Last().Quality)
            {
                _focBadFrames++;
                //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит.
                if (_focBadFrames >= MaxFocBadFrames)
                {
                    // if (IsZenith)
                    // {
                    Logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                    ReturnFocusAndExit();
                    return;
                    // }

                    // Logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                    // Repoint4Focus();
                    // _focBadFrames = 0;
                    // _focCycles = 0;
                }
                else
                {
                    Logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                }
                GetImForFocus(0);
                return; //еще раз к началу цикла
            }

            _focBadFrames = 0;
            
            //выходим из цикла когда ((FWHM<2.2) или (ухудшается FWHM) или измение FWHM<0.2)
            // Logger.AddLogEntry("FOCUS: check");

            if (fwhm < 3)
            {
                Logger.AddLogEntry("FOCUS: FWHM < 3");
                Logger.AddLogEntry("FOCUS: image is focused");
                FocusingDone(fwhm);
                return;
            }
            if (_oldFwhm < fwhm)
            {
                Logger.AddLogEntry("FOCUS: Old_FWHM < FWHM");
                if (fwhm<3)
                {
                    Logger.AddLogEntry("FOCUS: image is focused");
                    FocusingDone(fwhm);
                    return;
                }
                _shift *= -1;
                Logger.AddLogEntry("FOCUS: Old_FWHM < FWHM, but fwhm > 3, reverse");
            }
            if (Math.Abs(_oldFwhm - fwhm) < 0.2)
            {
                Logger.AddLogEntry("FOCUS: Math.Abs(Old_FWHM - FWHM)<0.2");
                Logger.AddLogEntry("FOCUS: image is focused");
                GoFocus((Frames[-1].Focus + Frames[-2].Focus)/2);
                FocusingDone(fwhm);
                return;
            }
            
            if(_focCycles <= MaxFocCycles)
            {
                _oldFwhm = fwhm;
                //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
                _shift = SmallShift(fwhm, _shift);
                _focCycles++;
                GetImForFocus(_shift);
                return;
            }
            //после много попыток дефокуса
            // Logger.AddLogEntry("FOCUS: Max cycles reached, start curve algorithm on previous frames, phase 3");
            Logger.AddLogEntry("FOCUS: Max cycles reached, return focus and exit");
            ReturnFocusAndExit();
        }
    
        private static void PhaseThree(string focusImPath)
        {
            var testShot = new GetDataFromFits(focusImPath);
            Logger.LogFrameInfo(testShot, CameraControl.cams.Last().Filter);
            if (testShot.Focused)
            {
                // FwhmBest = testShot.Fwhm;
                Logger.AddLogEntry("FOCUS: image is focused");
                FocusingDone(testShot.Fwhm);
                return;
            }
            
            Frames.Clear();
            // _zs.Clear();
            
            Logger.AddLogEntry("FOCUS: start curve algorithm on new frames, phase 4");
            _phase = 3;
            Logger.AddLogEntry(@"FOCUS: move to -1000");
            GetImForFocus(-1000);//переводим фокусер
                        //в крайнее положение,
                        //чтобы равномерно пройтись
                        //по диапазону 
        }
        #endregion

        #region Utils

        private static short BigShift(double fwhm, float shift)
        {   
            return (short) (Math.Sign(shift) * 55.4 * (6 - fwhm));
        }   
        
        private static short SmallShift(double fwhm, float shift)
        {
            return (short) (Math.Sign(shift) * 23.5 * (fwhm - 1.5));
        }

        private static void GetImForFocus(int z)
        {
            if (z != 0)
            {
                _sumShift += (short)z;  
                Logger.AddLogEntry($"FOCUS: Move focus {z}");
                Logger.AddLogEntry("FOCUS: SumShift=" + _sumShift);
                GoFocus(z);
            }
            if (!Head.isOnPause)
            {
                Head.StartExpAndCheckFuckup(_taskForFocus);
            }
        }

        private static void GoFocus(int z)
        {
            SerialFocus.FRun_To(z);
            var waitTime = 1000 + Math.Abs(z) / 100;
            Logger.AddLogEntry($"FOCUS: WaitTime={waitTime}");
            Thread.Sleep(waitTime);
        }

        private static void ReturnFocusAndExit()
        {
            GoFocus(_startFocusPos - SerialFocus.CurrentPosition);
            _sumShift = 0;
            FocusingDone(1);
        }

        private static void FocusingDone(double see)
        {
            Seeing = see;
            Logger.AddLogEntry($"FOCUS: Set seeing for autofocus = {see}");
            if (DeFocus != 0)
            {
                Logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                GoFocus(DeFocus);
            }
            IsFocusing = false;
            // CameraControl.PrepareToObs(Head.currentTask);
            if (!Head.isOnPause)
            {
                Logger.AddLogEntry("FOCUS: return to observation");
                Head.StartExpAndCheckFuckup();
            }
            else
            {
                Logger.AddLogEntry("FOCUS: Can't start exposure, on pause");
            }
        }
        
        #endregion
    }
}