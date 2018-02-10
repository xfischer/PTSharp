using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Box
    {
        public Vector Min;
        public Vector Max;
        
        public Box(Vector min, Vector max)
        {
            this.Min = min;
            this.Max = max;
        }

        public Box(){}
        
        public static Box BoxForShapes(Shape[] shapes)
        {
            if (shapes.Length == 0)
            {
                return new Box();
            }
            Box box = shapes[0].BoundingBox();
            foreach(Shape val in shapes)
            {
                box = box.Extend(val.BoundingBox());
            }
            return box;
        }

        public static Box BoxForTriangles(Triangle[] shapes)
        {
            if (shapes.Length == 0)
            {
                return new Box();
            }
            Box box = shapes[0].BoundingBox();
            foreach(Shape val in shapes)
            {
                box = box.Extend(val.BoundingBox());
            }
            return box;
        }

        public Vector Anchor(Vector anchor)
        {
            return this.Min.Add(this.Size().Mul(anchor));
        }

        public Vector Center()
        {
            return this.Anchor(new Vector(0.5, 0.5, 0.5));
        }

        public double OuterRadius()
        {
            return this.Min.Sub(this.Center()).Length();
        }

        public double InnerRadius()
        {
            return this.Center().Sub(this.Min).MaxComponent();
        }

        public Vector Size()
        {
            return this.Max.Sub(this.Min);
        }

        public Box Extend(Box b)
        {
            return new Box(this.Min.Min(b.Min), this.Max.Max(b.Max));
        }
        
        public Boolean Contains(Vector b)
        {
            return this.Min.X <= b.X && this.Max.X >= b.X &&
                   this.Min.Y <= b.Y && this.Max.Y >= b.Y &&
                   this.Min.Z <= b.Z && this.Max.Z >= b.Z;
        }

        public Boolean Intersects(Box b)
        {
            return !(this.Min.X > b.Max.X || this.Max.X < b.Min.X || this.Min.Y > b.Max.Y ||
                     this.Max.Y < b.Min.Y || this.Min.Z > b.Max.Z || this.Max.Z < b.Min.Z);
        }

        public double[] Intersect(Ray r)
        {
            double x1 = (this.Min.X - r.Origin.X) / r.Direction.X;  
            double y1 = (this.Min.Y - r.Origin.Y) / r.Direction.Y;  
            double z1 = (this.Min.Z - r.Origin.Z) / r.Direction.Z;  
            double x2 = (this.Max.X - r.Origin.X) / r.Direction.X;  
            double y2 = (this.Max.Y - r.Origin.Y) / r.Direction.Y;  
            double z2 = (this.Max.Z - r.Origin.Z) / r.Direction.Z;  
            if (x1 > x2)
            {
                double tempx = x1;
                x1 = x2;
                x2 = tempx;
            }
            if (y1 > y2)
            {
                double tempy = y1;
                y1 = y2;
                y2 = tempy;
            }
            if (z1 > z2)
            {
                double tempz = z1;
                z1 = z2;
                z2 = tempz;
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
                    left = this.Min.X <= point;
                    right = this.Max.X >= point;
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
    }
}
