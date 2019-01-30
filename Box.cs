using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTSharp
{
    struct Box
    {
        public Vector Min;
        public Vector Max;

        public Box(Vector min, Vector max)
        {
            Min = min;
            Max = max;
        }

        internal static Box BoxForShapes(IShape[] shapes)
        {
            if (shapes.Length.Equals(0))
            {
                return new Box();
            }
            Box box = shapes[0].GetBoundingBox();
            foreach (var shape in shapes)
            {
                box = box.Extend(shape.GetBoundingBox());
            }
            return box;
        }

        internal static Box BoxForTriangles(IShape[] shapes)
        {
            if (shapes.Length == 0)
            {
                return new Box();
            }
            Box box = shapes[0].GetBoundingBox();
            foreach (var shape in shapes)
            {
                box = box.Extend(shape.GetBoundingBox());
            }
            return box;
        }

        public Vector Anchor(Vector anchor) => Min.Add(Size().Mul(anchor));

        public Vector Center() => Anchor(new Vector(0.5, 0.5, 0.5));

        public double OuterRadius() => Min.Sub(Center()).Length();

        public double InnerRadius() => Center().Sub(Min).MaxComponent();

        public Vector Size() => Max.Sub(this.Min);

        public Box Extend(Box b) => new Box(Min.Min(b.Min), Max.Max(b.Max));

        public Boolean Contains(Vector b) => Min.X <= b.X && Max.X >= b.X &&
                   Min.Y <= b.Y && Max.Y >= b.Y &&
                   Min.Z <= b.Z && Max.Z >= b.Z;

        public Boolean Intersects(Box b) => !(this.Min.X > b.Max.X || this.Max.X < b.Min.X || this.Min.Y > b.Max.Y ||
                     this.Max.Y < b.Min.Y || this.Min.Z > b.Max.Z || this.Max.Z < b.Min.Z);

        public double[] Intersect(Ray r)
        {
            var x1 = (Min.X - r.Origin.X) / r.Direction.X;
            var y1 = (Min.Y - r.Origin.Y) / r.Direction.Y;
            var z1 = (Min.Z - r.Origin.Z) / r.Direction.Z;
            var x2 = (Max.X - r.Origin.X) / r.Direction.X;
            var y2 = (Max.Y - r.Origin.Y) / r.Direction.Y;
            var z2 = (Max.Z - r.Origin.Z) / r.Direction.Z;
            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
            }
            if (y1 > y2)
            {
                (y1, y2) = (y2, y1);
            }
            if (z1 > z2)
            {
                (z1, z2) = (z2, z1);
            }
            double t1 = Math.Max(Math.Max(x1, y1), z1);
            double t2 = Math.Min(Math.Min(x2, y2), z2);
            double[] result = { t1, t2 };

            return result;
        }

        public bool[] Partition(Axis axis, double point)
        {
            bool left;
            bool right;
            bool[] result = new bool[2];

            switch (axis)
            {
                case Axis.AxisX:
                    left = Min.X <= point;
                    right = Max.X >= point;
                    result[0] = left;
                    result[1] = right;
                    break;
                case Axis.AxisY:
                    left = this.Min.Y <= point;
                    right = this.Max.Y >= point;
                    result[0] = left;
                    result[1] = right;
                    break;
                case Axis.AxisZ:
                    left = this.Min.Z <= point;
                    right = this.Max.Z >= point;
                    result[0] = left;
                    result[1] = right;
                    break;
            }
            return result;
        }
    };
}        
