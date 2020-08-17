using UnityEngine;

namespace Devdeb.Maths.Geometry2D
{
    public struct Circle
    {
        static public Circle Default => new Circle(Vector2.zero, 1F);

        static public Vector2 Lerp(Vector2 startDirection, Vector2 endDirection, bool clockWise, float progress)
        {
            if (progress <= 0)
                return startDirection;
            else if (progress >= 1F)
                return endDirection;
            Vector2 ret = Geometry2D.Rotate(startDirection, (clockWise ? 1 : -1) * Geometry2D.Angle(startDirection, endDirection, clockWise) * progress * Mathf.Deg2Rad);
            return ret;
        }
        static public float ArcLength(Circle circle, Vector2 startDirection, Vector2 endDirection, bool clockWise) => Mathf.PI * circle.Radius * Geometry2D.Angle(startDirection, endDirection, clockWise) / 180;

        public Vector2 Position;
        public float Radius;

        public Circle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public float CircleArcLength => 2F * Mathf.PI * Radius;
    }
}