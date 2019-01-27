using Devdeb.Maths;
using Devdeb.Maths.Geometry2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LibraryTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static float Run(Circle circle, float deviation, Vector2 targetPosition)
        {
            Vector2 targetDirection = (targetPosition - circle.Position).normalized.normalized;
            Vector2 rotated90TargetDirection = new Vector2(-targetDirection.y, targetDirection.x);

        }
    }

}
