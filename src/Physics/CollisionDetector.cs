using BHS.Engine.Core.Math;
using BHS.Engine.Scene;

namespace BHS.Engine.Physics
{
    /// <summary>
    /// Детектор коллизий для 2D объектов
    /// </summary>
    public static class CollisionDetector
    {
        /// <summary>
        /// Проверка коллизии между шариком и стеной
        /// </summary>
        public static bool CheckBallWallCollision(Ball ball, Wall wall, out Vector2 collisionPoint, out Vector2 normal)
        {
            collisionPoint = Vector2.Zero;
            normal = Vector2.Zero;

            if (ball == null || wall == null)
                return false;

            // Получаем ближайшую точку на стене к центру шарика
            Vector2 closestPoint = wall.GetClosestPoint(ball.Position);
            
            // Проверяем, находится ли ближайшая точка в пределах отрезка стены
            Vector2 wallDirection = wall.End - wall.Start;
            float wallLengthSqr = wallDirection.SqrMagnitude;
            
            if (wallLengthSqr < MathHelper.Epsilon)
            {
                // Стена - это точка
                closestPoint = wall.Start;
            }
            else
            {
                float t = Vector2.Dot(ball.Position - wall.Start, wallDirection) / wallLengthSqr;
                t = MathHelper.Clamp(t, 0f, 1f);
                closestPoint = wall.Start + t * wallDirection;
            }

            // Расстояние от центра шарика до ближайшей точки на стене
            float distance = Vector2.Distance(ball.Position, closestPoint);

            // Если расстояние меньше радиуса, происходит коллизия
            if (distance <= ball.Radius)
            {
                collisionPoint = closestPoint;
                
                // Вычисляем нормаль от стены к шарику
                Vector2 direction = ball.Position - closestPoint;
                if (direction.SqrMagnitude > MathHelper.Epsilon)
                {
                    normal = direction.Normalized;
                }
                else
                {
                    // Если шарик точно на стене, используем нормаль стены
                    normal = wall.Normal;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка коллизии между двумя шариками
        /// </summary>
        public static bool CheckBallBallCollision(Ball ball1, Ball ball2, out Vector2 collisionPoint, out Vector2 normal)
        {
            collisionPoint = Vector2.Zero;
            normal = Vector2.Zero;

            if (ball1 == null || ball2 == null || ball1 == ball2)
                return false;

            float distance = Vector2.Distance(ball1.Position, ball2.Position);
            float combinedRadius = ball1.Radius + ball2.Radius;

            if (distance <= combinedRadius)
            {
                // Точка коллизии - середина между центрами шариков
                collisionPoint = (ball1.Position + ball2.Position) / 2f;
                
                // Нормаль направлена от ball1 к ball2
                Vector2 direction = ball2.Position - ball1.Position;
                if (direction.SqrMagnitude > MathHelper.Epsilon)
                {
                    normal = direction.Normalized;
                }
                else
                {
                    // Если шарики в одной точке, используем случайную нормаль
                    normal = new Vector2(1, 0);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка, находится ли точка внутри прямоугольника
        /// </summary>
        public static bool IsPointInRectangle(Vector2 point, Vector2 min, Vector2 max)
        {
            return point.X >= min.X && point.X <= max.X && 
                   point.Y >= min.Y && point.Y <= max.Y;
        }

        /// <summary>
        /// Проверка, пересекается ли окружность с прямоугольником
        /// </summary>
        public static bool CheckCircleRectangleCollision(Vector2 circleCenter, float radius, Vector2 rectMin, Vector2 rectMax)
        {
            // Находим ближайшую точку на прямоугольнике к центру окружности
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X),
                MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y)
            );

            // Проверяем расстояние
            return Vector2.Distance(circleCenter, closestPoint) <= radius;
        }

        /// <summary>
        /// Проверка, пересекаются ли два отрезка
        /// </summary>
        public static bool CheckLineLineIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            Vector2 d1 = line1End - line1Start;
            Vector2 d2 = line2End - line2Start;
            Vector2 d3 = line1Start - line2Start;

            float cross = d1.X * d2.Y - d1.Y * d2.X;
            
            if (MathHelper.Abs(cross) < MathHelper.Epsilon)
                return false; // Параллельные линии

            float t1 = (d3.X * d2.Y - d3.Y * d2.X) / cross;
            float t2 = (d3.X * d1.Y - d3.Y * d1.X) / cross;

            if (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1)
            {
                intersectionPoint = line1Start + t1 * d1;
                return true;
            }

            return false;
        }
    }
}
