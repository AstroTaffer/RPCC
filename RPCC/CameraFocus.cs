﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCC
{
    internal class CameraFocus
    {
        private const int MaxFocCycles = 5;
        private const int MaxFocBadFrames = 3;
        private const int MaxSumShifts = 1000;

        public bool StopFocus { get; set; } = false;

        /// <summary>
        ///     Функции фокусировки камеры.
        /// </summary>  
        private readonly SerialFocus _serialFocus = new SerialFocus();

        private readonly Logger _log;

        // переменные фокусировки
        public int focusPos;
        // public int framesAfterRunFocus;

        public bool initPhoto;
        public bool isFocused;
        public bool stopAll;

        private double FWHM_Best;
        // флаги работы камер

        public bool StopAll
        {
            get => stopAll;
            set => stopAll = value;
        }

        public bool StopSurvey { get; set; }

        public CameraFocus(Logger logger)
        {
            _log = logger;   
        }

        public bool InitPhoto => initPhoto;

        public bool Init()
        {
            return _serialFocus.Init();
        }
    
        public bool AutoFocus(bool isZenith = false, bool goFocus = true)
        {
            //Обнуляем значение переменной ручной остановки фокуса.

            var focCycles = 0; // focus cycles counter
            var focBadFrames = 0; // bad focus frames
            var frames = new List<AnalysisImageForFocus>();
            var startFocusPos = _serialFocus.currentPosition;
            const int n = 10;  //количество точек для построения кривой фокусировки
            var shift = 300; //шаг по фокусу
            var sumShift = 0;
            var zenithFlag = 0;
            var exit = false;

            // Reconfigure(); //TODO проверка камер

            //check photometer status after reconfigure
            if (!initPhoto)
            {
                //TODO флаг проверки работы камеры?
                _log.AddLogEntry("FOCUS: InitPhoto=false, exit");
                return false;
            }
            
            if (isZenith) Repoint4Focus(); //Перенаводимся в зенит для фокусировки

            do
            {
                //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
                if (focBadFrames == 0)
                {
                    frames.Add(new AnalysisImageForFocus(GetImForFocus(shift), _log));
                    sumShift += shift;
                    _log.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
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
                            _log.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            exit = true;
                            continue;
                        }

                        _log.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        Repoint4Focus();
                        zenithFlag += 1;
                        focBadFrames = 0;
                        focCycles = 0;
                        if (goFocus && shift != 0) shift /= Math.Abs(shift); //сохраняем знак сдвига
                        _log.AddLogEntry("FOCUS: Restart focus at Zenith");
                    }
                    else
                    {
                        _log.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }
                    
                    continue; //еще раз к началу цикла
                }

                //проверяем FWHM, если меньше 8, то рассчитываем сдвиг в ту же сторону еще на 90*(8-FWHM)
                if (goFocus && frames.Last().Fwhm < 6.0 && shift != 0)
                    shift = (int) (shift / Math.Abs(shift) * 90 * (6.0 - frames.Last().Fwhm));
                else
                    shift = 0;
                
                if (focCycles >= MaxFocCycles) exit = true;
                if (focBadFrames >= MaxFocBadFrames) exit = true;
                if (sumShift > MaxSumShifts) exit = true;
                if (shift == 0) exit = true;

            } while (!StopSurvey && !stopAll && !StopFocus && exit);

            //вышли из цикла
            if (StopSurvey || stopAll || StopFocus)
            {
                _log.AddLogEntry("FOCUS: aborted, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            //много плохих кадров
            if (focBadFrames >= MaxFocBadFrames)
            {
                _log.AddLogEntry("FOCUS: can't defocus, bad frames limit, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            //после трех попыток дефокуса
            if (frames.Last().Fwhm < 6.0 && goFocus)
            {
                _log.AddLogEntry("FOCUS: can't defocus after 3 iterations, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
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
                _log.AddLogEntry("Focus cycle #" + focCycles);
                var fwhm = frames.Last().Fwhm;
                if (focBadFrames == 0)
                {
                    //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
                    if (goFocus) shift = (int)(shift / Math.Abs(shift) * 45 * (fwhm - 1.5));
                    frames.Add(new AnalysisImageForFocus(GetImForFocus(shift), _log));
                    // _serialFocus.FRun_To(shift);
                    
                    sumShift += shift;
                    _log.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
                }

                if (!frames.Last().GetDataFromFits.Status)
                {
                    _log.AddLogEntry("FWHM information is not available, return focus and exit");
                    _serialFocus.FRun_To(-1 * sumShift);
                    FWHM_Best = 0;
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
                            _log.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            continue;
                        }

                        _log.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        zenithFlag += 1;
                        Repoint4Focus();
                        focBadFrames = 0;
                        focCycles = 0;
                    }
                    else
                    {
                        _log.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }

                    continue; //еще раз к началу цикла
                }

                focBadFrames = 0;

                //выходим из цикла когда ((FWHM<2.2) или (ухудшается FWHM) или измение FWHM<0.2)
                _log.AddLogEntry("FOCUS: check");

                if (fwhm < 2.2 || !goFocus || oldFwhm < fwhm || Math.Abs(oldFwhm - fwhm) < 0.2)
                {
                    _log.AddLogEntry("FOCUS: Focus ok");
                    goFocus = false;
                }

                if (fwhm < 2.2) _log.AddLogEntry("FOCUS: FWHM < 2.2");
                if (goFocus) _log.AddLogEntry("FOCUS: GoFocus=true");
                if (oldFwhm < fwhm) _log.AddLogEntry("FOCUS: Old_FWHM_W < West_FWHM.FWHM");
                if (Math.Abs(oldFwhm - fwhm) < 0.2) _log.AddLogEntry("FOCUS: Math.Abs(Old_FWHM_W - West_FWHM.FWHM)<0.2");
                oldFwhm = fwhm;
            } while (!StopSurvey && !stopAll && !StopFocus && focCycles < MaxFocCycles &&
                     focBadFrames < MaxFocBadFrames && goFocus);
            
            if (focCycles < MaxFocCycles) _log.AddLogEntry("FOCUS: Foc_Cycles<Max_Foc_Cycles");
            if (focBadFrames < MaxFocBadFrames) _log.AddLogEntry("FOCUS: Foc_Bad_Frames<Max_Foc_Bad_Frames");
            
            //вышли из цикла
            if (StopSurvey || stopAll || StopFocus)
            {
                _log.AddLogEntry("FOCUS: aborted, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }
            
            if (focBadFrames >= MaxFocBadFrames)
            {
                _log.AddLogEntry("FOCUS: Bad frames limit, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }


            if (focCycles < MaxFocCycles)
            {
                FWHM_Best = frames.Last().Fwhm;
                // framesAfterRunFocus = 0;
                return true;
            }
            
            //после много попыток дефокуса
            _log.AddLogEntry("FOCUS: Max cycles reached, start curve algorithm");

            var fwhms = frames.Select(frame => frame.Fwhm).ToList();
            var zs = frames.Select(frame => frame.GetDataFromFits.Focus).ToList();
            
            var newFocus = Curve_fitting(zs, fwhms);
            
            if (newFocus > 6000)
            {
                _serialFocus.FRun_To(-300);
                FWHM_Best = 0;
                return false;
            }
            _log.AddLogEntry(@"FOCUS: minimum position " + newFocus);
            
            var newShift = newFocus - _serialFocus.currentPosition;
            var testShot = new AnalysisImageForFocus(GetImForFocus(newShift), _log);
            
            if (testShot.CheckFocused())
            {
                FWHM_Best = testShot.Fwhm;
                _log.AddLogEntry("FOCUS: image is in focus");
                return true;
            }

            frames.Select(frame => frame.GetDataFromFits.DelOutputs());
            frames.Clear();
            zs.Clear();
                
            frames.Add(new AnalysisImageForFocus(GetImForFocus(-3000), _log)); //переводим фокусер
            //в крайнее положение,
            //чтобы равномерно пройтись
            //по диапазону 
            _log.AddLogEntry(@"FOCUS: move to -3000");
            const int criticalShift = 6000/n;
                
            for (var i = 0; i < n; i++) // Сделать N-1 снимков с малой экспозицией для
                // разных положений фокуса z, распределенных по
                // всему диапазону значений
            {
                frames.Add(new AnalysisImageForFocus(GetImForFocus(criticalShift), _log));
            }
            fwhms = frames.Select(frame => frame.Fwhm).ToList();
            zs = frames.Select(frame => frame.GetDataFromFits.Focus).ToList();
            
            frames.Select(frame => frame.GetDataFromFits.DelOutputs());
            newFocus = Curve_fitting(zs, fwhms);
            if (newFocus > 6000)
            {
                _serialFocus.FRun_To(startFocusPos - _serialFocus.currentPosition);
                FWHM_Best = 0;
                return false;
            }
                
            _log.AddLogEntry(@"FOCUS: minimum position " + newFocus);
            newShift = newFocus - _serialFocus.currentPosition;
            testShot = new AnalysisImageForFocus(GetImForFocus(newShift), _log);
                
            if (testShot.CheckFocused())
            {
                FWHM_Best = testShot.Fwhm;
                _log.AddLogEntry("FOCUS: image is in focus");
                return true;
            }

            _log.AddLogEntry("FOCUS: focus not found, return focus and exit");
            _serialFocus.FRun_To(startFocusPos - _serialFocus.currentPosition);
            FWHM_Best = 0;
            return false;
        }

        private int Curve_fitting(IReadOnlyList<int> zs, List<double> fwhms)
        {
            var rows = zs.Count;
            var columns = 1;    
            var zsArray = new double[rows, columns];
            for (var i = 0; i < rows; i++) zsArray[i, 0] = zs[i];
            var zaArr = new double[rows, columns];
            for (var i = 0; i < rows; i++) zaArr[i, 0] = new[] {zsArray[i, 0]}[0];
                    
            double[] par = {305.559,5.033,-203.950,-1.947};  //априорные параметры гиперболы
            var epsx = 1e-6;
            var maxits = 0;
            int info;
            alglib.lsfitstate state;
            alglib.lsfitreport rep;
                
            //
            // Fitting without weights
            //
            _log.AddLogEntry(@"FOCUS: start curve fitting");
            alglib.lsfitcreatefg(zaArr, fwhms.ToArray(), par, false, out state); //lsfitcreatefgh не работает
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitfit(state, Hyperbola, Hyper_grad, Hyper_hess, null, null);
            alglib.lsfitresults(state, out info, out par, out rep);
            if (info == 2) return (int) par.GetValue(2);
            _log.AddLogEntry("FOCUS: ERROR IN CURVE FITTING");
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
            _serialFocus.FRun_To(z);
            // TODO get im
            // return new double[2048, 2048];
            return new GetDataFromFits("path", _log, true);
        }

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