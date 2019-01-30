using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PTSharp
{
    struct Matrix
    {
        public double x00, x01, x02, x03;
        public double x10, x11, x12, x13;
        public double x20, x21, x22, x23;
        public double x30, x31, x32, x33;

        public Matrix(double x00_, double x01_, double x02_, double x03_,
                      double x10_, double x11_, double x12_, double x13_,
                      double x20_, double x21_, double x22_, double x23_,
                      double x30_, double x31_, double x32_, double x33_)
        {
            x00 = x00_;
            x01 = x01_;
            x02 = x02_;
            x03 = x03_;
            x10 = x10_;
            x11 = x11_;
            x12 = x12_;
            x13 = x13_;
            x20 = x20_;
            x21 = x21_;
            x22 = x22_;
            x23 = x23_;
            x30 = x30_;
            x31 = x31_;
            x32 = x32_;
            x33 = x33_;
        }

        public Matrix Identity()
        {
            return new Matrix(1, 0, 0, 0,
                              0, 1, 0, 0,
                              0, 0, 1, 0,
                              0, 0, 0, 1);
        }
        public Matrix Translate(Vector v)
        {
            return new Matrix(1, 0, 0, v.X,
                              0, 1, 0, v.Y,
                              0, 0, 1, v.Z,
                              0, 0, 0, 1);
        }

        public Matrix Scale(Vector v)
        {
            return new Matrix(v.X, 0, 0, 0,
                              0, v.Y, 0, 0,
                              0, 0, v.Z, 0,
                              0, 0, 0, 1);
        }

        public Matrix Rotate(Vector v, double a)
        {
            v = v.Normalize();
            var s = Math.Sin(a);
            var c = Math.Cos(a);
            var m = 1 - c;

            return new Matrix(m * v.X * v.X + c, m * v.X * v.Y + v.Z * s, m * v.Z * v.X - v.Y * s, 0,
                              m * v.X * v.Y - v.Z * s, m * v.Y * v.Y + c, m * v.Y * v.Z + v.X * s, 0,
                              m * v.Z * v.X + v.Y * s, m * v.Y * v.Z - v.X * s, m * v.Z * v.Z + c, 0,
                              0, 0, 0, 1);
            
        }
        public Matrix Frustum(double l, double r, double b, double t, double n, double f)
        {
            double t1 = 2 * n;
            double t2 = r - l;
            double t3 = t - b;
            double t4 = f - n;

            return new Matrix(t1 / t2, 0, (r + l) / t2, 0,
                              0, t1 / t3, (t + b) / t3, 0,
                              0, 0, (-f - n) / t4, (-t1 * f) / t4,
                              0, 0, -1, 0);
        }
        public Matrix Orthographic(double l, double r, double b, double t, double n, double f)
        {
            return new Matrix(2 / (r - l), 0, 0, -(r + l) / (r - l),
                              0, 2 / (t - b), 0, -(t + b) / (t - b),
                              0, 0, -2 / (f - n), -(f + n) / (f - n),
                              0, 0, 0, 1);
        }

        public Matrix Perspective(double fovy, double aspect, double near, double far)
        {
            double ymax = near * Math.Tan(fovy * Math.PI / 360);
            double xmax = ymax * aspect;
            return Frustum(-xmax, xmax, -ymax, ymax, near, far);
        }
        
        public static Matrix LookAtMatrix(Vector eye, Vector center, Vector up)
        {
            up = up.Normalize();
            var f = center.Sub(eye).Normalize();
            var s = f.Cross(up).Normalize();
            var u = s.Cross(f);
            var m = new Matrix(s.X, u.X, f.X, 0,
                               s.Y, u.Y, f.Y, 0,
                               s.Z, u.Z, f.Z, 0,
                               0, 0, 0, 1);

            return m.Transpose().Inverse().Translate(m, eye);
        }

        public Matrix Translate(Matrix m, Vector v)
        {
            return Translate(v).Mul(m);
        }

        public Matrix Scale(Matrix m, Vector v)
        {
            return Scale(v).Mul(m);
        }

        public Matrix Rotate(Matrix m, Vector v, double a)
        {
            return Rotate(v,a).Mul(m);
        }
               
        public Matrix Mul(Matrix b)
        {
            Matrix m = new Matrix();
            m.x00 = x00 * b.x00 + x01 * b.x10 + x02 * b.x20 + x03 * b.x30;
            m.x10 = x10 * b.x00 + x11 * b.x10 + x12 * b.x20 + x13 * b.x30;
            m.x20 = x20 * b.x00 + x21 * b.x10 + x22 * b.x20 + x23 * b.x30;
            m.x30 = x30 * b.x00 + x31 * b.x10 + x32 * b.x20 + x33 * b.x30;
            m.x01 = x00 * b.x01 + x01 * b.x11 + x02 * b.x21 + x03 * b.x31;
            m.x11 = x10 * b.x01 + x11 * b.x11 + x12 * b.x21 + x13 * b.x31;
            m.x21 = x20 * b.x01 + x21 * b.x11 + x22 * b.x21 + x23 * b.x31;
            m.x31 = x30 * b.x01 + x31 * b.x11 + x32 * b.x21 + x33 * b.x31;
            m.x02 = x00 * b.x02 + x01 * b.x12 + x02 * b.x22 + x03 * b.x32;
            m.x12 = x10 * b.x02 + x11 * b.x12 + x12 * b.x22 + x13 * b.x32;
            m.x22 = x20 * b.x02 + x21 * b.x12 + x22 * b.x22 + x23 * b.x32;
            m.x32 = x30 * b.x02 + x31 * b.x12 + x32 * b.x22 + x33 * b.x32;
            m.x03 = x00 * b.x03 + x01 * b.x13 + x02 * b.x23 + x03 * b.x33;
            m.x13 = x10 * b.x03 + x11 * b.x13 + x12 * b.x23 + x13 * b.x33;
            m.x23 = x20 * b.x03 + x21 * b.x13 + x22 * b.x23 + x23 * b.x33;
            m.x33 = x30 * b.x03 + x31 * b.x13 + x32 * b.x23 + x33 * b.x33;
            return m;
        }
               
        public Vector MulPosition(Vector b)
        {
            double x = x00 * b.X + x01 * b.Y + x02 * b.Z + x03;
            double y = x10 * b.X + x11 * b.Y + x12 * b.Z + x13;
            double z = x20 * b.X + x21 * b.Y + x22 * b.Z + x23;

            return new Vector(x, y, z);
        }

        public Vector MulDirection(Vector b)
        {
            double x = x00 * b.X + x01 * b.Y + x02 * b.Z;
            double y = x10 * b.X + x11 * b.Y + x12 * b.Z;
            double z = x20 * b.X + x21 * b.Y + x22 * b.Z;
            return new Vector(x, y, z).Normalize();
        }

        public Ray MulRay(Ray b)
        {
            return new Ray(MulPosition(b.Origin), MulDirection(b.Direction));
        }

        public Box MulBox(Box box)
        {
            Vector r = new Vector(x00, x10, x20);
            Vector u = new Vector(x01, x11, x21);
            Vector b = new Vector(x02, x12, x22);
            Vector t = new Vector(x03, x13, x23);
            Vector xa = r.MulScalar(box.Min.X);
            Vector xb = r.MulScalar(box.Max.X);
            Vector ya = u.MulScalar(box.Min.Y);
            Vector yb = u.MulScalar(box.Max.Y);
            Vector za = b.MulScalar(box.Min.Z);
            Vector zb = b.MulScalar(box.Max.Z);
            xa = xa.Min(xb);
            xb = xa.Max(xb);
            ya = ya.Min(yb);
            yb = ya.Max(yb);
            za = za.Min(zb);
            zb = za.Max(zb);
            var min = xa.Add(ya).Add(za).Add(t);
            var max = xb.Add(yb).Add(zb).Add(t);
            return new Box(min, max);
        }

        public Matrix Transpose()
        {
            return new Matrix(x00, x10, x20, x30,
                              x01, x11, x21, x31,
                              x02, x12, x22, x32,
                              x03, x13, x23, x33);
        }

        public double Determinant()
        {
            return (x00 * x11 * x22 * x33 - x00 * x11 * x23 * x32 +
                    x00 * x12 * x23 * x31 - x00 * x12 * x21 * x33 +
                    x00 * x13 * x21 * x32 - x00 * x13 * x22 * x31 -
                    x01 * x12 * x23 * x30 + x01 * x12 * x20 * x33 -
                    x01 * x13 * x20 * x32 + x01 * x13 * x22 * x30 -
                    x01 * x10 * x22 * x33 + x01 * x10 * x23 * x32 +
                    x02 * x13 * x20 * x31 - x02 * x13 * x21 * x30 +
                    x02 * x10 * x21 * x33 - x02 * x10 * x23 * x31 +
                    x02 * x11 * x23 * x30 - x02 * x11 * x20 * x33 -
                    x03 * x10 * x21 * x32 + x03 * x10 * x22 * x31 -
                    x03 * x11 * x22 * x30 + x03 * x11 * x20 * x32 -
                    x03 * x12 * x20 * x31 + x03 * x12 * x21 * x30);
        }

        public Matrix Inverse()
        {
            Matrix m = new Matrix();
            double d = Determinant();
            m.x00 = (x12 * x23 * x31 - x13 * x22 * x31 + x13 * x21 * x32 - x11 * x23 * x32 - x12 * x21 * x33 + x11 * x22 * x33) / d;
            m.x01 = (x03 * x22 * x31 - x02 * x23 * x31 - x03 * x21 * x32 + x01 * x23 * x32 + x02 * x21 * x33 - x01 * x22 * x33) / d;
            m.x02 = (x02 * x13 * x31 - x03 * x12 * x31 + x03 * x11 * x32 - x01 * x13 * x32 - x02 * x11 * x33 + x01 * x12 * x33) / d;
            m.x03 = (x03 * x12 * x21 - x02 * x13 * x21 - x03 * x11 * x22 + x01 * x13 * x22 + x02 * x11 * x23 - x01 * x12 * x23) / d;
            m.x10 = (x13 * x22 * x30 - x12 * x23 * x30 - x13 * x20 * x32 + x10 * x23 * x32 + x12 * x20 * x33 - x10 * x22 * x33) / d;
            m.x11 = (x02 * x23 * x30 - x03 * x22 * x30 + x03 * x20 * x32 - x00 * x23 * x32 - x02 * x20 * x33 + x00 * x22 * x33) / d;
            m.x12 = (x03 * x12 * x30 - x02 * x13 * x30 - x03 * x10 * x32 + x00 * x13 * x32 + x02 * x10 * x33 - x00 * x12 * x33) / d;
            m.x13 = (x02 * x13 * x20 - x03 * x12 * x20 + x03 * x10 * x22 - x00 * x13 * x22 - x02 * x10 * x23 + x00 * x12 * x23) / d;
            m.x20 = (x11 * x23 * x30 - x03 * x21 * x30 + x13 * x20 * x31 - x10 * x23 * x31 - x11 * x20 * x33 + x10 * x21 * x33) / d;
            m.x21 = (x03 * x21 * x30 - x01 * x23 * x30 - x03 * x20 * x31 + x00 * x23 * x31 + x01 * x20 * x33 - x00 * x21 * x33) / d;
            m.x22 = (x01 * x13 * x30 - x03 * x11 * x30 + x03 * x10 * x31 - x00 * x13 * x31 - x01 * x10 * x33 + x00 * x11 * x33) / d;
            m.x23 = (x03 * x11 * x20 - x01 * x13 * x20 - x03 * x10 * x21 + x00 * x13 * x21 + x01 * x10 * x23 - x00 * x11 * x23) / d;
            m.x30 = (x12 * x21 * x30 - x11 * x22 * x30 - x12 * x20 * x31 + x10 * x22 * x31 + x11 * x20 * x32 - x10 * x21 * x32) / d;
            m.x31 = (x01 * x22 * x30 - x02 * x21 * x30 + x02 * x20 * x31 - x00 * x22 * x31 - x01 * x20 * x32 + x00 * x21 * x32) / d;
            m.x32 = (x02 * x11 * x30 - x01 * x12 * x30 - x02 * x10 * x31 + x00 * x12 * x31 + x01 * x10 * x32 - x00 * x11 * x32) / d;
            m.x33 = (x01 * x12 * x20 - x02 * x11 * x20 + x02 * x10 * x21 - x00 * x12 * x21 - x01 * x10 * x22 + x00 * x11 * x22) / d;
            return m;
        }
    }
};
