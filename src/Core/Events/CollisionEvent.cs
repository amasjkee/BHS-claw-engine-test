using BHS.Engine.Scene;
using BHS.Engine.Core.Math;

namespace BHS.Engine.Core.Events
{
    /// <summary>
    /// Событие коллизии шарика со стеной
    /// </summary>
    public class CollisionEvent
    {
        public Ball Ball { get; set; }
        public Wall Wall { get; set; }
        public int WallId { get; set; }
        public Vector2 CollisionPoint { get; set; }
        public Vector2 Normal { get; set; }
    }
}
