using Leopotam.EcsLite;
using BHS.Engine.ECS.Components;

namespace BHS.Engine.ECS.Systems
{
    /// <summary>
    /// Система движения - двигаю объекты по их скорости и синхронизирую с сценой
    /// </summary>
    public class MovementSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var transformPool = world.GetPool<TransformComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var sceneLinkPool = world.GetPool<SceneObjectLink>();

            var filter = world.Filter<TransformComponent>().Inc<VelocityComponent>().End();

            float deltaTime = 1f / 60f;

            foreach (var entity in filter)
            {
                ref var transform = ref transformPool.Get(entity);
                ref var velocity = ref velocityPool.Get(entity);

                transform.Position += velocity.Velocity * deltaTime;

                if (velocity.Velocity.SqrMagnitude < 0.01f)
                {
                    velocity.Velocity = BHS.Engine.Core.Math.Vector2.Zero;
                }

                if (sceneLinkPool.Has(entity))
                {
                    ref var sceneLink = ref sceneLinkPool.Get(entity);
                    if (sceneLink.Reference != null)
                    {
                        sceneLink.Reference.Position = transform.Position;
                    }
                }
            }
        }
    }
}
