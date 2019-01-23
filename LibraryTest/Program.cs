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
            Circle circle = new Circle(Vector2.zero, 1);
            float angle0 = GetAngle(Vector2.left,Vector2.up, true);
            float angle1 = GetAngle(Vector2.left,Vector2.up, false);


            float angle2 = GetAngle(Vector2.up, Vector2.left, true);
            float angle3 = GetAngle(Vector2.up, Vector2.left, false);

            float distance  = (float)Math.PI * circle.Radius * GetAngle(Vector2.down, Vector2.right, true) / 180 ;

        }
        static float GetAngle(Vector2 from, Vector2 to, bool isClockRotate)
        {
           float rotateAngle = Vector2.SignedAngle(from, to);
            if(isClockRotate)
                if(rotateAngle < 0)
                    rotateAngle *= -1F;
                else
                    rotateAngle = 360F - rotateAngle;
            else
                if (rotateAngle < 0)
                    rotateAngle = 360F + rotateAngle;
            return rotateAngle;
        }
    }
}
