using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Cylinder : IShape
    {
        double Radius;
        double Z0, Z1;
        Material CylinderMaterial;

        Cylinder(double radius, double z0, double z1, Material material)
        {
            Radius = radius;
            Z0 = z0;
            Z1 = z1;
            CylinderMaterial = material;
        }

        internal static Cylinder NewCylinder(double radius, double z0, double z1, Material material)
        {
            return new Cylinder(radius, z0, z1, material);
        }

        internal static IShape NewTransformedCylinder(Vector v0, Vector v1, double radius, Material material)
        {
            Vector up = new Vector(0, 0, 1);
            Vector d = v1.Sub(v0);
            var z = d.Length();
            var a = Math.Acos(d.Normalize().Dot(up));
            Matrix m = new Matrix().Translate(v0);
            if (a != 0)
            {
                Vector u = d.Cross(up).Normalize();
                m = new Matrix().Rotate(u, a).Translate(v0);
            }
            var c = NewCylinder(radius, 0, z, material);
            return (IShape)TransformedShape.NewTransformedShape(c, m);
        }

        Box IShape.BoundingBox()
        {
            double r = Radius;
            return new Box(new Vector(-r, -r, Z0), new Vector(r, r, Z1));
        }

        Hit IShape.Intersect(Ray ray)
        {
            double r = Radius;
            Vector o = ray.Origin;
            Vector d = ray.Direction;
            double a = (d.X * d.X) + (d.Y * d.Y);
            double b = (2 * o.X * d.X) + (2 * o.Y * d.Y);
            double c = (o.X * o.X) + (o.Y * o.Y) - (r * r);
            double q = (b * b) - (4 * a * c);
            if (q < Util.EPS)
            {
                return Hit.NoHit;
            }
            double s = Math.Sqrt(q);
            double t0 = (-b + s) / (2 * a);
            double t1 = (-b - s) / (2 * a);
            if (t0 > t1)
            {
                (t0, t1) = (t1, t0);
            }
            double z0 = o.Z + t0 * d.Z;
            double z1 = o.Z + t1 * d.Z;
            if (t0 > Util.EPS && Z0 < z0 && z0 < Z1)
            {
                return new Hit(this, t0, null);
            }
            if (t1 > Util.EPS && Z0 < z1 && z1 < Z1)
            {
                return new Hit(this, t1, null);
            }
            return Hit.NoHit;
        }

        Vector IShape.UV(Vector p)
        {
            return new Vector();
        }

        Material IShape.MaterialAt(Vector p)
        {
            return CylinderMaterial;
        }

        Vector IShape.NormalAt(Vector p)
        {
            p.Z = 0;
            return p.Normalize();
        }

        void IShape.Compile() { }
    }
}
