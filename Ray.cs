using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Ray
    {
        internal Vector Origin, Direction;
        internal Boolean reflected;
        public double bouncep;

        internal Ray(Vector Origin_, Vector Direction_)
        {
            Origin = Origin_;
            Direction = Direction_;
        }

        internal Ray(Vector Origin_, Vector Direction_, Boolean reflected_, double p)
        {
            Origin = Origin_;
            Direction = Direction_;
            reflected = reflected_;
            bouncep = p;
        }

        internal Vector Position(double t) => Origin.Add(Direction.MulScalar(t));

        internal Ray Reflect(Ray i) => new Ray(Origin, Direction.Reflect(i.Direction));

        public Ray Refract(Ray i, double n1, double n2) => new Ray(Origin, Direction.Refract(i.Direction, n1, n2));

        public double Reflectance(Ray i, double n1, double n2) => Direction.Reflectance(i.Direction, n1, n2);

        public Ray WeightedBounce(double u, double v, Random rand)
        {
            var radius = Math.Sqrt(u);
            var theta = 2 * Math.PI * v;
            Vector s = Direction.Cross(Vector.RandomUnitVector(rand)).Normalize();
            Vector t = Direction.Cross(s);
            Vector d = new Vector();
            d = d.Add(s.MulScalar(radius * Math.Cos(theta)));
            d = d.Add(t.MulScalar(radius * Math.Sin(theta)));
            d = d.Add(Direction.MulScalar(Math.Sqrt(1 - u)));
            return new Ray(Origin, d);
        }

        public Ray ConeBounce(double theta, double u, double v, Random rand) => new Ray(Origin, Util.Cone(Direction, theta, u, v, rand));

        public Ray Bounce(HitInfo info, double u, double v, BounceType bounceType, Random rand)
        {
            Ray n = info.Ray;
            var material = info.material;
            var n1 = 1.0;
            var n2 = material.Index;

            if (info.inside)
            {
                (n1, n2) = (n2, n1);
            }

            double p;

            if (material.Reflectivity >= 0)
            {
                p = material.Reflectivity;
            }
            else
            {
                p = n.Reflectance(this, n1, n2);
            }

            bool reflect = false;

            switch (bounceType)
            {
                case BounceType.BounceTypeAny:
                    reflect = rand.NextDouble() < p;
                    break;
                case BounceType.BounceTypeDiffuse:
                    reflect = false;
                    break;
                case BounceType.BounceTypeSpecular:
                    reflect = true;
                    break;
            }
            
            if (reflect)
            {
                Ray reflectedRay = n.Reflect(this);
                Ray cbounce = reflectedRay.ConeBounce(material.Gloss, u, v, rand);
                cbounce.reflected = true;
                cbounce.bouncep = p;
                return cbounce;
            }
            else if (material.Transparent)
            {
                Ray refractedRay = n.Refract(this, n1, n2);
                refractedRay.Origin = refractedRay.Origin.Add(refractedRay.Direction.MulScalar(1e-4));
                Ray rcbounce = refractedRay.ConeBounce(material.Gloss, u, v, rand);
                rcbounce.reflected = true;
                rcbounce.bouncep = 1 - p;
                return refractedRay;
            }
            else
            {
                Ray wBounce = WeightedBounce(u, v, rand);
                wBounce.reflected = false;
                wBounce.bouncep = 1 - p;
                return wBounce;
            }
        }
    }
}
