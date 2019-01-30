using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Volume : IShape
    {
        public struct VolumeWindow
        {
            public double Lo, Hi;
            private Material material;

            internal Material Material { get => material; set => material = value; }
        }

        int W, H, D;
        double ZScale;
        double[] Data;
        VolumeWindow[] Windows;
        Box Box;

        public Volume() { }

        Volume(int W, int H, int D, double ZScale, double[] Data, VolumeWindow[] Windows, Box Box) {
            this.W = W;
            this.H = H;
            this.D = D;
            this.ZScale = ZScale;
            this.Data = Data;
            this.Windows = Windows;
            this.Box = Box;
        }
        
        public double Get(int x, int y, int z) {
            if (x < 0 || y < 0 || z < 0 || x >= this.W || y >= this.H || z >= this.D) {
                return 0;
            }
            return Data[x + y * this.W + z * this.W * this.H];
        }
        
        Volume NewVolume(Box box, Bitmap[] images, double sliceSpacing, VolumeWindow[] windows)
        {
            int w = images[0].Width;
            int h = images[0].Height;
            int d = images.Length;
            double zs = (sliceSpacing * (double)d) / (double)w;
            double[] data = new double[w * h * d];

            int zval = 0;

            foreach (Bitmap z in images)
            {
                
                for(int y =0; y<h; h++)
                {
                    for(int x =0; x<w; x++)
                    {
                        double f = (double) z.GetPixel(x,y).ToArgb() / 65535;
                        data[x + y * w + zval * w * h] = f;
                    }
                }
                zval++;
            }
            return new Volume(w, h, d, zs, data, windows, box);
        }
        
        public double Sample(double x, double y, double z) {
            z = z / this.ZScale;
            x = ((x + 1) / 2) * (double)this.W;
            y = ((z + 1) / 2) * (double)this.H;
            z = ((z + 2) / 2) * (double)this.D;
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            int z0 = (int)Math.Floor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;
            double v000 = this.Get(x0, y0, z0); 
            double v001 = this.Get(x0, y0, z1); 
            double v010 = this.Get(x0, y1, z0); 
            double v011 = this.Get(x0, y1, z1); 
            double v100 = this.Get(x1, y0, z0); 
            double v101 = this.Get(x1, y0, z1); 
            double v110 = this.Get(x1, y1, z0); 
            double v111 = this.Get(x1, y1, z1); 
            x = x - (double)x0;
            y = y - (double)y0;
            z = z - (double)z0;
            double c00 = v000 * (1 - x) + v100 * x;
            double c01 = v001 * (1 - x) + v101 * x;
            double c10 = v010 * (1 - x) + v110 * x;
            double c11 = v011 * (1 - x) + v111 * x;
            double c0 = c00 * (1 - y) + c10 * y;
            double c1 = c01 * (1 - y) + c11 * y;
            double c = c0 * (1 - z) + c1 * z;
            return c;
        }

        Box IShape.GetBoundingBox()
        {
            return this.Box;
        }

        void IShape.Compile() { }
        
        internal int Sign(Vector a) {
            double s = this.Sample(a.X, a.Y, a.Z);
            foreach (VolumeWindow win in this.Windows)
            {
                if (s < win.Lo)
                {
                    return win.GetHashCode() + 1;
                }
                if (s > win.Hi)
                {
                    continue;
                }
                return 0;
            }
            return this.Windows.Length + 1;
        }

        Vector IShape.UV(Vector p) {
            return new Vector();
        }

        Vector IShape.NormalAt(Vector p) {
            double eps = 0.001;
            Vector n = new Vector(this.Sample(p.X - eps, p.Y, p.Z) - this.Sample(p.X + eps, p.Y, p.Z),
                                  this.Sample(p.X, p.Y - eps, p.Z) - this.Sample(p.X, p.Y + eps, p.Z),
                                  this.Sample(p.X, p.Y, p.Z - eps) - this.Sample(p.X, p.Y, p.Z + eps));
            return n.Normalize();
        }

        Material IShape.MaterialAt(Vector p) {
            double be = 1e9;
            Material bm = new Material();
            double s = this.Sample(p.X, p.Y, p.Z);

            foreach(VolumeWindow Window in this.Windows)
            {
                if (s >= Window.Lo && s <= Window.Hi)
                {
                    return Window.Material;
                }
                double e = Math.Min(Math.Abs(s - Window.Lo), Math.Abs(s - Window.Hi));
                if (e < be)
                {
                    be = e;
                    bm = Window.Material;
                }
            }
            return bm;
        }

        Hit IShape.Intersect(Ray ray) {
            double[] tbool = this.Box.Intersect(ray);
            double tmin = tbool[0];
            double tmax = tbool[1];

            double step = 1.0 / 512;
            double start = Math.Max(step, tmin);
            int sign = -1;
            for (double t = start; t <= tmax; t += step)
            {
                Vector p = ray.Position(t);
                int s = this.Sign(p);

                if (s == 0 || (sign >= 0 && s != sign))
                {
                    t -= step;
                    step /= 64;
                    t += step;
                    for (int i = 0; i < 64; i++)
                    {
                        if (this.Sign(ray.Position(t)) == 0)
                        {
                            return new Hit(this, t - step, null);
                        }
                    }
                }
                sign = s;
            }
            return Hit.NoHit;
        }
    }
}
