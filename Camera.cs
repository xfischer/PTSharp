using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Camera
    {
        Vector p, u, v, w;
        double m;
        double focalDistance;
        double apertureRadius;

        public Camera() { }

        public static Camera LookAt(Vector eye, Vector center, Vector up, double fovy)
        {
            Camera c = new Camera();
            c.p = eye;
            c.w = center.Sub(eye).Normalize();
            c.u = up.Cross(c.w).Normalize();
            c.v = c.w.Cross(c.u).Normalize();
            c.m = 1 / Math.Tan(fovy * Math.PI / 360);
            return c;
        }
        
        public void SetFocus(Vector focalPoint_, double apertureRadius_)
        {
            focalDistance = focalPoint_.Sub(p).Length();
            apertureRadius = apertureRadius_;
        }
        
        public Ray CastRay(int x, int y, int w, int h, double u, double v, Random rand)
        {
            double aspect = (double)w / (double)h;
            var px = (((double)x + u - 0.5) / ((double)w - 1)) * 2 - 1;
            var py = (((double)y + v - 0.5) / ((double)h - 1)) * 2 - 1;
            Vector d = new Vector();
            d = d.Add(this.u.MulScalar(-px * aspect));
            d = d.Add(this.v.MulScalar(-py));
            d = d.Add(this.w.MulScalar(this.m));
            d = d.Normalize();
            var p1 = this.p;
            if (apertureRadius > 0)
            {
                var focalPoint = p.Add(d.MulScalar(focalDistance));
                var angle = rand.NextDouble() * 2 * Math.PI;
                var radius = rand.NextDouble() * apertureRadius;
                p1 = p1.Add(this.u.MulScalar(Math.Cos(angle) * radius));
                p1 = p1.Add(this.v.MulScalar(Math.Sin(angle) * radius));
                d = focalPoint.Sub(p1).Normalize();
            }
            return new Ray(p1, d);
        }
    }
}
