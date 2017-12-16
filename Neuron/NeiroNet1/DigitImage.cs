using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroNet
{
    public class DigitImage
    {
        public int width; // 28
        public int height; // 28
        public int[,] pixels; // 0 (белый) – 255 (черный) - не учитываем
        public int label; // '0' - '9'
        public DigitImage(int width, int height, int[,] pixels, int label)
        {
            this.width = width;
            this.height = height;
            this.pixels = new int[height, width];
            for (int i = 0; i < height; ++i)
                for (int j = 0; j < width; ++j)
                    this.pixels[i, j] = pixels[i, j];
            this.label = label;
        }
    }

}
