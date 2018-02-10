using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Color
    {
        public double R; 
        public double G; 
        public double B; 
        public int alpha;
        
        public Color(Color c)
        {
            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
        }

        public Color(double R, double G, double B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public static Color Black = new Color(0, 0, 0);
        public static Color White = new Color(1, 1, 1);

        public Color()
        {

        }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length != 4) return;
            R = double.Parse(data[1]);
            G = double.Parse(data[2]);
            B = double.Parse(data[3]);
        }

        public static Color NewColor(int r, int g, int b)
        {
            return new Color((double)r / 65535, (double)g / 65535, (double)b / 65535);
        }
        
        public static Color HexColor(int x)
        {
            double red = Convert.ToDouble(x >> 16 & 0xff)/ 255;
            double green = Convert.ToDouble(x >> 8 & 0xff)/ 255;
            double blue = Convert.ToDouble(x & 0xff)/ 255;
            return new Color(red, green, blue).Pow(2.2);
        }
        
        public Color Pow(double b)
        {
            return new Color(Math.Pow(this.R, b), Math.Pow(this.G, b), Math.Pow(this.B, b));
        }

        public int getIntFromColor(double red, double green, double blue)
        {
            UInt16 r = Convert.ToUInt16(Math.Max(0, Math.Min(255, red * 255)));
            UInt16 g = Convert.ToUInt16(Math.Max(0, Math.Min(255, green * 255)));
            UInt16 b = Convert.ToUInt16(Math.Max(0, Math.Min(255, blue * 255)));
            return 255 << 24 | r << 16 | g << 8 | b;
        }

        public int getIntFromColor64(double red, double green, double blue)
        {
            UInt16 r = Convert.ToUInt16(Math.Max(0, Math.Min(65535, red * 65535)));
            UInt16 g = Convert.ToUInt16(Math.Max(0, Math.Min(65535, green * 65535)));
            UInt16 b = Convert.ToUInt16(Math.Max(0, Math.Min(65535, blue * 65535)));
            return 65535 << 24 | r << 16 | g << 8 | b;
        }
        
       public static Color Kelvin(double K)
        {
            double red, green, blue;
            double a, b, c, x;

            // red
            if (K >= 6600)
            {
                a = 351.97690566805693;  
                b = 0.114206453784165;   
                c = -40.25366309332127;  
                x = K / 100 - 55;        

                red = a + b * x + c * Math.Log(x); 
            }
            else
            {
                red = 255;
            }

            if (K >= 6600)
            {
                a = 325.4494125711974;   
                b = 0.07943456536662342; 
                c = -28.0852963507957;   
                x = K / 100 - 50;

                green = a + b * x + c * Math.Log(x);
            }
            else if (K >= 1000)
            {
                a = -155.25485562709179;
                b = -0.44596950469579133;
                c = 104.49216199393888;
                x = K / 100 - 2;
                green = a + b * x + c * Math.Log(x);

            }
            else
            {
                green = 0;
            }

            if (K >= 6600)
            {
                blue = 255;
            }
            else if (K >= 2000)
            {
                a = -254.76935184120902;
                b = 0.8274096064007395;
                c = 115.67994401066147;
                x = K / 100 - 10;

                blue = a + b * x + c * Math.Log(x);

            }
            else
            {
                blue = 0;
            }

            red = Math.Min(1, red / 255);
            green = Math.Min(1, green / 255);
            blue = Math.Min(1, blue / 255);

            return new Color(red, green, blue);
        }

        public Color Mix(Color b, double pct)
        {
            Color a = this.MulScalar(1-pct);
            b = b.MulScalar(pct);
            return a.Add(b);
        }

        public Color MulScalar(double b)
        {
            return new Color(this.R * b, this.G * b, this.B * b);
        }

        public Color Add(Color b)
        {
            return new Color(this.R + b.R, this.G + b.G, this.B + b.B);
        }
        
        public Color Sub(Color b)
        {
            return new Color(this.R - b.R, this.G - b.G, this.B - b.B);
        }
        
        public Color Mul(Color b)
        {
            return new Color(this.R * b.R, this.G * b.G, this.B * b.B);
        }

        public Color Div(Color b)
        {
            return new Color(this.R / b.R, this.G / b.G, this.B / b.B);
        }
        
        public Color DivScalar(double b)
        {
            return new Color(this.R / b, this.G / b, this.B / b);
        }
        
        public Color Min(Color b)
        {
            return new Color(Math.Min(this.R, b.R), Math.Min(this.G, b.G), Math.Min(this.B, b.B));
        }
        
        public Color Max(Color b)
        {
            return new Color(Math.Max(this.R, b.R), Math.Max(this.G, b.G), Math.Max(this.B, b.B));
        }
        
        public double MinComponent()
        {
            return Math.Min(Math.Min(this.R, this.G), this.B);
        }
        
        public double MaxComponent()
        {
            return Math.Max(Math.Max(this.R, this.G), this.B);
        }
    }
}
