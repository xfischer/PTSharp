using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public interface Texture
    {
        Color Sample(double u, double v);
        Vector NormalSample(double u, double v);
        Vector BumpSample(double u, double v);
        Texture Pow(double a);
        Texture MulScalar(double a);
    }

    public class ColorTexture : Texture
    {
        int Width;
        int Height;
        Color[] Data;
        IDictionary<string, Texture> textureMap = new Dictionary<string, Texture>();

        ColorTexture() {} 

        ColorTexture(int width, int height, Color[] data)
        {
            this.Width = width;
            this.Height = height;
            this.Data = data;
        }

        public Texture GetTexture(String path)
        {
            Texture t;
            if (textureMap.ContainsKey(path))
            {
                textureMap.TryGetValue(path, out t);
                return t;
            } else
            {
                return null;
            }
        }

        public Texture LoadTexture(String path)
        {
            Console.WriteLine("Loding image...");
            Bitmap image = Util.LoadImage(path);
            if(image == null)
            {
                Console.WriteLine("Image load failed");
            } else
            {
                Console.WriteLine("Load Texture >> Success");
            }
            return NewTexture(image);
        }

        public static Texture NewTexture(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Color[] data = new Color[width * height];

            for (int y =0; y< height; y++)
            {
                for (int x = 0; x<width; x++)
                {
                    int index = y * height + x;
                    System.Drawing.Color pixcolor = image.GetPixel(x, y);
                    int alpha = pixcolor.A;
                    int red = pixcolor.R;
                    int green = pixcolor.G;
                    int blue = pixcolor.B;
                    data[index] = new Color((double)red/65535, (double)green/65535, (double)blue/65535).Pow(2.2);        
                }
            }
            return new ColorTexture(width, height, data);
        }
        
        public Texture Pow(double a)
        {
            for (int i = 0; i < this.Data.Length; i++)
            {
                this.Data[i] = this.Data[i].Pow(a);
            }
            return this;
        }

        public Texture MulScalar(double a)
        {
            for (int i = 0; i < this.Data.Length; i++)
            {
                this.Data[i] = this.Data[i].MulScalar(a);
            }
            return this;
        }

        public Color bilinearSample(double u, double v)
        {
            if (u == 1)
                u -= Util.EPS;
            if (v == 1)
                v -= Util.EPS;
            double w = (double)this.Width - 1;
            double h = (double)this.Height - 1;
            int X, Y, x0, y0, x1, y1;
            double x, y;
            X = (int)(u * w);
            Y = (int)(v * h);
            x = Util.Fract(u * w);
            y = Util.Fract(v * h);
            x0 = (int)(X);
            y0 = (int)(Y);
            x1 = x0 + 1;
            y1 = y0 + 1;
            Color c00 = this.Data[y0 * this.Width + x0];
            Color c01 = this.Data[y1 * this.Width + x0];
            Color c10 = this.Data[y0 * this.Width + x1];
            Color c11 = this.Data[y1 * this.Width + x1];
            Color c = Color.Black;
            c = c.Add(c00.MulScalar((1 - x) * (1 - y)));
            c = c.Add(c10.MulScalar(x * (1 - y)));
            c = c.Add(c01.MulScalar((1 - x) * y));
            c = c.Add(c11.MulScalar(x * y));
            return c;
        }

        public Color Sample(double u, double v)
        {
            u = Util.Fract(Util.Fract(u) + 1);
            v = Util.Fract(Util.Fract(v) + 1);
            return this.bilinearSample(u, 1 - v);
        }

        public Vector NormalSample(double u, double v)
        {
            Color c = Sample(u, v);
            return new Vector(c.R * 2 - 1, c.G * 2 - 1, c.B * 2 - 1).Normalize();
        }

        public Vector BumpSample(double u, double v)
        {
            u = Util.Fract(Util.Fract(u) + 1);
            v = Util.Fract(Util.Fract(v) + 1);
            v = 1 - v;
            int x = (int)(u * this.Width);
            int y = (int)(v * this.Height);
            int x1 = Util.ClampInt(x - 1, 0, this.Width - 1);
            int x2 = Util.ClampInt(x + 1, 0, this.Width - 1);
            int y1 = Util.ClampInt(y - 1, 0, this.Height - 1);
            int y2 = Util.ClampInt(y + 1, 0, this.Height - 1);
            Color cx = this.Data[y * this.Width + x1].Sub(this.Data[y * this.Width + x2]);
            Color cy = this.Data[y1 * this.Width + x].Sub(this.Data[y2 * this.Width + x]);
            return new Vector(cx.R, cy.R, 0);
        }
    }
}

