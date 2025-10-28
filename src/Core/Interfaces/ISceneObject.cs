using BHS.Engine.Core.Math;

namespace BHS.Engine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для всех объектов сцены
    /// </summary>
    public interface ISceneObject
    {
        /// <summary>
        /// Уникальный идентификатор объекта
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Позиция объекта в мире
        /// </summary>
        Vector2 Position { get; set; }
    }
}
