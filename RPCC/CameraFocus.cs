using System;

namespace RPCC
{
    internal class CameraFocus
    {
        private const double _maxEll = 0.25;
        private const int _minStars = 10;
        private readonly SerialFocus _serialFocus = new SerialFocus();
        public double ell;

        // переменные фокусировки
        public double focus_pos;
        public int framesAfterRunFocus;

        // переменные кадра
        public double FWHM;
        public double FWHM_Best;

        // числа в конфиг
        private double FWHM_FOCUSED = 3.0;
        private double FWHM_GOOD = 3.2;
        public bool initPhoto;

        private readonly Logger logger;
        public double stars_num;
        public bool stopAll;

        // флаги работы камер
        public bool stopSurvey;

        /// <summary>
        ///     Функции фокусировки камеры.
        /// </summary>
        public CameraFocus(Logger logger)
        {
            this.logger = logger;
        }

        public bool InitPhoto => initPhoto;
        public bool StopSurvey => stopSurvey;
        public bool StopAll => stopAll;

        public bool Init()
        {
            return _serialFocus.Init();
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

        /*
        если фокусировка в зените - перенаводимся, ждем окончания процедуры!
        необходимость фокусировки каждой камеры задается отдельно переменными GoFocus_Х, они же флаги для выхода из цикла (если обе false)
        но снимаем всегда двумя камерами - контроль вытянутости и кол-ва звезд!

        функция обнуляет глобальную переменную Frames_after_RunFocus и обновляет поля в структуре Tube_FWHM {double FWHM; double Ell; int NStar; int FocPosition; double Best; int Bad_Frames;}
        ~90 шагов на 1 пикс FWHM
        PV ошибка измерения FWHM ~0.5пикселя

        ДОБАВИТЬ КО ВСЕМ РЕТУРНАМ УСТАНОВКУ параметров Tube_FWHM!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        это почти делает setFWHM
        */
        public bool AutoFocus(string Where, bool GoFocus)
        {
            logger.AddLogEntry("FOCUS: Focus module is started");
            //Обнуляем значение переменной ручной остановки фокуса.
            var stopFocus = false;
            Reconfigure(); //TODO проверка камер

            //check photometer status after reconfigure

            if (!initPhoto)
            {
                //TODO флаг проверки работы камеры?
                logger.AddLogEntry("FOCUS: InitPhoto=false, exit.");
                FWHM_Best = 0;
                return false;
            }


            if (Where == "Zenith") Repoint4Focus(); //Перенаводимся в зенит для фокусировки

            /*
                дуем на 500 шагов к середине диапазона фокусировки (+ для H и C, - для всех остальных)
                проверяем FWHM, если меньше 8, то рассчитываем сдвиг в ту же сторону еще на 70*(8-FWHM)
                проверяем, если FWHM не около 8, то что-то с фотометром. (контроль положения)
                теперь точно в дефокусе и точно знаем где фокус (в - для H и C, в + для всех остальных)
                контролируем кол-во плохих кадров, общее число циклов, сумму сдвигов
                как избежать повторного сдвига при перезапуске фокусировки?
                если что-то пошло не так при фокусировке => возвращаем фокус обратно!
            */
            var shift = -300; //set default defocus shift
            logger.AddLogEntry("FOCUS: Defocus is started");
            var focCycles = 0; //focus cycles counter
            var focBadFrames = 0; //bad focus frames
            var exit = false; //exit flag
            var zenithFlag = 0; //zenith flag
            var sumShift = 0; //accumulate shifts 
            const int maxFocBadFrames = 5;
            const int maxFocCycles = 15;
            do
            {
                //сдвигаем в самом начале и если последнее измерение было хорошим, пропускаем если измерение было плохим
                if (focBadFrames == 0)
                {
                    _serialFocus.FRun_To(shift);
                    sumShift += shift;
                    logger.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
                }

                //get frames and set new fwhm
                if (!RunFocusImaging())
                {
                    logger.AddLogEntry("FWHM information is not available, return focus and exit");
                    _serialFocus.FRun_To(-1 * sumShift);
                    return false;
                }

                //проверяем валидность измерений (кол-во звезд, вытянутость), счетчик плохих кадров + или 0.
                if (!CheckImageQuality())
                {
                    focBadFrames++;
                    //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит и рестарт процедуры, но без первого сдвига.
                    if (focBadFrames >= maxFocBadFrames)
                    {
                        if (zenithFlag > 0)
                        {
                            logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            exit = true;
                            continue;
                        }

                        logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        Repoint4Focus();
                        zenithFlag += 1;
                        focBadFrames = 0;
                        focCycles = 0;
                        if (GoFocus && shift != 0) shift /= Math.Abs(shift); //сохраняем знак сдвига
                        logger.AddLogEntry("FOCUS: Restart focus at Zenith");
                    }
                    else
                    {
                        logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }

                    continue; //еще раз к началу цикла
                }

                //проверяем FWHM, если меньше 8, то рассчитываем сдвиг в ту же сторону еще на 90*(8-FWHM)
                if (GoFocus && FWHM < 6.0 && shift != 0)
                    shift = (int) (shift / Math.Abs(shift) * 90 * (6.0 - FWHM));
                else
                    shift = 0;

                if (focCycles >= 5) exit = true;
                if (focBadFrames >= maxFocBadFrames) exit = true;
                if (sumShift > 1000) exit = true;
                if (shift == 0) exit = true;
            } while (!stopSurvey && !stopAll && !stopFocus && !exit);

            //вышли из цикла
            if (stopSurvey || stopAll || stopFocus)
            {
                logger.AddLogEntry("FOCUS: aborted, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            //много плохих кадров
            if (focBadFrames >= maxFocBadFrames)
            {
                logger.AddLogEntry("FOCUS: can't defocus, bad frames limit, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            //после трех попыток дефокуса
            if (FWHM < 6.0 && GoFocus)
            {
                logger.AddLogEntry("FOCUS: can't defocus after 3 iterations, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false; //что-то сломалось, выходим
            }

            //теперь точно в дефокусе и точно знаем где фокус
            //начинаем движение в сторону фокуса
            focCycles = 0;
            focBadFrames = 0;
            shift = 1;
            var oldFwhm = FWHM;
            zenithFlag = 0;
            do
            {
                logger.AddLogEntry("Focus cycle #" + focCycles);
                if (focBadFrames == 0)
                {
                    //сдвиг считаем как 45*(FWHM-1.5), тогда должны дойти до фокуса за 4-5 шагов.
                    if (GoFocus) shift = (int)(shift / Math.Abs(shift) * 45 * (FWHM - 1.5));
                    _serialFocus.FRun_To(shift);
                    sumShift += shift;
                    logger.AddLogEntry("FOCUS: SumShift=" + sumShift);
                    focCycles++;
                }

                if (!RunFocusImaging())
                {
                    logger.AddLogEntry("FWHM information is not available, return focus and exit");
                    _serialFocus.FRun_To(-1 * sumShift);
                    FWHM_Best = 0;
                    return false;
                }

                //проверяем валидность измерений (кол-во звезд, вытянутость в двух трубах), счетчик плохих кадров+ или 0.
                if (!CheckImageQuality())
                {
                    focBadFrames++;
                    //если плохих >=Max_Foc_Bad_Frames => репойнт в зенит.
                    if (focBadFrames >= maxFocBadFrames)
                    {
                        if (zenithFlag > 0)
                        {
                            logger.AddLogEntry("FOCUS: Bad frames limit at Zenith, exit");
                            continue;
                        }

                        logger.AddLogEntry("FOCUS: Bad frames limit, repoint to Zenith");
                        zenithFlag += 1;
                        Repoint4Focus();
                        focBadFrames = 0;
                        focCycles = 0;
                        logger.AddLogEntry("FOCUS: Restart focus at Zenith");
                    }
                    else
                    {
                        logger.AddLogEntry("FOCUS: Bad frames, zenith_flag another cycle");
                    }

                    continue; //еще раз к началу цикла
                }

                focBadFrames = 0;

                //выходим из цикла когда ((FWHM<2.2) или (ухудшается FWHM) или измение FWHM<0.2)
                logger.AddLogEntry("check\n");

                if (FWHM < 2.2 || !GoFocus || oldFwhm < FWHM ||
                    Math.Abs(oldFwhm - FWHM) < 0.2)
                {
                    logger.AddLogEntry("Focus ok\n");
                    GoFocus = false;
                }

                if (FWHM < 2.2) logger.AddLogEntry("FWHM < 2.2\n");
                if (GoFocus) logger.AddLogEntry("GoFocus=true\n");
                if (oldFwhm < FWHM) logger.AddLogEntry("Old_FWHM_W < West_FWHM.FWHM\n");
                if (Math.Abs(oldFwhm - FWHM) < 0.2) logger.AddLogEntry("Math.Abs(Old_FWHM_W - West_FWHM.FWHM)<0.2\n");

                oldFwhm = FWHM;
            } while (!stopSurvey && !stopAll && !stopFocus && focCycles < maxFocCycles &&
                     focBadFrames < maxFocBadFrames && GoFocus);

            if (focCycles < maxFocCycles) logger.AddLogEntry("Foc_Cycles<Max_Foc_Cycles\n");
            if (focBadFrames < maxFocBadFrames) logger.AddLogEntry("Foc_Bad_Frames<Max_Foc_Bad_Frames\n");

            //вышли из цикла
            if (stopSurvey || stopAll || stopFocus)
            {
                logger.AddLogEntry("FOCUS: aborted, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            //после много попыток дефокуса
            if (focCycles >= maxFocCycles)
            {
                logger.AddLogEntry("FOCUS: Max cycles reached, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            if (focBadFrames >= maxFocBadFrames)
            {
                logger.AddLogEntry("FOCUS: Bad frames limit, return focus and exit");
                _serialFocus.FRun_To(-1 * sumShift);
                FWHM_Best = 0;
                return false;
            }

            FWHM_Best = FWHM;
            framesAfterRunFocus = 0;
            return true;
        }

        private void Reconfigure()
        {
            /*
             * Эта штука должна проверять состояние камер и прочей железяки,
             * вызывается из соответствующего места, здесь это просто затычка
             */
            throw new NotImplementedException();
        }

        private bool RunFocusImaging()
        {
            // TODO get im
            throw new NotImplementedException();
        }

        private bool CheckImageQuality()
        {
            if (FWHM > _maxEll)
            {
                //поехали обе трубы, игнорируем
                logger.AddLogEntry("Images stretched.");
                return false;
            }

            if (stars_num < _minStars)
            {
                //мало звезд на обоих кадрах, игнорируем, скорее всего облако. Но может обе в диком дефокусе !
                logger.AddLogEntry("Few stars on both images.");
                return false;
            }
            logger.AddLogEntry("Focus images ok!.");
            return true;
        }
    }
}