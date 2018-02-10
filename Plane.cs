using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Plane : Shape
    {
        Vector Point;
        Vector Normal;
        Material Material;
        Box box;

        Plane()
        {

        }

        Plane(Vector point, Vector normal, Material mat)
        {
            this.Point = point;
            this.Normal = normal;
            this.Material = mat;
            this.box = new Box(new Vector(-Util.INF, -Util.INF, -Util.INF), new Vector(Util.INF, Util.INF, Util.INF));
        }

        public static Plane NewPlane(Vector point, Vector normal, Material material)
        {
            return new Plane(point, normal.Normalize(), material);
        }

        public Box BoundingBox()
        {
            return new Box(new Vector(-Util.INF, -Util.INF, -Util.INF), new Vector(Util.INF, Util.INF, Util.INF));
        }
    
        public Hit Intersect(Ray ray)
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
        
        public Vector NormalAt(Vector a)
        {
            return this.Normal;
        }

        public Vector UV(Vector a)
        {
            return new Vector();
        }
        
        public Material MaterialAt(Vector a)
        {
            return this.Material;
        }
        
        public void Compile()
        {

        }
    }
}
