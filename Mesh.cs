using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Mesh : Shape
    {
        Box box;
        Tree tree;
        Material material;

        public Triangle[] Triangles;
        public List<Triangle> TriangleList;
        
        public Mesh() {}

        public Mesh(Triangle[] triangles, Box box, Tree tree)
        {
            this.Triangles = triangles;
            this.box = box;
            this.tree = tree;
        }

        public Mesh(Triangle[] triangles, Box box, Tree tree, Material material)
        {
            this.Triangles = triangles;
            this.box = box;
            this.tree = tree;
            this.material = material;
        }

        public static Mesh NewMesh(Triangle[] triangle)
        {
            return new Mesh(triangle, null, null);
        }

        public static Mesh NewMesh(Triangle[] triangle, Material material)
        {
            return new Mesh(triangle, null, null);
        }

        void dirty()
        {
            this.box = null;
            this.tree = null;
        }
        
        Mesh Copy()
        {
            Triangle[] triangle = new Triangle[this.Triangles.Length];
            
            for(int i=0; i<this.Triangles.Length; i++)
            {
                triangle[i] = this.Triangles[i];
            }
            return NewMesh(triangle);
        }
        
        public void Compile()
        {
            if (this.tree == null)
            {
                Shape[] shapes = new Shape[this.Triangles.Length];
                
                for(int i=0; i< this.Triangles.Length; i++)
                {
                    shapes[i] = this.Triangles[i];
                }
                this.tree = Tree.NewTree(shapes);
            }
        }

        void Add(Mesh b)
        {
            
            foreach(Triangle t in b.Triangles)
            {
                TriangleList = new List<Triangle>();
                this.material = b.material;
                TriangleList.Add(t);
            }    
            
            this.Triangles = TriangleList.ToArray();
            this.dirty();
        }
        
        public Box BoundingBox()
        {
            if (this.box == null)
            {
                Vector min = this.Triangles[0].V1;
                Vector max = this.Triangles[0].V1;

                foreach(Triangle t in this.Triangles)
                {
                    min = min.Min(t.V1).Min(t.V2).Min(t.V3);
                    max = max.Max(t.V1).Max(t.V2).Max(t.V3);
                }
                this.box = new Box(min, max);
            }
            return this.box;
        }
        
        public Hit Intersect(Ray r)
        {
            return this.tree.Intersect(r);
        }
        
        public Vector UV(Vector p)
        {
            return new Vector();
        }
        
        public Material MaterialAt(Vector p)
        {
            return new Material();
        }

        public Vector NormalAt(Vector p)
        {
            return new Vector();
        }

        Vector smoothNormalsThreshold(Vector normal, Vector[] normals, double threshold)
        {
            Vector result = new Vector(0,0,0);
            foreach (Vector x in normals)
            {
                if (x.Dot(normal) >= threshold)
                {
                    result = result.Add(x);
                }
            }
            return result.Normalize();
        }

        void SmoothNormalsThreshold(double radians)
        {
            double threshold = Math.Cos(radians);
            Dictionary<Vector, Vector[]> lookup = new Dictionary<Vector, Vector[]>();

            foreach (Triangle t in this.Triangles)
            {
                lookup[t.V1].Append(t.N1);
                lookup[t.V2].Append(t.N2);
                lookup[t.V3].Append(t.N3);

            }
            foreach (Triangle t in this.Triangles)
            {
                t.N1 = smoothNormalsThreshold(t.N1, lookup[t.N1], threshold);
                t.N2 = smoothNormalsThreshold(t.N2, lookup[t.N2], threshold);
                t.N3 = smoothNormalsThreshold(t.N3, lookup[t.N3], threshold);
            }
        }
        
        void SmoothNormals()
        {
            Dictionary<Vector, Vector> lookup = new Dictionary<Vector, Vector>();

            foreach (Triangle t in this.Triangles)
            {
                lookup[t.V1] = lookup[t.V1].Add(t.N1);
                lookup[t.V2] = lookup[t.V2].Add(t.N2);
                lookup[t.V3] = lookup[t.V3].Add(t.N3);
            }

            foreach(KeyValuePair<Vector, Vector> kv in lookup)
            {
                lookup[kv.Key] = kv.Value.Normalize();
            }

            foreach (Triangle t in this.Triangles)
            {
                t.N1 = lookup[t.V1];
                t.N2 = lookup[t.V2];
                t.N3 = lookup[t.V3]; 
            }
        }
        
        void UnitCube()
        {
            this.FitInside(new Box(new Vector(0, 0, 0), new Vector(1, 1, 1)), new Vector(0, 0, 0));
            this.MoveTo(new Vector(0, 0, 0), new Vector(0.5, 0.5, 0.5));
        }
        
        public void MoveTo(Vector position, Vector anchor)
        {
            Matrix matrix = new Matrix().Translate(position.Sub(this.BoundingBox().Anchor(anchor)));
            this.Transform(matrix);
        }
        
        public void Transform(Matrix matrix)
        {
            foreach(Triangle t in this.Triangles)
            {
                t.V1 = matrix.MulPosition(t.V1);
                t.V2 = matrix.MulPosition(t.V2);
                t.V3 = matrix.MulPosition(t.V3);
                t.N1 = matrix.MulDirection(t.N1);
                t.N2 = matrix.MulDirection(t.N2);
                t.N3 = matrix.MulDirection(t.N3);
            }
            this.dirty();
        }
        
        void FitInside(Box box, Vector anchor)
        {
            double scale = box.Size().Div(this.BoundingBox().Size()).MinComponent();
            Vector extra = box.Size().Sub(this.BoundingBox().Size().MulScalar(scale));
            Matrix matrix = new Matrix().Identity();
            matrix = matrix.Translate(this.BoundingBox().Min.Negate());
            matrix = matrix.Scale(new Vector(scale, scale, scale));
            matrix = matrix.Translate(box.Min.Add(extra.Mul(anchor)));
            this.Transform(matrix);
        }
        
        void SetMaterial(Material material)
        {
            foreach(Triangle t in this.Triangles)
            {
                t.TriangleMaterial = material;
            }
        }
    }
}
