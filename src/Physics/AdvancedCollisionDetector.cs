using BHS.Engine.Core.Math;
using BHS.Engine.Scene;

namespace BHS.Engine.Physics
{
    /// <summary>
    /// Продвинутый детектор коллизий с поддержкой произвольных углов стен
    /// </summary>
    public static class AdvancedCollisionDetector
    {
        /// <summary>
        /// Проверка коллизии шарика с произвольной стеной (под любым углом)
        /// </summary>
        public static bool CheckBallWallCollisionAdvanced(Ball ball, Wall wall, out Vector2 collisionPoint, out Vector2 normal, out float penetrationDepth)
        {
            collisionPoint = Vector2.Zero;
            normal = Vector2.Zero;
            penetrationDepth = 0f;

            if (ball == null || wall == null)
                return false;

            Vector2 closestPoint = GetClosestPointOnLineSegment(ball.Position, wall.Start, wall.End);

            float distance = Vector2.Distance(ball.Position, closestPoint);

            if (distance <= ball.Radius)
            {
                collisionPoint = closestPoint;
                penetrationDepth = ball.Radius - distance;

                Vector2 direction = ball.Position - closestPoint;
                if (direction.SqrMagnitude > MathHelper.Epsilon)
                {
                    normal = direction.Normalized;
                }
                else
                {
                    Vector2 wallDir = (wall.End - wall.Start).Normalized;
                    Vector2 perpNormal1 = new Vector2(-wallDir.Y, wallDir.X);
                    Vector2 perpNormal2 = new Vector2(wallDir.Y, -wallDir.X);

                    if (ball.Velocity.SqrMagnitude > MathHelper.Epsilon)
                    {
                        float dot1 = Vector2.Dot(ball.Velocity.Normalized, perpNormal1);
                        float dot2 = Vector2.Dot(ball.Velocity.Normalized, perpNormal2);
                        normal = (dot1 > dot2) ? perpNormal1 : perpNormal2;
                    }
                    else
                    {
                        normal = perpNormal1;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Получение ближайшей точки на отрезке к заданной точке
        /// </summary>
        private static Vector2 GetClosestPointOnLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineDirection = lineEnd - lineStart;
            float lineLengthSqr = lineDirection.SqrMagnitude;

            if (lineLengthSqr < MathHelper.Epsilon)
            {
                return lineStart;
            }

            float t = Vector2.Dot(point - lineStart, lineDirection) / lineLengthSqr;
            t = MathHelper.Clamp(t, 0f, 1f);

            return lineStart + t * lineDirection;
        }

        /// <summary>
        /// Получение нормали к стене (направлена наружу)
        /// </summary>
        private static Vector2 GetWallNormal(Wall wall)
        {
            Vector2 wallDirection = (wall.End - wall.Start).Normalized;
            return new Vector2(-wallDirection.Y, wallDirection.X);
        }

        /// <summary>
        /// Проверка коллизии с учетом движения (Continuous Collision Detection)
        /// Улучшенная версия с более точным расчетом времени столкновения
        /// </summary>
        public static bool CheckBallWallCollisionCCD(Ball ball, Wall wall, Vector2 nextPosition, out Vector2 collisionPoint, out Vector2 normal, out float collisionTime)
        {
            collisionPoint = Vector2.Zero;
            normal = Vector2.Zero;
            collisionTime = 0f;

            if (ball == null || wall == null)
                return false;

            Vector2 movementDirection = nextPosition - ball.Position;
            float movementLength = movementDirection.Magnitude;

            if (movementLength < MathHelper.Epsilon)
                return false;

            Vector2 movementNormalized = movementDirection / movementLength;

            Vector2 closestPointStart = GetClosestPointOnLineSegment(ball.Position, wall.Start, wall.End);
            Vector2 closestPointEnd = GetClosestPointOnLineSegment(nextPosition, wall.Start, wall.End);

            float distanceToWallStart = Vector2.Distance(ball.Position, closestPointStart);
            float distanceToWallEnd = Vector2.Distance(nextPosition, closestPointEnd);

            if (distanceToWallStart <= ball.Radius)
            {
                collisionTime = 0f;
                collisionPoint = closestPointStart;

                Vector2 direction = ball.Position - closestPointStart;
                if (direction.SqrMagnitude > MathHelper.Epsilon)
                {
                    normal = direction.Normalized;
                }
                else
                {
                    normal = GetWallNormal(wall);
                }

                return true;
            }

            if (distanceToWallEnd >= distanceToWallStart)
                return false;


            Vector2 wallDirection = wall.End - wall.Start;
            float wallLengthSqr = wallDirection.SqrMagnitude;

            if (wallLengthSqr < MathHelper.Epsilon)
            {
                return CheckPointWallCollisionCCD(ball, wall.Start, movementDirection, movementLength, out collisionPoint, out normal, out collisionTime);
            }




            float bestT = 0f;
            float bestS = 0f;
            float minDistance = float.MaxValue;

            for (int i = 0; i <= 10; i++) // Дискретизация для поиска минимума
            {
                float t = i / 10f;
                Vector2 ballPosAtT = ball.Position + t * movementDirection;
                Vector2 closestOnWall = GetClosestPointOnLineSegment(ballPosAtT, wall.Start, wall.End);
                float distance = Vector2.Distance(ballPosAtT, closestOnWall);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestT = t;
                    Vector2 wallVec = closestOnWall - wall.Start;
                    bestS = Vector2.Dot(wallVec, wallDirection) / wallLengthSqr;
                    bestS = MathHelper.Clamp(bestS, 0f, 1f);
                }
            }

            if (minDistance <= ball.Radius)
            {
                collisionTime = bestT;
                Vector2 ballPosAtCollision = ball.Position + bestT * movementDirection;
                Vector2 wallPosAtCollision = wall.Start + bestS * wallDirection;

                collisionPoint = wallPosAtCollision;

                Vector2 direction = ballPosAtCollision - wallPosAtCollision;
                if (direction.SqrMagnitude > MathHelper.Epsilon)
                {
                    normal = direction.Normalized;
                }
                else
                {
                    Vector2 startToWall = ball.Position - closestPointStart;
                    if (startToWall.SqrMagnitude > MathHelper.Epsilon)
                    {
                        normal = startToWall.Normalized;
                    }
                    else
                    {
                        normal = GetWallNormal(wall);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка коллизии с точечной стеной (для случая, когда стена - это точка)
        /// </summary>
        private static bool CheckPointWallCollisionCCD(Ball ball, Vector2 wallPoint, Vector2 movementDirection, float movementLength, out Vector2 collisionPoint, out Vector2 normal, out float collisionTime)
        {
            collisionPoint = wallPoint;
            normal = Vector2.Zero;
            collisionTime = 0f;

            float distanceToWall = Vector2.Distance(ball.Position, wallPoint);

            if (distanceToWall <= ball.Radius)
            {
                collisionTime = 0f;
                normal = (ball.Position - wallPoint).Normalized;
                return true;
            }

            Vector2 directionToWall = wallPoint - ball.Position;
            float dotProduct = Vector2.Dot(movementDirection.Normalized, directionToWall.Normalized);

            if (dotProduct <= 0)
                return false; // Шарик удаляется от стены


            float a = movementDirection.SqrMagnitude;
            float b = 2 * Vector2.Dot(ball.Position - wallPoint, movementDirection);
            float c = Vector2.SqrDistance(ball.Position, wallPoint) - ball.Radius * ball.Radius;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return false;

            float sqrtDiscriminant = MathHelper.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);

            float t = float.MaxValue;
            if (t1 >= 0 && t1 <= 1)
                t = MathHelper.Min(t, t1);
            if (t2 >= 0 && t2 <= 1)
                t = MathHelper.Min(t, t2);

            if (t < float.MaxValue)
            {
                collisionTime = t;
                Vector2 ballPosAtCollision = ball.Position + t * movementDirection;
                normal = (ballPosAtCollision - wallPoint).Normalized;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка, находится ли точка внутри прямоугольника
        /// </summary>
        public static bool IsPointInRectangle(Vector2 point, Vector2 rectMin, Vector2 rectMax)
        {
            return point.X >= rectMin.X && point.X <= rectMax.X &&
                   point.Y >= rectMin.Y && point.Y <= rectMax.Y;
        }

        /// <summary>
        /// Проверка пересечения окружности с прямоугольником
        /// </summary>
        public static bool CheckCircleRectangleCollision(Vector2 circleCenter, float radius, Vector2 rectMin, Vector2 rectMax)
        {
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X),
                MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y)
            );

            return Vector2.Distance(circleCenter, closestPoint) <= radius;
        }

        /// <summary>
        /// Проверка пересечения двух окружностей
        /// </summary>
        public static bool CheckCircleCircleCollision(Vector2 center1, float radius1, Vector2 center2, float radius2)
        {
            float distance = Vector2.Distance(center1, center2);
            return distance <= (radius1 + radius2);
        }
    }
}
