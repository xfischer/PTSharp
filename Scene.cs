using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Scene
    {
        public Color Color;
        public Texture Texture;
        public double TextureAngle;
        public List<Shape> ShapeList;
        public Shape[] Lights;
        public Shape[] Shapes;
        public Tree Tree;
        public int rays;
        public int raycount;

        public Scene(Color color, Texture texture, double textureAngle, Shape[] shapes, Shape[] lights, Tree tree, int rays) {
            this.Color = color;
            this.Texture = texture;
            this.TextureAngle = textureAngle;
            this.Shapes = shapes;
            this.Lights = lights;
            this.Tree = tree;
            this.rays = rays;
        }

        public Scene() {
            this.Color = new Color();
            this.Shapes = new Shape[] { };
            this.Lights = new Shape[] { };
            ShapeList = new List<Shape>();
            this.Texture = null;
        }
        
        public void Compile() {
            foreach(Shape shape in Shapes) {
                shape.Compile();
            }
            if (this.Tree == null) {
                this.Tree = Tree.NewTree(Shapes);
            }
        }
        
        public void Add(Shape shape)
        {
            ShapeList.Add(shape);
            this.Shapes = ShapeList.ToArray();

            // Small check to see if material is null
            if(shape.MaterialAt(new Vector()) == null)
            {
                Console.WriteLine("Material is non-existent for shape:"+shape.ToString());
                this.ShapeList.Add(shape);
            } else if (shape.MaterialAt(new Vector()).Emittance > 0)
            {
                this.ShapeList.Add(shape);
                this.Lights = ShapeList.ToArray();
            }
            
        }

        int RayCount()
        {
            return this.rays;
        }
        
        public Hit Intersect(Ray r)
        {
            this.rays++;
            return this.Tree.Intersect(r);
        }
    }
}
