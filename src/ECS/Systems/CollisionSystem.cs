using System;
using Leopotam.EcsLite;
using BHS.Engine.ECS.Components;
using BHS.Engine.Physics;
using BHS.Engine.Scene;
using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS.Systems
{
    /// <summary>
    /// Система обработки коллизий
    /// </summary>
    public class CollisionSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var transformPool = world.GetPool<TransformComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var circleColliderPool = world.GetPool<CircleColliderComponent>();
            var wallColliderPool = world.GetPool<WallColliderComponent>();
            var sceneLinkPool = world.GetPool<SceneObjectLink>();

            // Фильтр для шариков (сущности с Transform, Velocity, CircleCollider)
            var ballFilter = world.Filter<TransformComponent>()
                .Inc<VelocityComponent>()
                .Inc<CircleColliderComponent>()
                .End();

            // Фильтр для стен (сущности с WallCollider)
            var wallFilter = world.Filter<WallColliderComponent>().End();

            // Проверяем коллизии каждого шарика со всеми стенами
            foreach (var ballEntity in ballFilter)
            {
                ref var ballTransform = ref transformPool.Get(ballEntity);
                ref var ballVelocity = ref velocityPool.Get(ballEntity);
                ref var ballCollider = ref circleColliderPool.Get(ballEntity);

                foreach (var wallEntity in wallFilter)
                {
                    ref var wallCollider = ref wallColliderPool.Get(wallEntity);

                    // Создаем временные объекты для проверки коллизии
                    var ball = new Ball(ballTransform.Position, ballCollider.Radius, ballVelocity.Velocity);
                    var wall = new Wall(wallCollider.Start, wallCollider.End);

                    // Проверяем коллизию
                    if (CollisionDetector.CheckBallWallCollision(ball, wall, out var collisionPoint, out var normal))
                    {
                        // Рассчитываем отражение
                        var newVelocity = PhysicsCalculator.CalculateBallWallReflection(ball, wall, collisionPoint, normal);
                        
                        // Обновляем скорость в ECS
                        ballVelocity.Velocity = newVelocity;

                        // Синхронизируем с объектом сцены
                        if (sceneLinkPool.Has(ballEntity))
                        {
                            ref var sceneLink = ref sceneLinkPool.Get(ballEntity);
                            if (sceneLink.Reference is Ball sceneBall)
                            {
                                sceneBall.Velocity = newVelocity;
                            }
                        }

                        // Выводим информацию о столкновении
                        Console.WriteLine($"Шарик столкнулся со стеной ID: {wallCollider.WallId}");
                        Console.WriteLine($"  Позиция шарика: {ballTransform.Position}");
                        Console.WriteLine($"  Новая скорость: {newVelocity}");
                        Console.WriteLine($"  Точка столкновения: {collisionPoint}");
                        Console.WriteLine($"  Нормаль: {normal}");
                        Console.WriteLine();
                    }
                    else
                    {
                        // Отладочная информация для проверки расстояний
                        if (ballTransform.Position.X > 90 || ballTransform.Position.Y > 90 || 
                            ballTransform.Position.X < 10 || ballTransform.Position.Y < 10)
                        {
                            float distanceToWall = Vector2.Distance(ballTransform.Position, wallCollider.Start);
                            Console.WriteLine($"DEBUG: Шарик близко к стене {wallCollider.WallId}, расстояние: {distanceToWall:F2}");
                        }
                    }
                }
            }
        }
    }
}
