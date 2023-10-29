using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;


namespace kostka_rgb.Models
{
    public delegate int ColorCallback(int x, int y);

    public class ColorFormula
    {
        public BitmapSource GradientImage { get; }

        public ColorFormula()
        {
            (ColorCallback r, ColorCallback g, ColorCallback b) = _values[_index++ % _values.Length];
            GradientImage = GenerateGradient(r, g, b);
        }

        private static BitmapSource GenerateGradient(ColorCallback red, ColorCallback green, ColorCallback blue)
        {
            var bitmap = Gradient(red, green, blue);
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }

        private static Bitmap Gradient(ColorCallback red, ColorCallback green, ColorCallback blue)
        {
            var bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            var data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            unsafe
            {
                byte* bytes = (byte*)data.Scan0.ToPointer();

                for (int y = 0; y < data.Height; y++)
                {
                    int o = y * data.Stride;

                    for (int x = 0; x < data.Width; x++)
                    {
                        bytes[o + x * 3 + 0] = (byte)red(x, y);
                        bytes[o + x * 3 + 1] = (byte)green(x, y);
                        bytes[o + x * 3 + 2] = (byte)blue(x, y);
                    }
                }
            }

            bmp.UnlockBits(data);
            return bmp;
        }
        private static int _index = 0;
        private static readonly (ColorCallback r, ColorCallback g, ColorCallback b)[] _values = new (ColorCallback r, ColorCallback g, ColorCallback b)[]
        {
                ((x, y) => x, (x, y) => y, (x, y) => 255),
                ((x, y) => 0, (x, y) => x, (x, y) => y),
                ((x, y) => y, (x, y) => 0, (x, y) => x),
                ((x, y) => y, (x, y) => x, (x, y) => 0),
                ((x, y) => x, (x, y) => 255, (x, y) => y),
                ((x, y) => 255, (x, y) => y, (x, y) => x),
        };
    }
}
