using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Util
    {
        public static double INF = 1e9;
        public static double EPS = 1e-9;
        public static double Radians(double degrees) => degrees * Math.PI / 180;
        public static double Degrees(double radians) => radians * 180 / Math.PI;
        public static Vector Cone(Vector direction, double theta, double u, double v, Random rand)
        {
            double m1, m2, a;
            Vector q;
            Vector s;
            Vector t, d;

            if (theta < EPS)
            {
                return direction;
            }
           
            theta = theta * (1 - (2 * Math.Cos(u) / Math.PI));
            m1 = Math.Sin(theta);
            m2 = Math.Cos(theta);
            a = v * 2 * Math.PI;
            q = Vector.RandomUnitVector(rand);
            s = direction.Cross(q);
            t = direction.Cross(s);
            d = new Vector();
            d = d.Add(s.MulScalar(m1 * Math.Cos(a)));
            d = d.Add(t.MulScalar(m1 * Math.Sin(a)));
            d = d.Add(direction.MulScalar(m2));
            d = d.Normalize();
            
            return d;
        }
        
        public static Bitmap LoadImage(String path)
        {
            try
            {
                Bitmap image1 = new Bitmap(path); 
                return image1;
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("There was an error opening the bitmap." +
                    "Please check the path.");
                return null;
            }
        }
        
        void SavePNG(String path, Bitmap bitmap)
        {
            try
            {
                if(bitmap != null)
                bitmap.Save(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("There was an error writing image to file..." +
                    "Please check the path.");
            }
        }
                
        public static double Median(double[] items)
        {
            switch(items.Length)
            {
                case 0:
                    return 0;
                case var d when items.Length % 2 == 1:
                    return items[items.Length / 2];
                default:
                    var a = items[items.Length / 2 - 1];
                    var b = items[items.Length / 2];
                    return (a + b) / 2;
            }
        }

        Tuple<int, double> Modf(double input)
        {
            var dec = (int)Math.Truncate(input);
            var frac = input - Math.Truncate(input);
            return new Tuple<int, double>(dec, frac);
        }

        public static double Fract(double x)
        {
            double ret = x - Math.Truncate(x);
            return x;
        }


        public static double Clamp(double x, double lo, double hi)
        {
            if (x < lo)
                return lo;
            if (x > hi)
                return hi;
            return x;
        }
        
        public static int ClampInt(int x, int lo, int hi)
        {
            if (x < lo)
                return lo;
            if (x > hi)
                return hi;
            return x;
        }

        static String NumberString(double x)
        {
            return x.ToString();
        }
        
        double[] ParseFloats(String[] items)
        {
            List<double> result = new List<double>(items.Length);
            foreach(String item in items)
            {
                double f = Double.Parse(item);
                result.Add(f);
            }
            return result.ToArray();
        }
        
        int[] ParseInts(String[] items)
        {
            List<int> result = new List<int>(items.Length);
            foreach (String item in items)
            {
                int f = int.Parse(item);
                result.Add(f);
            }
            return result.ToArray();
        }
    }
}
