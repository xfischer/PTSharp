using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Plane : IShape
    {
        Vector Point;
        Vector Normal;
        Material Material;
        Box box;

        Plane() { }

        Plane(Vector point, Vector normal, Material mat)
        {
            this.Point = point;
            this.Normal = normal;
            this.Material = mat;
            this.box = new Box(new Vector(-Util.INF, -Util.INF, -Util.INF), new Vector(Util.INF, Util.INF, Util.INF));
        }

        internal static Plane NewPlane(Vector point, Vector normal, Material material)
        {
            return new Plane(point, normal.Normalize(), material);
        }

        Box IShape.BoundingBox()
        {
            return new Box(new Vector(-Util.INF, -Util.INF, -Util.INF), new Vector(Util.INF, Util.INF, Util.INF));
        }

        Hit IShape.Intersect(Ray ray)
        {
            double d = this.Normal.Dot(ray.Direction);
            if (Math.Abs(d) < Util.EPS)
            {
                return Hit.NoHit;
            }
            Vector a = this.Point.Sub(ray.Origin);
            double t = a.Dot(this.Normal) / d;
            if (t < Util.EPS)
            {
                return Hit.NoHit;
            }
            return new Hit(this, t, null);
        }
        
        Vector IShape.NormalAt(Vector a)
        {
            return this.Normal;
        }

        Vector IShape.UV(Vector a)
        {
            return new Vector();
        }

        void IShape.Compile() { }

        public Material MaterialAt(Vector v)
        {
            return this.Material;
        }
    }
}
