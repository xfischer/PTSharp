using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    internal class Mesh : IShape
    {
        private Box meshbox;
        private Tree meshtree;
        private Material meshmaterial;

        private Triangle[] Triangles;
        private List<Triangle> TriangleList;

        public Mesh() { }

        internal Mesh(Triangle[] triangles, Box box, Tree tree)
        {
            Triangles = triangles;
            meshbox = box;
            meshtree = tree;
        }

        internal Mesh(Triangle[] triangles, Box box, Tree tree, Material material)
        {
            Triangles = triangles;
            meshbox = box;
            meshtree = tree;
            meshmaterial = material;
        }

        internal static Mesh NewMesh(Triangle[] triangle)
        {
            return new Mesh(triangle, new Box(), new Tree());
        }

        internal static Mesh NewMesh(Triangle[] triangle, Material material)
        {
            return new Mesh(triangle, new Box(), new Tree());
        }

        internal void Compile()
        {
            throw new NotImplementedException();
        }

        void dirty()
        {
            meshbox = new Box();
            meshtree = new Tree();
        }
        
        Mesh Copy()
        {
            Triangle[] triangle = new Triangle[Triangles.Length];
            
            for(int i=0; i<Triangles.Length; i++)
            {
                triangle[i] = Triangles[i];
            }
            return NewMesh(triangle);
        }

        internal Hit Intersect(Ray r)
        {
            throw new NotImplementedException();
        }

        void IShape.Compile()
        {
            if (meshtree == null)
            {
                IShape[] shapes = new IShape[Triangles.Length];

                for (int i = 0; i < Triangles.Length; i++)
                {
                    shapes[i] = Triangles[i];
                }
                meshtree = Tree.NewTree(shapes);
            }
        }

        void Add(Mesh b)
        {
            
            foreach(Triangle t in b.Triangles)
            {
                TriangleList = new List<Triangle>();
                meshmaterial = b.meshmaterial;
                TriangleList.Add(t);
            }    
            
            Triangles = TriangleList.ToArray();
            dirty();
        }

        Box IShape.GetBoundingBox()
        {
            if (meshbox.Equals(null))
            {
                Vector min = Triangles[0].V1;
                Vector max = Triangles[0].V1;

                foreach (Triangle t in Triangles)
                {
                    min = min.Min(t.V1).Min(t.V2).Min(t.V3);
                    max = max.Max(t.V1).Max(t.V2).Max(t.V3);
                }
                meshbox = new Box(min, max);
            }
            return meshbox;
        }

        Hit IShape.Intersect(Ray r)
        {
            return meshtree.Intersect(r);
        }
        
        Vector IShape.UV(Vector p)
        {
            return new Vector();
        }
        
        Material IShape.MaterialAt(Vector p)
        {
            return meshmaterial;
        }

        Vector IShape.NormalAt(Vector p)
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

            foreach (Triangle t in Triangles)
            {
                lookup[t.V1].Append(t.N1);
                lookup[t.V2].Append(t.N2);
                lookup[t.V3].Append(t.N3);

            }
            foreach (Triangle t in Triangles)
            {
                t.N1 = smoothNormalsThreshold(t.N1, lookup[t.N1], threshold);
                t.N2 = smoothNormalsThreshold(t.N2, lookup[t.N2], threshold);
                t.N3 = smoothNormalsThreshold(t.N3, lookup[t.N3], threshold);
            }
        }
        
        void SmoothNormals()
        {
            Dictionary<Vector, Vector> lookup = new Dictionary<Vector, Vector>();

            foreach (Triangle t in Triangles)
            {
                lookup[t.V1] = lookup[t.V1].Add(t.N1);
                lookup[t.V2] = lookup[t.V2].Add(t.N2);
                lookup[t.V3] = lookup[t.V3].Add(t.N3);
            }

            foreach(KeyValuePair<Vector, Vector> kv in lookup)
            {
                lookup[kv.Key] = kv.Value.Normalize();
            }

            foreach (Triangle t in Triangles)
            {
                t.N1 = lookup[t.V1];
                t.N2 = lookup[t.V2];
                t.N3 = lookup[t.V3]; 
            }
        }
        void UnitCube()
        {
            FitInside(new Box(new Vector(0, 0, 0), new Vector(1, 1, 1)), new Vector(0, 0, 0));
            MoveTo(new Vector(0, 0, 0), new Vector(0.5, 0.5, 0.5));
        }
        
        public void MoveTo(Vector position, Vector anchor)
        {
            Matrix matrix = new Matrix().Translate(position.Sub(GetBoundingBox().Anchor(anchor)));
            Transform(matrix);
        }

        private Box GetBoundingBox()
        {
            if (meshbox.Equals(null))
            {
                Vector min = Triangles[0].V1;
                Vector max = Triangles[0].V1;

                foreach (Triangle t in Triangles)
                {
                    min = min.Min(t.V1).Min(t.V2).Min(t.V3);
                    max = max.Max(t.V1).Max(t.V2).Max(t.V3);
                }
                meshbox = new Box(min, max);
            }
            return meshbox;
        }

        internal void Transform(Matrix matrix)
        {
            foreach(Triangle t in Triangles)
            {
                t.V1 = matrix.MulPosition(t.V1);
                t.V2 = matrix.MulPosition(t.V2);
                t.V3 = matrix.MulPosition(t.V3);
                t.N1 = matrix.MulDirection(t.N1);
                t.N2 = matrix.MulDirection(t.N2);
                t.N3 = matrix.MulDirection(t.N3);
            }
            dirty();
        }
        
        void FitInside(Box box, Vector anchor)
        {
            double scale = box.Size().Div(this.GetBoundingBox().Size()).MinComponent();
            Vector extra = box.Size().Sub(GetBoundingBox().Size().MulScalar(scale));
            Matrix matrix = new Matrix();
            matrix = matrix.Identity();
            matrix = matrix.Translate(GetBoundingBox().Min.Negate());
            matrix = matrix.Scale(new Vector(scale, scale, scale));
            matrix = matrix.Translate(box.Min.Add(extra.Mul(anchor)));
            Transform(matrix);
        }
        
        void SetMaterial(Material material)
        {
            foreach(Triangle t in Triangles)
            {
                t.TriangleMaterial = material;
            }
        }
    }
}
