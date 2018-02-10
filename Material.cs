using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Material
    {
        public Color Color;
        public Texture Texture;
        public Texture NormalTexture;
        public Texture BumpTexture;
        public Texture GlossTexture;
        public double BumpMultiplier;
        public double Emittance;
        public double Index;
        public double Gloss;
        public double Tint;
        public double Reflectivity;
        public Boolean Transparent;
        
        public Material()
        {

        }

        public Material(Color color, Texture texture, Texture normaltexture, Texture bumptexture, Texture glosstexture, double b, double e, double i, double g, double tint, double r, Boolean t)
        {
            this.Color = color;
            this.Texture = texture;
            this.NormalTexture = normaltexture;
            this.BumpTexture = bumptexture;
            this.GlossTexture = glosstexture;
            this.BumpMultiplier = b;
            this.Emittance = e;
            this.Index = i;
            this.Gloss = g;
            this.Tint = tint;
            this.Reflectivity = r;
            this.Transparent = t;
        }

        public static Material DiffuseMaterial(Color color)
        {
            return new Material(color, null, null, null, null, 1, 0, 1, 0, 0, -1, false);
        }

        public static Material SpecularMaterial(Color color, double index)
        {
            return new Material(color, null, null, null, null, 1, 0, index, 0, 0, -1, false);
        }

        public static Material GlossyMaterial(Color color, double index, double gloss)
        {
            return new Material(color, null, null, null, null, 1, 0, index, gloss, 0, -1, false);
        }

        public static Material ClearMaterial(double index, double gloss)
        {
            return new Material(new Color(0,0,0), null, null, null, null, 1, 0, index, gloss, 0, -1, true);
        }

        public static Material TransparentMaterial(Color color, double index, double gloss, double tint)
        {
            return new Material(color, null, null, null, null, 1, 0, index, gloss, tint, -1, true);
        }

        public static Material MetallicMaterial(Color color, double gloss, double tint)
        {
            return new Material(color, null, null, null, null, 1, 0, 1, gloss, tint, 1, false);
        }

        public static Material LightMaterial(Color color, double emittance)
        {
            return new Material(color, null, null, null, null, 1, emittance, 1, 0, 0, -1, false);
        }

        public static Material MaterialAt(Shape shape, Vector point)
        {
            Material material = shape.MaterialAt(point);
            Vector uv = shape.UV(point);
            if (material.Texture != null)
            {
                material.Color = material.Texture.Sample(uv.X, uv.Y);
            }
            if (material.GlossTexture != null)
            {
                Color c = material.GlossTexture.Sample(uv.X, uv.Y);
                material.Gloss = (c.R + c.G + c.B) / 3;
            }
            return material;
        }
    }
}
