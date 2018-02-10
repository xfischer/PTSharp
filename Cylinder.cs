using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Cylinder : Shape
    {
        double Radius;
        double Z0, Z1;
        Material CylinderMaterial;

        Cylinder(double radius, double z0, double z1, Material material)
        {
            this.Radius = radius;
            this.Z0 = z0;
            this.Z1 = z1;
            this.CylinderMaterial = material;
        }

        public static Cylinder NewCylinder(double radius, double z0, double z1, Material material)
        {
            return new Cylinder(radius, z0, z1, material);
        }

        public static Shape NewTransformedCylinder(Vector v0, Vector v1, double radius, Material material)
        {
            Vector up = new Vector(0, 0, 1);
            Vector d = v1.Sub(v0);
            double z = d.Length();
            double a = Math.Acos(d.Normalize().Dot(up));
            Matrix m = new Matrix().Translate(v0);
            if (a != 0)
            {
                Vector u = d.Cross(up).Normalize();
                m = new Matrix().Rotate(u, a).Translate(v0);
            }
            Cylinder c = NewCylinder(radius, 0, z, material);
            return TransformedShape.NewTransformedShape(c, m);
        }

        public Box BoundingBox()
        {
            double r = this.Radius;
            return new Box(new Vector(-r, -r, this.Z0), new Vector(r, r, this.Z1));
        }
        
        public Hit Intersect(Ray ray)
        {
            double r = this.Radius;
            Vector o = ray.Origin;
            Vector d = ray.Direction;
            double a = d.X * d.X + d.Y * d.Y;
            double b = 2 * o.X * d.X + 2 * o.Y * d.Y;
            double c = o.X * o.X + o.Y * o.Y - r * r;
            double q = b * b - 4 * a * c;
            if (q < Util.EPS)
            {
                return Hit.NoHit;
            }
            double s = Math.Sqrt(q);
            double t0 = (-b + s) / (2 * a);
            double t1 = (-b - s) / (2 * a);
            if (t0 > t1)
            {
                double temp = t0;
                t0 = t1;
                t1 = temp;
            }
            double z0 = o.Z + t0 * d.Z;
            double z1 = o.Z + t1 * d.Z;
            if (t0 > Util.EPS && this.Z0 < z0 && z0 < this.Z1)
            {
                return new Hit(this, t0, null);
            }
            if (t1 > Util.EPS && this.Z0 < z1 && z1 < this.Z1)
            {
                return new Hit(this, t1, null);
            }
            return Hit.NoHit;
        }

        public Vector UV(Vector p)
        {
            return new Vector();
        }

        public Material MaterialAt(Vector p)
        {
            return this.CylinderMaterial;
        }

        public Vector NormalAt(Vector p)
        {
            p.Z = 0;
            return p.Normalize();
        }

        public void Compile()
        {
            
        }
    }
}
