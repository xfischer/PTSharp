using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    struct Vector
    {
        public static Vector ORIGIN = new Vector(0, 0, 0);
        public double X, Y, Z;//, W;
        public int Index { get; set; }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Index = 0;
        }
        public static Vector RandomUnitVector(Random rand)
        {
            for (;;)
            {
                double x, y, z;

                if (rand.Equals(null))
                {
                    x = new Random().NextDouble() * 2 - 1;
                    y = new Random().NextDouble() * 2 - 1;
                    z = new Random().NextDouble() * 2 - 1;
                }
                else
                {
                    x = rand.NextDouble() * 2 - 1;
                    y = rand.NextDouble() * 2 - 1;
                    z = rand.NextDouble() * 2 - 1;
                }
                if (x * x + y * y + z * z > 1)
                {
                    continue;
                }
                return new Vector(x, y, z).Normalize();
            }
        }

        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

        public double LengthN(double n)
        {
            if (n == 2)
            {
                return Length();
            }
            var a = Abs();
            return Math.Pow(Math.Pow(a.X, n) + Math.Pow(a.Y, n) + Math.Pow(a.Z, n), 1 / n);
        }

        public double Dot(Vector b)
        {
            return X * b.X + Y * b.Y + Z * b.Z;
        }

        public Vector Cross(Vector b)
        {
            return new Vector(Y * b.Z - Z * b.Y,
                              Z * b.X - X * b.Z,
                              X * b.Y - Y * b.X);
        }

        public Vector Normalize()
        {
            var d = Length();
            return new Vector(X / d, Y / d, Z / d);
        }

        public Vector Negate()
        {
            return new Vector(-X, -Y, -Z);
        }

        Vector Abs()
        {
            return new Vector(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public Vector Add(Vector b) => new Vector(X + b.X, Y + b.Y, Z + b.Z);

        public Vector Sub(Vector b) => new Vector(X - b.X, Y - b.Y, Z - b.Z);

        public Vector Mul(Vector b) => new Vector(X * b.X, Y * b.Y, Z * b.Z);

        public Vector Div(Vector b) => new Vector(X / b.X, Y / b.Y, Z / b.Z);

        public Vector Mod(Vector b) => new Vector(X - (b.X * Math.Floor(X / b.X)),
                                                  Y - (b.Y * Math.Floor(Y / b.Y)),
                                                  Z - (b.Z * Math.Floor(Z / b.Z)));

        public Vector AddScalar(double b) => new Vector(X + b, Y + b, Z + b);

        public Vector SubScalar(double b) => new Vector(X - b, Y - b, Z - b);

        public Vector MulScalar(double b) => new Vector(X * b, Y * b, Z * b);

        public Vector DivScalar(double b) => new Vector(X / b, Y / b, Z / b);

        public Vector Min(Vector b)
        {
            return new Vector(Math.Min(X, b.X), Math.Min(Y, b.Y), Math.Min(Z, b.Z));
        }

        public Vector Max(Vector b) => new Vector(Math.Max(X, b.X), Math.Max(Y, b.Y), Math.Max(Z, b.Z));

        public Vector MinAxis()
        {
            var x = Math.Abs(this.X);
            var y = Math.Abs(this.Y);
            var z = Math.Abs(this.Z);

            if (x <= y && x <= z)
            {
                return new Vector(1, 0, 0);
            }
            else if (y <= x && y <= z)
            {
                return new Vector(0, 1, 0);
            }
            return new Vector(0, 0, 1);
        }

        public double MinComponent() => Math.Min(Math.Min(X, Y), Z);

        public double MaxComponent() => Math.Max(Math.Max(X, Y), Z);

        public Vector Reflect(Vector i) => i.Sub(MulScalar(2 * Dot(i)));

        public Vector Refract(Vector i, double n1, double n2)
        {
            var nr = n1 / n2;
            var cosI = -Dot(i);
            var sinT2 = nr * nr * (1 - cosI * cosI);

            if(sinT2 > 1)
            {
                return new Vector();
            }
            var cosT = Math.Sqrt(1 - sinT2);

            return i.MulScalar(nr).Add(MulScalar(nr * cosI - cosT));
        }

        public double Reflectance(Vector i, double n1, double n2)
        {
            var nr = n1 / n2;
            var cosI = -Dot(i);
            var sinT2 = nr * nr * (1 - cosI * cosI);

            if(sinT2 > 1)
            {
                return 1;
            }
            var cosT = Math.Sqrt(1 - sinT2);
            var rOrth = (n1 * cosI - n2 * cosT) / (n1 * cosI + n2 * cosT);
            var rPar = (n2 * cosI - n1 * cosT) / (n2 * cosI + n1 * cosT);
            return (rOrth * rOrth + rPar * rPar) / 2;
        }
    };
}
