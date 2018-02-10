using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Cube : Shape
    {
        Vector Min;
        Vector Max;
        Material CubeMaterial;
        Box Box;

        Cube(Vector min, Vector max, Material material, Box box)
        {
            this.Min = min;
            this.Max = max;
            this.CubeMaterial = material;
            this.Box = box;
        }

        public static Cube NewCube(Vector min, Vector max, Material material)
        {
            Box box = new Box(min, max);
            return new Cube(min, max, material, box);
        }

        public void Compile()
        {

        }

        public Box BoundingBox()
        {
            return this.Box;
        }

        public Hit Intersect(Ray r)
        {
            Vector n = this.Min.Sub(r.Origin).Div(r.Direction);
            Vector f = this.Max.Sub(r.Origin).Div(r.Direction);

            n = n.Min(f);
            f = n.Max(f);

            double t0 = Math.Max(Math.Max(n.X, n.Y), n.Z);
            double t1 = Math.Min(Math.Min(f.X, f.Y), f.Z);

            if (t0 > 0 && t0 < t1)
            {
                return new Hit(this, t0, null);
            }
            return Hit.NoHit;
        }
        
        public Vector UV(Vector p)
        {
            p = p.Sub(this.Min).Div(this.Max.Sub(this.Min));
            return new Vector(p.X, p.Z, 0);
        }

        public Material MaterialAt(Vector p)
        {
            return this.CubeMaterial;
        }
        
        public Vector NormalAt(Vector p)
        {
            if (p.X < this.Min.X + Util.EPS) 
            {
                return new Vector(-1, 0, 0);  
            }
            else if (p.X > this.Max.X - Util.EPS) 
            {
                return new Vector(1, 0, 0);   
            }
            else if (p.Y < this.Min.Y + Util.EPS) 
            {
                return new Vector(0, -1, 0);  
            }
            else if (p.Y > this.Max.Y - Util.EPS)  
            {
                return new Vector(0, 1, 0);   
            }
            else if (p.Z < this.Min.Z + Util.EPS)  
            {
                return new Vector(0, 0, -1);  
            }
            else if (p.Z > this.Max.Z - Util.EPS)  
            {
                return new Vector(0, 0, 1);   
            }
            return new Vector(0, 1, 0);
        }
        
        public Mesh CubeMesh()
        {
            Vector a = this.Min;
            Vector b = this.Max;
            Vector z = new Vector();
            Material m = this.CubeMaterial;
            Vector v000 = new Vector(a.X, a.Y, a.Z);
            Vector v001 = new Vector(a.X, a.Y, b.Z);
            Vector v010 = new Vector(a.X, b.Y, a.Z);
            Vector v011 = new Vector(a.X, b.Y, b.Z);
            Vector v100 = new Vector(b.X, a.Y, a.Z);
            Vector v101 = new Vector(b.X, a.Y, b.Z);
            Vector v110 = new Vector(b.X, b.Y, a.Z);
            Vector v111 = new Vector(b.X, b.Y, b.Z);

            Triangle[] triangles = {
                Triangle.NewTriangle(v000, v100, v110, z, z, z, m),
                Triangle.NewTriangle(v000, v110, v010, z, z, z, m),
                Triangle.NewTriangle(v001, v101, v111, z, z, z, m),
                Triangle.NewTriangle(v001, v111, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v100, v101, z, z, z, m),
                Triangle.NewTriangle(v000, v101, v001, z, z, z, m),
                Triangle.NewTriangle(v010, v110, v111, z, z, z, m),
                Triangle.NewTriangle(v010, v111, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v010, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v011, v001, z, z, z, m),
                Triangle.NewTriangle(v100, v110, v111, z, z, z, m),
                Triangle.NewTriangle(v100, v111, v101, z, z, z, m)
            };
            return Mesh.NewMesh(triangles);
        }
    }
}
