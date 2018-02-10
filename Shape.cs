using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public interface Shape
    {
        void Compile();
        Box BoundingBox();
        Hit Intersect(Ray ray);
        Vector UV(Vector uv);
        Vector NormalAt(Vector normal);
        Material MaterialAt(Vector v);
    }

    public class TransformedShape : Shape
    {
        public Shape Shape;
        public Matrix Matrix;
        public Matrix Inverse;

        public TransformedShape() { }

        public TransformedShape(Shape s, Matrix m, Matrix im) {
            this.Shape = s;
            this.Matrix = m;
            this.Inverse = im;
        }

        public TransformedShape(Shape s, Matrix m) {
            this.Shape = s;
            this.Matrix = m;
            this.Inverse = m.Inverse();
        }
        
        public void Compile() {

        }
        
        public static Shape NewTransformedShape(Shape s, Matrix m) {
            return new TransformedShape(s, m, m.Inverse());
        }
        
        public Box BoundingBox() {
            return this.Matrix.MulBox(this.Shape.BoundingBox());
        }

        public Hit Intersect(Ray r)
        {
            Ray shapeRay = this.Inverse.MulRay(r);
            Hit hit = this.Shape.Intersect(shapeRay);
            if (!hit.Ok())
            {
                return hit;
            }
            Shape shape = hit.Shape;
            Vector shapePosition = shapeRay.Position(hit.T);
            Vector shapeNormal = Shape.NormalAt(shapePosition);
            Vector position = this.Matrix.MulPosition(shapePosition);
            Vector normal = this.Inverse.Transpose().MulDirection(shapeNormal);
            Material material = Material.MaterialAt(this, shapePosition);
            bool inside = false;
            if (shapeNormal.Dot(shapeRay.Direction) > 0)
            {
                normal = normal.Negate();
                inside = true;
            }
            Ray ray = new Ray(position, normal);
            HitInfo info = new HitInfo(shape, position, normal, ray, material, inside);
            hit.T = position.Sub(r.Origin).Length();
            hit.HInfo = info;
            return hit;
        }

        public Vector UV(Vector uv)
        {
            return Shape.UV(uv);
        }

        public Vector NormalAt(Vector normal)
        {
            return Shape.NormalAt(normal);
        }

        public Material MaterialAt(Vector v)
        {
            return Shape.MaterialAt(v);
        }
    }
}
