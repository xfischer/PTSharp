using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PTSharp
{
    public enum Channel
    {
        ColorChannel, VarianceChannel, StandardDeviationChannel, SamplesChannel
    }

    class Buffer
    {
        public int W, H;
        public Pixel[] Pixels;
        public List<Pixel> PixelList;
        byte[] imageBuffer;

        public Buffer() {}

        public Buffer(int width, int height)
        {
            this.W = width;
            this.H = height;
            imageBuffer = new byte[256 * 4 * height];
            this.Pixels = new Pixel[width * height];
            PixelList = new List<Pixel>(width * height);

            for (int i =0; i< this.Pixels.Length; i++)
            {
                Pixels[i] = new Pixel(0, new PTSharp.Color(0, 0, 0), new PTSharp.Color(0, 0, 0));
            }
        }

        public Buffer(int width, int height, Pixel[] pbuffer)
        {
            this.W = width;
            this.H = height;
            this.Pixels = pbuffer;
        }

        public static Buffer NewBuffer(int w, int h)
        {
            Pixel[] pixbuffer = new Pixel[w * h];
            for (int i = 0; i < pixbuffer.Length; i++)
            {
                pixbuffer[i] = new Pixel(0, new PTSharp.Color(0, 0, 0), new PTSharp.Color(0, 0, 0));
            }
            return new Buffer(w, h, pixbuffer);
        }

        public Buffer Copy()
        {
            Pixel[] pixcopy = new Pixel[this.W * this.H];
            Array.Copy(this.Pixels, 0, pixcopy, 0, this.Pixels.Length);
            return new Buffer(this.W, this.H, pixcopy);
        }
        
        public void AddSample(int x, int y, Color sample)
        {
            this.Pixels[y * this.W + x].AddSample(sample);
        }
        
        public int Samples(int x, int y)
        {
            return this.Pixels[y * this.W + x].Samples;
        }
        
        public Color Color(int x, int y)
        {
            return this.Pixels[y * this.W + x].Color();
        }
        
        public Color Variance(int x, int y)
        {
            return this.Pixels[y * this.W + x].Variance();
        }
        
        public Color StandardDeviation(int x, int y)
        {
            return this.Pixels[y * this.W + x].StandardDeviation();
        }

        public Bitmap Image(Channel channel)
        {
            Bitmap bmp = new Bitmap(W, H);
            double maxSamples=0;

            if (channel == Channel.SamplesChannel)
            {
                foreach (Pixel pix in this.Pixels)
                {
                    maxSamples = Math.Max(maxSamples, pix.Samples);
                }
            }

            for (int y = 0; y < this.H; y++)
            {
                for (int x = 0; x < this.W; x++)
                {
                    Color pixelColor = new Color();
                    switch (channel)
                    {
                        case Channel.ColorChannel:
                            pixelColor = this.Pixels[y * this.W + x].Color().Pow(1 / 2.2);
                            break;
                        case Channel.VarianceChannel:
                            pixelColor = Pixels[y * this.W + x].Variance();
                            break;
                        case Channel.StandardDeviationChannel:
                            pixelColor = Pixels[y * this.W + x].StandardDeviation();
                            break;
                        case Channel.SamplesChannel:
                            float p = (float)(Pixels[y * this.W + x].Samples / maxSamples);
                            pixelColor = new Color(p, p, p);
                            break;
                    }
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(pixelColor.getIntFromColor(pixelColor.R, pixelColor.G, pixelColor.B)));
                }
            }
            return bmp;
        }
        
        public class Pixel
        {
            public int Samples;
            public Color M;
            public Color V;

            public Pixel()
            {
                this.Samples = 0;
                this.M = new Color(0, 0, 0);
                this.V = new Color(0, 0, 0);
            }

            public Pixel(int Samples, Color M, Color V)
            {
                this.Samples = Samples;
                this.M = M;
                this.V = V;
            }

            public void AddSample(Color sample)
            {
                this.Samples++;

                if (this.Samples == 1)
                {
                    this.M = sample;
                    return;
                }

                Color m = this.M;
                this.M = this.M.Add(sample.Sub(this.M).DivScalar((double)this.Samples));
                this.V = this.V.Add(sample.Sub(m).Mul(sample.Sub(this.M)));
            }

            public Color Color()
            {
                return this.M;
            }

            public Color Variance()
            {
                if (this.Samples < 2)
                {
                    return new Color(0, 0, 0);
                }
                return this.V.DivScalar((double)(this.Samples - 1));
            }

            public Color StandardDeviation()
            {
                return this.Variance().Pow(0.5);
            }
        }
    }
}
