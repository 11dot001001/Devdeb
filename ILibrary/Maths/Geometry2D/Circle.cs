using System.Collections.Generic;
using UnityEngine;

namespace Devdeb.Maths.Geometry2D
{
    public struct Circle
    {
        public Vector2 Position;
        public float Radius;

        public Circle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}