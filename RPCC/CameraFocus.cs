using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCC
{
    internal class CameraFocus
    {
        private const int maxFocBadFrames = 5;
        private const int maxFocCycles = 15;

        /// <summary>
        ///     Функции фокусировки камеры.
        /// </summary>
        private readonly SerialFocus _serialFocus = new SerialFocus();

        private readonly Logger log;

        // переменные фокусировки
        public double focus_pos;
        public int framesAfterRunFocus;

        public bool initPhoto;
        public bool isFocused;
        public bool stopAll;

        // флаги работы камер
        public bool stopSurvey;

        public CameraFocus(Logger logger)
        {
            log = logger;   
        }

        public bool InitPhoto => initPhoto;
        public bool StopSurvey => stopSurvey;
        public bool StopAll => stopAll;

        public bool Init()
        {
            return _serialFocus.Init();
        }

        public bool AutoFocus()
        {
            //Обнуляем значение переменной ручной остановки фокуса.
            var stopFocus = false;
            var exit = false; // exit flag
            var counter = 0; // focus cycles counter
            // var focBadFrames = 0; // bad focus frames
            var frames = new List<AnalysisImageForFocus>();
            var z = _serialFocus.currentPosition;
            var zs = new List<int> {z};  //список позиций фокусировщика 
            const int n = 20;  //количество точек для построения кривой фокусировки
            const int shift = 150;
            double rKron = 0;  //радиус крона

            // Reconfigure(); //TODO проверка камер

            //check photometer status after reconfigure
            if (!initPhoto)
            {
                //TODO флаг проверки работы камеры?
                log.AddLogEntry("FOCUS: InitPhoto=false, exit.");
                return false;
            }

            Repoint4Focus(); // Перенаводимся в зенит для фокусировки

            do
            {
                log.AddLogEntry("FOCUS: Focus module is started");
                if (counter == 0)
                {
                    frames.Add(new AnalysisImageForFocus(getImForFocus(z), rKron)); //делаем снимок
                    var lastIndex = frames.Count - 1;
                    if (!frames[lastIndex].CheckStarsNum())
                    {
                        //мало звезд - игнорируем, скорее всего облако. Но может в диком дефокусе!
                        //TODO check clouds
                        frames.RemoveAt(lastIndex);
                        continue;
                    }

                    if (frames[lastIndex].CheckFocused())
                    {
                        focus_pos = z;
                        isFocused = true;
                        log.AddLogEntry("FOCUS: already focused");
                        break;
                        // изображение сфокусировано, выходим из цикла
                    }

                    rKron = frames[lastIndex].RKron;
                    _serialFocus
                        .FRun_To(-3000); //переводим фокусер в крайнее положение, чтобы равномерно пройтись по диапазону 
                }

                counter++;

                for (var i = 0; i < n; i++) // Сделать N-1 снимков с малой экспозицией для
                    // разных положений фокуса z, распределенных по
                    // всему диапазону значений, найти центроиды звезд
                {
                    z = _serialFocus.currentPosition;
                    zs.Add(z);
                    frames.Add(new AnalysisImageForFocus(getImForFocus(z), rKron));
                    _serialFocus.FRun_To(shift);
                }

                //Для каждого кадра находим среднее значение HFD
                var hfds = frames.Select(frame => frame.MeanHfd).ToList();

                double[] par = {-100, 5, 1500, -2};
                var epsx = 0.000001;
                var maxits = 0;
                int info;
                alglib.lsfitstate state;
                alglib.lsfitreport rep;

                var rows = zs.Count;
                var columns = 1;
                var zsArray = new double[rows, columns];
                for (var i = 0; i < rows; i++) zsArray[i, 0] = zs[i];
                var zaArr = new double[rows, columns];
                for (var i = 0; i < rows; i++) zaArr[i, 0] = new[] {zsArray[i, 0]}[0];

                //
                // Fitting without weights
                //
                alglib.lsfitcreatefgh(zaArr, hfds.ToArray(), par, out state);
                alglib.lsfitsetcond(state, epsx, maxits);
                alglib.lsfitfit(state, Hyperbola, Hyper_grad, Hyper_hess, null, null);
                alglib.lsfitresults(state, out info, out par, out rep);
                // Console.WriteLine("{0}", info); // EXPECTED: 2
                // Console.WriteLine("{0}", alglib.ap.format(par, 1));  //нужен второй параметр
                // Console.ReadLine();

                var newFocus = (int) par[2] - _serialFocus.currentPosition;
                var testShot = new AnalysisImageForFocus(getImForFocus(newFocus, true), rKron);

                if (testShot.CheckFocused()) stopFocus = true;
                /*
                 * todo минимально необходимый алгоритм. Потом при необходимости дополнить
                 */
            } while (!stopSurvey && !stopAll && !stopFocus && !exit);

            return true;
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
            // grad[0] = x[2] * (x[0] - x[3]) / (Math.Pow(x[1], 2) * h);
            grad[0] = -x[1] * Math.Pow(z[0] - x[2], 2) / (Math.Pow(x[0], 3) * h);
            grad[1] = h;
            grad[2] = -grad[0];
            grad[3] = 1;
        }

        private static void Hyper_hess(double[] x, double[] z, ref double func, double[] grad, double[,] hess, object o)
        {
            var h = H(x, z[0]);
            func = x[2] * h + x[4];
            // grad[0] = x[2] * (x[0] - x[3]) / (Math.Pow(x[1], 2) * h);
            grad[0] = -x[1] * Math.Pow(z[0] - x[2], 2) / (Math.Pow(x[0], 3) * h);
            grad[1] = h;
            grad[2] = -grad[0];
            grad[3] = 1;

            // hess[0, 0] =
            //     x[2] * (Math.Pow(x[1], -2) - Math.Pow(x[0] - x[3], 2) / (Math.Pow(x[1], 4) * Math.Pow(h, 2))) / h;
            // hess[0, 1] = -(2 * x[2] * (x[0] - x[3]) / (Math.Pow(x[1], 3) * h)) +
            //              x[2] * Math.Pow(x[0] - x[3], 3) / (Math.Pow(x[1], 5) * Math.Pow(h, 3));
            // hess[1, 0] = hess[0, 1];
            // hess[0, 2] = (x[0] - x[3]) / (Math.Pow(x[1], 2) * h);
            // hess[2, 0] = hess[0, 2];
            // hess[0, 3] = -(x[2] / (Math.Pow(x[1], 2) * h)) +
            //              x[2] * Math.Pow(x[0] - x[3], 2) / Math.Pow(x[1], 4) * Math.Pow(h, 3);
            // hess[3, 0] = hess[0, 3];
            // hess[0, 4] = 0;
            // hess[4, 0] = 0;
            hess[0, 0] = 3 * x[1] * Math.Pow(z[0] - x[2], 2) / Math.Pow(x[0], 4) * h -
                         x[1] * Math.Pow(z[0] - x[2], 4) / (Math.Pow(x[0], 6) * Math.Pow(h, 3));
            hess[0, 1] = -(Math.Pow(z[0] - x[2], 2) / (Math.Pow(x[0], 3) * h));
            hess[1, 0] = hess[0, 1];
            hess[0, 2] = 2 * x[1] * (z[0] - x[2]) / (Math.Pow(x[0], 3) * h) -
                         x[1] * Math.Pow(z[0] - x[2], 3) / (Math.Pow(x[0], 5) * Math.Pow(h, 3));
            hess[2, 0] = hess[0, 2];
            hess[0, 3] = 0;
            hess[3, 0] = 0;
            hess[1, 1] = 0;
            hess[1, 2] = -((z[0] - x[2]) / (Math.Pow(x[0], 2) * h));
            hess[2, 1] = hess[1, 2];
            hess[1, 3] = 0;
            hess[3, 1] = 0;
            hess[2, 2] = x[1] / (Math.Pow(z[1], 2) * h) -
                         x[1] * Math.Pow(z[0] - x[2], 2) / (Math.Pow(x[0], 4) * Math.Pow(h, 3));
            hess[2, 3] = 0;
            hess[3, 2] = 0;
            hess[3, 3] = 0;
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

        private GetDataFromFITS getImForFocus(int z, bool check = false)
        {
            _serialFocus.FRun_To(z);
            // TODO get im
            // return new double[2048, 2048];
            return new GetDataFromFITS("path", check);
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