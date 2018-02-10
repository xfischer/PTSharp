using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Vector
    {
        public static Vector ORIGIN = new Vector(0, 0, 0);

        public double X, Y, Z;

        public Vector() {}

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public static Vector RandomUnitVector(Random rnd)
        {
            for (;;)
            {
                double x, y, z;

                if (rnd == null)
                {
                    x = new Random().NextDouble() * 2 - 1;
                    y = new Random().NextDouble() * 2 - 1;
                    z = new Random().NextDouble() * 2 - 1;
                }
                else
                {
                    x = rnd.NextDouble() * 2 - 1;
                    y = rnd.NextDouble() * 2 - 1;
                    z = rnd.NextDouble() * 2 - 1;
                }
                if (x * x + y * y + z * z > 1)
                {
                    continue;
                }
                return new Vector(x, y, z).Normalize();
            }
        }
        
        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }
        
        public double LengthN(double n)
        {
            if (n == 2)
            {
                return this.Length();
            }
            Vector a = this.Abs();
            return Math.Pow(Math.Pow(a.X, n) + Math.Pow(a.Y, n) + Math.Pow(a.Z, n), 1 / n);
        }
        
        public double Dot(Vector b)
        {
            return this.X * b.X + this.Y * b.Y + this.Z * b.Z;
        }

        public Vector Cross(Vector b)
        {
            return new Vector(this.Y * b.Z - this.Z * b.Y,  
                              this.Z * b.X - this.X * b.Z, 
                              this.X * b.Y - this.Y * b.X);
        }

        public Vector Normalize()
        {
            double d = this.Length();
            return new Vector(this.X / d, this.Y / d, this.Z / d);
        }

        public Vector Negate()
        {
            return new Vector(-this.X, -this.Y, -this.Z);
        }

        Vector Abs()
        {
            return new Vector(Math.Abs(this.X), Math.Abs(this.Y), Math.Abs(this.Z));
        }

        public Vector Add(Vector b)
        {
            return new Vector(this.X + b.X, this.Y + b.Y, this.Z + b.Z);
        }

        public Vector Sub(Vector b)
        {
            return new Vector(this.X - b.X, this.Y - b.Y, this.Z - b.Z);
        }

        public Vector Mul(Vector b)
        {
            return new Vector(this.X * b.X, this.Y * b.Y, this.Z * b.Z);
        }

        public Vector Div(Vector b)
        {
            return new Vector(this.X / b.X, this.Y / b.Y, this.Z / b.Z);
        }

        public Vector Mod(Vector b)
        {
            return new Vector(this.X - b.X * Math.Floor(this.X / b.X), 
                              this.Y - b.Y * Math.Floor(this.Y / b.Y), 
                              this.Z - b.Z * Math.Floor(this.Z / b.Z));
        }
        
        public Vector AddScalar(double b)
        {
            return new Vector(this.X + b, this.Y + b, this.Z + b);
        }

        public Vector SubScalar(double b)
        {
            return new Vector(this.X - b, this.Y - b, this.Z - b);
        }

        public Vector MulScalar(double b)
        {
            return new Vector(this.X * b, this.Y * b, this.Z * b);
        }

        public Vector DivScalar(double b)
        {
            return new Vector(this.X / b, this.Y / b, this.Z / b);
        }

        public Vector Min(Vector b)
        {
            return new Vector(Math.Min(this.X, b.X), Math.Min(this.Y, b.Y), Math.Min(this.Z, b.Z));
        }

        public Vector Max(Vector b)
        {
            return new Vector(Math.Max(this.X, b.X), Math.Max(this.Y, b.Y), Math.Max(this.Z, b.Z));
        }

        public Vector MinAxis()
        {
            double x, y, z;
            x = Math.Abs(this.X);
            y = Math.Abs(this.Y);
            z = Math.Abs(this.Z);

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

        public double MinComponent()
        {
            return Math.Min(Math.Min(this.X, this.Y), this.Z);
        }

        public double MaxComponent()
        {
            return Math.Max(Math.Max(this.X, this.Y), this.Z);
        }

        public Vector Reflect(Vector i)
        {
            return i.Sub(this.MulScalar(2 * this.Dot(i)));
        }

        public Vector Refract(Vector i, double n1, double n2)
        {
            double nr = n1 / n2;
            double cosI = -this.Dot(i);
            double sinT2 = nr * nr * (1 - cosI * cosI);
            if (sinT2 > 1)
            {
                return new Vector();
            }
            double cosT = Math.Sqrt(1 - sinT2);
            return i.MulScalar(nr).Add(this.MulScalar(nr * cosI - cosT));
        }

        public double Reflectance(Vector i, double n1, double n2)
        {
            double nr = n1 / n2;
            double cosI = -this.Dot(i);
            double sinT2 = nr * nr * (1 - cosI * cosI);
            if (sinT2 > 1)
            {
                return 1;
            }
            double cosT = Math.Sqrt(1 - sinT2);
            double rOrth = (n1 * cosI - n2 * cosT) / (n1 * cosI + n2 * cosT);
            double rPar = (n2 * cosI - n1 * cosT) / (n2 * cosI + n1 * cosT);
            return (rOrth * rOrth + rPar * rPar) / 2;
        }
    }
}
