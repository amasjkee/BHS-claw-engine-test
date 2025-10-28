using System;
using BHS.Engine.Core.Interfaces;
using BHS.Engine.Core.Math;

namespace BHS.Engine.Scene
{
    /// <summary>
    /// Базовый класс для всех объектов сцены - стены и шарики
    /// </summary>
    public abstract class SceneObject : ISceneObject
    {
        public Guid Id { get; }
        public Vector2 Position { get; set; }

        protected SceneObject(Vector2 position)
        {
            Id = Guid.NewGuid();
            Position = position;
        }

        /// <summary>
        /// Обновляю объект каждый кадр
        /// </summary>
        public virtual void Update(float deltaTime)
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name} (ID: {Id}, Position: {Position})";
        }
    }
}
