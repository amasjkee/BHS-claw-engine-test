using BHS.Engine.Core.Math;
using BHS.Engine.Scene;

namespace BHS.Engine.Physics
{
    /// <summary>
    /// Продвинутый калькулятор физических расчетов с поддержкой произвольных углов
    /// </summary>
    public static class AdvancedPhysicsCalculator
    {
        /// <summary>
        /// Расчет отражения шарика от произвольной стены с улучшенной точностью
        /// </summary>
        public static Vector2 CalculateAdvancedReflection(Ball ball, Wall wall, Vector2 collisionPoint, Vector2 normal, float penetrationDepth)
        {
            if (ball == null || wall == null)
                return ball?.Velocity ?? Vector2.Zero;

            Vector2 normalizedNormal = normal.Normalized;

            float dotProduct = Vector2.Dot(ball.Velocity, normalizedNormal);

            if (dotProduct >= 0)
                return ball.Velocity;

            Vector2 reflection = ball.Velocity - 2 * dotProduct * normalizedNormal;

            return reflection;
        }

        /// <summary>
        /// Коррекция позиции шарика при проникновении в стену
        /// </summary>
        public static Vector2 CorrectBallPosition(Ball ball, Vector2 collisionPoint, Vector2 normal, float penetrationDepth)
        {
            if (ball == null)
                return ball.Position;

            Vector2 correction = normal * penetrationDepth;
            return ball.Position + correction;
        }

        /// <summary>
        /// Расчет отражения с учетом угла стены и коэффициента трения
        /// </summary>
        public static Vector2 CalculateReflectionWithFriction(Ball ball, Wall wall, Vector2 collisionPoint, Vector2 normal, float frictionCoefficient = 0.1f)
        {
            if (ball == null || wall == null)
                return ball?.Velocity ?? Vector2.Zero;

            Vector2 normalizedNormal = normal.Normalized;

            float velocityAlongNormal = Vector2.Dot(ball.Velocity, normalizedNormal);
            Vector2 reflectionVelocity = ball.Velocity - 2 * velocityAlongNormal * normalizedNormal;

            Vector2 wallDirection = (wall.End - wall.Start).Normalized;
            float velocityAlongWall = Vector2.Dot(ball.Velocity, wallDirection);
            Vector2 frictionVelocity = wallDirection * velocityAlongWall * (1 - frictionCoefficient);

            return reflectionVelocity + frictionVelocity;
        }

        /// <summary>
        /// Расчет упругого столкновения между двумя шариками
        /// </summary>
        public static (Vector2 velocity1, Vector2 velocity2) CalculateElasticCollision(
            Ball ball1, Ball ball2, Vector2 collisionPoint, Vector2 normal, float restitution = 1.0f)
        {
            if (ball1 == null || ball2 == null)
                return (ball1?.Velocity ?? Vector2.Zero, ball2?.Velocity ?? Vector2.Zero);

            Vector2 normalizedNormal = normal.Normalized;

            Vector2 relativeVelocity = ball2.Velocity - ball1.Velocity;

            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normalizedNormal);

            if (velocityAlongNormal > 0)
                return (ball1.Velocity, ball2.Velocity);

            float impulse = -(1 + restitution) * velocityAlongNormal;
            impulse /= (1 / ball1.Mass + 1 / ball2.Mass);

            Vector2 impulseVector = impulse * normalizedNormal;

            Vector2 newVelocity1 = ball1.Velocity - impulseVector / ball1.Mass;
            Vector2 newVelocity2 = ball2.Velocity + impulseVector / ball2.Mass;

            return (newVelocity1, newVelocity2);
        }

        /// <summary>
        /// Расчет силы трения с учетом поверхности
        /// </summary>
        public static Vector2 CalculateSurfaceFriction(Vector2 velocity, Vector2 surfaceNormal, float frictionCoefficient, float deltaTime)
        {
            if (velocity.SqrMagnitude < MathHelper.Epsilon)
                return Vector2.Zero;

            Vector2 surfaceDirection = new Vector2(-surfaceNormal.Y, surfaceNormal.X);
            float velocityAlongSurface = Vector2.Dot(velocity, surfaceDirection);

            if (MathHelper.Abs(velocityAlongSurface) < MathHelper.Epsilon)
                return velocity;

            Vector2 frictionForce = -surfaceDirection * velocityAlongSurface * frictionCoefficient;
            return velocity + frictionForce * deltaTime;
        }

        /// <summary>
        /// Расчет гравитационного воздействия
        /// </summary>
        public static Vector2 CalculateGravity(Vector2 currentVelocity, Vector2 gravity, float deltaTime)
        {
            return currentVelocity + gravity * deltaTime;
        }

        /// <summary>
        /// Ограничение скорости максимальным значением
        /// </summary>
        public static Vector2 ClampVelocity(Vector2 velocity, float maxSpeed)
        {
            if (velocity.SqrMagnitude > maxSpeed * maxSpeed)
            {
                return velocity.Normalized * maxSpeed;
            }
            return velocity;
        }

        /// <summary>
        /// Применение демпфирования к скорости
        /// </summary>
        public static Vector2 ApplyDamping(Vector2 velocity, float dampingFactor, float deltaTime)
        {
            float damping = MathHelper.Max(0, 1 - dampingFactor * deltaTime);
            return velocity * damping;
        }

        /// <summary>
        /// Расчет угла отражения от поверхности
        /// </summary>
        public static float CalculateReflectionAngle(Vector2 incident, Vector2 normal)
        {
            Vector2 reflection = Vector2.Reflect(incident, normal);
            return MathHelper.Atan2(reflection.Y, reflection.X);
        }

        /// <summary>
        /// Проверка, движется ли объект в направлении цели
        /// </summary>
        public static bool IsMovingTowards(Vector2 position, Vector2 velocity, Vector2 target)
        {
            Vector2 directionToTarget = (target - position).Normalized;
            return Vector2.Dot(velocity.Normalized, directionToTarget) > 0;
        }

        /// <summary>
        /// Расчет времени до столкновения (упрощенная версия)
        /// </summary>
        public static float CalculateTimeToCollision(Vector2 position1, Vector2 velocity1, Vector2 position2, Vector2 velocity2, float combinedRadius)
        {
            Vector2 relativePosition = position2 - position1;
            Vector2 relativeVelocity = velocity2 - velocity1;

            float a = relativeVelocity.SqrMagnitude;
            float b = 2 * Vector2.Dot(relativePosition, relativeVelocity);
            float c = relativePosition.SqrMagnitude - combinedRadius * combinedRadius;

            if (a < MathHelper.Epsilon)
                return float.MaxValue; // Нет относительного движения

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return float.MaxValue; // Нет пересечения

            float t1 = (-b - MathHelper.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b + MathHelper.Sqrt(discriminant)) / (2 * a);

            if (t1 >= 0) return t1;
            if (t2 >= 0) return t2;

            return float.MaxValue;
        }
    }
}
