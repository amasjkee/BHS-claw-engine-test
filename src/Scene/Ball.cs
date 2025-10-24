using BHS.Engine.Core.Math;

namespace BHS.Engine.Scene
{
    /// <summary>
    /// Шарик - окружность с радиусом и скоростью для физической симуляции
    /// </summary>
    public class Ball : SceneObject
    {
        public float Radius { get; set; }
        public Vector2 Velocity { get; set; }
        public float Mass { get; set; } = 1.0f;

        public Ball(Vector2 position, float radius, Vector2 velocity) : base(position)
        {
            Radius = radius;
            Velocity = velocity;
        }

        /// <summary>
        /// Двигаю шарик по его скорости
        /// </summary>
        public override void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }

        /// <summary>
        /// Проверяю столкновение с другим шариком
        /// </summary>
        public bool Intersects(Ball other)
        {
            float distance = Vector2.Distance(Position, other.Position);
            return distance <= (Radius + other.Radius);
        }

        /// <summary>
        /// Проверяю содержит ли шарик точку
        /// </summary>
        public bool ContainsPoint(Vector2 point)
        {
            return Vector2.Distance(Position, point) <= Radius;
        }

        /// <summary>
        /// Отражаю шарик от стены по нормали
        /// </summary>
        public void Reflect(Vector2 normal)
        {
            Velocity = Vector2.Reflect(Velocity, normal.Normalized);
        }

        /// <summary>
        /// Останавливаю шарик
        /// </summary>
        public void Stop()
        {
            Velocity = Vector2.Zero;
        }

        /// <summary>
        /// Устанавливаю новую скорость
        /// </summary>
        public void SetVelocity(Vector2 newVelocity)
        {
            Velocity = newVelocity;
        }

        /// <summary>
        /// Добавляю скорость к текущей
        /// </summary>
        public void AddVelocity(Vector2 velocityDelta)
        {
            Velocity += velocityDelta;
        }

        public override string ToString()
        {
            return $"Ball (ID: {Id}, Position: {Position}, Radius: {Radius:F2}, Velocity: {Velocity})";
        }
    }
}
