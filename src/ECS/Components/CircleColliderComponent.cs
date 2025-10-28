using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS.Components
{
    /// <summary>
    /// Компонент коллайдера окружности (для шариков)
    /// </summary>
    public struct CircleColliderComponent
    {
        /// <summary>
        /// Радиус окружности
        /// </summary>
        public float Radius;

        /// <summary>
        /// Масса объекта (для физических расчетов)
        /// </summary>
        public float Mass;

        public CircleColliderComponent(float radius, float mass = 1.0f)
        {
            Radius = radius;
            Mass = mass;
        }

        public override string ToString()
        {
            return $"CircleCollider(Radius: {Radius:F2}, Mass: {Mass:F2})";
        }
    }
}
