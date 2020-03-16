using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace palette_replacement
{
    class Program
    {
        static void Main(string[] args)
        {
            //Загружаем используемый контейнер в переменную типа Bitmap:
            Bitmap img = new Bitmap("C.bmp");

            // Шаг 2.Загружаем ЦВЗ в переменную типа Bitmap:
            Bitmap watermark = new Bitmap("qr.bmp");

            // Шаг 3.Для корректного извлечения ЦВЗ введем в рассмотрение специальную переменную, которая будет встраиваться в контейнер вместе с
            //ЦВЗ и указывать на место окончания последовательности встроенных бит
            //в контейнере:
            string end = "end";

            //Шаг 4.Используя функцию readBlue(), считываем в специальные переменные матрицы синего цвета из контейнера и ЦВЗ:
            int[,] blueArr = readBlue(img);
            int[,] wmArr = readBlue(watermark);
            //Результаты выполнения функций представлены на рис. 4.67 и 4.68.
            //Функция readBlue():
            private static int[,] readBlue(Bitmap.img) {
                        int[,] blueArr = new int[img.Width, img.Height];
                        for (int i = 0; i < img.Width; i++)
                            for (int j = 0; j < img.Height; j++)
                                blueArr[i, j] = img.GetPixel(i, j).B;
                        return blueArr;
            }

            //            Функция преобразует считанное изображение в переменной типа Bitmap
            //и возвращает матрицу, совпадающую по размерам с изображением и содержащую значения интенсивностей синего каждого его пикселя.
            
            //Шаг 5.Для сохранения палитры использован класс ColorInPalette,
            //содержащий два члена: значение интенсивности и индекс в палитре. Для
            //сортировки палитры для класса использован компаратор.
            public class ColorInPalette : IComparable
                    {
                        int val;
                        int indx;
                        public int Value
                        {
                            get
                            {
                                return val;
                            }
                            set
                            {
                            val = value;
                            }
                        }
                        public int Index
                        {
                            get
                            {
                                return indx;
                            }
                            set
                            {
                                indx = value;
                            }
                        }
                        public ColorInPalette(int v, int i)
                        {
                            this.val = v;
                            this.indx = i;
                        }
                        public int CompareTo(object obj)
                        {
                            if (obj == null) return 1;
                            ColorInPalette otherColorInPalette = obj as ColorInPalette;
                            if (otherColorInPalette != null)
                                return this.val.CompareTo(otherColorInPalette.val);
                            else
                                throw new ArgumentException("Object is not Color Value");
                        }
                    };
                    //Шаг 6. Для получения палитры использовалась функция getColorTable().
           // Вызов функции getColorTable() :
            List<ColorInPalette> bluePlt = getColorTable(blueArr);
                    Код функции getColorTable() :
            private static List<ColorInPalette> getColorTable(int[,] clrArr)
                    {
                        List<ColorInPalette> plt = new List<ColorInPalette>();
                        List<int> valTab = new List<int>();
                        int p = 0;
                        ColorInPalette cip;
                        for (int i = 0; i < clrArr.GetLength(0) && p < 256; i++)
                            for (int j = 0; j < clrArr.GetLength(1) && p < 256; j++)
                            {
                                if (valTab.Contains(clrArr[i, j]) == false)
                                {
                                    valTab.Add(clrArr[i, j]);
                                    p++;
                                }
                            }
                        for (int i = 0; i < valTab.Count; i++)
                        {
                            cip = new ColorInPalette(valTab[i], i);
                            plt.Add(cip);
                        }
                        return plt;
                    }
            //        Функция преобразует матрицу интенсивностей синей составляющей изображения и возвращает список объектов класса
            //ColorInPalette.
            
            //Шаг 7. Для сортировки палитры написана функция pltSort(). Функция преобразует список List <ColorInPalette>, полученный на шаге 6, и использует встроенный метод структуры List, позволяющий
            //сортировать список по одному из полей.
            //Затем она переписывает значения интенсивностей и индексов в двумерный массив.
            private static int[,] pltSort(List<ColorInPalette> plt)
                    {
                        int[,] sortPlt = new int[plt.Count, 2];
                        plt.Sort();
                        for (int i = 0; i < plt.Count; i++)
                        {
                            ColorInPalette c = plt[i];
                            sortPlt[i, 0] = c.Index;
                            sortPlt[i, 1] = c.Value;
                        }
                        return sortPlt;
                    }
                    //Вызов функции pltSort() :
            int[,] sortPlt = pltSort(bluePlt);

            //Шаг 8. Для формирования массива битов ЦВЗ использовалась функция bwImg2bin(), преобразующая матрицу интенсивностей для ЦВЗ в одномерный бинарный массив.
            private static int[] bwImg2bin(int[,] imgMatrix)
                    {
                        int[] binImg = new int[imgMatrix.Length * 8];
                        for (int i = 0, k = 0; i < imgMatrix.GetLength(0); i++)
                            for (int j = 0; j < imgMatrix.GetLength(1); j++)
                            {
                            int[] b = dec2bin(imgMatrix[i, j]);
                                Array.Copy(b, 0, binImg, k * 8, 8);
                                k++;
                            }
                        return binImg;
                    }
                    //Вызов функции bwImg2bin() :
            int[] binWm = bwImg2bin(wmArr);
                    
            //Шаг 9. Добавим в конец полученной бинарной последовательности
                  //  признак окончания (также в двоичном виде):
            int[] terminator = str2bin(end); // признак конца ЦВЗ в двоичном виде
                    int[] binWmTrtr = new int[binWm.Length + terminator.Length];
                    // двоичное представление ЦВЗ + признак конца
                    Array.Copy(binWm, 0, binWmTrtr, 0, binWm.Length);
            Array.Copy(terminator, 0, binWmTrtr, binWm.Length, terminator.Length);
            //Для перевода символьной строки(признака конца ЦВЗ) в бинарный
            //массив использована функция str2bin():
            private static int[] str2bin(string str)
                    {
                        int[] binStr = new int[str.Length * 8];
                        int[] decStr = new int[str.Length];
            int i = 0, k = 0;
                        foreach (char c in str)
                        {
                            decStr[i] = System.Convert.ToInt32(c);
                            i++;
                        }
                        for (int j = 0; j < decStr.Length; j++)
                        {
                            int[] b = dec2bin(decStr[j]);
                            Array.Copy(b, 0, binStr, k * 8, 8);
                            k++;
                        }
                        return binStr;
                    }
                    //Для перевода матрицы десятичных чисел в двоичный вид предложена
                    //функция bwImg2bin() :
            private static int[] bwImg2bin(int[,] imgMatrix)
                    {
                        int[] binImg = new int[imgMatrix.Length * 8];
                        for (int i = 0, k = 0; i < imgMatrix.GetLength(0); i++)
                        {
                            for (int j = 0; j < imgMatrix.GetLength(1); j++)
                            {
                                int[] b = dec2bin(imgMatrix[i, j]);
                                Array.Copy(b, 0, binImg, k * 8, 8);
                                k++;
                            }
                        }
                        return binImg;
                    }
            //        Шаг 10. Проведём проверку, не является ли встраиваемое сообщение
            //слишком большим для данного контейнера:
            if (blueArr.Length<binWmTrtr.Length)
            {
            Console.WriteLine("Размер ЦВЗ слишком велик для встраивания
            в это изображение>");
            Console.ReadKey();
            }
            //    Шаг 11. Если размер встраиваемой информации не превышает допустимого значения, производим встраивание, которое осуществляется с помощью функции embed(). Данная функция преобразует отсортированную
            //палитру(матрицу интенсивности синей составляющей изображения), в которую будет встраиваться информация и бинарное представление ЦВЗ с
            //добавленным в конец признаком конца.В результате выполнения функции формируется новый массив интенсивностей синей составляющей изображения.
            private static int[,] embed(int[,] sortPlt, int[,] blueArr,
            int[] watermark)
            {
            int[,] newBlueArr = blueArr;
                int k = 0, n = 0, y = 0;
            for (int i = 0; i<blueArr.GetLength(0); i++)
            for(int j = 0; j<blueArr.GetLength(1) && k<watermark.Length; j++)
            {
            int pix = blueArr[i, j];
            for(int l = 0; l<sortPlt.GetLength(0); l++)
            {
            if (sortPlt[l, 1] == pix) // ищем цвет пикселя в палитре
            {
            n = sortPlt[l, 0]; // индекс цвета в неотсоритированной таблице
            y = l; // индекс цвета в отсортированной палитре
            break;
            }
            }
            if(n%2!=watermark[k]) // если НЗБ индекса не совпадает с текущим битом
            // ЦВЗ, ищем ближайший по палитре цвет, индекс
            // которого (в первоначальной палитре) подходит
            {
            int aL = -1000; // если дойдём до границы палитры,
            int aH = 1000; // не встретив подолдящее значение, то значение
            // 1000 останется
            for (int p = 1; p<sortPlt.GetLength(0); p++)
            if (y - p >= 0) // проверка выхода за пределы матрицы
            {
            if (sortPlt[y - p, 0] % 2 == watermark[k]) // берём из отсор-
            // тированной таблицы индекс ближайшего по интенсив-
            // ности цвета при условии, что НЗБ индекса цвета в
            // неотсортированной палитре равен текущему биту ЦВЗ
            {
            aL = sortPlt[y - p, 1]; // берём значение этого цвета
            break;
            }
            }
            for (int q = 1; q<sortPlt.GetLength(0); q++) // то же в другую
            if (y + q< 256) // сторону палитры
            {
            if (sortPlt[y + q, 0] % 2 == watermark[k])
            {
            aH = sortPlt[y + q, 1];
            break;
            }
            }
            if(pix - aL <= aH - pix) // пикселю нового контейнера присваивается
            // то из них, которое на цветовой оси
            // находится ближе к интенсивности рiх
            newBlueArr[i, j] = aL;
            else
            newBlueArr[i, j] = aH;
            }
            k++;
            }
            //Вызов функции embed() :
            int[,] stegoCont = embed(sortPlt,
            blueArr, binWmTrtr);
            
            //Шаг 12. Для записи новой матрицы
            //«синего цвета» в изображение-контейнер используется функция writeBlue(), преобразующая Bitmap изображения и вставляющая в
            //него матрицу синего:
            private static void writeBlue(int[,]
            blueMtrx, Bitmap img)
            {
                for (int i = 0; i < img.Width; i++)
                    for (int j = 0; j < img.Height; j++)
                    {
                        Color pix = img.GetPixel(i, j);
                        Color newClr = Color.FromArgb
                        (img.GetPixel(i, j).R,
                        img.GetPixel(i, j).G,
                        blueMtrx[i, j]);
                        img.SetPixel(i, j, newClr);
                    }
            }
            
            //Вызов функции writeBlue() :
            Bitmap newImg = new Bitmap(img);
            newImg = writeBlue(stegoCont, newImg);
            //Шаг 13. Создадим новый файл C2.bmp, в который запишем новый
            //Bitmap:
            newImg.Save(«C2.bmp», ImageFormat.Bmp);


        // ================================
        // ========== Извлечение ==========
        // ================================


       // Шаг 1.Для извлечения ЦВЗ в методе замены палитры требуется наличие исходного изображения, поскольку необходимо учитывать индексы в
//палитре исходной матрицы интенсивности. С этой целью вначале загружаем исходное изображение, а затем изображение со встроенным ЦВЗ:
Bitmap imgOld = new Bitmap("C.bmp");
        Bitmap imgNew = new Bitmap("C2.bmp");
   
//Теперь возьмём матрицы интенсивностей синей составляющей этих изображений:
int[,] blueMatrixOld = readBlue(imgOld);
        int[,] blueMatrixNew = readBlue(imgNew);
        //Шаг 2.Для реализации извлечения была использована функция extract(), преобразующая матрицы исходного контейнера(initArr) и контейнера со встроенной информацией(stegoArr), а также бинарное представление
//признака окончания ЦВЗ(terminator) для его однозначного выделения из
//контейнера.В результате выполнения функции получаем бинарный массив, содержащий биты ЦВЗ.
private static int[] extract(int[,] initArr, int[,] stegoArr,
int[] terminator)
        {
            int[] wtrmrkTrmtr = new int[stegoArr.Length];
            List<ColorInPalette> bluePlt = getColorTable(initArr);
            // палитра исходного контейнера
            int[,] sortPltInit = pltSort(bluePlt);
            // отсортированная палитра исходного контейнера
            List<ColorInPalette> bluePlt2 = getColorTable(stegoArr);
            // палитра стегоконтейнера
            int[,] sortPltStego = pltSort(bluePlt2);
            // отсортированная палитра стегоконтейнера
            int p = 0;
            for (int i = 0; i < stegoArr.GetLength(0); i++)
            {
                for (int j = 0; j < stegoArr.GetLength(1); j++)
                {
                    int pix = stegoArr[i, j];
                    for (int k = 0; k < sortPltStego.GetLength(0); k++)
                    {
                        if (sortPltStego[k, 1] == pix)
                        {
                            wtrmrkTrmtr[p] = sortPltInit[k, 0] % 2;
                            p++;
                        }
                    }
                }
            }
            int indx = findIndexOfArr(wtrmrkTrmtr, terminator);
            int[] watermark = new int[indx];
            Array.Copy(wtrmrkTrmtr, 0, watermark, 0, indx);
            return watermark;
        }
        //Вызов функции extract():
int[] watermark2 = extract(blueMatrixOld, blueMatrixNew,
terminator);


//Шаг 3.Создадим новый Bitmap и запишем в его матрицы красного,
//зелёного и синего полученный двумерный массив:
        Bitmap wm = new Bitmap(wmMatr.GetLength(0), wmMatr.GetLength(0));
        writeBlue(wmMatr, wm);
        writeRed(wmMatr, wm);
        writeGreen(wmMatr, wm);
        //Функция writeRed():
private static void writeRed(int[,] redMtrx, Bitmap img)
    {
        for (int i = 0; i < img.Width; i++)
            for (int j = 0; j < img.Height; j++)
            {
                Color pix = img.GetPixel(i, j);
                Color newClr = Color.FromArgb(redMtrx[i, j], img.GetPixel(i, j).G,
                img.GetPixel(i, j).B);
                img.SetPixel(i, j, newClr);
            }
    }
//Функция writeGreen():
private static void writeGreen(int[,] greenMtrx, Bitmap img)
    {
        for (int i = 0; i < img.Width; i++)
            for (int j = 0; j < img.Height; j++)
            {
                Color pix = img.GetPixel(i, j);
                Color newClr = Color.FromArgb(img.GetPixel(i, j).R, greenMtrx[i, j],
                img.GetPixel(i, j).B);
                img.SetPixel(i, j, newClr);
            }
    }
    //Шаг 4. Создадим новый файл qr2.bmp, в который запишем полученный Bitmap.
    wm.Save("qr2.bmp", ImageFormat.Bmp);

        // ==========================================================================================
        // ========= Оценка качества восприятия стеганоконтейнера после скрытия информации ==========
        // ==========================================================================================
        
        // Для оценки качества встраивания информации в контейнер
        //и подсчета искажений используется функция mse(), реализующая алгоритм
        //среднеквадратической ошибки MSE(Mean Square Error) :
        private static double mse(int[,] image1, int[,] image2)
    {
        double result = 0, addition;
        for (int i = 0; i < image1.GetLength(0); i++)
            for (int j = 0; j < image1.GetLength(1); j++)
            {
                addition = Math.Pow(image1[i, j] - image2[i, j], 2);
                result += addition;
            }
        result = result / image1.Length;
        return result;
    }
//    В изображение встраивались ЦВЗ различных размеров: 84×84, 100×100
//и 170×170. 
            }
    }
}
