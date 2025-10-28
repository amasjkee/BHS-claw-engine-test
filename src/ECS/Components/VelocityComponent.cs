using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS.Components
{
    /// <summary>
    /// Компонент скорости объекта в ECS
    /// </summary>
    public struct VelocityComponent
    {
        public Vector2 Velocity;

        public VelocityComponent(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public override string ToString()
        {
            return $"Velocity(Velocity: {Velocity})";
        }
    }
}
