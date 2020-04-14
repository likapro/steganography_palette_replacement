using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace replacing_the_palette
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Привет! Давай спрячем QR код в изображение методом замены палитры.");
            Console.WriteLine("Для этого нам нужно: \n " +
                "1) Изображение-контейнер формата bmp \n " +
                "2) QR код \n " +
                "3) Файл формата txt, чтобы записывать туда промежуточную информацию. Программа создаст его сама");

            Console.WriteLine("Введите путь до изображения-контейнера:");
            var pathImg = Console.ReadLine();
            while (!File.Exists(pathImg))
            {
                Console.WriteLine("Такого файла нет!");
                Console.WriteLine("Введите путь до изображения-контейнера:");
                pathImg = Console.ReadLine();
            }

            Console.WriteLine("Введите путь до QR кода:");
            var pathQR = Console.ReadLine();
            while (!File.Exists(pathQR))
            {
                Console.WriteLine("Такого файла нет!");
                Console.WriteLine("Введите путь до QR кода:");
                pathQR = Console.ReadLine();
            }

            Console.WriteLine("Введите путь куда сохранить текстовый файл:");
            var pathText = Console.ReadLine() + "\\The_method_palette.txt";
            Console.ReadKey();


            Bitmap img = new Bitmap(pathImg);
            Bitmap qr = new Bitmap(pathQR);

            using (StreamWriter sw = new StreamWriter(pathText, false, Encoding.Default))
            {
                sw.WriteLine("Размер контейнера: " + img.Size);
                sw.WriteLine("Размер ЦВЗ: " + qr.Size);
            }

            // Проверим размеры изображений
            var sizeImg = img.Height * img.Width;
            var sizeQR = qr.Height * qr.Width;

            if (sizeImg / 8 < sizeQR + 24)
            {
                Console.WriteLine("ERROR!!! \n Размер контейнера слишком мал");
                Console.ReadKey();
                using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
                {
                    sw.WriteLine("ERROR!!! \n Размер контейнера слишком мал: " + sizeImg / 8 + " < " + sizeQR + 24);
                }

                return;
            }


            int[,] blueArray = ReadPixel(img);
            // Запишем в текстовый файл значения синей составляющей пикселя верхнего левого угла изображения 15х15 px
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения синей составляющей пикселя верхнего левого угла контейнера 20х20 px");

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        sw.Write(blueArray[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }

            // Для получения палитры использовалась функция getColorTable().
            List<ColorInPalette> bluePalette = GetColorTable(blueArray);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Массив палитры:");
                foreach (var el in bluePalette)
                {
                    sw.WriteLine(el.Index + "\t" + el.Value);
                }
            }

            int[,] sortPalette = SortPalette(bluePalette);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Отсортированный массив палитры:");
                for (int i = 0; i < sortPalette.GetLength(0); i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sw.Write(sortPalette[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }
            int countColorInPalette = sortPalette.GetLength(0);

            int[,] paletteQR = ReadPixel(qr);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения яркости пикселя верхнего левого угла ЦВЗ 20х20 px");
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        sw.Write(paletteQR[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }

            var qrBinary = Dec2Bin(paletteQR);

            string end = "end";
            int[] endBinary = Str2bin(end);
            int[] qrPlusEnd = new int[qrBinary.Length + endBinary.Length];
            Array.Copy(qrBinary, 0, qrPlusEnd, 0, qrBinary.Length);
            Array.Copy(endBinary, 0, qrPlusEnd, qrBinary.Length, endBinary.Length);

            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения пикселей QR кода в бинарном виде с окончанием end:");
                for (int i = 0; i < qrPlusEnd.Length; i++)
                {
                    sw.Write(qrPlusEnd[i]);
                }
            }

            int[,] blueStegocont = Embed(sortPalette, blueArray, qrPlusEnd, countColorInPalette);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения синей составляющей пикселя верхнего левого угла контейнера со втроенной ЦВЗ 20х20 px");
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        sw.Write(blueStegocont[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }

            int indexBackslash = pathImg.LastIndexOf("\\", pathImg.Length - 1);
            var directoria = pathImg.Substring(0, indexBackslash);
            var pathImgWithQR = directoria + "\\Stegocontainer.bmp";
            Bitmap imgWithQR = new Bitmap(img);
            imgWithQR = WriteBlue(blueStegocont, imgWithQR);
            imgWithQR.Save(@pathImgWithQR);



            // ================================
            // ========== Извлечение ==========
            // ================================

            Console.WriteLine("А теперь давай достанем из контейнера наше спрятанное изображение.");
            Console.WriteLine("Для этого нам нужно: \n " +
                "1) Изображение-контейнер со встроенным ЦВЗ \n " +
                "2) Исходное изображение (контейнер без ЦВЗ)\n ");

            Console.ReadKey();

            // Теперь возьмём матрицы интенсивностей синей составляющей этих изображений:
            int[,] blueMatrixOld = ReadPixel(img);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения синей составляющей пикселя верхнего левого угла исходного изображения 20х20 px");

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        sw.Write(blueMatrixOld[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }

            int[,] blueMatrixNew = ReadPixel(imgWithQR);
            using (StreamWriter sw = new StreamWriter(pathText, true, Encoding.Default))
            {
                sw.WriteLine();
                sw.WriteLine("Значения синей составляющей пикселя верхнего левого угла контейнера 20х20 px");

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        sw.Write(blueMatrixNew[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }

            int[] extractQRBinary = ExtractBinary(blueMatrixOld, blueMatrixNew, endBinary);

            // Создадим новый Bitmap и запишем в его матрицы красного, зелёного и синего полученный двумерный массив:
            var extractQR = new Bitmap(paletteQR.GetLength(0), paletteQR.GetLength(0));

            var extractQRDec = Bin2Dec(extractQRBinary);
            var extractQRArray = ArrayQR(extractQRDec);

            WritePixel(extractQRArray, extractQR);

            var pathExtractQR = directoria + "\\Extract_QR.bmp";
            extractQR.Save(@pathExtractQR);

            // ==========================================================================================
            // ========= Оценка качества восприятия стеганоконтейнера после скрытия информации ==========
            // ==========================================================================================

            var snr = SNR(img, imgWithQR);
            var psnr = PSNR(img, imgWithQR);

            Console.WriteLine("Отношение сигнал/шум: " + snr);
            Console.WriteLine("Максимальное отношение сигнал/шум: " + psnr);
            Console.ReadKey();
        }

        private static int[,] ArrayQR(int[] arrayDecimal)
        {
            var x = Convert.ToInt32(Math.Sqrt(arrayDecimal.Length));
            var arrayQR = new int[x, x];
            var k = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < x; j++, k++)
                {
                    arrayQR[i, j] = arrayDecimal[k];
                }

            return arrayQR;
        }

        private static int[] Bin2Dec(int[] binaryArray)
        {
            int[] decimalArray = new int[binaryArray.Length / 8];

            for (int i = 0, k = 0; i < binaryArray.Length; k++)
            {
                string strBin = "";
                for (int j = 0; j < 8; j++, i++)
                    strBin += Convert.ToString(binaryArray[i]);

                decimalArray[k] = Convert.ToInt32(strBin, 2);
            }

            return decimalArray;
        }

        /// <summary>
        /// Функция преобразует считанное изображение в переменной типа Bitmap и возвращает матрицу, совпадающую по размерам с изображением и содержащую значения интенсивностей синего каждого его пикселя
        /// </summary>
        private static int[,] ReadPixel(Bitmap img)
        {
            int[,] blueArr = new int[img.Height, img.Width];
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    blueArr[j, i] = img.GetPixel(i, j).B;

            return blueArr;
        }

        //Для сохранения палитры использован класс ColorInPalette,
        //содержащий два члена: значение интенсивности и индекс в палитре. Для
        //сортировки палитры для класса использован компаратор.
        public class ColorInPalette : IComparable
        {
            public int Value;
            public int Index;

            public ColorInPalette(int v, int i)
            {
                Value = v;
                Index = i;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;
                if (obj is ColorInPalette otherColorInPalette)
                {
                    return Value.CompareTo(otherColorInPalette.Value);
                }
                else
                    throw new ArgumentException("Object is not Color Value");
            }
        }

        /// <summary>
        /// Функция преобразует матрицу интенсивностей синей составляющей изображения и возвращает список объектов класса ColorInPalette
        /// </summary>
        private static List<ColorInPalette> GetColorTable(int[,] clrArr)
        {
            var plt = new List<ColorInPalette>();
            var valTab = new List<int>();
            int p = 0;
            ColorInPalette cip;

            for (int i = 0; i < clrArr.GetLength(0) && p < 256; i++)
            {
                for (int j = 0; j < clrArr.GetLength(1) && p < 256; j++)
                {
                    if (!valTab.Contains(clrArr[i, j]))
                    {
                        valTab.Add(clrArr[i, j]);
                        p++;
                    }
                }
            }

            for (int i = 0; i < valTab.Count; i++)
            {
                cip = new ColorInPalette(valTab[i], i);
                plt.Add(cip);
            }

            return plt;
        }

        // Для сортировки палитры написана функция SortPalette(). Функция преобразует список List<ColorInPalette>, полученный на шаге 6, и использует встроенный метод структуры List, позволяющий
        // сортировать список по одному из полей.
        // Затем она переписывает значения интенсивностей и индексов в двумерный массив.
        private static int[,] SortPalette(List<ColorInPalette> palette)
        {
            int[,] sortPlt = new int[palette.Count, 2];
            palette.Sort();
            for (int i = 0; i < palette.Count; i++)
            {
                ColorInPalette c = palette[i];
                sortPlt[i, 0] = c.Index;
                sortPlt[i, 1] = c.Value;
            }

            return sortPlt;
        }

        private static int[] Dec2Bin(int[,] arr)
        {
            var result = new int[arr.Length * 8];
            int k = 0;

            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    var a = Convert.ToString(arr[i, j], 2);
                    while (a.Length < 8)
                        a = "0" + a;

                    var b = new int[8];
                    for (int n = 0; n < 8; n++)
                    {
                        string x = a[n].ToString();
                        b[n] = Int32.Parse(x);
                    }
                    Array.Copy(b, 0, result, k * 8, 8);
                    k++;
                }

            return result;
        }

        private static int[] Dec2Bin(int[] arr)
        {
            var result = new int[arr.Length * 8];
            int k = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                var a = Convert.ToString(arr[i], 2);
                while (a.Length < 8)
                    a = "0" + a;

                var b = new int[8];
                for (int n = 0; n < 8; n++)
                {
                    string x = a[n].ToString();
                    b[n] = Int32.Parse(x);
                }
                Array.Copy(b, 0, result, k * 8, 8);
                k++;
            }

            return result;
        }

        // Производим встраивание, которое осуществляется с помощью функции embed(). Данная функция преобразует отсортированную
        // палитру (матрицу интенсивности синей составляющей изображения), в которую будет встраиваться информация и бинарное представление ЦВЗ с
        // добавленным в конец признаком конца. В результате выполнения функции формируется новый массив интенсивностей синей составляющей изображения.
        private static int[,] Embed(int[,] sortPlt, int[,] blueArr, int[] watermark, int countColorInPalette)
        {
            int[,] newBlueArr = blueArr;
            int k = 0, n = 0, y = 0;
            for (int i = 0; i < blueArr.GetLength(0); i++)
            {
                for (int j = 0; j < blueArr.GetLength(1) && k < watermark.Length; j++)
                {
                    int pix = blueArr[i, j];
                    for (int l = 0; l < sortPlt.GetLength(0); l++)
                    {
                        if (sortPlt[l, 1] == pix) // ищем цвет пикселя в палитре
                        {
                            n = sortPlt[l, 0]; // индекс цвета в неотсоритированной таблице
                            y = l; // индекс цвета в отсортированной палитре
                            break;
                        }
                    }

                    if (n % 2 != watermark[k]) // если НЗБ индекса не совпадает с текущим битом ЦВЗ, ищем ближайший по палитре цвет, индекс которого (в первоначальной палитре) подходит
                    {
                        int aL = -1000; // если дойдём до границы палитры,
                        int aH = 1000; // не встретив подолдящее значение, то значение 1000 останется
                        for (int p = 1; p < sortPlt.GetLength(0); p++)
                        {
                            if (y - p >= 0) // проверка выхода за пределы матрицы
                            {
                                if (sortPlt[y - p, 0] % 2 == watermark[k]) // берём из отсортированной таблицы индекс ближайшего по интенсивности цвета при условии, что НЗБ индекса цвета в неотсортированной палитре равен текущему биту ЦВЗ
                                {
                                    aL = sortPlt[y - p, 1]; // берём значение этого цвета
                                    break;
                                }
                            }
                        }

                        for (int q = 1; q < sortPlt.GetLength(0); q++) // то же в другую
                        {
                            if (y + q < countColorInPalette) // сторону палитры
                            {
                                if (sortPlt[y + q, 0] % 2 == watermark[k])
                                {
                                    aH = sortPlt[y + q, 1];
                                    break;
                                }
                            }
                        }

                        if (pix - aL <= aH - pix) // пикселю нового контейнера присваивается то из них, которое на цветовой оси находится ближе к интенсивности рiх
                            newBlueArr[i, j] = aL;
                        else
                            newBlueArr[i, j] = aH;
                    }
                    k++;
                }
            }

            return newBlueArr;
        }

        // Для записи новой матрицы
        //«синего цвета» в изображение-контейнер используется функция writeBlue(), преобразующая Bitmap изображения и вставляющая в
        //него матрицу синего:
        private static Bitmap WriteBlue(int[,] blueMtrx, Bitmap img)
        {
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pix = img.GetPixel(j, i);
                    Color newClr = Color.FromArgb
                    (img.GetPixel(j, i).R,
                    img.GetPixel(j, i).G,
                    blueMtrx[i, j]);
                    img.SetPixel(j, i, newClr);
                }
            }

            return img;
        }

        private static int[] Str2bin(string str)
        {
            int[] binStr = new int[str.Length * 8];
            int[] decStr = new int[str.Length];
            int i = 0;
            foreach (char c in str)
            {
                decStr[i] = System.Convert.ToInt32(c);
                i++;
            }

            binStr = Dec2Bin(decStr);

            return binStr;
        }

        // ========================================
        // ========== Функции извлечения ==========
        // ========================================

        // Для реализации извлечения была использована функция extract(), преобразующая матрицы исходного контейнера(initArr) и контейнера со встроенной информацией(stegoArr), а также бинарное представление
        // признака окончания ЦВЗ(terminator) для его однозначного выделения из контейнера.В результате выполнения функции получаем бинарный массив, содержащий биты ЦВЗ.
        private static int[] ExtractBinary(int[,] initArr, int[,] stegoArr, int[] terminator)
        {
            int[] qrWithEndBinary = new int[stegoArr.Length];

            List<ColorInPalette> palette = GetColorTable(initArr);            // палитра исходного контейнера
            var p = 0;
            for (int i = 0; i < stegoArr.GetLength(0); i++)
            {
                for (int j = 0; j < stegoArr.GetLength(1); j++)
                {
                    int pix = stegoArr[i, j];
                    for (int k = 0; k < palette.Count; k++)
                    {
                        if (palette[k].Value == pix)
                        {
                            qrWithEndBinary[p] = palette[k].Index % 2;
                            p++;
                            break;
                        }
                    }
                }
            }

            int index = FindIndexOfArr(qrWithEndBinary, terminator);
            if (index == -1)
            {
                Console.WriteLine("Из контейнера не получается извлечь ЦВЗ");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            int[] watermark = new int[index];
            Array.Copy(qrWithEndBinary, 0, watermark, 0, index);

            return watermark;
        }

        public static int FindIndexOfArr(int[] array, int[] subArray)
        {
            int index = -1;
            for (int i = 0; i < array.Length - subArray.Length + 1; i++)
            {
                index = i;
                for (int j = 0; j < subArray.Length; j++)
                {
                    if (array[i + j] != subArray[j])
                    {
                        index = -1;
                        break;
                    }
                }
                if (index >= 0)
                    return index;
            }
            return -1;
        }

        private static void WritePixel(int[,] arrayPixel, Bitmap img)
        {
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color newClr = Color.FromArgb(arrayPixel[i, j], arrayPixel[i, j], arrayPixel[i, j]);
                    img.SetPixel(j, i, newClr);
                }

            }
        }


        // ==================================================================================================
        // ========= Функции Оценка качества восприятия стеганоконтейнера после скрытия информации ==========
        // ==================================================================================================

        private static double SNR(Bitmap emptyContainer, Bitmap filledСontainer)
        {
            double snr, numerator = 0, denominator = 0;
            for (int i = 0; i < emptyContainer.Width; i++)
                for (int j = 0; j < emptyContainer.Height; j++)
                {
                    numerator += emptyContainer.GetPixel(i, j).B;
                    denominator += emptyContainer.GetPixel(i, j).B - filledСontainer.GetPixel(i, j).B;
                }

            snr = 20 * Math.Log10(numerator / denominator);

            return snr;
        }

        private static double PSNR(Bitmap emptyContainer, Bitmap filledСontainer)
        {
            double psnr, numerator = 0, mse = 0;
            for (int i = 0; i < emptyContainer.Width; i++)
                for (int j = 0; j < emptyContainer.Height; j++)
                {
                    if (numerator < Math.Pow(emptyContainer.GetPixel(i, j).B, 2)) numerator = Math.Pow(emptyContainer.GetPixel(i, j).B, 2);
                    mse += Math.Pow(emptyContainer.GetPixel(i, j).B - filledСontainer.GetPixel(i, j).B, 2) / emptyContainer.Width / emptyContainer.Height;
                }

            psnr = 20 * Math.Log10(numerator / Math.Sqrt(mse));

            return psnr;
        }
    }
}

