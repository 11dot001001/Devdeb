using Devdeb.Maths.Equation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdeb.Maths.Geometry2D
{
    public static class Geometry2D
    {
        private enum DirectionSector { A, B, C, D, E }

        private static DirectionSector DefineSector(Circle circle, Vector2 direction, Circle relation)
        {
            ExternalTangencyBetweenTwoCircles(circle, relation, out Vector2 externalPoint1, out Vector2 externalPoint2, out Vector2 externalPoint3, out Vector2 externalPoint4);
            InternalTangencyBetweenTwoCircles(circle, relation, out Vector2 internalPoint1, out Vector2 internalPoint2, out Vector2 internalPoint3, out Vector2 internalPoint4);

            externalPoint1 = externalPoint1 - circle.Position;
            externalPoint2 = externalPoint2 - circle.Position;
            internalPoint1 = internalPoint1 - circle.Position;
            internalPoint2 = internalPoint2 - circle.Position;

            if (DirectionBetweenTwoDirections(direction, externalPoint1, internalPoint1))
                return DirectionSector.A;
            else if (DirectionBetweenTwoDirections(direction, externalPoint2, internalPoint2))
                return DirectionSector.B;
            else if (DirectionBetweenTwoDirections(direction, internalPoint1, internalPoint2))
                return DirectionSector.C;
            else if (DirectionBetweenTwoDirections(direction, externalPoint1, circle.Position - relation.Position))
                return DirectionSector.D;
            else if (DirectionBetweenTwoDirections(direction, externalPoint2, circle.Position - relation.Position))
                return DirectionSector.E;
            throw new Exception();
        }

        public static void CircleDirection2CircleDirection(Circle circle1, Vector2 circle1Direction, Circle circle2, Vector2 circle2Direction, out Vector2 circle1EndDirection, out Vector2 circle2EndDirection)
        {
            circle1Direction = circle1Direction.normalized * circle1.Radius;
            circle2Direction = circle2Direction.normalized * circle2.Radius;
            circle1EndDirection = circle1Direction;
            circle2EndDirection = circle2Direction;

            ExternalTangencyBetweenTwoCircles(circle1, circle2, out Vector2 externalPoint1, out Vector2 externalPoint2, out Vector2 externalPoint3, out Vector2 externalPoint4);
            InternalTangencyBetweenTwoCircles(circle1, circle2, out Vector2 internalPoint1, out Vector2 internalPoint2, out Vector2 internalPoint3, out Vector2 internalPoint4);

            externalPoint1 = externalPoint1 - circle1.Position;
            externalPoint2 = externalPoint2 - circle1.Position;
            internalPoint1 = internalPoint1 - circle1.Position;
            internalPoint2 = internalPoint2 - circle1.Position;
            externalPoint3 = externalPoint3 - circle2.Position;
            externalPoint4 = externalPoint4 - circle2.Position;
            internalPoint3 = internalPoint3 - circle2.Position;
            internalPoint4 = internalPoint4 - circle2.Position;

            DirectionSector direction1 = DefineSector(circle1, circle1Direction, circle2);
            DirectionSector direction2 = DefineSector(circle2, circle2Direction, circle1);

            Vector2 directionToPoint = circle2.Position + circle2Direction - circle1.Position + circle1Direction;
            switch (direction1)
            {
                case DirectionSector.A:
                {
                    switch (direction2)
                    {
                        case DirectionSector.A:
                        {
                            circle1EndDirection = internalPoint1;
                            circle2EndDirection = internalPoint3;
                        }
                        break;
                        case DirectionSector.B:
                        {
                            Point2CirclePoint(circle1.Position + circle1Direction, circle2, circle2Direction, out circle2EndDirection);
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.C:
                        {
                            Point2CirclePoint(circle1.Position + circle1Direction, circle2, circle2Direction, out circle2EndDirection);
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.D:
                        {
                            circle1EndDirection = internalPoint1;
                            circle2EndDirection = internalPoint3;
                        }
                        break;
                        case DirectionSector.E:
                        {
                            circle2EndDirection = NearestTangencyToDirection(circle1.Position + circle1Direction, circle2, directionToPoint);
                        }
                        break;
                    }
                }
                break;
                case DirectionSector.B:
                {
                    switch (direction2)
                    {
                        case DirectionSector.A:
                        {
                            Point2CirclePoint(circle1.Position + circle1Direction, circle2, circle2Direction, out circle2EndDirection);
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.B:
                        {
                            circle1EndDirection = internalPoint2;
                            circle2EndDirection = internalPoint4;
                        }
                        break;
                        case DirectionSector.C:
                        {
                            Point2CirclePoint(circle1.Position + circle1Direction, circle2, circle2Direction, out circle2EndDirection);
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.D:
                        {
                            circle2EndDirection = NearestTangencyToDirection(circle1.Position + circle1Direction, circle2, directionToPoint);
                        }
                        break;
                        case DirectionSector.E:
                        {
                            circle1EndDirection = internalPoint2;
                            circle2EndDirection = internalPoint4;
                        }
                        break;
                    }
                }
                break;
                case DirectionSector.C:
                {
                    Point2CirclePoint(circle1.Position + circle1Direction, circle2, circle2Direction, out circle2EndDirection);
                    Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                }
                break;
                case DirectionSector.D:
                {
                    switch (direction2)
                    {
                        case DirectionSector.A:
                        {
                            circle1EndDirection = internalPoint1;
                            circle2EndDirection = internalPoint3;
                        }
                        break;
                        case DirectionSector.B:
                        {
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.C:
                        {
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.D:
                        {
                            circle1EndDirection = internalPoint1;
                            circle2EndDirection = internalPoint3;
                        }
                        break;
                        case DirectionSector.E:
                        {
                            circle1EndDirection = externalPoint1;
                            circle2EndDirection = externalPoint3;
                        }
                        break;
                    }
                }
                break;
                case DirectionSector.E:
                {
                    switch (direction2)
                    {
                        case DirectionSector.A:
                        {
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.B:
                        {
                            circle1EndDirection = internalPoint2;
                            circle2EndDirection = internalPoint4;
                        }
                        break;
                        case DirectionSector.C:
                        {
                            Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                        }
                        break;
                        case DirectionSector.D:
                        {
                            circle1EndDirection = externalPoint2;
                            circle2EndDirection = externalPoint4;
                        }
                        break;
                        case DirectionSector.E:
                        {
                            circle1EndDirection = internalPoint2;
                            circle2EndDirection = internalPoint4;
                        }
                        break;
                    }
                }
                break;
            }
        }
        public static void CircleDirection2CircleDirection(Circle circle1, Vector2 circle1Direction, Circle circle2, Vector2 circle2Direction, bool isClockRotation, float rotateAngle, out Vector2 circle1EndDirection, out Vector2 circle2EndDirection)
        {
            circle1Direction = circle1Direction.normalized * circle1.Radius;
            circle2Direction = circle2Direction.normalized * circle2.Radius;
            circle1EndDirection = circle1Direction;
            circle2EndDirection = circle2Direction;

            ExternalTangencyBetweenTwoCircles(circle1, circle2, out Vector2 externalPoint1, out Vector2 externalPoint2, out Vector2 externalPoint3, out Vector2 externalPoint4);
            InternalTangencyBetweenTwoCircles(circle1, circle2, out Vector2 internalPoint1, out Vector2 internalPoint2, out Vector2 internalPoint3, out Vector2 internalPoint4);

            externalPoint1 = externalPoint1 - circle1.Position;
            externalPoint2 = externalPoint2 - circle1.Position;
            internalPoint1 = internalPoint1 - circle1.Position;
            internalPoint2 = internalPoint2 - circle1.Position;
            externalPoint3 = externalPoint3 - circle2.Position;
            externalPoint4 = externalPoint4 - circle2.Position;
            internalPoint3 = internalPoint3 - circle2.Position;
            internalPoint4 = internalPoint4 - circle2.Position;

            DirectionSector direction1 = DefineSector(circle1, circle1Direction, circle2);
            DirectionSector direction2 = DefineSector(circle2, circle2Direction, circle1);

            Vector2 directionToPoint = circle2.Position + circle2Direction - circle1.Position + circle1Direction;
            switch (direction1)
            {
                case DirectionSector.A:
                {
                    if (isClockRotation)
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.B:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                //circle2EndDirection = NearestTangencyToDirection(circle1.Position + circle1Direction, circle2, directionToPoint);
                                circle1EndDirection = externalPoint1;
                                circle2EndDirection = externalPoint3;
                            }
                            break;
                        }
                    else
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.B:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = externalPoint2;
                                circle2EndDirection = externalPoint4;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                        }
                }
                break;
                case DirectionSector.B:
                {
                    if (isClockRotation)
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.B:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = externalPoint1;
                                circle2EndDirection = externalPoint3;
                            }
                            break;
                        }
                    else
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.B:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = externalPoint2;
                                circle2EndDirection = externalPoint4;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                        }
                }
                break;
                case DirectionSector.C:
                {
                    if (isClockRotation)
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.B:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = externalPoint1;
                                circle2EndDirection = externalPoint3;
                            }
                            break;
                        }
                    else
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.B:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = externalPoint2;
                                circle2EndDirection = externalPoint4;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                        }
                }
                break;
                case DirectionSector.D:
                {
                    if (isClockRotation)
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.B:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = externalPoint1;
                                circle2EndDirection = externalPoint3;
                            }
                            break;
                        }
                    else
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.B:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint2, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = externalPoint2;
                                circle2EndDirection = externalPoint4;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                        }
                }
                break;
                case DirectionSector.E:
                {
                    if (isClockRotation)
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.B:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, externalPoint1, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = internalPoint1;
                                circle2EndDirection = internalPoint3;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = externalPoint1;
                                circle2EndDirection = externalPoint3;
                            }
                            break;
                        }
                    else
                        switch (direction2)
                        {
                            case DirectionSector.A:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.B:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                            case DirectionSector.C:
                            {
                                Point2CirclePoint(circle2.Position + circle2Direction, circle1, circle1Direction, out circle1EndDirection);
                            }
                            break;
                            case DirectionSector.D:
                            {
                                circle1EndDirection = externalPoint2;
                                circle2EndDirection = externalPoint4;
                            }
                            break;
                            case DirectionSector.E:
                            {
                                circle1EndDirection = internalPoint2;
                                circle2EndDirection = internalPoint4;
                            }
                            break;
                        }
                }
                break;
            }
        }

        public static Vector2 NearestTangencyToDirection(Vector2 startPoint, Circle circle, Vector2 direction)
        {
            Point2CircleTangencyDirectionOriented(startPoint, circle, out Vector2 leftTangencyDirection, out Vector2 rightTangencyDirection);
            float a = Vector2.Angle(circle.Position + leftTangencyDirection - startPoint, direction);
            float b = Vector2.Angle(circle.Position + rightTangencyDirection - startPoint, direction);
            return a < b ? leftTangencyDirection : rightTangencyDirection;
        }
        public static bool DirectionBetweenTwoDirections(Vector2 direction, Vector2 direction1, Vector2 direction2)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            angle = angle < 0 ? 360 + angle : angle;
            float angle1 = Vector2.SignedAngle(Vector2.right, direction1);
            angle1 = angle1 < 0 ? 360 + angle1 : angle1;
            float angle2 = Vector2.SignedAngle(Vector2.right, direction2);
            angle2 = angle2 < 0 ? 360 + angle2 : angle2;
            if (angle1 > angle2)
            {
                float temp = angle1;
                angle1 = angle2;
                angle2 = temp;
            }
            if (angle2 - angle1 > 180)
            {
                float temp = angle2;
                angle2 = angle1 + 360;
                angle1 = temp;
                if (angle < 180)
                    angle += 360;
            }

            return angle >= angle1 && angle <= angle2;
        }
        public static bool DirectionBetweenTwoDirections(Vector2 direction, Vector2 direction1, Vector2 direction2, Vector2 relation) => DirectionBetweenTwoDirections(direction, relation, direction1) || DirectionBetweenTwoDirections(direction, relation, direction2);

        public static void Point2CirclePoint(Vector2 point1, Circle circle2, Vector2 circle2Direction, out Vector2 circleStartDirection)
        {
            circleStartDirection = circle2Direction;
            if (!DirectionBetweenTangency(point1, circle2, circle2Direction))
                circleStartDirection = NearestTangencyToDirection(point1, circle2, circle2.Position + circle2Direction - point1);
        }
        public static bool DirectionBetweenTangency(Vector2 startPoint, Circle circle, Vector2 circleDirection)
        {
            Point2CircleTangencyDirection(startPoint, circle, out Vector2 leftTangencyDirection, out Vector2 rightTangencyDirection);
            return DirectionBetweenTwoDirections(circleDirection, leftTangencyDirection, rightTangencyDirection);
        }
        public static bool PointInCircleOfRangeTangencyAngle(Vector2 point, Circle circle, Vector2 circlePoint, out Vector2 leftTangencyDirection, out Vector2 rightTangencyDirection)
        {
            Point2CircleTangencyDirectionOriented(point, circle, out leftTangencyDirection, out rightTangencyDirection);
            Vector2 circlePointDirection = circlePoint - circle.Position;
            return Vector2.SignedAngle(leftTangencyDirection, circlePointDirection) >= 0 && Vector2.SignedAngle(rightTangencyDirection, circlePointDirection) <= 0;
        }

        public static bool ExternalTangencyBetweenTwoCircles(Circle circle1, Circle circle2, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2, out Vector2 intersectedPoint3, out Vector2 intersectedPoint4)
        {
            if (circle1.Position.Equals(circle2.Position))
                throw new ArithmeticException("The centers of circles are equal.");
            Circle o1 = circle1;
            Circle o2 = circle2;
            if (o1.Radius == o2.Radius)
            {
                Vector2 direction = (o1.Position - o2.Position).normalized;
                intersectedPoint1 = Rotate(direction, 90 * Mathf.Deg2Rad).normalized;
                intersectedPoint2 = Rotate(direction, -90 * Mathf.Deg2Rad).normalized;

                intersectedPoint3 = o2.Position + intersectedPoint1 * o2.Radius;
                intersectedPoint4 = o2.Position + intersectedPoint2 * o2.Radius;
                intersectedPoint1 = o1.Position + intersectedPoint1 * o1.Radius;
                intersectedPoint2 = o1.Position + intersectedPoint2 * o1.Radius;
                return true;
            }
            else
            {
                bool swap = false;
                if (o1.Radius < o2.Radius)
                {
                    o1 = circle2;
                    o2 = circle1;
                    swap = true;
                }

                float o3Radius = (o2.Position - o1.Position).magnitude / 2;
                Circle o3 = new Circle((o2.Position - o1.Position).normalized * o3Radius + o1.Position, o3Radius);
                if (!Circle2CircelIntersectOriented(o3, new Circle(o1.Position, o1.Radius - o2.Radius), out Vector2 rightIntersectedDirection, out Vector2 leftIntersectedDirection))
                {
                    intersectedPoint1 = Vector2.zero;
                    intersectedPoint2 = Vector2.zero;
                    intersectedPoint3 = Vector2.zero;
                    intersectedPoint4 = Vector2.zero;
                    return false;
                }
                leftIntersectedDirection = (leftIntersectedDirection - o1.Position).normalized;
                rightIntersectedDirection = (rightIntersectedDirection - o1.Position).normalized;
                if (swap)
                {
                    intersectedPoint1 = o2.Position + rightIntersectedDirection * o2.Radius;
                    intersectedPoint2 = o2.Position + leftIntersectedDirection * o2.Radius;
                    intersectedPoint3 = o1.Position + rightIntersectedDirection * o1.Radius;
                    intersectedPoint4 = o1.Position + leftIntersectedDirection * o1.Radius;
                }
                else
                {
                    intersectedPoint1 = o1.Position + leftIntersectedDirection * o1.Radius;
                    intersectedPoint2 = o1.Position + rightIntersectedDirection * o1.Radius;
                    intersectedPoint3 = o2.Position + leftIntersectedDirection * o2.Radius;
                    intersectedPoint4 = o2.Position + rightIntersectedDirection * o2.Radius;
                }
                return true;
            }
        }
        public static bool InternalTangencyBetweenTwoCircles(Circle circle1, Circle circle2, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2, out Vector2 intersectedPoint3, out Vector2 intersectedPoint4)
        {
            if (circle1.Position.Equals(circle2.Position))
                throw new ArithmeticException("The centers of circles are equal.");
            Circle o1 = circle1;
            Circle o2 = circle2;

            float o3Radius = (o2.Position - o1.Position).magnitude / 2;
            Circle o3 = new Circle((o2.Position - o1.Position).normalized * o3Radius + o1.Position, o3Radius);
            if (!Circle2CircelIntersectOriented(o3, new Circle(o1.Position, o1.Radius + o2.Radius), out Vector2 rightIntersectedDirection, out Vector2 leftIntersectedDirection))
            {
                intersectedPoint1 = Vector2.zero;
                intersectedPoint2 = Vector2.zero;
                intersectedPoint3 = Vector2.zero;
                intersectedPoint4 = Vector2.zero;
                return false;
            }
            leftIntersectedDirection = (leftIntersectedDirection - o1.Position).normalized;
            rightIntersectedDirection = (rightIntersectedDirection - o1.Position).normalized;

            intersectedPoint1 = o1.Position + leftIntersectedDirection * o1.Radius;
            intersectedPoint2 = o1.Position + rightIntersectedDirection * o1.Radius;
            intersectedPoint3 = o2.Position - leftIntersectedDirection * o2.Radius;
            intersectedPoint4 = o2.Position - rightIntersectedDirection * o2.Radius;
            return true;
        }

        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            return new Vector2(vector.x * cos + vector.y * sin, vector.x * -sin + vector.y * cos);
        }
        public static void Rotate(Circle circle, Vector2 tangencyDirection1, Vector2 tangencyDirection2, float rotateAngle, out List<Vector2> directions) => Rotate(circle, tangencyDirection1, tangencyDirection2, rotateAngle, ShortestClockwiseRotation(tangencyDirection1, tangencyDirection2), out directions);
        public static void Rotate(Circle circle, Vector2 tangencyDirection1, Vector2 tangencyDirection2, float rotateAngle, bool isClockWise, out List<Vector2> directions)
        {
            directions = new List<Vector2>();
            float angle;
            float factor = isClockWise ? 1 : -1;
            angle = Vector2.Angle(tangencyDirection1, tangencyDirection2);
            for (; ; )
            {
                if (angle < rotateAngle)
                {
                    directions.Add(tangencyDirection2);
                    break;
                }
                tangencyDirection1 = Rotate(tangencyDirection1, factor * rotateAngle * Mathf.Deg2Rad);
                angle = Vector2.Angle(tangencyDirection1, tangencyDirection2);
                directions.Add(tangencyDirection1);
            }
        }
        public static void AddRotate(Circle circle, Vector2 tangencyDirection1, Vector2 tangencyDirection2, float rotateAngle, bool isClockWise, ref List<Vector2> way)
        {
            Rotate(circle, tangencyDirection1, tangencyDirection2, rotateAngle, isClockWise, out List<Vector2> way2);
            way.AddRange(way2);
        }

        public static bool Line2CircleIntersectOriented(Vector2 point1, Vector2 point2, Circle circle, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2)
        {
            if (!Line2CircleIntersect(point1, point2, circle, out intersectedPoint1, out intersectedPoint2))
                return false;
            if ((point1 - intersectedPoint1).magnitude < (point1 - intersectedPoint2).magnitude)
            {
                Vector2 temp = point1;
                intersectedPoint1 = intersectedPoint2;
                intersectedPoint2 = temp;
            }
            return true;
        }
        public static bool Line2CircleIntersect(Vector2 point1, Vector2 point2, Circle circle, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2)
        {
            if (point1.Equals(point2))
                throw new ArithmeticException("The line points are equal.");
            if (circle.Radius <= 0)
                throw new ArithmeticException("The circle radius is not greater than zero.");
            float a, b, c;
            if (point2.x == point1.x)
            {
                a = 1;
                b = -2 * circle.Position.y;
                c = Mathf.Pow(point1.x, 2) - 2 * point1.x * circle.Position.x + Mathf.Pow(circle.Position.x, 2) + Mathf.Pow(circle.Position.y, 2) - Mathf.Pow(circle.Radius, 2);
                if (!Equations.QuadraticEquationRoots(a, b, c, out float y1, out float y2))
                {
                    intersectedPoint1 = Vector2.zero;
                    intersectedPoint2 = Vector2.zero;
                    return false;
                }
                intersectedPoint1 = new Vector2(point1.x, y2);
                intersectedPoint2 = new Vector2(point1.x, y1);
                return true;
            }
            else
            {
                float kLine = (point2.y - point1.y) / (point2.x - point1.x);
                float bLine = (-point2.y + point1.y) * point1.x / (point2.x - point1.x) + point1.y;

                a = Mathf.Pow(kLine, 2) + 1;
                b = -2 * circle.Position.x + 2 * kLine * bLine - 2 * circle.Position.y * kLine;
                c = Mathf.Pow(circle.Position.x, 2) + Mathf.Pow(bLine, 2) - 2 * circle.Position.y * bLine + Mathf.Pow(circle.Position.y, 2) - Mathf.Pow(circle.Radius, 2);
                if (!Equations.QuadraticEquationRoots(a, b, c, out float x1, out float x2))
                {
                    intersectedPoint1 = Vector2.zero;
                    intersectedPoint2 = Vector2.zero;
                    return false;
                }
                intersectedPoint1 = new Vector2(x1, kLine * x1 + bLine);
                intersectedPoint2 = new Vector2(x2, kLine * x2 + bLine);
                return true;
            }
        }

        public static bool Circle2CircelIntersectOriented(Circle circle1, Circle circle2, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2)
        {
            if (!Circle2CircelIntersect(circle1, circle2, out intersectedPoint1, out intersectedPoint2))
                return false;
            if (Vector2.SignedAngle(circle2.Position - circle1.Position, intersectedPoint1 - circle1.Position) < 0)
            {
                Vector2 temp = intersectedPoint1;
                intersectedPoint1 = intersectedPoint2;
                intersectedPoint2 = temp;
            }
            return true;
        }
        public static bool Circle2CircelIntersect(Circle circle1, Circle circle2, out Vector2 intersectedPoint1, out Vector2 intersectedPoint2)
        {
            if (circle1.Position.Equals(circle2.Position))
                throw new ArithmeticException("The centers of circles are equal.");
            circle2.Position -= circle1.Position;

            float a = -2 * circle2.Position.x;
            float b = -2 * circle2.Position.y;
            float c = circle2.Position.x * circle2.Position.x + circle2.Position.y * circle2.Position.y + circle1.Radius * circle1.Radius - circle2.Radius * circle2.Radius;

            if (b * b < 0.1f)
            {
                float a2 = 1;
                float b2 = -2 * circle2.Position.y;
                float c2 = -c / a * -c / a - 2 * -c / a * circle2.Position.x + circle2.Position.x * circle2.Position.x + circle2.Position.y * circle2.Position.y - circle2.Radius * circle2.Radius;
                if (!Equations.QuadraticEquationRoots(a2, b2, c2, out float y1, out float y2))
                {
                    intersectedPoint1 = Vector2.zero;
                    intersectedPoint2 = Vector2.zero;
                    return false;
                }
                intersectedPoint1 = circle1.Position + new Vector2(-c / a, y2);
                intersectedPoint2 = circle1.Position + new Vector2(-c / a, y1);
                return true;
            }
            else
            {
                float kLine = -a / b;
                float bLine = -c / b;

                a = kLine * kLine + 1;
                b = 2 * kLine * bLine;
                c = bLine * bLine - circle1.Radius * circle1.Radius;
                if (!Equations.QuadraticEquationRoots(a, b, c, out float x1, out float x2))
                {
                    intersectedPoint1 = Vector2.zero;
                    intersectedPoint2 = Vector2.zero;
                    return false;
                }
                intersectedPoint1 = circle1.Position + new Vector2(x1, kLine * x1 + bLine);
                intersectedPoint2 = circle1.Position + new Vector2(x2, kLine * x2 + bLine);
                return true;
            }
        }

        public static void Point2CircleTangencyDirection(Vector2 point, Circle circle, out Vector2 tangencyDirection1, out Vector2 tangencyDirection2)
        {
            Vector2 temp = point - circle.Position;
            if (temp.magnitude < circle.Radius)
                throw new ArithmeticException("The point is in a circle.");
            Vector2 oa = point - circle.Position;
            Vector2 oan = oa.normalized;
            Vector2 p = oan * circle.Radius;
            Vector2 p90 = new Vector2(p.normalized.y, -p.normalized.x) + p;

            Line2CircleIntersect(circle.Position + p, circle.Position + p90, new Circle(circle.Position, oa.magnitude), out tangencyDirection1, out tangencyDirection2);

            tangencyDirection1 = (tangencyDirection1 - circle.Position).normalized * circle.Radius;
            tangencyDirection2 = (tangencyDirection2 - circle.Position).normalized * circle.Radius;
        }
        public static void Point2CircleTangencyDirectionOriented(Vector2 point, Circle circle, out Vector2 leftTangencyDirection, out Vector2 rightTangencyDirection)
        {
            Point2CircleTangencyDirection(point, circle, out leftTangencyDirection, out rightTangencyDirection);
            if (Vector2.SignedAngle((point - circle.Position).normalized * circle.Radius, rightTangencyDirection) < 0)
            {
                Vector2 temp = leftTangencyDirection;
                leftTangencyDirection = rightTangencyDirection;
                rightTangencyDirection = temp;
            }
        }

        public static bool ShortestClockwiseRotation(Vector2 startDirection, Vector2 endDirection) => Vector2.SignedAngle(startDirection, endDirection) > 0 ? false : true;
    }
}