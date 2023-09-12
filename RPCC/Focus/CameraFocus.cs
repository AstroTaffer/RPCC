using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int MaxFocCycles = 3;
        private const int MaxFocBadFrames = 3;
        private const int MaxSumShifts = 1000;
        public static double FwhmBest { get; set; }
        public static bool StopFocus { get; set; } = false;

        // public bool IsFocused { get; set; }
        public static bool StopAll { get; set; }
        public static bool StopSurvey { get; set; }
        public static bool IsAutoFocus { get; set; }
        public static bool IsZenith { get; set; } = false;
        public static int DeFocus { get; set; } = 0;
        private static short _focCycles = 0; // focus cycles counter
        private static short _focBadFrames = 0; // bad focus frames
        private static List<GetDataFromFits> frames = new List<GetDataFromFits>();
        private static int _startFocusPos;
        private const int N = 10; //количество точек для построения кривой фокусировки
        private static short _shift = -300; //шаг по фокусу
        private static short _sumShift = 0;
        private static short _zenithFlag = 0;
        private static bool _exit = false;
        private static ObservationTask task;
        private static short Phase;
        private static double OldFwhm;
        private static List<double> fwhms;
        private static List<int> zs;
        private const int criticalShift = 2000 / N;
        private static short FrameCounter = 0;

        public static void StartAutoFocus(ObservationTask observationTask)
        {
            task = observationTask;
            _focCycles = 0; //
            _focBadFrames = 0; //
            frames.Clear();
            _startFocusPos = SerialFocus.CurrentPosition;
            _sumShift = 0;
            _zenithFlag = 0;
            _exit = false;
            Phase = 0;
            
            //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим

            _sumShift += _shift;
            Logger.AddLogEntry("FOCUS: SumShift=" + _sumShift);
            _focCycles++;
            GetImForFocus(_shift);
        }
        
            
        private static void CamFocusCallBack(string focusImPath)
        {
            if (task.Status != 1) 
            {
                Logger.AddLogEntry("FOCUS: task ended, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                
                return;
            }

            if (!IsAutoFocus)
            {
                Logger.AddLogEntry("FOCUS: Autofocus disable, exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                
                return;
            }
            switch (Phase)
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
                case 3:
                    PhaseFour(focusImPath);
                    break;
                case 4:
                    PhaseFour(focusImPath);
                    break;
                case 5:
                    PhaseFour(focusImPath);
                    break;
            }
        }

        private static void PhaseOne(string focusImPath)
        {
            if (IsZenith)
            {
                Repoint4Focus(); //Перенаводимся в зенит для фокусировки
            }
            
            //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
            if (_focBadFrames == 0)
            {
                _sumShift += _shift;
                Logger.AddLogEntry("FOCUS: SumShift=" + _sumShift);
                _focCycles++;
            }
            
            frames.Add(new GetDataFromFits(focusImPath));
            
            if (!frames.Last().Status)
            {
                Logger.AddLogEntry("FOCUS: FWHM information is not available, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                //TODO doExp();
            }
            
            //проверяем валидность измерений (кол-во звезд, вытянутость), счетчик плохих кадров + или 0.
            if (!frames.Last().CheckImageQuality())
            {
                _focBadFrames++;
                //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит и рестарт процедуры, но без первого сдвига.
                if (_focBadFrames >= MaxFocBadFrames)
                {
                    if (IsZenith)
                    {
                        Logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                        _exit = true;
                        //TODO doExp();
                        return;
                    }
                    Logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                    Repoint4Focus();
                    IsZenith = true;
                    _focBadFrames = 0;
                    _focCycles = 0;
                }
                else
                {
                    Logger.AddLogEntry("FOCUS: Bad frames, another cycle");
                }

                GetImForFocus(_shift); //еще раз к началу цикла
                return;
            }
            
            if (_focCycles >= MaxFocCycles || _focBadFrames >= MaxFocBadFrames || _sumShift > MaxSumShifts 
                || frames.Last().Fwhm < 6.0 || _shift != 0) Phase = 1;
            
            //проверяем FWHM, если меньше 6, то рассчитываем сдвиг в ту же сторону еще на 90*(6-FWHM)
            if (Phase == 0)
            {
                _shift = (short) (_shift / Math.Abs(_shift) * 90 * (6.0 - frames.Last().Fwhm));
                GetImForFocus(_shift);
                return;
            }

            //много плохих кадров
            if (_focBadFrames >= MaxFocBadFrames)
            {
                Logger.AddLogEntry("FOCUS: can't defocus, bad frames limit, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                return;
            }

            //после трех попыток дефокуса
            if (frames.Last().Fwhm < 6.0)
            {
                Logger.AddLogEntry($"FOCUS: can't defocus after {MaxFocCycles} iterations, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                return; //что-то сломалось, выходим
            }

            //теперь точно в дефокусе и точно знаем где фокус
            //начинаем движение в сторону фокуса
            Logger.AddLogEntry("FOCUS: start phase 2");
            _focCycles = 0;
            _focBadFrames = 0;
            _shift = 1;
            OldFwhm = frames.Last().Fwhm;
            //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
            _shift = (short) (_shift / Math.Abs(_shift) * 45 * (OldFwhm - 1.5));
            GetImForFocus(_shift);
        }

        private static void PhaseTwo(string focusImPath)
        {
            Logger.AddLogEntry("Focus cycle #" + _focCycles);
            frames.Add(new GetDataFromFits(focusImPath));
            var fwhm = frames.Last().Fwhm;
            
            if (!frames.Last().Status)
            {
                Logger.AddLogEntry("FWHM information is not available, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                return;
            }

            //проверяем валидность измерений (кол-во звезд, вытянутость в двух трубах), счетчик плохих кадров+ или 0.
            if (!frames.Last().CheckImageQuality())
            {
                _focBadFrames++;
                //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит.
                if (_focBadFrames >= MaxFocBadFrames)
                {
                    if (IsZenith)
                    {
                        Logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                        return;
                    }

                    Logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                    IsZenith = true;
                    Repoint4Focus();
                    _focBadFrames = 0;
                    _focCycles = 0;
                }
                else
                {
                    Logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                }
                GetImForFocus(_shift);
                return; //еще раз к началу цикла
            }

            _focBadFrames = 0;
            
            //выходим из цикла когда ((FWHM<2.2) или (ухудшается FWHM) или измение FWHM<0.2)
            Logger.AddLogEntry("FOCUS: check");

            if (fwhm < 2.2) Logger.AddLogEntry("FOCUS: FWHM < 2.2");
            if (OldFwhm < fwhm) Logger.AddLogEntry("FOCUS: Old_FWHM_W < West_FWHM.FWHM");
            if (Math.Abs(OldFwhm - fwhm) < 0.2)
                Logger.AddLogEntry("FOCUS: Math.Abs(Old_FWHM_W - West_FWHM.FWHM)<0.2");
            
            if (fwhm < 2.2 || OldFwhm < fwhm || Math.Abs(OldFwhm - fwhm) < 0.2)
            {
                Logger.AddLogEntry("FOCUS: image is focused");
                if (DeFocus != 0)
                {
                    Logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                    SerialFocus.FRun_To(DeFocus);
                }
                FwhmBest = frames.Last().Fwhm;
                //TODO doExp();
                return;
            }
            if(_focCycles < MaxFocCycles && _focBadFrames < MaxFocBadFrames)
            {
                OldFwhm = fwhm;
                //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
                _shift = (short) (_shift / Math.Abs(_shift) * 45 * (fwhm - 1.5));
                _sumShift += _shift;
                GetImForFocus(_shift);
                Logger.AddLogEntry("FOCUS: SumShift=" + _sumShift);
                _focCycles++;
                return;
            }
            
            if (_focCycles < MaxFocCycles) Logger.AddLogEntry("FOCUS: Foc_Cycles<Max_Foc_Cycles");
            if (_focBadFrames < MaxFocBadFrames) Logger.AddLogEntry("FOCUS: Foc_Bad_Frames<Max_Foc_Bad_Frames");

            //вышли из цикла
            if (task.Status != 1 || !IsAutoFocus)
            {
                Logger.AddLogEntry("FOCUS: aborted, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                return;
            }

            if (_focBadFrames >= MaxFocBadFrames)
            {
                Logger.AddLogEntry("FOCUS: Bad frames limit, return focus and exit");
                SerialFocus.FRun_To(-1 * _sumShift);
                FwhmBest = 0;
                return;
            }
            
            //после много попыток дефокуса
            Logger.AddLogEntry("FOCUS: Max cycles reached, start curve algorithm on previous frames, phase 3");
            Phase = 2;

            fwhms = frames.Select(frame => frame.Fwhm).ToList();
            zs = frames.Select(frame => frame.Focus).ToList();

            var newFocus = Curve_fitting(zs, fwhms, zs[fwhms.IndexOf(fwhms.Min())]);

            if (Math.Abs(newFocus) < 3000)
            {
                Logger.AddLogEntry(@"FOCUS: minimum position " + newFocus);

                var newShift = newFocus - SerialFocus.CurrentPosition;
                GetImForFocus(newShift);
            } else
            {
                Logger.AddLogEntry("Math.Abs(newFocus) > 3000");
            }
        }
    
        private static void PhaseThree(string focusImPath)
        {
            var testShot = new GetDataFromFits(focusImPath);
            if (testShot.CheckFocused())
            {
                FwhmBest = testShot.Fwhm;
                Logger.AddLogEntry("FOCUS: image is focused");
                if (DeFocus == 0) return;
                Logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                SerialFocus.FRun_To(DeFocus);
                return;
            }
            
            frames.Select(frame => frame.DelOutputs());
            frames.Clear();
            zs.Clear();
            
            Logger.AddLogEntry("FOCUS: start curve algorithm on new frames, phase 4");
            Phase = 3;
            Logger.AddLogEntry(@"FOCUS: move to -1000");
            GetImForFocus(-1000);//переводим фокусер
                        //в крайнее положение,
                        //чтобы равномерно пройтись
                        //по диапазону 
        }
    
        private static void PhaseFour(string focusImPath)
        {
            frames.Add(new GetDataFromFits(focusImPath));
            // Сделать N-1 снимков с малой экспозицией для
            // разных положений фокуса z, распределенных по
            // всему диапазону значений
            if (FrameCounter < N)
            {
                GetImForFocus(criticalShift);
                return;
            }
            fwhms = frames.Select(frame => frame.Fwhm).ToList();
            zs = frames.Select(frame => frame.Focus).ToList();

            frames.Select(frame => frame.DelOutputs()); // удаляем все лишние файлы

            var newFocus = Curve_fitting(zs, fwhms,
                zs[fwhms.IndexOf(fwhms.Min())]); // строим кривую фокусировки и фитируем ее гиперболой
            if (Math.Abs(newFocus) > 3000)
            {
                Logger.AddLogEntry("Math.Abs(newFocus) > 3000");
                SerialFocus.FRun_To(_startFocusPos - SerialFocus.CurrentPosition);
                FwhmBest = 0;
                return;
            }
            Logger.AddLogEntry(@"FOCUS: minimum position " + newFocus);
            var newShift = newFocus - SerialFocus.CurrentPosition;
            Phase = 4;
            GetImForFocus(newShift);
        }

        private static void PhaseFive(string focusImPath)
        {
            var testShot = new GetDataFromFits(focusImPath);
            if (testShot.CheckFocused())
            {
                FwhmBest = testShot.Fwhm;
                Logger.AddLogEntry("FOCUS: image is focused");
                if (DeFocus == 0) return;
                Logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                SerialFocus.FRun_To(DeFocus);
                return;
            }

            Logger.AddLogEntry("FOCUS: focus not found, return focus and exit");
            SerialFocus.FRun_To(_startFocusPos - SerialFocus.CurrentPosition);
            FwhmBest = 0;
        }

        private static void Repoint4Focus()
        {
            SiTechExeSocket.GoToAltAz(180, 90);
            Logger.AddLogEntry("Repointing to zenith for focusing");
        }

        private static void GetImForFocus(int z)
        {
            SerialFocus.FRun_To(z);
            // TODO get im
            // CamCtrl.MakeFocusIm();
            // return new GetDataFromFits("path", true);
        }


        #region Curve

        private static int Curve_fitting(IReadOnlyList<int> zs, List<double> fwhms, double minC = -203.950)
        {
            var rows = zs.Count;
            var columns = 1;
            var zsArray = new double[rows, columns];
            for (var i = 0; i < rows; i++) zsArray[i, 0] = zs[i];
            var zaArr = new double[rows, columns];
            for (var i = 0; i < rows; i++) zaArr[i, 0] = new[] {zsArray[i, 0]}[0];

            double[] par = {305.559, 5.033, minC, -1.947}; //априорные параметры гиперболы
            var epsx = 1e-6;
            var maxits = 0;
            int info;
            alglib.lsfitstate state;
            alglib.lsfitreport rep;

            //
            // Fitting without weights
            //
            Logger.AddLogEntry(@"FOCUS: start curve fitting");
            alglib.lsfitcreatefg(zaArr, fwhms.ToArray(), par, false, out state); //lsfitcreatefgh не работает
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitfit(state, Hyperbola, Hyper_grad, Hyper_hess, null, null);
            alglib.lsfitresults(state, out info, out par, out rep);
            if (info == 2) return (int) par.GetValue(2);
            Logger.AddLogEntry("FOCUS: ERROR IN CURVE FITTING");
            return 15000;
        }

        private static void Hyperbola(double[] x, double[] z, ref double func, object o)
        {
            func = x[1] * H(x, z[0]) + x[3];
        }

        private static double H(double[] x, double z)
        {
            return Math.Sqrt(1 + Math.Pow(z - x[2], 2) / Math.Pow(x[0], 2));
        }

        private static void Hyper_grad(double[] x, double[] z, ref double func, double[] grad, object o)
        {
            var h = H(x, z[0]);
            func = x[1] * h + x[3];

            grad[0] = -x[1] * (z[0] - x[2]) / (Math.Pow(x[0], 3.0 / 2.0) * h);
            grad[1] = h;
            grad[2] = x[1] * (z[0] - x[2]) / (Math.Pow(x[0], 2) * h);
            grad[3] = 1;
        }

        private static void Hyper_hess(double[] x, double[] z, ref double func, double[] grad, double[,] hess, object o)
        {
            var h = H(x, z[0]);
            func = x[1] * h + x[3];

            grad[0] = -x[1] * Math.Pow(z[0] - x[2], 2) / (Math.Pow(x[0], 3) * h);
            grad[1] = h;
            grad[2] = -x[1] * (z[0] - x[2]) / (Math.Pow(x[0], 2) * h);
            grad[3] = 1;

            var commonFactor = x[1] / (Math.Pow(x[0], 3) * Math.Pow(h, 3));
            var c1 = commonFactor * Math.Pow(x[2] - z[0], 3);
            var c2 = 2 * commonFactor * (x[2] - z[0]);

            var element = c2 + c1;

            var a = Math.Pow(x[0], 3) * Math.Pow(h, 3);
            var b = x[1] * Math.Pow(h, 3);
            var c = Math.Pow(x[0], 2) * h;

            hess[0, 0] = -(b * Math.Pow(x[2] - z[0], 4) / (a * Math.Pow(c, 3))) +
                         3 * b * Math.Pow(x[2] - z[0], 2) / (Math.Pow(x[0], 4) * h);
            hess[0, 1] = -(Math.Pow(x[2] - z[0], 2) / (Math.Pow(x[0], 3) * h));
            hess[0, 2] = element;
            hess[0, 3] = 0;

            hess[1, 0] = hess[0, 1];
            hess[1, 1] = 0;
            hess[1, 2] = -((x[2] - z[0]) / c);
            hess[1, 3] = 0;

            hess[2, 0] = hess[0, 2];
            hess[2, 1] = -((x[2] - z[0]) / c);
            hess[2, 2] = -(b * Math.Pow(x[2] - z[0], 2) / (Math.Pow(x[0], 4) * Math.Pow(h, 3))) +
                         x[1] / (Math.Pow(x[0], 2) * h);
            hess[2, 3] = 0;

            hess[3, 0] = hess[3, 1] = hess[3, 2] = hess[3, 3] = 0;
        }

        #endregion
    }
}