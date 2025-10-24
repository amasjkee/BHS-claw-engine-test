using System;
using Leopotam.EcsLite;
using BHS.Engine.ECS.Components;
using BHS.Engine.Physics;
using BHS.Engine.Scene;
using BHS.Engine.Core.Math;
using BHS.Engine.Core.Events;

namespace BHS.Engine.ECS.Systems
{
    /// <summary>
    /// Система коллизий с защитой от туннелирования - обрабатываю столкновения шариков со стенами
    /// </summary>
    public class AdvancedCollisionSystem : IEcsRunSystem
    {
        public static event Action<CollisionEvent> OnCollision;
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var transformPool = world.GetPool<TransformComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var circleColliderPool = world.GetPool<CircleColliderComponent>();
            var wallColliderPool = world.GetPool<WallColliderComponent>();
            var sceneLinkPool = world.GetPool<SceneObjectLink>();

            var ballFilter = world.Filter<TransformComponent>()
                .Inc<VelocityComponent>()
                .Inc<CircleColliderComponent>()
                .End();

            var wallFilter = world.Filter<WallColliderComponent>().End();

            float deltaTime = 1f / 60f;

            foreach (var ballEntity in ballFilter)
            {
                ref var ballTransform = ref transformPool.Get(ballEntity);
                ref var ballVelocity = ref velocityPool.Get(ballEntity);
                ref var ballCollider = ref circleColliderPool.Get(ballEntity);

                Vector2 currentPosition = ballTransform.Position;
                Vector2 nextPosition = currentPosition + ballVelocity.Velocity * deltaTime;

                var ball = new Ball(currentPosition, ballCollider.Radius, ballVelocity.Velocity);

                foreach (var wallEntity in wallFilter)
                {
                    ref var wallCollider = ref wallColliderPool.Get(wallEntity);

                    var wall = new Wall(wallCollider.Start, wallCollider.End);

                    bool collisionDetected = false;
                    
                    if (AdvancedCollisionDetector.CheckBallWallCollisionCCD(ball, wall, nextPosition, out var collisionPoint, out var normal, out var collisionTime))
                    {
                        Vector2 collisionPosition = currentPosition + ballVelocity.Velocity * collisionTime * deltaTime;
                        
                        var newVelocity = AdvancedPhysicsCalculator.CalculateAdvancedReflection(ball, wall, collisionPoint, normal, 0f);
                        
                        ballTransform.Position = collisionPosition;
                        ballVelocity.Velocity = newVelocity;

                        if (sceneLinkPool.Has(ballEntity))
                        {
                            ref var sceneLink = ref sceneLinkPool.Get(ballEntity);
                            if (sceneLink.Reference is Ball sceneBall)
                            {
                                sceneBall.Position = collisionPosition;
                                sceneBall.Velocity = newVelocity;
                            }
                        }

                        Console.WriteLine($"Шарик столкнулся со стеной ID: {wallCollider.WallId} (CCD)");
                        Console.WriteLine($"  Позиция шарика: {collisionPosition}");
                        Console.WriteLine($"  Новая скорость: {newVelocity}");
                        Console.WriteLine($"  Время столкновения: {collisionTime:F3}");
                        Console.WriteLine($"  Точка столкновения: {collisionPoint}");
                        Console.WriteLine($"  Нормаль: {normal}");
                        Console.WriteLine();

                        OnCollision?.Invoke(new CollisionEvent
                        {
                            Ball = ball,
                            Wall = wall,
                            WallId = wallCollider.WallId,
                            CollisionPoint = collisionPoint,
                            Normal = normal
                        });

                        collisionDetected = true;
                    }
                    else if (AdvancedCollisionDetector.CheckBallWallCollisionAdvanced(ball, wall, out var fallbackCollisionPoint, out var fallbackNormal, out var penetrationDepth))
                    {
                        Vector2 correctedPosition = AdvancedPhysicsCalculator.CorrectBallPosition(ball, fallbackCollisionPoint, fallbackNormal, penetrationDepth);
                        
                        var newVelocity = AdvancedPhysicsCalculator.CalculateAdvancedReflection(ball, wall, fallbackCollisionPoint, fallbackNormal, penetrationDepth);
                        
                        ballTransform.Position = correctedPosition;
                        ballVelocity.Velocity = newVelocity;

                        if (sceneLinkPool.Has(ballEntity))
                        {
                            ref var sceneLink = ref sceneLinkPool.Get(ballEntity);
                            if (sceneLink.Reference is Ball sceneBall)
                            {
                                sceneBall.Position = correctedPosition;
                                sceneBall.Velocity = newVelocity;
                            }
                        }

                        Console.WriteLine($"Шарик столкнулся со стеной ID: {wallCollider.WallId} (Fallback)");
                        Console.WriteLine($"  Позиция шарика: {correctedPosition}");
                        Console.WriteLine($"  Новая скорость: {newVelocity}");
                        Console.WriteLine($"  Глубина проникновения: {penetrationDepth:F3}");
                        Console.WriteLine($"  Точка столкновения: {fallbackCollisionPoint}");
                        Console.WriteLine($"  Нормаль: {fallbackNormal}");
                        Console.WriteLine();

                        OnCollision?.Invoke(new CollisionEvent
                        {
                            Ball = ball,
                            Wall = wall,
                            WallId = wallCollider.WallId,
                            CollisionPoint = fallbackCollisionPoint,
                            Normal = fallbackNormal
                        });

                        collisionDetected = true;
                    }

                    if (collisionDetected)
                    {
                        break;
                    }
                }
            }
        }
    }
}
