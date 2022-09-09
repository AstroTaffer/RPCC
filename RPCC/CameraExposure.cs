using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCC
{
    internal class CameraExposure
    {
        /// <summary>
        ///     Функции съемки камеры.
        ///     Настройка параметров съемки.
        /// </summary>
    }
}

/*
Создать класс класс FliCameraDevice со следующим содержимым (поля класса будут приватными)
    Характеристики камеры(домен, дескриптор, серийный номер ...)
 
При подключении камер и смене параметров нужно скармливать всем камерам текущий конфиг
Чтобы не возникало проблем с кормежкой конфига пустоте, нужен флаг isCamerasConnected
или последовательная кормежка при переборе элементов массива подключенных камер
*/