using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public interface Sampler
    {
        Color Sample(Scene scene, Ray ray, Random rand);
    }

    public enum LightMode
    {
        LightModeRandom, LightModeAll
    }

    public enum SpecularMode
    {
        SpecularModeNaive, SpecularModeFirst, SpecularModeAll
    }

    public enum BounceType
    {
        BounceTypeAny, BounceTypeDiffuse, BounceTypeSpecular
    }

    public class DefaultSampler : Sampler
    {
        int FirstHitSamples;
        int MaxBounces;
        bool DirectLighting;
        bool SoftShadows;
        public LightMode LMode;
        public SpecularMode SMode;

        DefaultSampler(int FirstHitSamples, int MaxBounces, bool DirectLighting, bool SoftShadows, LightMode LM, SpecularMode SM)
        {
            this.FirstHitSamples = FirstHitSamples;
            this.MaxBounces = MaxBounces;
            this.DirectLighting = DirectLighting;
            this.SoftShadows = SoftShadows;
            this.LMode = LM;
            this.SMode = SM;
        }

        public static DefaultSampler NewSampler(int firstHitSamples, int maxBounces)
        {
            return new DefaultSampler(firstHitSamples, maxBounces, true, true, LightMode.LightModeRandom, SpecularMode.SpecularModeNaive);
        }

        DefaultSampler NewDirectSampler()
        {
            return new DefaultSampler(1, 0, true, false, LightMode.LightModeAll, SpecularMode.SpecularModeAll);
        }

        public Color Sample(Scene scene, Ray ray, Random rand)
        {

            return this.sample(scene, ray, true, this.FirstHitSamples, 0, rand);
        }

        Color sample(Scene scene, Ray ray, bool emission, int samples, int depth, Random rand)
        {
            if (depth > MaxBounces)
            {
                return Color.Black;
            }
            var hit = scene.Intersect(ray);
            if (!hit.Ok())
            {
                return sampleEnvironment(scene, ray);
            }
            var info = hit.Info(ray);
            var material = info.material;
            var result = Color.Black;
            if (material.Emittance > 0)
            {
                if (DirectLighting && !emission)
                    return Color.Black;
                result = result.Add(material.Color.MulScalar(material.Emittance * (double)samples));
            }
            var n = (int)Math.Sqrt((double)samples);
            BounceType ma, mb;
            if (SMode == SpecularMode.SpecularModeAll || (depth == 0 && SMode == SpecularMode.SpecularModeFirst))
            {
                ma = BounceType.BounceTypeDiffuse;
                mb = BounceType.BounceTypeSpecular;
            }
            else
            {
                ma = BounceType.BounceTypeAny;
                mb = BounceType.BounceTypeAny;
            }
            for (int u = 0; u < n; u++)
            {
                for (int v = 0; v < n; v++)
                {
                    for (BounceType mode = ma; mode <= mb; mode++)
                    {
                        var fu = ((double)u + rand.NextDouble()) / n;
                        var fv = ((double)v + rand.NextDouble()) / n;
                        //ray.Bounce(info, fu, fv, mode, rand);
                        Ray newRay = null;

                        if (mode == ma)
                            newRay = ray.Bounce(info, fu, fv, mode, rand);

                        if (mode == mb)
                            newRay = ray.Bounce(info, fu, fv, mode, rand);

                        if (mode == BounceType.BounceTypeAny)
                        {
                            newRay.bouncep = 1;
                        }

                        if (newRay.bouncep > 0 && newRay.reflected)
                        {
                            //specular
                            var indirect = this.sample(scene, newRay, newRay.reflected, 1, depth + 1, rand);
                            var tinted = indirect.Mix(material.Color.Mul(indirect), material.Tint);
                            result = result.Add(tinted.MulScalar(newRay.bouncep));
                        }

                        if (newRay.bouncep > 0 && !newRay.reflected)
                        {
                            //diffuse
                            Color indirect = this.sample(scene, newRay, newRay.reflected, 1, depth + 1, rand);
                            Color direct = Color.Black;
                            if (this.DirectLighting)
                            {
                                direct = this.sampleLights(scene, info.Ray, rand);
                            }
                            result = result.Add(material.Color.Mul(direct.Add(indirect)).MulScalar(newRay.bouncep));
                        }
                        
                    }
                }
            }
            result = result.DivScalar((double)(n * n));
            return result;
        }

        Color sampleEnvironment(Scene scene, Ray ray)
        {
            if (scene.Texture != null)
            {
                Vector d = ray.Direction;
                double u = Math.Atan2(d.Z, d.X) + scene.textureAngle;
                double v = Math.Atan2(d.Y, new Vector(d.X, 0, d.Z).Length());
                u = (u + Math.PI) / (2 * Math.PI);
                v = (v + Math.PI / 2) / Math.PI;
                return scene.Texture.Sample(u, v);
            }
            return scene.color;
        }

        Color sampleLights(Scene scene, Ray n, Random rand)
        {
            int nLights = scene.lights.Length;
            if (nLights == 0)
            {
                return Color.Black;
            }
            if (LMode == LightMode.LightModeAll)
            {
                Color result = new Color();
                foreach (IShape light in scene.lights)
                {
                    result = result.Add(this.sampleLight(scene, n, rand, light));
                }
                return result;
            }
            else
            {
                IShape light = scene.lights[rand.Next(nLights)];
                return this.sampleLight(scene, n, rand, light).MulScalar((double)nLights);
            }
        }

        Color sampleLight(Scene scene, Ray n, Random rand, IShape light)
        {
            Vector center;
            double radius;

            switch (light)
            {
                case Sphere sphere:
                    radius = sphere.Radius;
                    center = sphere.Center;
                    break;
                default:
                    Box box = light.GetBoundingBox();
                    radius = box.OuterRadius();
                    center = box.Center();
                    break;
            }
            
            Vector point = center;
            if (SoftShadows)
            {
                for (; ; )
                {
                    var x = (rand.NextDouble() * 2) - 1;
                    var y = (rand.NextDouble() * 2) - 1;
                    if ((x * x) + (y * y) <= 1)
                    {
                        Vector l = center.Sub(n.Origin).Normalize();
                        Vector u = l.Cross(Vector.RandomUnitVector(rand)).Normalize();
                        Vector v = l.Cross(u);
                        point = new Vector();
                        point = point.Add(u.MulScalar(x * radius));
                        point = point.Add(v.MulScalar(y * radius));
                        point = point.Add(center);
                        break;
                    }
                }
            }

            Ray ray = new Ray(n.Origin, point.Sub(n.Origin).Normalize());

            var diffuse = ray.Direction.Dot(n.Direction);
            if (diffuse <= 0)
            {
                return Color.Black;
            }

            Hit hit = scene.Intersect(ray);
            if (!hit.Ok() || hit.Shape != light)
            {
                return Color.Black;
            }

            var hyp = center.Sub(n.Origin).Length();
            var opp = radius;
            var theta = Math.Asin(opp / hyp);
            var adj = opp / Math.Tan(theta);
            var d = Math.Cos(theta) * adj;
            var r = Math.Sin(theta) * adj;
            var coverage = (r * r) / (d * d);
            if (hyp < opp)
            {
                coverage = 1;
            }
            coverage = Math.Min(coverage, 1);
            Material material = Material.MaterialAt(light, point);
            var m = material.Emittance * diffuse * coverage;
            return material.Color.MulScalar(m);
        }
    }
}
