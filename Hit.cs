using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Hit
    {
        static double INF = 1e9;
        public Shape Shape;
        public double T;
        public HitInfo HInfo;

        public static Hit NoHit = new Hit(null, INF, null);

        public Hit(Shape shape, double t, HitInfo hinfo)
        {
            this.Shape = shape;
            this.T = t;
            this.HInfo = hinfo;
        }
        
        public Boolean Ok()
        {
            return this.T < INF;
        }
        
        public HitInfo Info(Ray r)
        {
            if (this.HInfo != null)
            {
                return this.HInfo;
            }
            Shape shape = this.Shape;
            Vector position = r.Position(this.T);
            Vector normal = this.Shape.NormalAt(position);
            Material material = Material.MaterialAt(this.Shape, normal);
            Boolean inside = false;
            if (normal.Dot(r.Direction) > 0)
            {
                normal = normal.Negate();
                inside = true;
                if(shape is Volume)
                {
                    inside = false;
                }
            }
            Ray ray = new Ray(position, normal);
            return new HitInfo(Shape, position, normal, ray, material, inside);
        }
    }

    public class HitInfo
    {
        public Shape shape;
        public Vector position;
        public Vector normal;
        public Ray Ray;
        public Material material;
        public Boolean inside;

        public HitInfo(Shape shape, Vector position, Vector normal, Ray r, Material mat, Boolean inside)
        {
            this.shape = shape;
            this.position = position;
            this.normal = normal;
            this.Ray = r;
            this.material = mat;
            this.inside = inside;
        }
    }
}


