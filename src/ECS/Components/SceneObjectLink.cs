using BHS.Engine.Core.Interfaces;

namespace BHS.Engine.ECS.Components
{
    /// <summary>
    /// Компонент связи с объектом сцены для синхронизации
    /// </summary>
    public struct SceneObjectLink
    {
        /// <summary>
        /// Ссылка на объект сцены
        /// </summary>
        public ISceneObject Reference;

        public SceneObjectLink(ISceneObject reference)
        {
            Reference = reference;
        }

        public override string ToString()
        {
            return $"SceneObjectLink(Reference: {Reference?.GetType().Name ?? "null"})";
        }
    }
}
