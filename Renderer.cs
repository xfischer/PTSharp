using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Renderer
    {
        Scene Scene;
        Camera Camera;
        Sampler Sampler;
        Buffer PBuffer;
        int SamplesPerPixel;
        public bool StratifiedSampling;
        public int AdaptiveSamples;
        double AdaptiveThreshold;
        double AdaptiveExponent;
        public int FireflySamples;
        double FireflyThreshold;
        int NumCPU;
        int width;
        int height;
        int iterations;
        String pathTemplate;

        Renderer() {}
        
        Renderer(Scene scn, Camera cam, Sampler sampler, int width, int height)
        {
            this.Scene = scn;
            this.Camera = cam;
            this.Sampler = sampler;
            this.width = width;
            this.height = height;
            this.PBuffer = new Buffer(width, height);
            this.SamplesPerPixel = 1;
            this.StratifiedSampling = false;
            this.AdaptiveSamples = 0;
            this.AdaptiveThreshold = 1;
            this.AdaptiveExponent = 1;
            this.FireflySamples = 0;
            this.FireflyThreshold = 1;
            this.NumCPU = Environment.ProcessorCount;
        }
        
        public static Renderer NewRenderer(Scene scene, Camera camera, Sampler sampler, int w, int h)
        {
            Renderer r = new Renderer();
            r.Scene = scene;
            r.Camera = camera;
            r.Sampler = sampler;
            r.PBuffer = new Buffer(w, h);
            r.SamplesPerPixel = 1;
            r.StratifiedSampling = false;
            r.AdaptiveSamples = 0;
            r.AdaptiveThreshold = 1;
            r.FireflySamples = 0;
            r.FireflyThreshold = 1;
            r.NumCPU = Environment.ProcessorCount;
            return r;
        }

        public System.Drawing.Bitmap IterativeRender(String pathTemplate, int iterations)
        {
            this.iterations = iterations;

            for (int iter = 1; iter < this.iterations; iter++)
            {
                Console.WriteLine("[Iteration:" + iter + " of " + iterations + "]");
                this.run();
                this.pathTemplate = pathTemplate;
                System.Drawing.Bitmap finalrender = this.PBuffer.Image(Channel.ColorChannel);
                Console.WriteLine("Writing file...");
                finalrender.Save(pathTemplate);
            }
            return PBuffer.Image(Channel.ColorChannel);
        }
        
        void writeImage(String path, Buffer buf, Channel channel)
        {
            System.Drawing.Bitmap finalrender = buf.Image(channel);
            finalrender.Save(path);
            Console.WriteLine("Wrote image to location:" + path);
        }

        public void run()
        {
            Scene scene = Scene;
            Camera camera = Camera;
            Sampler sampler = Sampler;
            Buffer buf = PBuffer;
            (int w, int h) = (buf.W, buf.H);
            int spp = SamplesPerPixel;
            int sppRoot = (int)(Math.Sqrt((double)(SamplesPerPixel)));
            int ncpu = 1;
            scene.Compile();
            scene.rays = 0;
            Random rand = new Random();
            double fu, fv;
          
            for (int i = 0; i < ncpu; i++)
            {
                for (int y = i; y < h; y += ncpu)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (StratifiedSampling)
                        {
                            for (int u = 0; u < sppRoot; u++)
                            {
                                for (int v = 0; v < sppRoot; v++)
                                {
                                    fu = ((double)u + 0.5) / (double)sppRoot;
                                    fv = ((double)v + 0.5) / (double)sppRoot;
                                    Ray ray = camera.CastRay(x, y, w, h, fu, fv, rand);
                                    Color sample = sampler.Sample(scene, ray, rand);
                                    buf.AddSample(x, y, sample);
                                }
                            }
                        }
                        else
                        {
                            // Random subsampling
                            for (int ii = 0; ii < spp; ii++)
                            {
                                fu = rand.NextDouble();
                                fv = rand.NextDouble();
                                Ray ray = camera.CastRay(x, y, w, h, fu, fv, rand);
                                Color sample = sampler.Sample(scene, ray, rand);
                                buf.AddSample(x, y, sample);
                            }
                        }
                        // Adaptive Sampling
                        if (AdaptiveSamples > 0)
                        {
                            double v = buf.StandardDeviation(x, y).MaxComponent();
                            v = Util.Clamp(v / AdaptiveThreshold, 0, 1);
                            v = Math.Pow(v, AdaptiveExponent);
                            int samples = (int)(v * AdaptiveSamples);
                            for (int d = 0; d < samples; d++)
                            {

                                fu = rand.NextDouble();
                                fv = rand.NextDouble();
                                Ray ray = camera.CastRay(x, y, w, h, fu, fv, rand);
                                Color sample = sampler.Sample(scene, ray, rand);
                                buf.AddSample(x, y, sample);
                            }
                        }

                        if (FireflySamples > 0)
                        {
                            if (PBuffer.StandardDeviation(x, y).MaxComponent() > FireflyThreshold)
                            {
                                for (int e = 0; e < FireflySamples; e++)
                                {
                                    fu = rand.NextDouble();
                                    fv = rand.NextDouble();
                                    Ray ray = camera.CastRay(x, y, w, h, fu, fv, rand);
                                    Color sample = sampler.Sample(scene, ray, rand);
                                    PBuffer.AddSample(x, y, sample);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        void FrameRender(String path, int iterations)
        {
            for (int i = 1; i <= iterations; i++)
            {
                Console.WriteLine("Iterations " + i + "of " + iterations);
                this.run();
            }
            Buffer buf = PBuffer.Copy();
            this.writeImage(path, buf, Channel.ColorChannel);
        }
    }
}
