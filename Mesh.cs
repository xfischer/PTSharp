using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    internal delegate double func(Vector d);

    class SphericalHarmonic : IShape, SDF
    {
        Material PositiveMaterial;
        Material NegativeMaterial;
        func harmonicFunction;
        Mesh mesh;
        
        IShape NewSphericalHarmonic(int l, int m, Material pm, Material nm)
        {
            SphericalHarmonic sh = new SphericalHarmonic();
            sh.PositiveMaterial = pm;
            sh.NegativeMaterial = nm;
            sh.harmonicFunction = shFunc(l, m);
            sh.mesh = new MC().NewSDFMesh(sh, sh.BoundingBox(), 0.01);
            return sh;
        }

        Material IShape.MaterialAt(Vector p)
        {
            double h = this.EvaluateHarmonic(p);
            if (h < 0)
            {
                return this.NegativeMaterial;
            }
            else
            {
                return this.PositiveMaterial;
            }

        }

        void IShape.Compile()
        {
            this.mesh.Compile();
        }

        Box BoundingBox()
        {
            double r = 1;
            return new Box(new Vector(-r, -r, -r), new Vector(r, r, r));
        }

        public Hit Intersect(Ray r)
        {
            Hit hit = this.mesh.Intersect(r);
            if (!hit.Ok())
            {
                return Hit.NoHit;
            }
            return new Hit(this, hit.T, null);
        }

        Vector IShape.UV(Vector p)
        {
            double u = Math.Atan2(p.Z, p.X);
            double v = Math.Atan2(p.Y, new Vector(p.X, 0, p.Z).Length());
            u = (1 - (u + Math.PI)) / (2 * Math.PI);
            v = (v + Math.PI / 2) / Math.PI;
            return new Vector(u, v, 0);
        }
        
        Vector IShape.NormalAt(Vector p)
        {
            double e = 0.0001;
            double x = p.X;
            double y = p.Y;
            double z = p.Z;

            Vector n = new Vector(
                this.Evaluate(new Vector(x - e, y, z)) - this.EvaluateHarmonic(new Vector(x + e, y, z)),
                this.Evaluate(new Vector(x, y - e, z)) - this.Evaluate(new Vector(x, y + e, z)),
                this.Evaluate(new Vector(x, y, z - e)) - this.Evaluate(new Vector(x, y, z + e))
            );

            return n.Normalize();
        }

        double Evaluate(Vector p)
        {
            return p.Length() - Math.Abs(this.harmonicFunction(p.Normalize()));
        }
            
        double EvaluateHarmonic(Vector p)
        {
            return this.harmonicFunction(p.Normalize());
        }

        static double sh00(Vector d)
        {
            return 0.282095;
        }

        double sh1n1(Vector d)
        {
            return -0.488603 * d.Y;
        }

        double sh10(Vector d)
        {
            return 0.488603 * d.Z;
        }

        double sh1p1(Vector d)
        {
            return -0.488603 * d.X;
        }

        double sh2n2(Vector d)
        {
            // 0.5 * sqrt(15/pi) * x * y
            return 1.092548 * d.X * d.Y;
        }

        double sh2n1(Vector d)
        {
            // -0.5 * sqrt(15/pi) * y * z
            return -1.092548 * d.Y * d.Z;
        }

        double sh20(Vector d)
        {
            // 0.25 * sqrt(5/pi) * (-x^2-y^2+2z^2)
            return 0.315392 * (-d.X * d.X - d.Y * d.Y + 2.0 * d.Z * d.Z);
        }

        double sh2p1(Vector d)
        {
            // -0.5 * sqrt(15/pi) * x * z
            return -1.092548 * d.X * d.Z;
        }

        double sh2p2(Vector d)
        {
            // 0.25 * sqrt(15/pi) * (x^2 - y^2)
            return 0.546274 * (d.X * d.X - d.Y * d.Y);
        }

        double sh3n3(Vector d)
        {
            // -0.25 * sqrt(35/(2pi)) * y * (3x^2 - y^2)
            return -0.590044 * d.Y * (3.0 * d.X * d.X - d.Y * d.Y);
        }

        double sh3n2(Vector d)
        {
            // 0.5 * sqrt(105/pi) * x * y * z
            return 2.890611 * d.X * d.Y * d.Z;
        }

        double sh3n1(Vector d)
        {
            // -0.25 * sqrt(21/(2pi)) * y * (4z^2-x^2-y^2)
            return -0.457046 * d.Y * (4.0 * d.Z * d.Z - d.X * d.X - d.Y * d.Y);
        }

        double sh30(Vector d)
        {
            // 0.25 * sqrt(7/pi) * z * (2z^2 - 3x^2 - 3y^2)
            return 0.373176 * d.Z * (2.0 * d.Z * d.Z - 3.0 * d.X * d.X - 3.0 * d.Y * d.Y);
        }

        double sh3p1(Vector d)
        {
            // -0.25 * sqrt(21/(2pi)) * x * (4z^2-x^2-y^2)
            return -0.457046 * d.X * (4.0 * d.Z * d.Z - d.X * d.X - d.Y * d.Y);
        }

        double sh3p2(Vector d)
        {
            // 0.25 * sqrt(105/pi) * z * (x^2 - y^2)
            return 1.445306 * d.Z * (d.X * d.X - d.Y * d.Y);
        }

        double sh3p3(Vector d)
        {
            // -0.25 * sqrt(35/(2pi)) * x * (x^2-3y^2)
            return -0.590044 * d.X * (d.X * d.X - 3.0 * d.Y * d.Y);
        }
        double sh4n4(Vector d)
        {
            // 0.75 * sqrt(35/pi) * x * y * (x^2-y^2)
            return 2.503343 * d.X * d.Y * (d.X * d.X - d.Y * d.Y);
        }

        double sh4n3(Vector d)
        {
            // -0.75 * sqrt(35/(2pi)) * y * z * (3x^2-y^2)
            return -1.770131 * d.Y * d.Z * (3.0 * d.X * d.X - d.Y * d.Y);
        }

        double sh4n2(Vector d)
        {
            // 0.75 * sqrt(5/pi) * x * y * (7z^2-1)
            return 0.946175 * d.X * d.Y * (7.0 * d.Z * d.Z - 1.0);
        }

        double sh4n1(Vector d)
        {
            // -0.75 * sqrt(5/(2pi)) * y * z * (7z^2-3)
            return -0.669047 * d.Y * d.Z * (7.0 * d.Z * d.Z - 3.0);
        }

        double sh40(Vector d)
        {
            // 3/16 * sqrt(1/pi) * (35z^4-30z^2+3)
            double z2 = d.Z * d.Z;
            return 0.105786 * (35.0 * z2 * z2 - 30.0 * z2 + 3.0);
        }

        double sh4p1(Vector d)
        {
            // -0.75 * sqrt(5/(2pi)) * x * z * (7z^2-3)
            return -0.669047 * d.X * d.Z * (7.0 * d.Z * d.Z - 3.0);
        }

        double sh4p2(Vector d)
        {
            // 3/8 * sqrt(5/pi) * (x^2 - y^2) * (7z^2 - 1)
            return 0.473087 * (d.X * d.X - d.Y * d.Y) * (7.0 * d.Z * d.Z - 1.0);
        }

        double sh4p3(Vector d)
        {
            // -0.75 * sqrt(35/(2pi)) * x * z * (x^2 - 3y^2)
            return -1.770131 * d.X * d.Z * (d.X * d.X - 3.0 * d.Y * d.Y);
        }

        double sh4p4(Vector d)
        {
            // 3/16*sqrt(35/pi) * (x^2 * (x^2 - 3y^2) - y^2 * (3x^2 - y^2))
            double x2 = d.X * d.X;
            double y2 = d.Y * d.Y;
            return 0.625836 * (x2 * (x2 - 3.0 * y2) - y2 * (3.0 * x2 - y2));
        }
        
        func shFunc(int l, int m)
        {
            func f = null;

            if (l == 0 && m == 0)
            {
                f = sh00;
            }
            else if (l == 1 && m == -1)
            {
                f = sh1n1;
            }
            else if (l == 1 && m == 0)
            {
                f = sh10;
            }
            else if (l == 1 && m == 1)
            {
                f = sh1p1;
            }
            else if (l == 2 && m == -2)
            {
                f = sh2n2;
            }
            else if (l == 2 && m == -1)
            {
                f = sh2n1;
            }
            else if (l == 2 && m == 0)
            {
                f = sh20;
            }
            else if (l == 2 && m == 1)
            {
                f = sh2p1;
            }
            else if (l == 2 && m == 2)
            {
                f = sh2p2;
            }
            else if (l == 3 && m == -3)
            {
                f = sh3n3;
            }
            else if (l == 3 && m == -2)
            {
                f = sh3n2;
            }
            else if (l == 3 && m == -1)
            {
                f = sh3n1;
            }
            else if (l == 3 && m == 0)
            {
                f = sh30;
            }
            else if (l == 3 && m == 1)
            {
                f = sh3p1;
            }
            else if (l == 3 && m == 2)
            {
                f = sh3p2;
            }
            else if (l == 3 && m == 3)
            {
                f = sh3p3;
            }
            else if (l == 4 && m == -4)
            {
                f = sh4n4;
            }
            else if (l == 4 && m == -3)
            {
                f = sh4n3;
            }
            else if (l == 4 && m == -2)
            {
                f = sh4n2;
            }
            else if (l == 4 && m == -1)
            {
                f = sh4n1;
            }
            else if (l == 4 && m == 0)
            {
                f = sh40;
            }
            else if (l == 4 && m == 1)
            {
                f = sh4p1;
            }
            else if (l == 4 && m == 2)
            {
                f = sh4p2;
            }
            else if (l == 4 && m == 3)
            {
                f = sh4p3;
            }
            else if (l == 4 && m == 4)
            {
                f = sh4p4;
            }
            else
            {
                Console.WriteLine("unsupported spherical harmonic");
            }
            return f;
        }

        public Box GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        double SDF.Evaluate(Vector p)
        {
            throw new NotImplementedException();
        }
    }
}
