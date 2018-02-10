using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Triangle : Shape {
        
        static double EPS = 1e-9;
        public Material TriangleMaterial;
        public Vector V1, V2, V3;
        public Vector N1, N2, N3;
        public Vector T1, T2, T3;

        public static Triangle NewTriangle(Vector v1, Vector v2, Vector v3, Vector t1, Vector t2, Vector t3, Material material) {
            Triangle t = new Triangle();
            t.V1 = v1;
            t.V2 = v2;
            t.V3 = v3;
            t.T1 = t1;
            t.T2 = t2;
            t.T3 = t3;
            t.TriangleMaterial = material;
            t.FixNormals();
            return t;
        }

        Vector[] Vertices()
        {
            Vector[] ve = { this.V1, this.V2, this.V3 };
            return ve;
        }

        public void Compile()
        {
            
        }

        public Box BoundingBox()
        {
            Vector min = this.V1.Min(this.V2).Min(this.V3);
            Vector max = this.V1.Max(this.V2).Max(this.V3);
            return new Box(min, max);
        }

        Vector Normal()
        {
            Vector e1 = this.V2.Sub(this.V1);
            Vector e2 = this.V3.Sub(this.V1);
            return e1.Cross(e2).Normalize();
        }

        public void FixNormals() {
            Vector n = this.Normal();

            Vector zero = null;

            if (this.N1 == zero) {
                this.N1 = n;
            }
            if (this.N2 == zero) {
                this.N2 = n;
            }
            if (this.N3 == zero) {
                this.N3 = n;
            }
        }
        
        public Hit Intersect(Ray r)
        {
            double e1x = this.V2.X - this.V1.X;
            double e1y = this.V2.Y - this.V1.Y;
            double e1z = this.V2.Z - this.V1.Z;
            double e2x = this.V3.X - this.V1.X;
            double e2y = this.V3.Y - this.V1.Y;
            double e2z = this.V3.Z - this.V1.Y;
            double px = r.Direction.Y * e2z - r.Direction.Z * e2y;
            double py = r.Direction.Z * e2x - r.Direction.X * e2z;
            double pz = r.Direction.X * e2y - r.Direction.Y * e2z;
            double det = e1x * px + e1y * py + e1z * pz;
            if (det > -EPS && det < EPS)
            {
                return Hit.NoHit;
            }
            double inv = 1 / det;
            double tx = r.Origin.X - this.V1.X;
            double ty = r.Origin.Y - this.V1.Y;
            double tz = r.Origin.Z - this.V1.Z;
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
        
        public Vector UV(Vector p) {
            double[] centric = this.Barycentric(p);
            Vector n = new Vector();
            
            // Small check to see if there are no texture coords
            if(T1 == null || T2 == null || T3 == null)
            {
                n = n.Add(new Vector().MulScalar(centric[0]));
                n = n.Add(new Vector().MulScalar(centric[1]));
                n = n.Add(new Vector().MulScalar(centric[2]));
            } else
            {
                n = n.Add(this.T1.MulScalar(centric[0]));
                n = n.Add(this.T2.MulScalar(centric[1]));
                n = n.Add(this.T3.MulScalar(centric[2]));
            }
            return new Vector(n.X, n.Y, 0);
        }
        
        double[] Barycentric(Vector p)  {
            Vector v0 = this.V2.Sub(this.V1);
            Vector v1 = this.V3.Sub(this.V1);
            Vector v2 = p.Sub(this.V1);
            double d00 = v0.Dot(v0);
            double d01 = v0.Dot(v1);
            double d11 = v1.Dot(v1);
            double d20 = v2.Dot(v0);
            double d21 = v2.Dot(v1);
            double d = d00 * d11 - d01 * d01;
            double v = (d11 * d20 - d01 * d21) / d;
            double w = (d00 * d21 - d01 * d20) / d;
            double u = 1 - v - w;
            double[] ret = { u, v, w };
            return ret;
        }

        public Material MaterialAt(Vector p) {
            return this.TriangleMaterial;
        }
        
        double Area(Triangle t) {
            Vector e1 = t.V2.Sub(t.V1);
            Vector e2 = t.V3.Sub(t.V1);
            Vector n = e1.Cross(e2);
            return n.Length() / 2;
        }
        
        public Vector NormalAt(Vector p)
        {
            double[] bcentric = this.Barycentric(p);
            double u = bcentric[0];
            double v = bcentric[1];
            double w = bcentric[2];
            Vector n = new Vector(0,0,0);
            n = n.Add(this.N1.MulScalar(bcentric[0]));
            n = n.Add(this.N2.MulScalar(bcentric[1]));
            n = n.Add(this.N3.MulScalar(bcentric[2]));
            n = n.Normalize();
            if (this.TriangleMaterial.NormalTexture != null) {
                Vector b = new Vector();
                b = b.Add(this.T1.MulScalar(u));
                b = b.Add(this.T2.MulScalar(v));
                b = b.Add(this.T3.MulScalar(w));
                Vector ns = this.TriangleMaterial.NormalTexture.NormalSample(b.X, b.Y);
                Vector dv1 = this.V2.Sub(this.V1);
                Vector dv2 = this.V3.Sub(this.V1);
                Vector dt1 = this.T2.Sub(this.T1);
                Vector dt2 = this.T3.Sub(this.T1);
                Vector T = dv1.MulScalar(dt2.Y).Sub(dv2.MulScalar(dt1.Y)).Normalize();
                Vector B = dv2.MulScalar(dt1.X).Sub(dv1.MulScalar(dt2.X)).Normalize();
                Vector N = T.Cross(B);
                Matrix matrix = new Matrix(T.X, B.X, N.X, 0,
                                           T.Y, B.Y, N.Y, 0,
                                           T.Z, B.Z, N.Z, 0,
                                           0, 0, 0, 1);
                matrix.MulDirection(ns);
            }
            if (this.TriangleMaterial.BumpTexture != null)
            {
                Vector b = new Vector();
                b = b.Add(this.T1.MulScalar(u));
                b = b.Add(this.T2.MulScalar(v));
                b = b.Add(this.T3.MulScalar(w));
                Vector bump = this.TriangleMaterial.BumpTexture.BumpSample(b.X, b.Y);
                Vector dv1 = this.V2.Sub(this.V1);
                Vector dv2 = this.V3.Sub(this.V1);
                Vector dt1 = this.T2.Sub(this.T1);
                Vector dt2 = this.T3.Sub(this.T1);
                Vector tangent = dv1.MulScalar(dt2.Y).Sub(dv2.MulScalar(dt1.Y)).Normalize();
                Vector bitangent = dv2.MulScalar(dt1.X).Sub(dv1.MulScalar(dt2.X)).Normalize();

                n = n.Add(tangent.MulScalar(bump.X * this.TriangleMaterial.BumpMultiplier));
                n = n.Add(bitangent.MulScalar(bump.Y * this.TriangleMaterial.BumpMultiplier));

            }
            n.Normalize();
            return n;
        }
    }
}
