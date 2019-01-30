namespace PTSharp
{
    interface IShape
    {
        void Compile();
        Box GetBoundingBox();
        Hit Intersect(Ray ray);
        Vector UV(Vector uv);
        Vector NormalAt(Vector normal);
        Material MaterialAt(Vector v);
        //Box BoundingBox();
    }
}
