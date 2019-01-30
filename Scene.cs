using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Scene
    {
        internal Color color;
        internal ITexture Texture;
        internal double textureAngle;
        private List<IShape> shapeList;
        private List<IShape> lightList;
        internal IShape[] lights;
        private IShape[] shapes;
        private Tree tree;
        internal int rays;
        
        Scene(Color color_, ITexture texture_, double textureAngle_, IShape[] shapes_, IShape[] lights_, Tree tree_, int rays_) {
            color =color_;
            Texture = texture_;
            textureAngle = textureAngle_;
            shapes = shapes_;
            lights = lights_;
            tree = tree_;
            rays = rays_;
        }

        public Scene()
        {
            color = new Color();
            shapes = new IShape[] { };
            lights = new IShape[] { };
            shapeList = new List<IShape>();
            lightList = new List<IShape>();
            Texture = null;
        }
        
        public void Compile()
        {
            foreach (IShape shape in shapes)
            {
                shape.Compile();
            }
            if (tree == null)
            {
                tree = Tree.NewTree(shapes);
            }
        }

        internal void Add(IShape shape)
        {
            shapeList.Add(shape);
            shapes = shapeList.ToArray();
            if(shape.MaterialAt(new Vector()).Emittance > 0)
            {
                lightList.Add(shape);
                lights = lightList.ToArray();
            }
        }

        int RayCount()
        {
            return rays;
        }
        
        internal Hit Intersect(Ray r)
        {
            Interlocked.Increment(ref rays);
            return tree.Intersect(r);
        }
    }
}
