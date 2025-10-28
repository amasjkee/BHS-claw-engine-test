using BHS.Engine.Core.Math;

namespace BHS.Engine.Scene
{
    /// <summary>
    /// Стена - отрезок между двумя точками для коллизий
    /// </summary>
    public class Wall : SceneObject
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        /// <summary>
        /// Получаю нормаль к стене (направлена наружу)
        /// </summary>
        public Vector2 Normal
        {
            get
            {
                Vector2 direction = (End - Start).Normalized;
                return new Vector2(-direction.Y, direction.X);
            }
        }

        public float Length => Vector2.Distance(Start, End);

        public Wall(Vector2 start, Vector2 end) : base(Vector2.Zero)
        {
            Start = start;
            End = end;
            Position = (start + end) / 2f;
        }

        /// <summary>
        /// Проверяю содержит ли стена точку с учетом толщины
        /// </summary>
        public bool ContainsPoint(Vector2 point, float thickness = 0.1f)
        {
            Vector2 wallDirection = End - Start;
            float wallLengthSqr = wallDirection.SqrMagnitude;

            if (wallLengthSqr < MathHelper.Epsilon)
                return Vector2.Distance(point, Start) <= thickness;

            float t = Vector2.Dot(point - Start, wallDirection) / wallLengthSqr;
            t = MathHelper.Clamp(t, 0f, 1f);

            Vector2 closestPoint = Start + t * wallDirection;
            return Vector2.Distance(point, closestPoint) <= thickness;
        }

        /// <summary>
        /// Нахожу ближайшую точку на стене к заданной точке
        /// </summary>
        public Vector2 GetClosestPoint(Vector2 point)
        {
            Vector2 wallDirection = End - Start;
            float wallLengthSqr = wallDirection.SqrMagnitude;

            if (wallLengthSqr < MathHelper.Epsilon)
                return Start;

            float t = Vector2.Dot(point - Start, wallDirection) / wallLengthSqr;
            t = MathHelper.Clamp(t, 0f, 1f);

            return Start + t * wallDirection;
        }

        public override string ToString()
        {
            return $"Wall (ID: {Id}, Start: {Start}, End: {End}, Length: {Length:F2})";
        }
    }
}
