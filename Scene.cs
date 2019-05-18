using System.Collections.Generic;

namespace PTSharp
{
    public class Scene
    {
        internal Color Color = new Color();
        internal ITexture Texture = null;
        internal double TextureAngle = 0;
        private List<IShape> shapeList = new List<IShape>();
        private List<IShape> lightList = new List<IShape>();
        internal IShape[] Lights = new IShape[] { };
        private IShape[] Shapes = new IShape[] { };
        private Tree tree;
        internal int rays = 0;

        public Scene() { }
            
        public void Compile()
        {
            foreach(IShape shape in Shapes)
            {
                shape.Compile();
            }
            if (tree is null)
            {
                tree = Tree.NewTree(Shapes);
            }
        }

        internal void Add(IShape shape)
        {
            shapeList.Add(shape);
            if(shape.MaterialAt(new Vector()).Emittance > 0)
            {
                lightList.Add(shape);
                Lights = lightList.ToArray();
            }
            Shapes = shapeList.ToArray();
        }

        int RayCount()
        {
            return rays;
        }
        
        internal Hit Intersect(Ray r)
        {
            rays++;
            return tree.Intersect(r);
        }
    }
}
