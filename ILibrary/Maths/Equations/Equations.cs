using UnityEngine;

namespace ILibrary.Maths.Equation
{
    public static class Equations
    {
        /// <summary>
        /// Returns the discriminant of the quadratic equation.
        /// </summary>
        /// <param name="a">The a coefficient.</param>
        /// <param name="b">The b coefficient.</param>
        /// <param name="c">The c coefficient.</param>
        /// <returns>The discriminant of the quadratic equation.</returns>
        public static float Discriminant(float a, float b, float c) => b * b - 4 * a * c;
        /// <summary>
        /// Returns the roots of quadratic equation.
        /// </summary>
        /// <param name="a">The a coefficient.</param>
        /// <param name="b">The b coefficient.</param>
        /// <param name="c">The c coefficient.</param>
        /// <param name="x1">The root of eqiation.</param>
        /// <param name="x2">The root of equation.</param>
        /// <returns>true if the roots exist; otherwise, false.</returns>
        public static bool QuadraticEquationRoots(float a, float b, float c, out float x1, out float x2)
        {
            float discriminant = Discriminant(a, b, c);
            if (discriminant < 0)
            {
                x1 = 0;
                x2 = 0;
                return false;
            }
            float sqrt = Mathf.Sqrt(discriminant);
            x1 = (-b - sqrt) / (2 * a);
            x2 = (-b + sqrt) / (2 * a);
            return true;
        }
    }
}
