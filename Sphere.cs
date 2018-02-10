using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Sphere : Shape
    {
        
        public Vector Center;
        public double Radius;
        public Material SphereMaterial;
        Box Box;

        Sphere(Vector center, double radius, Material material, Box box)
        {
            this.Center = center;
            this.Radius = radius;
            this.SphereMaterial = material;
            this.Box = box;
        }
        
        public static Sphere NewSphere(Vector center, double radius, Material material) {
            Vector min = new Vector(center.X - radius, center.Y - radius, center.Z - radius);
            Vector max = new Vector(center.X + radius, center.Y + radius, center.Z + radius);
            Box box = new Box(min, max);
            return new Sphere(center, radius, material, box);
        }
        
        public Box BoundingBox()
        {
            return this.Box;
        }

        public Hit Intersect(Ray r) {
            Vector to = r.Origin.Sub(this.Center);
            double b = to.Dot(r.Direction);
            double c = to.Dot(to) - this.Radius * this.Radius;
            double d = b * b - c;
            if (d > 0)
            {
                d = Math.Sqrt(d);
                double t1 = -b - d;
                if (t1 > Util.EPS)
                {
                    return new Hit(this, t1, null);
                }
                double t2 = -b + d;
                if (t2 > Util.EPS)
                {
                    return new Hit(this, t2, null);
                }
            }
            return Hit.NoHit;
        }

        public Vector UV(Vector p) {
            p = p.Sub(this.Center);
            double u = Math.Atan2(p.Z, p.X);
            double v = Math.Atan2(p.Y, new Vector(p.X, 0, p.Z).Length());
            u = 1 - (u + Math.PI) / (2 * Math.PI);
            v = (v + Math.PI / 2) / Math.PI;
            return new Vector(u, v, 0);
        }
        
        public Material MaterialAt(Vector p) {
            return this.SphereMaterial;
        }

        public Vector NormalAt(Vector p) {
            return p.Sub(this.Center).Normalize();
        }

        public void Compile()
        {

        }
    }
}
