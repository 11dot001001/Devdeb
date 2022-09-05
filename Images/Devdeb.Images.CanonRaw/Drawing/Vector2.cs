using System;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public struct Vector2
    {
        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public double Magnitude => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));

        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    }
}
