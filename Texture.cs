using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class ColorTexture : ITexture
    {
        public int Width;
        public int Height;
        public Color[] Data;
        
        internal static IDictionary<string, ITexture> textures = new Dictionary<string, ITexture>();

        internal ColorTexture()
        {
            Width = 0;
            Height = 0;
            Data = new Color[] { };
        }
        
        ColorTexture(int width, int height, Color[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        internal ITexture LoadTexture(String path)
        {
            Console.WriteLine("Loading image...");
            Bitmap image = Util.LoadImage(path);
            if (image == null)
            {
                Console.WriteLine("Image load failed");
            }
            else
            {
                Console.WriteLine("Load Texture >> Success");
            }
            return NewTexture(image);
        }

        internal static ITexture GetTexture(string path)
        {
            if(textures.ContainsKey(path))
            {
                Console.WriteLine("Texture: "+ path + " ... OK");
                return textures[path];
            } else
            {
                Console.WriteLine("Adding texture to list...");
                ITexture img = new ColorTexture().LoadTexture(path);
                textures.Add(path, img);
                return img;
            }
        }
        
        ITexture NewTexture(Bitmap image)
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = image.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.Left, (int)boundsF.Top, (int)boundsF.Width, (int)boundsF.Height);

            int yMax = (int)boundsF.Height;
            int xMax = (int)boundsF.Width;

            Color[] imgdata = new Color[xMax * yMax];
            
            for (int y = 0; y < yMax; y++)
            {
                for (int x = 0; x < xMax; x++)
                {
                    int index = y * xMax + x;
                    System.Drawing.Color pixelcolor = image.GetPixel(x, y);
                    imgdata[index] = new Color((double)(pixelcolor.R)  / 255, (double)(pixelcolor.G) / 255, (double)(pixelcolor.B) / 255).Pow(2.2); 
                }
            }
            return new ColorTexture(xMax, yMax, imgdata);
        }
        
        ITexture ITexture.Pow(double a)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = Data[i].Pow(a);
            }
            return this;
        }

        ITexture ITexture.MulScalar(double a)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = Data[i].MulScalar(a);
            }
            return this;
        }

        Tuple<double,double> Modf(double input)
        {
            double integral;
            double fractional;
            if (input < 1)
            {
                if (input < 0)
                {
                    integral = Math.Truncate(input);
                    fractional = input - integral;
                    return new Tuple<double, double>(-integral, -fractional);
                }
                if (input == 0)
                {
                    return new Tuple<double, double>(-0, -0);
                }
                return new Tuple<double, double>(0, input);
            }
            integral = Math.Truncate(input);
            fractional = input - integral;
            return new Tuple<double, double>(integral,fractional);
        }

        internal Color bilinearSample(double u, double v)
        {   
             if (u == 1)
                 u -= Util.EPS;
             if (v == 1)
                 v -= Util.EPS;
             var w = (double)Width - 1;
             var h = (double)Height - 1;

             double X, Y;
             double x, y;

             (X, x) = (Modf(u * w).Item1, Modf(u * w).Item2);
             (Y, y) = (Modf(v * h).Item1, Modf(v * h).Item2);

             var x0 = (int)X;
             var y0 = (int)Y;
             var x1 = x0 + 1;
             var y1 = y0 + 1;

             Color c = Color.Black;
             Color c00 = Data[y0 * this.Width + x0];
             Color c01 = Data[y1 * this.Width + x0];
             Color c10 = Data[y0 * this.Width + x1];
             Color c11 = Data[y1 * this.Width + x1];
             c = c.Add(c00.MulScalar((1 - x) * (1 - y)));
             c = c.Add(c10.MulScalar(x * (1 - y)));
             c = c.Add(c01.MulScalar((1 - x) * y));
             c = c.Add(c11.MulScalar(x * y));
             return c;
        }

        double Fract(double x)
        {
            x = Modf(x).Item2;
            return x;
        }

        Color ITexture.Sample(double u, double v)
        {
            u = Fract(Fract(u) + 1);
            v = Fract(Fract(v) + 1);
            return bilinearSample(u, 1 - v);
        }

        Vector ITexture.NormalSample(double u, double v)
        {
            u = Fract(Fract(u) + 1);
            v = Fract(Fract(v) + 1);
            Color c = bilinearSample(u, 1 - v);
            return new Vector(c.R * 2 - 1, c.G * 2 - 1, c.B * 2 - 1).Normalize();
        }

        Vector ITexture.BumpSample(double u, double v)
        {
            u = Fract(Fract(u) + 1);
            v = Fract(Fract(v) + 1);
            v = 1 - v;
            int x = (int)(u * (double)Width);
            int y = (int)(v * (double)Height);
            int x1 = Util.ClampInt(x - 1, 0, this.Width - 1);
            int x2 = Util.ClampInt(x + 1, 0, this.Width - 1);
            int y1 = Util.ClampInt(y - 1, 0, this.Height - 1);
            int y2 = Util.ClampInt(y + 1, 0, this.Height - 1);
            Color cx = Data[y * Width + x1].Sub(Data[y * Width + x2]);
            Color cy = Data[y1 * Width + x].Sub(Data[y2 * Width + x]);
            return new Vector(cx.R, cy.R, 0);
        }
    }
}

