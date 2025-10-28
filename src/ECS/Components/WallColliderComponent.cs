using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS.Components
{
    /// <summary>
    /// Компонент коллайдера стены (отрезок)
    /// </summary>
    public struct WallColliderComponent
    {
        /// <summary>
        /// Начальная точка стены
        /// </summary>
        public Vector2 Start;

        /// <summary>
        /// Конечная точка стены
        /// </summary>
        public Vector2 End;

        /// <summary>
        /// Идентификатор стены для отладки
        /// </summary>
        public int WallId;

        public WallColliderComponent(Vector2 start, Vector2 end, int wallId = 0)
        {
            Start = start;
            End = end;
            WallId = wallId;
        }

        /// <summary>
        /// Длина стены
        /// </summary>
        public float Length => Vector2.Distance(Start, End);

        /// <summary>
        /// Нормаль к стене (направлена наружу)
        /// </summary>
        public Vector2 Normal
        {
            get
            {
                Vector2 direction = (End - Start).Normalized;
                return new Vector2(-direction.Y, direction.X);
            }
        }

        public override string ToString()
        {
            return $"WallCollider(Start: {Start}, End: {End}, ID: {WallId})";
        }
    }
}
