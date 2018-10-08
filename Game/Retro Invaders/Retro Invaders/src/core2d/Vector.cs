namespace MPP.core2d
{
    public class Vector
    {
        private Vector() { }
        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
        }

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
