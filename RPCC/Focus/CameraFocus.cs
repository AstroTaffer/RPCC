using System;
using System.Collections.Generic;
using System.Linq;
using RPCC.Cams;
using RPCC.Utils;

namespace RPCC.Focus
{
    public class CameraFocus
    {
        
        /// <summary>
        ///     Функции фокусировки камеры.
        /// </summary>
        /// 
        private const int MaxFocCycles = 5;
        private const int MaxFocBadFrames = 3;
        private const int MaxSumShifts = 1000;
        private readonly Logger _logger;
        
        public SerialFocus SerialFocus { get; }
        public double FwhmBest { get; set; }
        public bool StopFocus { get; set; } = false;
        // public bool IsFocused { get; set; }
        public bool StopAll { get; set; }
        public bool StopSurvey { get; set; }
        public bool isAutoFocus { get; set; }
        public bool IsZenith { get; set; } = false;
        public int DeFocus { get; set; } = 0;
        // public bool InitPhoto { get; set; }
        // private CameraControl _cameraControl;
        
        internal CameraFocus(Logger logger, Settings settings)
        {
            _logger = logger;
            // _cameraControl = cameraControl;
            SerialFocus = new SerialFocus(logger, settings);
        }

        public bool Init()
        {
            return SerialFocus.Init();
        }

        public bool AutoFocus( bool goFocus = true)
        {
            if (!isAutoFocus) return false;
            
            //Обнуляем значение переменной ручной остановки фокуса.

            var focCycles = 0; // focus cycles counter
            var focBadFrames = 0; // bad focus frames
            var frames = new List<AnalysisImageForFocus>();
            var startFocusPos = SerialFocus.CurrentPosition;
            const int n = 10; //количество точек для построения кривой фокусировки
            var shift = -300; //шаг по фокусу
            var sumShift = 0;
            var zenithFlag = 0;
            var exit = false;

            // Reconfigure(); //TODO проверка камер

            //check photometer status after reconfigure
            // if (!InitPhoto)
            // {
            //     //TODO флаг проверки работы камеры?
            //     _logger.AddLogEntry("FOCUS: InitPhoto=false, exit");
            //     return false;
            // }

            if (IsZenith) {
                Repoint4Focus(); //Перенаводимся в зенит для фокусировки\
                zenithFlag = 1;
            }

            do
            {
                //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
                if (focBadFrames == 0)
                {
                    sumShift += shift;
                    _logger.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
                }
                
                frames.Add(new AnalysisImageForFocus(GetImForFocus(shift), _logger));
                if (!frames.Last().GetDataFromFits.Status)
                {
                    _logger.AddLogEntry("FOCUS: FWHM information is not available, return focus and exit");
                    SerialFocus.FRun_To(-1*sumShift);
                    FwhmBest = 0;
                    return false;
                }
                
                //проверяем валидность измерений (кол-во звезд, вытянутость), счетчик плохих кадров + или 0.
                if (!frames.Last().CheckImageQuality())
                {
                    focBadFrames++;
                    //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит и рестарт процедуры, но без первого сдвига.
                    if (focBadFrames >= MaxFocBadFrames)
                    {
                        if (zenithFlag > 0)
                        {
                            _logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            exit = true;
                            continue;
                        }

                        _logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        Repoint4Focus();
                        zenithFlag += 1;
                        focBadFrames = 0;
                        focCycles = 0;
                        // if (goFocus && shift != 0) shift /= Math.Abs(shift); //сохраняем знак сдвига
                        _logger.AddLogEntry("FOCUS: Restart focus at Zenith");
                    }
                    else
                    {
                        _logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }

                    continue; //еще раз к началу цикла
                }

                //проверяем FWHM, если меньше 6, то рассчитываем сдвиг в ту же сторону еще на 90*(6-FWHM)
                if (goFocus && frames.Last().Fwhm < 6.0 && shift != 0)
                    shift = (int) (shift / Math.Abs(shift) * 90 * (6.0 - frames.Last().Fwhm));
                else
                    shift = 0;
                
                if (focCycles >= MaxFocCycles) exit = true;
                if (focBadFrames >= MaxFocBadFrames) exit = true;
                if (sumShift > MaxSumShifts) exit = true;
                if (shift == 0) exit = true;
            } while (!StopSurvey && !StopAll && !StopFocus && exit);

            //вышли из цикла
            if (StopSurvey || StopAll || StopFocus)
            {
                _logger.AddLogEntry("FOCUS: aborted, return focus and exit");
                SerialFocus.FRun_To(-1 * sumShift);
                FwhmBest = 0;
                return false;
            }

            //много плохих кадров
            if (focBadFrames >= MaxFocBadFrames)
            {
                _logger.AddLogEntry("FOCUS: can't defocus, bad frames limit, return focus and exit");
                SerialFocus.FRun_To(-1 * sumShift);
                FwhmBest = 0;
                return false;
            }

            //после трех попыток дефокуса
            if (frames.Last().Fwhm < 6.0 && goFocus)
            {
                _logger.AddLogEntry("FOCUS: can't defocus after 3 iterations, return focus and exit");
                SerialFocus.FRun_To(-1 * sumShift);
                FwhmBest = 0;
                return false; //что-то сломалось, выходим
            }

            //теперь точно в дефокусе и точно знаем где фокус
            //начинаем движение в сторону фокуса
            focCycles = 0;
            focBadFrames = 0;
            shift = 1;
            var oldFwhm = frames.Last().Fwhm;
            // zenithFlag = 0;

            do
            {
                _logger.AddLogEntry("Focus cycle #" + focCycles);
                var fwhm = frames.Last().Fwhm;
                if (focBadFrames == 0)
                {
                    //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
                    if (goFocus) shift = (int) (shift / Math.Abs(shift) * 45 * (fwhm - 1.5));
                    frames.Add(new AnalysisImageForFocus(GetImForFocus(shift), _logger));

                    sumShift += shift;
                    _logger.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
                }

                if (!frames.Last().GetDataFromFits.Status)
                {
                    _logger.AddLogEntry("FWHM information is not available, return focus and exit");
                    SerialFocus.FRun_To(-1 * sumShift);
                    FwhmBest = 0;
                    return false;
                }

                //проверяем валидность измерений (кол-во звезд, вытянутость в двух трубах), счетчик плохих кадров+ или 0.
                if (!frames.Last().CheckImageQuality())
                {
                    focBadFrames++;
                    //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит.
                    if (focBadFrames >= MaxFocCycles)
                    {
                        if (zenithFlag > 0)
                        {
                            _logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            continue;
                        }

                        _logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        zenithFlag += 1;
                        Repoint4Focus();
                        focBadFrames = 0;
                        focCycles = 0;
                    }
                    else
                    {
                        _logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }

                    continue; //еще раз к началу цикла
                }

                focBadFrames = 0;

                //выходим из цикла когда ((FWHM<2.2) или (ухудшается FWHM) или измение FWHM<0.2)
                _logger.AddLogEntry("FOCUS: check");

                if (fwhm < 2.2 || !goFocus || oldFwhm < fwhm || Math.Abs(oldFwhm - fwhm) < 0.2)
                {
                    _logger.AddLogEntry("FOCUS: image is in focus");
                    if (DeFocus != 0)
                    {
                        _logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                        SerialFocus.FRun_To(DeFocus);
                    }

                    goFocus = false;
                }

                if (fwhm < 2.2) _logger.AddLogEntry("FOCUS: FWHM < 2.2");
                if (goFocus) _logger.AddLogEntry("FOCUS: GoFocus=true");
                if (oldFwhm < fwhm) _logger.AddLogEntry("FOCUS: Old_FWHM_W < West_FWHM.FWHM");
                if (Math.Abs(oldFwhm - fwhm) < 0.2)
                    _logger.AddLogEntry("FOCUS: Math.Abs(Old_FWHM_W - West_FWHM.FWHM)<0.2");
                oldFwhm = fwhm;
            } while (!StopSurvey && !StopAll && !StopFocus && focCycles < MaxFocCycles &&
                     focBadFrames < MaxFocBadFrames && goFocus);

            if (focCycles < MaxFocCycles) _logger.AddLogEntry("FOCUS: Foc_Cycles<Max_Foc_Cycles");
            if (focBadFrames < MaxFocBadFrames) _logger.AddLogEntry("FOCUS: Foc_Bad_Frames<Max_Foc_Bad_Frames");

            //вышли из цикла
            if (StopSurvey || StopAll || StopFocus)
            {
                _logger.AddLogEntry("FOCUS: aborted, return focus and exit");
                SerialFocus.FRun_To(-1 * sumShift);
                FwhmBest = 0;
                return false;
            }

            if (focBadFrames >= MaxFocBadFrames)
            {
                _logger.AddLogEntry("FOCUS: Bad frames limit, return focus and exit");
                SerialFocus.FRun_To(-1 * sumShift);
                FwhmBest = 0;
                return false;
            }


            if (focCycles < MaxFocCycles)
            {
                FwhmBest = frames.Last().Fwhm;
                // framesAfterRunFocus = 0;
                return true;
            }

            //после много попыток дефокуса
            _logger.AddLogEntry("FOCUS: Max cycles reached, start curve algorithm");

            var fwhms = frames.Select(frame => frame.Fwhm).ToList();
            var zs = frames.Select(frame => frame.GetDataFromFits.Focus).ToList();

            var newFocus = Curve_fitting(zs, fwhms, zs[fwhms.IndexOf(fwhms.Min())]);

            if (newFocus > 6000)
            {
                SerialFocus.FRun_To(-300);
                FwhmBest = 0;
                return false;
            }

            _logger.AddLogEntry(@"FOCUS: minimum position " + newFocus);

            var newShift = newFocus - SerialFocus.CurrentPosition;
            var testShot = new AnalysisImageForFocus(GetImForFocus(newShift), _logger);

            if (testShot.CheckFocused())
            {
                FwhmBest = testShot.Fwhm;
                _logger.AddLogEntry("FOCUS: image is in focus");
                if (DeFocus == 0) return true;
                _logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                SerialFocus.FRun_To(DeFocus);
                return true;
            }

            frames.Select(frame => frame.GetDataFromFits.DelOutputs());
            frames.Clear();
            zs.Clear();

            frames.Add(new AnalysisImageForFocus(GetImForFocus(-3000), _logger)); //переводим фокусер
            //в крайнее положение,
            //чтобы равномерно пройтись
            //по диапазону 
            _logger.AddLogEntry(@"FOCUS: move to -3000");
            const int criticalShift = 6000 / n;

            for (var i = 0; i < n; i++) // Сделать N-1 снимков с малой экспозицией для
                // разных положений фокуса z, распределенных по
                // всему диапазону значений
                frames.Add(new AnalysisImageForFocus(GetImForFocus(criticalShift), _logger));
            fwhms = frames.Select(frame => frame.Fwhm).ToList();
            zs = frames.Select(frame => frame.GetDataFromFits.Focus).ToList();

            frames.Select(frame => frame.GetDataFromFits.DelOutputs()); // удаляем все лишние файлы

            newFocus = Curve_fitting(zs, fwhms,
                zs[fwhms.IndexOf(fwhms.Min())]); // строим кривую фокусировки и фитируем ее гиперболой
            if (newFocus > 6000)
            {
                SerialFocus.FRun_To(startFocusPos - SerialFocus.CurrentPosition);
                FwhmBest = 0;
                return false;
            }

            _logger.AddLogEntry(@"FOCUS: minimum position " + newFocus);
            newShift = newFocus - SerialFocus.CurrentPosition;
            testShot = new AnalysisImageForFocus(GetImForFocus(newShift), _logger);

            if (testShot.CheckFocused())
            {
                FwhmBest = testShot.Fwhm;
                _logger.AddLogEntry("FOCUS: image is in focus");
                if (DeFocus == 0) return true;
                _logger.AddLogEntry("FOCUS: defocus to " + DeFocus);
                SerialFocus.FRun_To(DeFocus);
                return true;
            }

            _logger.AddLogEntry("FOCUS: focus not found, return focus and exit");
            SerialFocus.FRun_To(startFocusPos - SerialFocus.CurrentPosition);
            FwhmBest = 0;
            return false;
        }


        private bool Repoint4Focus()
        {
            return false;
            // TODO перенавестись на зенит 
            // bool success;
            // target.pos=FocusPos;
            // // { std::ostringstream CCL; CCL <<"FOCUS:: Repointing to focus position"; ErrlogSingl::instance().write( CCL.str() );}
            // // target.object = "FOCUS";
            // success = repoint(); //survey.cpp Device::repoint
            // return success;
        }

        private GetDataFromFits GetImForFocus(int z)
        {
            SerialFocus.FRun_To(z);
            // _cameraControl.StartExposure();
            // TODO get im
            // return new double[2048, 2048];
            return new GetDataFromFits("path", _logger, true);
        }

        #region Curve

        private int Curve_fitting(IReadOnlyList<int> zs, List<double> fwhms, double minC = -203.950)
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
            _logger.AddLogEntry(@"FOCUS: start curve fitting");
            alglib.lsfitcreatefg(zaArr, fwhms.ToArray(), par, false, out state); //lsfitcreatefgh не работает
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitfit(state, Hyperbola, Hyper_grad, Hyper_hess, null, null);
            alglib.lsfitresults(state, out info, out par, out rep);
            if (info == 2) return (int) par.GetValue(2);
            _logger.AddLogEntry("FOCUS: ERROR IN CURVE FITTING");
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

        // private void Reconfigure()
        // {
        //     /*
        //      * Эта штука должна проверять состояние камер и прочей железяки,
        //      * вызывается из соответствующего места, здесь это просто затычка
        //      */
        //     throw new NotImplementedException();
        // }
    }
}