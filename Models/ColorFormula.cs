using System;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace kostka_rgb.Models
{
    // A delegate type for methods that calculate a color component value based on x and y coordinates.
    public delegate int ColorCallback(int x, int y);

    public static class ColorFormula
    {
        #region Cube
        public static BitmapSource GetNextGradient()
        {
            (ColorCallback r, ColorCallback g, ColorCallback b) = _values[_index++ % _values.Length];
            return GenerateGradient(r, g, b);
        }

        public static BitmapSource GetGradientById(int id)
        {

            (ColorCallback r, ColorCallback g, ColorCallback b) = _values[id % _values.Length];
            // Generates the gradient image using the selected color pattern.
            return GenerateGradient(r, g, b);
        }

        private static BitmapSource GenerateGradient(ColorCallback red, ColorCallback green, ColorCallback blue)
        {
            // Generates a Bitmap using the color callbacks.
            var bitmap = Gradient(red, green, blue);
            // Converts the Bitmap to a BitmapSource for use with WPF.
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }

        // This method generates a Bitmap object representing the gradient.
        private static Bitmap Gradient(ColorCallback red, ColorCallback green, ColorCallback blue)
        {
            // Creates a new 256x256 bitmap with 24 bits per pixel RGB format.
            var bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            // Locks the bitmap's bits to system memory.
            var data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            // Unsafe code block allows for pointer operations.
            unsafe
            {
                byte* bytes = (byte*)data.Scan0.ToPointer();// Gets the pointer to the first pixel data.

                // Loops through the bitmap's pixels to set their color values.
                for (int y = 0; y < data.Height; y++)
                {
                    int o = y * data.Stride; // Calculates the byte offset for the current row.

                    for (int x = 0; x < data.Width; x++)
                    {
                        // Sets the color of the pixel using the color callbacks.
                        bytes[o + x * 3 + 0] = (byte)red(x, y);
                        bytes[o + x * 3 + 1] = (byte)green(x, y);
                        bytes[o + x * 3 + 2] = (byte)blue(x, y);
                    }
                }
            }

            //Just for visual Debug
            //SaveGradient(bmp, (_index - 1) + ".png");

            bmp.UnlockBits(data);

            return bmp;
        }
        
        // Static index to keep track of which color pattern to use next
        private static int _index = 0;
        // An array of tuples containing color callbacks for different gradient patterns.
        private static readonly (ColorCallback r, ColorCallback g, ColorCallback b)[] _values = new (ColorCallback r, ColorCallback g, ColorCallback b)[]
        {
                // Definitions of color gradients based on the x, y coordinates.
                ((x, y) => x, (x, y) => y, (x, y) => 255),
                ((x, y) => 0, (x, y) => x, (x, y) => y),
                ((x, y) => y, (x, y) => 0, (x, y) => x),
                ((x, y) => y, (x, y) => x, (x, y) => 0),
                ((x, y) => x, (x, y) => 255, (x, y) => y),
                ((x, y) => 255, (x, y) => y, (x, y) => x),
        };

        #endregion

        public static BitmapSource GenerateHSVGradient(bool floor)
        {
            Bitmap bitmap = CreateHSVBaseTexture(floor);
            // Converts the Bitmap to a BitmapSource for use with WPF.
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }

        public static Bitmap CreateHSVBaseTexture(bool floor)
        {
            int width = 512;
            int height = 512;
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb
            );

            // Coordinates for the center of the base
            float centerX = width / 2f;
            float centerY = height / 2f;
            // Use the smallest dimension to ensure the circle fits within the texture
            float radius = Math.Min(centerX, centerY);

            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate distance from center to get saturation
                        float distance = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                        float saturation = distance / radius;
                        saturation = saturation > 1 ? 1 : saturation; // Clamp saturation to 1

                        // Calculate angle for hue
                        float angle = (float)Math.Atan2(y - centerY, x - centerX) * (180 / (float)Math.PI);
                        float hue = angle < 0 ? angle + 360 : angle; // Normalize angle to be between 0 and 360

                        float value = 1;
                        Color color;
                        if (floor)
                        {
                            
                            color = saturation < 1 ? ConvertHsvToRgb(hue, saturation, value) : Color.White;
                        }
                        else
                        {

                            
                            color = saturation < 1 ? ConvertHsvToRgb(hue, saturation, saturation) : Color.White;
                        }
                        
                        // Set pixel color
                        row[x * 3 + 2] = color.R; // Red
                        row[x * 3 + 1] = color.G; // Green
                        row[x * 3 + 0] = color.B; // Blue
                    }
                }
            }

            bmp.UnlockBits(data);
            if(floor)SaveGradient(bmp, "HSV_Base_Gradient.png"); // Save the gradient for debugging
            else SaveGradient(bmp, "HSV_Curve_Gradient.png"); // Save the gradient for debugging
            return bmp;
        }

        public static Bitmap CreateHSVCurveTexture()
        {
            int width = 512;
            int height = 512;

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb
            );

            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate HSV values
                        float hue = (float)x / (width - 1) * 360;
                        float value = 1 - (float)y / (height - 1);
                        float saturation = 1;

                        // Convert HSV to RGB
                        Color color = ConvertHsvToRgb(hue, saturation, value);

                        // Set pixel color
                        row[x * 3 + 2] = color.R; // Red
                        row[x * 3 + 1] = color.G; // Green
                        row[x * 3 + 0] = color.B; // Blue
                    }
                }
            }

            bmp.UnlockBits(data);
            SaveGradient(bmp, "HSV_Curve_Gradient.png"); // Save the gradient for debugging
            return bmp;
        }

        public static Color ConvertHsvToRgb(double h, double s, double v)
        {
            double rd, gd, bd;

            double hh = h / 60.0;
            int sector = (int)Math.Floor(hh);
            double frac = hh - sector;
            double p = v * (1 - s);
            double q = v * (1 - s * frac);
            double t = v * (1 - s * (1 - frac));

            switch (sector)
            {
                case 0:
                    rd = v;
                    gd = t;
                    bd = p;
                    break;
                case 1:
                    rd = q;
                    gd = v;
                    bd = p;
                    break;
                case 2:
                    rd = p;
                    gd = v;
                    bd = t;
                    break;
                case 3:
                    rd = p;
                    gd = q;
                    bd = v;
                    break;
                case 4:
                    rd = t;
                    gd = p;
                    bd = v;
                    break;
                default:
                    rd = v;
                    gd = p;
                    bd = q;
                    break;
            }

            return Color.FromArgb((int)(rd * 255.0), (int)(gd * 255.0), (int)(bd * 255.0));
        }
        
        
        // Saves the bitmap as a PNG file to current directory (in .bin)
        private static void SaveGradient(Bitmap bmp, string name)
        {
            // Get the current directory.
            string currentDirectory = Directory.GetCurrentDirectory();

            // Specify the folder name you want to check and create.
            string folderName = "Gradients";

            // Combine the current directory path and folder name.
            string folderPath = Path.Combine(currentDirectory, folderName);

            // Check if the directory exists.
            if (!Directory.Exists(folderPath))
            {
                // Create the directory if it does not exist.
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, name);
            bmp.Save(filePath, ImageFormat.Png);
        }
    }
}
