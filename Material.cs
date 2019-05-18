using System;

namespace PTSharp
{
    struct Material
    {
        public Color Color;
        public ITexture Texture;
        public ITexture NormalTexture;
        public ITexture BumpTexture;
        public ITexture GlossTexture;
        public double BumpMultiplier;
        public double Emittance;
        public double Index;
        public double Gloss;
        public double Tint;
        public double Reflectivity;
        public bool Transparent;

        public Material(Color color, ITexture texture, ITexture normaltexture, ITexture bumptexture, ITexture glosstexture, double b, double e, double i, double g, double tint, double r, Boolean t)
        {
            Color = color;
            Texture = texture;
            NormalTexture = normaltexture;
            BumpTexture = bumptexture;
            GlossTexture = glosstexture;
            BumpMultiplier = b;
            Emittance = e;
            Index = i;
            Gloss = g;
            Tint = tint;
            Reflectivity = r;
            Transparent = t;
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
            return new Material(new Color(0, 0, 0), null, null, null, null, 1, 0, index, gloss, 0, -1, true);
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

        internal static Material MaterialAt(IShape shape, Vector point)
        {
            var material = shape.MaterialAt(point);
            var uv = shape.UV(point);
            if (material.Texture != null)
            {
                material.Color = material.Texture.Sample(uv.X, uv.Y);
            }
            if(material.GlossTexture != null)
            {
                var c = material.GlossTexture.Sample(uv.X, uv.Y);
                material.Gloss = (c.r + c.g + c.b) / 3;
             }
            return material;
        }
    };
}
