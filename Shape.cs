using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class TransformedShape : IShape
    {
        private IShape Shape;
        private Matrix matrix;
        private Matrix inverse;

        TransformedShape() { }

        TransformedShape(IShape s, Matrix m, Matrix im)
        {
            Shape = s;
            Matrix = m;
            Inverse = im;
        }
        TransformedShape(IShape s, Matrix m)
        {
            Shape = s;
            Matrix = m;
            Inverse = m.Inverse();
        }

        void IShape.Compile() { }

        internal static IShape NewTransformedShape(IShape s, Matrix m)
        {
            return new TransformedShape(s, m, m.Inverse());
        }

        Box IShape.GetBoundingBox()
        {
            return Matrix.MulBox(Shape.GetBoundingBox());
        }

        internal Matrix Matrix { get => matrix; set => matrix = value; }
        internal Matrix Inverse { get => inverse; set => inverse = value; }

        Hit IShape.Intersect(Ray r)
        {
            Ray shapeRay = Inverse.MulRay(r);
            Hit hit = this.Shape.Intersect(shapeRay);
            if (!hit.Ok())
            {
                return hit;
            }
            var shape = hit.Shape;
            var shapePosition = shapeRay.Position(hit.T);
            var shapeNormal = shape.NormalAt(shapePosition);
            var position = this.Matrix.MulPosition(shapePosition);
            var normal = this.Inverse.Transpose().MulDirection(shapeNormal);
            var material = Material.MaterialAt(shape, shapePosition);
            bool inside = false;
            if (shapeNormal.Dot(shapeRay.Direction) > 0)
            { 
                normal = normal.Negate();
                inside = true;
            }
            var ray = new Ray(position, normal);
            var info = new HitInfo(hit.Shape, position, normal, ray, material, inside);
            hit.T = position.Sub(r.Origin).Length();
            hit.HInfo = info;
            return hit;
        }

        Vector IShape.UV(Vector uv)
        {
            return Shape.UV(uv);
        }

        Vector IShape.NormalAt(Vector normal)
        {
            return Shape.NormalAt(normal);
        }

        Material IShape.MaterialAt(Vector v)
        {
            return Shape.MaterialAt(v);
        }
    }
}
