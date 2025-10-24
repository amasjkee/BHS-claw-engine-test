using BHS.Engine.Core.Math;
using BHS.Engine.Scene;

namespace BHS.Engine.Physics
{
    /// <summary>
    /// Калькулятор физических расчетов для отражений и столкновений
    /// </summary>
    public static class PhysicsCalculator
    {
        /// <summary>
        /// Расчет отражения шарика от стены
        /// </summary>
        public static Vector2 CalculateBallWallReflection(Ball ball, Wall wall, Vector2 collisionPoint, Vector2 normal)
        {
            if (ball == null || wall == null)
                return ball?.Velocity ?? Vector2.Zero;

            // Нормализуем нормаль
            Vector2 normalizedNormal = normal.Normalized;
            
            // Формула отражения: V' = V - 2 * (V · N) * N
            float dotProduct = Vector2.Dot(ball.Velocity, normalizedNormal);
            Vector2 reflection = ball.Velocity - 2 * dotProduct * normalizedNormal;

            return reflection;
        }

        /// <summary>
        /// Расчет отражения между двумя шариками (эластичное столкновение)
        /// </summary>
        public static (Vector2 velocity1, Vector2 velocity2) CalculateBallBallReflection(
            Ball ball1, Ball ball2, Vector2 collisionPoint, Vector2 normal)
        {
            if (ball1 == null || ball2 == null)
                return (ball1?.Velocity ?? Vector2.Zero, ball2?.Velocity ?? Vector2.Zero);

            Vector2 normalizedNormal = normal.Normalized;
            
            // Относительная скорость
            Vector2 relativeVelocity = ball2.Velocity - ball1.Velocity;
            
            // Скорость вдоль нормали
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normalizedNormal);
            
            // Не разделяем, если объекты уже разделяются
            if (velocityAlongNormal > 0)
                return (ball1.Velocity, ball2.Velocity);

            // Коэффициент восстановления (1.0 = полностью эластичное, 0.0 = неэластичное)
            float restitution = 1.0f;
            
            // Импульс
            float impulse = -(1 + restitution) * velocityAlongNormal;
            impulse /= (1 / ball1.Mass + 1 / ball2.Mass);

            // Применяем импульс
            Vector2 impulseVector = impulse * normalizedNormal;
            
            Vector2 newVelocity1 = ball1.Velocity - impulseVector / ball1.Mass;
            Vector2 newVelocity2 = ball2.Velocity + impulseVector / ball2.Mass;

            return (newVelocity1, newVelocity2);
        }

        /// <summary>
        /// Расчет силы трения (простая модель)
        /// </summary>
        public static Vector2 CalculateFriction(Vector2 velocity, float frictionCoefficient, float deltaTime)
        {
            if (velocity.SqrMagnitude < MathHelper.Epsilon)
                return Vector2.Zero;

            Vector2 frictionForce = -velocity.Normalized * frictionCoefficient;
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

        /// <summary>
        /// Применение демпфирования к скорости
        /// </summary>
        public static Vector2 ApplyDamping(Vector2 velocity, float dampingFactor, float deltaTime)
        {
            float damping = MathHelper.Max(0, 1 - dampingFactor * deltaTime);
            return velocity * damping;
        }
    }
}
