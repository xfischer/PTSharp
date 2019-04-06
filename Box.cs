using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTSharp
{
    class Box
    {
        public Vector Min;
        public Vector Max;
        internal bool left;
        internal bool right;

        internal Box() { }

        public Box(Vector min, Vector max)
        {
            Min = min;
            Max = max;
        }

        internal static Box BoxForShapes(IShape[] shapes)
        {
	        if(shapes.Length == 0)
            {
                return new Box();
            }
            var box = shapes[0].BoundingBox();

           foreach(var shape in shapes)
            {
                box = box.Extend(shape.BoundingBox());

	        }
            return box;
        }

        internal static Box BoxForTriangles(Triangle[] shapes)
        {
            if (shapes.Length == 0)
            {
                return new Box();
            }
            Box box = shapes[0].BoundingBox();
            foreach (var shape in shapes)
            {
                box = box.Extend(shape.BoundingBox());
            }
            return box;
        }

        public Vector Anchor(Vector anchor) => Min.Add(Size().Mul(anchor));

        public Vector Center() => Anchor(new Vector(0.5, 0.5, 0.5));

        public double OuterRadius() => Min.Sub(Center()).Length();

        public double InnerRadius() => Center().Sub(Min).MaxComponent();

        public Vector Size()
        {
            return Max.Sub(Min);
        }

        public Box Extend(Box b) => new Box(Min.Min(b.Min), Max.Max(b.Max));

        public bool Contains(Vector b) => Min.X <= b.X && Max.X >= b.X &&
                                             Min.Y <= b.Y && Max.Y >= b.Y &&
                                             Min.Z <= b.Z && Max.Z >= b.Z;

        public bool Intersects(Box b) => !(this.Min.X > b.Max.X || this.Max.X < b.Min.X || this.Min.Y > b.Max.Y ||
                     this.Max.Y < b.Min.Y || this.Min.Z > b.Max.Z || this.Max.Z < b.Min.Z);

        public (double, double) Intersect(Ray r)
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

            return (t1, t2);
        }

        public (bool, bool) Partition(Axis axis, double point)
        {
            switch (axis)
            {
                case Axis.AxisX:
                    left = Min.X <= point;
                    right = Max.X >= point;
                    break;
                case Axis.AxisY:
                    left = Min.Y <= point;
                    right = Max.Y >= point;
                    break;
                case Axis.AxisZ:
                    left = Min.Z <= point;
                    right = Max.Z >= point;
                    break;
            }
            return (left, right);
        }
    }
}        
