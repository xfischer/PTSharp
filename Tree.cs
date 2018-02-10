using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    public class Tree
    {

        public Box Box;
        public Node Root;

        public Tree()
        {

        }

        public Tree(Box box, Node root)
        {
            this.Box = box;
            this.Root = root;
        }

        public static Tree NewTree(Shape[] shapes)
        {
            Console.Out.WriteLine("Building k-d tree: " + shapes.Length);
            Box box = Box.BoxForShapes(shapes);
            Node node = Node.NewNode(shapes);
            node.Split(0);
            return new Tree(box, node);
        }

        public Hit Intersect(Ray r)
        {
            double[] tm = this.Box.Intersect(r);

            double tmin = tm[0];
            double tmax = tm[1];

            if (tmax < tmin || tmax <= 0) {
                return Hit.NoHit;
            }
            return this.Root.Intersect(r, tmin, tmax);
        }

        public class Node {
            Axis Axis;
            double Point;
            Shape[] Shapes;
            Node Left;
            Node Right;

            public Node(Axis axis, double point, Shape[] shapes, Node left, Node right) {
                this.Axis = axis;
                this.Point = point;
                this.Shapes = shapes;
                this.Left = left;
                this.Right = right;
            }
            
            public static Node NewNode(Shape[] shapes) {
                return new Node(Axis.AxisNone, 0, shapes, null, null);
            }

            public Hit Intersect(Ray r, double tmin, double tmax) {
                double tsplit = 0;
                Boolean leftFirst = false;
                switch (this.Axis)
                {
                    case Axis.AxisNone:
                        return this.IntersectShapes(r);
                    case Axis.AxisX:
                        tsplit = (this.Point - r.Origin.X) / r.Direction.X;
                        leftFirst = (r.Origin.X < this.Point) || (r.Origin.X == this.Point && r.Direction.X <= 0);
                        break;
                    case Axis.AxisY:
                        tsplit = (this.Point - r.Origin.Y) / r.Direction.Y;
                        leftFirst = (r.Origin.Y < this.Point) || (r.Origin.Y == this.Point && r.Direction.Y <= 0);
                        break;
                    case Axis.AxisZ:
                        tsplit = (this.Point - r.Origin.Z) / r.Direction.Z;
                        leftFirst = (r.Origin.Z < this.Point) || (r.Origin.Z == this.Point && r.Direction.Z <= 0);
                        break;
                }

                Node first, second;
                if (leftFirst)
                {
                    first = this.Left;
                    second = this.Right;
                }
                else
                {
                    first = this.Right;
                    second = this.Left;
                }
                if (tsplit > tmax || tsplit <= 0)
                {
                    return first.Intersect(r, tmin, tmax);
                }
                else if (tsplit < tmin) 
                {
                    return second.Intersect(r, tmin, tmax);
                } else {
                    Hit h1 = first.Intersect(r, tmin, tmax);
                    if (h1.T <= tsplit)
                    {
                        return h1;
                    }
                    Hit h2 = second.Intersect(r, tsplit, Math.Min(tmax, h1.T));
                    if (h1.T <= h2.T)
                    {
                        return h1;
                    }
                    else
                    {
                        return h2;
                    }
                }
            }

            public Hit IntersectShapes(Ray r)
            {
                Hit hit = Hit.NoHit;
                foreach (Shape shapes in this.Shapes)
                {
                    Hit h = shapes.Intersect(r);
                    if (h.T < hit.T)
                    {
                        hit = h;
                    }
                }
                return hit;
            }

            public double Median(List<Double> list)
            {
                int middle = list.Count() / 2;

                if (list.Count==0)
                {
                    return 0;
                }
                else if (list.Count() % 2 == 1)
                {
                    return list.ElementAt(middle);
                }
                else
                {
                    Double a = list.ElementAt(list.Count() / 2 - 1);
                    Double b = list.ElementAt(list.Count() / 2);
                    return (a + b) / 2;
                }

            }
                        
            public int PartitionScore(Axis axis, double point) {
                int left = 0;
                int right = 0;
                foreach(Shape shape in this.Shapes) {
                    Box box = shape.BoundingBox();
                    Boolean[] lr = box.Partition(axis, point);
                    if (lr[0] == true) {
                        left++;
                    }
                    if (lr[1] == true) {
                        right++;
                    }
                }
                if (left >= right) {
                    return left;
                }
                else {
                    return right;
                }
            }

            List<Shape[]> Partition(int size, Axis axis, double point)
            {
                List<Shape> left = new List<Shape>();
                List<Shape> right = new List<Shape>();
                List<Shape[]> ShapeList = new List<Shape[]>();

                foreach (Shape shape in this.Shapes)
                {
                    Box box = shape.BoundingBox();
                    Boolean[] lr = box.Partition(axis, point);
                    if (lr[0] == true)
                    {
                        // Append left 
                        left.Add(shape);
                    }
                    if (lr[1] == true)
                    {
                        // Append right 
                        right.Add(shape);
                    }
                }

                ShapeList.Add(left.ToArray());
                ShapeList.Add(right.ToArray());
                return ShapeList;
            }

            public void Split(int depth) {
                
                if (this.Shapes.Length < 8)
                {
                    return;
                }

                List<double> xs = new List<double>(this.Shapes.Length * 2);
                List<double> ys = new List<double>(this.Shapes.Length * 2);
                List<double> zs = new List<double>(this.Shapes.Length * 2);

                foreach (Shape shape in this.Shapes) {
                    Box box = shape.BoundingBox();
                    xs.Add(box.Min.X); //xs = append(xs, box.Min.X)
                    xs.Add(box.Max.X); //xs = append(xs, box.Max.X)
                    ys.Add(box.Min.Y); //ys = append(ys, box.Min.Y)
                    ys.Add(box.Max.Y); //ys = append(ys, box.Max.Y)
                    zs.Add(box.Min.Z); //zs = append(zs, box.Min.Z)
                    zs.Add(box.Max.Z); //zs = append(zs, box.Max.Z)
                }

                xs.Sort();
                ys.Sort();
                zs.Sort();
                
                double mx = Util.Median(xs.ToArray());
                double my = Util.Median(ys.ToArray());
                double mz = Util.Median(zs.ToArray());
                int best = (int)(((double)this.Shapes.Length) * 0.85);
                Axis bestAxis = Axis.AxisNone;
                double bestPoint = 0.0;

                int sx = this.PartitionScore(Axis.AxisX, mx);
                if (sx < best) {
                    best = sx;
                    bestAxis = Axis.AxisX;
                    bestPoint = mx;
                }

                int sy = this.PartitionScore(Axis.AxisY, my);
                if (sy < best) {
                    best = sy;
                    bestAxis = Axis.AxisY;
                    bestPoint = my;
                }

                int sz = this.PartitionScore(Axis.AxisZ, mz);
                if (sz < best) {
                    best = sz;
                    bestAxis = Axis.AxisZ;
                    bestPoint = mz;
                }

                if (bestAxis == Axis.AxisNone) {
                    return;
                }

                List<Shape[]> Partition = this.Partition(best, bestAxis, bestPoint);
                this.Axis = bestAxis;
                this.Point = bestPoint;
                Shape[] leftarray = Partition.ElementAt(0);
                Shape[] rightarray = Partition.ElementAt(1);
                this.Left = Node.NewNode(leftarray);
                this.Right = Node.NewNode(rightarray);
                this.Left.Split(depth + 1);
                this.Right.Split(depth + 1);
                this.Shapes = null;
            }
        }
    }
}
