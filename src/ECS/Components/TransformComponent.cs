using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS.Components
{
    /// <summary>
    /// Компонент позиции объекта в ECS
    /// </summary>
    public struct TransformComponent
    {
        public Vector2 Position;

        public TransformComponent(Vector2 position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"Transform(Position: {Position})";
        }
    }
}
