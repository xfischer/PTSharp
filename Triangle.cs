using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    struct UVW
    {
        public double u;
        public double v;
        public double w;
    };

    struct TriVec
    {
        public Vector v1;
        public Vector v2;
        public Vector v3;
    };

    class Triangle : IShape
    {
        
        static double EPS = 1e-9;
        internal Material TriangleMaterial;
        public Vector V1, V2, V3;
        public Vector N1, N2, N3;
        public Vector T1, T2, T3;

        public Triangle(Vector v1, Vector v2, Vector v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public Triangle (Vector v1, Vector v2, Vector v3, Material triangleMaterial)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            TriangleMaterial = triangleMaterial;
        }

        public Triangle(Material triangleMaterial, Vector v1, Vector v2, Vector v3, Vector t1, Vector t2, Vector t3)
        {
            TriangleMaterial = triangleMaterial;
            V1 = v1;
            V2 = v2;
            V3 = v3;
            T1 = t1;
            T2 = t2;
            T3 = t3;
        }

        internal static Triangle NewTriangle(Vector v1, Vector v2, Vector v3, Vector t1, Vector t2, Vector t3, Material material)
        {
            Triangle t = new Triangle(material, v1, v2, v3, t1, t2, t3);
            t.FixNormals();
            return t;
        }

        TriVec Vertices()
        {
            TriVec t = new TriVec();
            t.v1 = V1;
            t.v2 = V2;
            t.v3 = V3;
            return t;
        }

        public void Compile() { }

        Box IShape.GetBoundingBox()
        {
            Vector min = this.V1.Min(this.V2).Min(this.V3);
            Vector max = this.V1.Max(this.V2).Max(this.V3);
            return new Box(min, max);
        }

        Vector IShape.NormalAt(Vector p)
        {
            var uvw = Barycentric(p);
            var n = new Vector();

            n = n.Add(N1.MulScalar(uvw.u));
            n = n.Add(N2.MulScalar(uvw.v));
            n = n.Add(N3.MulScalar(uvw.w));
            n = n.Normalize();

            if (TriangleMaterial.NormalTexture != null)
            {
                var b = new Vector();
                b = b.Add(T1.MulScalar(uvw.u));
                b = b.Add(T2.MulScalar(uvw.v));
                b = b.Add(T3.MulScalar(uvw.w));
                var ns = TriangleMaterial.NormalTexture.NormalSample(b.X, b.Y);
                var dv1 = V2.Sub(V1);
                var dv2 = V3.Sub(V1);
                var dt1 = T2.Sub(T1);
                var dt2 = T3.Sub(T1);
                var T = dv1.MulScalar(dt2.Y).Sub(dv2.MulScalar(dt1.Y)).Normalize();
                var B = dv2.MulScalar(dt1.X).Sub(dv1.MulScalar(dt2.X)).Normalize();
                var N = T.Cross(B);

                var matrix = new Matrix(T.X, B.X, N.X, 0,
                                        T.Y, B.Y, N.Y, 0,
                                        T.Z, B.Z, N.Z, 0,
                                         0, 0, 0, 1);
                n = matrix.MulDirection(ns);
            }
            if (TriangleMaterial.BumpTexture != null)
            {
                var b = new Vector();

                b = b.Add(T1.MulScalar(uvw.u));
                b = b.Add(T2.MulScalar(uvw.v));
                b = b.Add(T3.MulScalar(uvw.w));

                var bump = TriangleMaterial.BumpTexture.BumpSample(b.X, b.Y);

                var dv1 = V2.Sub(V1);
                var dv2 = V3.Sub(V1);
                var dt1 = T2.Sub(T1);
                var dt2 = T3.Sub(T1);

                var tangent = dv1.MulScalar(dt2.Y).Sub(dv2.MulScalar(dt1.Y)).Normalize();
                var bitangent = dv2.MulScalar(dt1.X).Sub(dv1.MulScalar(dt2.X)).Normalize();

                n = n.Add(tangent.MulScalar(bump.X * TriangleMaterial.BumpMultiplier));
                n = n.Add(bitangent.MulScalar(bump.Y * TriangleMaterial.BumpMultiplier));

            }
            n = n.Normalize();
            return n;
        }
               
        Hit IShape.Intersect(Ray r)
        {
            var e1x = V2.X - V1.X;
            var e1y = V2.Y - V1.Y;
            var e1z = V2.Z - V1.Z;

            var e2x = V3.X - V1.X;
            var e2y = V3.Y - V1.Y;
            var e2z = V3.Z - V1.Y;

            var px = r.Direction.Y * e2z - r.Direction.Z * e2y;
            var py = r.Direction.Z * e2x - r.Direction.X * e2z;
            var pz = r.Direction.X * e2y - r.Direction.Y * e2z;
                       
            double det = e1x * px + e1y * py + e1z * pz;
            if (det > -EPS && det < EPS)
            {
                return Hit.NoHit;
            }
            double inv = 1 / det;
            double tx = r.Origin.X - V1.X;
            double ty = r.Origin.Y - V1.Y;
            double tz = r.Origin.Z - V1.Z;
            double u = (tx * px + ty * py + tz * pz) * inv;
            if (u < 0 || u > 1)
            {
                return Hit.NoHit;
            }
            double qx = ty * e1z - tz * e1y;
            double qy = tz * e1x - tx * e1z;
            double qz = tx * e1y - ty * e1x;
            double v = (r.Direction.X * qx + r.Direction.Y * qy + r.Direction.Z * qz) * inv;
            if (v < 0 || u + v > 1)
            {
                return Hit.NoHit;
            }
            double d = (e2x * qx + e2y * qy + e2z * qz) * inv;
            if (d < EPS)
            {
                return Hit.NoHit;
            }
            return new Hit(this, d, null);
        }

        Vector IShape.UV(Vector p)
        {
            var bcentric = Barycentric(p);
            var n = new Vector();
            n = n.Add(T1.MulScalar(bcentric.u));
            n = n.Add(T2.MulScalar(bcentric.v));
            n = n.Add(T3.MulScalar(bcentric.w));
            return new Vector(n.X, n.Y, 0);
        }
        
        double Area() {
            var e1 = V2.Sub(V1);
            var e2 = V3.Sub(V1);
            var n = e1.Cross(e2);
            return n.Length() / 2;
        }
        
        Vector Normal()
        {
            var e1 = V2.Sub(V1);
            var e2 = V3.Sub(V1);
            return e1.Cross(e2).Normalize();
        }
        UVW Barycentric(Vector p)
        {
            var v0 = V2.Sub(V1);
            var v1 = V3.Sub(V1);
            var v2 = p.Sub(V1);
            var d00 = v0.Dot(v0);
            var d01 = v0.Dot(v1);
            var d11 = v1.Dot(v1);
            var d20 = v2.Dot(v0);
            var d21 = v2.Dot(v1);
            var d = d00 * d11 - d01 * d01;
            var v = (d11 * d20 - d01 * d21) / d;
            var w = (d00 * d21 - d01 * d20) / d;
            var u = 1 - v - w;
            UVW uvw;
            uvw.u = u;
            uvw.v = v;
            uvw.w = w;
            return uvw;
        }
        public void FixNormals()
        {
            Vector n = this.Normal();
            Vector zero = new Vector();
            if (N1.Equals(zero))
            {
                N1 = n;
            }
            if (N2.Equals(zero))
            {
                N2 = n;
            }
            if (N3.Equals(zero))
            {
                N3 = n;
            }
        }
        
        Material IShape.MaterialAt(Vector v) => TriangleMaterial;
    }
}
