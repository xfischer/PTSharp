using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    interface SDF 
    {
        double Evaluate(Vector p);
        Box GetBoundingBox();
    }

    class SDFShape : SDF
    {
        SDF sdf;
        Material material;

        public Box GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        SDFShape(SDF sdf, Material material)
        {
            this.sdf = sdf;
            this.material = material;
        }

        SDF NewSDFShape(SDF sdf, Material material)
        {
            return new SDFShape(sdf, material);
        }
                

        public void Compile() { }
       
        Hit Intersect(Ray ray)
        {
            double epsilon = 0.0001;
            double start = 0.0001;
            double jumpSize = 0.001;
            Box box = this.GetBoundingBox();
            double[] t_ = box.Intersect(ray);
            double t1 = t_[0];
            double t2 = t_[1];

            if (t2 < t1 || t2 < 0)
            {
                return Hit.NoHit;
            }

            double t = Math.Max(start, t1);
            bool jump = true;

            for (int i = 0; i < 1000; i++)
            {
                double d = this.Evaluate(ray.Position(t));
                if(jump && d < 0)
                {
                    t -= jumpSize;
                    jump = false;
                    continue;
                }

                if ( d < epsilon)
                {
                    return new Hit((IShape)this, t, null);
                }

                if (jump && d < jumpSize)
                {
                    d = jumpSize;
        
                }

                t += d;

                if( t > t2)
                {
                    return Hit.NoHit;

                }
            }
            return Hit.NoHit;
        }

        public Material MaterialAt(Vector v)
        {
            return this.material;
        }

        public Vector NormalAt(Vector p)
        {
            double e = 0.0001;
            double x = p.X;
            double y = p.Y;
            double z = p.Z;

            return new Vector(this.Evaluate(new Vector(x - e, y, z)) - this.Evaluate(new Vector(x + e, y, z)),
                              this.Evaluate(new Vector(x, y - e, z)) - this.Evaluate(new Vector(x, y + e, z)),
                              this.Evaluate(new Vector(x, y, z - e)) - this.Evaluate(new Vector(x, y, z + e)));
        }

        public Vector UV(Vector uv)
        {
            return new Vector();
        }

        public double Evaluate(Vector p)
        {
            return sdf.Evaluate(p);
        }
    }

    class SphereSDF : SDF
    {
        double Radius;
        double Exponent;

        SphereSDF(double Radius, double Exponent)
        {
            this.Radius = Radius;
            this.Exponent = Exponent;
        }
        
        public double Evaluate(Vector p)
        {
            return p.LengthN(this.Exponent) - this.Radius;
        }
        
        SDF NewSphereSDF(double radius)
        {
            return new SphereSDF(radius, 2);
        }

        Box SDF.GetBoundingBox()
        {
            double r = this.Radius;
            return new Box(new Vector(-r, -r, -r), new Vector(r, r, r));
        }
    }

    class CubeSDF : SDF
    {
        Vector Size;

        CubeSDF(Vector size)
        {
            this.Size = size;
        }
        
        SDF NewCubeSDF(Vector size)
        {
            return new CubeSDF(size);
        }
        
        public double Evaluate(Vector p)
        {
            double x = p.X;
            double y = p.Y;
            double z = p.Z;

            if (x < 0)
                x = -x;
            if (y < 0)
                y = -y;
            if (z < 0)
                z = -z;

            x -= this.Size.X / 2;
            y -= this.Size.Y / 2;
            z -= this.Size.Z / 2;

            double a = x;
            if (y > a)
                a = y;
            if (z > a)
                a = z;
            if (a > 0)
                a = 0;
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (z < 0)
                z = 0;
            double b = Math.Sqrt(x * x + y * y + z * z);
            return a + b;
        }

        public Box GetBoundingBox()
        {
            double x, y, z;
            x = this.Size.X / 2;
            y = this.Size.Y / 2;
            z = this.Size.Z / 2;
            return new Box(new Vector(-y, -y, -z), new Vector(x, y, z));
        }
    }

    class CylinderSDF:SDF
    {
        double Radius;
        double Height;

        CylinderSDF(double Radius, double Height)
        {
            this.Radius = Radius;
            this.Height = Height;
        }
        
        SDF NewCylinderSDF(double radius, double height)
        {
            return new CylinderSDF(radius, height);
        }

        public Box GetBoundingBox()
        {
            double r = this.Radius;
            double h = this.Height / 2;
            return new Box(new Vector(-r, -h, -r), new Vector(r, h, r));
        }

        public double Evaluate(Vector p)
        {
            double x = Math.Sqrt(p.X * p.X + p.Z * p.Z);
            double y = p.Y;

            if (x < 0)
                x = -x;
            if (y < 0)
                y = -y;
            x -= this.Radius;
            y -= this.Height / 2;
            double a = x;

            if (y > a)
                a = y;
            if (a > 0)
                a = 0;
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;

            double b = Math.Sqrt(x * x + y * y);
            return a + b;
        }
    }

    class CapsuleSDF:SDF
    {
        Vector A,B;
        double Radius;
        double Exponent;

        CapsuleSDF(Vector A, Vector B, double Radius, double Exponent)
        {
            this.A = A;
            this.B = B;
            this.Radius = Radius;
            this.Exponent = Exponent;
        }
        
        SDF NewCapsuleSDF(Vector a, Vector b, double radius)
        {
            return new CapsuleSDF(a, b, radius, 2);
        }

        public double Evaluate(Vector p)
        {
            Vector pa = p.Sub(this.A);
            Vector ba = this.B.Sub(this.A);
            double h = Math.Max(0, Math.Min(1, pa.Dot(ba) / ba.Dot(ba)));
            return pa.Sub(ba.MulScalar(h)).LengthN(this.Exponent) - this.Radius;
        }

        Box SDF.GetBoundingBox()
        {
            Vector a = this.A.Min(this.B);
            Vector b = this.A.Max(this.B);
            return new Box(a.SubScalar(this.Radius), b.AddScalar(this.Radius));
        }
    }

    class TorusSDF:SDF
    {
        double MajorRadius;
        double MinRadius;
        double MajorExponent;
        double MinorExponent;

        TorusSDF(double MajorRadius, double MinRadius, double MajorExponent, double MinorExponent)
        {
            this.MajorRadius = MajorRadius;
            this.MinRadius = MinRadius;
            this.MajorExponent = MajorExponent;
            this.MinorExponent = MinorExponent;
        }
        
        SDF NewTorusSDF(double major, double minor)
        {
            return new TorusSDF(major, minor, 2, 2);
        }

        public double Evaluate(Vector p)
        {
            Vector q = new Vector(new Vector(p.X, p.Y, 0).LengthN(this.MajorExponent) - this.MajorRadius, p.Z, 0);
            return q.LengthN(this.MinorExponent) - this.MinRadius;
        }

        public Box GetBoundingBox()
        {
            double a = this.MinRadius;
            double b = this.MinRadius + this.MajorRadius;
            return new Box(new Vector(-b, -b, a), new Vector(b, b, a));
        }
    }

    class TransformSDF:SDF
    {
        SDF SDF;
        Matrix Matrix;
        Matrix Inverse;
        
        TransformSDF(SDF SDF, Matrix Matrix, Matrix Inverse)
        {
            this.SDF = SDF;
            this.Matrix = Matrix;
            this.Inverse = Inverse;
        }

        SDF NewTransformSDF(SDF sdf, Matrix matrix)
        {
            return new TransformSDF(sdf, matrix, matrix.Inverse());
        }

        public double Evaluate(Vector p)
        {
            Vector q = this.Inverse.MulPosition(p);
            return this.SDF.Evaluate(q);
        }

        public Box GetBoundingBox()
        {
            return this.Matrix.MulBox(this.SDF.GetBoundingBox());
        }
    }

    class ScaleSDF:SDF
    {
        SDF SDF;
        double Factor;

        ScaleSDF(SDF sdf, double Factor)
        {
            this.SDF = sdf;
            this.Factor = Factor;
        }
        
        SDF NewScaleSDF(SDF sdf, double factor)
        {
            return new ScaleSDF(sdf, factor);
        }
        
        public double Evaluate(Vector p)
        {
            return this.SDF.Evaluate(p.DivScalar(this.Factor)) * this.Factor;
        }

        public Box GetBoundingBox()
        {
            double f = this.Factor;
            Matrix m = new Matrix().Scale(new Vector(f, f, f));
            return m.MulBox(this.SDF.GetBoundingBox());
        }
    }

    class UnionSDF : SDF
    {
        SDF[] Items;

        UnionSDF(SDF[] Items)
        {
            this.Items = Items;
        }

        SDF NewUnionSDF(SDF[] items)
        {
            return new UnionSDF(items);
        }

        public double Evaluate(Vector p)
        {
            double result = 0;
            int i = 0;
            foreach (SDF item in this.Items)
            {
                double d = item.Evaluate(p);
                if (i == 0 || d < result)
                {
                    result = d;
                }
                i++;
            }
            return result;
        }

        public Box GetBoundingBox()
        {
            Box result = new Box();
            Box box;
            int i = 0;

            foreach (SDF item in Items)
            {
                box = item.GetBoundingBox();
                if (i == 0)
                {
                    result = box;
                }
                else
                {
                    result = result.Extend(box);
                }
                i++;
            }
            return result;
        }
    }
    
    class DifferenceSDF: SDF
    {
        SDF[] Items;

        DifferenceSDF(SDF[] Items)
        {
            this.Items = Items;
        }
        
        SDF NewDifferenceSDF(SDF[] items)
        {
            return new DifferenceSDF(items);
        }

    
        public double Evaluate(Vector p)
        {
            double result = 0;
            int i = 0;

            foreach (SDF item in this.Items)
            {
                double d = item.Evaluate(p);
                if (i == 0)
                {
                    result = d;
                } else if (-d > result)
                {
                    result = -d;
                }
                i++;
            }
            return result;
        }

        public Box GetBoundingBox()
        {
            return this.Items[0].GetBoundingBox();
        }
    }

    class IntersectionSDF : SDF
    {
        SDF[] Items;
        
        IntersectionSDF(SDF[] Items)
        {
            this.Items = Items;
        }

        SDF NewIntersectionSDF(SDF[] items)
        {
            return new IntersectionSDF(items);
        }

        public double Evaluate(Vector p)
        {
            double result = 0;

            int i = 0;

            foreach (SDF item in this.Items)
            {
                double d = item.Evaluate(p);
                if (i == 0 || d > result)
                {
                    result = d;
                }
                i++;
            }
            return result;
        }

        public Box GetBoundingBox()
        {
            Box result = new Box();
            int i = 0;

            foreach (SDF item in this.Items)
            {
                Box box = item.GetBoundingBox();
                if (i == 0)
                {
                    result = box;
                }
                else
                {
                    result = result.Extend(box);
                }
                i++;
            }
            return result;
        }
    }
    
    class RepeatSDF:SDF
    {
        SDF SDF;
        Vector Step;

        RepeatSDF(SDF sdf, Vector step)
        {
            this.SDF = sdf;
            this.Step = step;
        }

        SDF NewRepeaterSDF(SDF sdf, Vector step)
        {
            return new RepeatSDF(sdf, step);
        }
        
        public double Evaluate(Vector p)
        {
            Vector q = p.Mod(this.Step).Sub(this.Step.DivScalar(2));
            return this.SDF.Evaluate(q);
        }

        public Box GetBoundingBox()
        {
            return new Box();
        }
    }
}
