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
                Vector2 velocity = ballVelocity.Velocity;

                float distancePerFrame = velocity.Magnitude * deltaTime;

                int numSubSteps = 1;
                if (distancePerFrame > ballCollider.Radius)
                {
                    numSubSteps = (int)Math.Ceiling(distancePerFrame / ballCollider.Radius);
                    numSubSteps = Math.Min(numSubSteps, 20); // Ограничиваем максимум
                }

                Vector2 finalPosition = currentPosition;
                Vector2 finalVelocity = velocity;

                for (int step = 0; step < numSubSteps; step++)
                {
                    float stepDeltaTime = deltaTime / numSubSteps;
                    Vector2 stepPosition = finalPosition;
                    Vector2 stepNextPosition = finalPosition + finalVelocity * stepDeltaTime;

                    var ball = new Ball(stepPosition, ballCollider.Radius, finalVelocity);

                    bool stepCollisionDetected = false;

                    foreach (var wallEntity in wallFilter)
                    {
                        ref var wallCollider = ref wallColliderPool.Get(wallEntity);

                        var wall = new Wall(wallCollider.Start, wallCollider.End);

                        if (AdvancedCollisionDetector.CheckBallWallCollisionCCD(ball, wall, stepNextPosition, out var collisionPoint, out var normal, out var collisionTime))
                        {
                            Vector2 collisionPosition = stepPosition + finalVelocity * collisionTime * stepDeltaTime;

                            var newVelocity = AdvancedPhysicsCalculator.CalculateAdvancedReflection(ball, wall, collisionPoint, normal, 0f);

                            finalPosition = collisionPosition;
                            finalVelocity = newVelocity;

                            Console.WriteLine($"Шарик столкнулся со стеной ID: {wallCollider.WallId} (CCD, шаг {step + 1}/{numSubSteps})");
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

                            stepCollisionDetected = true;
                            break;
                        }
                        else if (AdvancedCollisionDetector.CheckBallWallCollisionAdvanced(ball, wall, out var fallbackCollisionPoint, out var fallbackNormal, out var penetrationDepth))
                        {
                            Vector2 correctedPosition = AdvancedPhysicsCalculator.CorrectBallPosition(ball, fallbackCollisionPoint, fallbackNormal, penetrationDepth);

                            var newVelocity = AdvancedPhysicsCalculator.CalculateAdvancedReflection(ball, wall, fallbackCollisionPoint, fallbackNormal, penetrationDepth);

                            finalPosition = correctedPosition;
                            finalVelocity = newVelocity;

                            Console.WriteLine($"Шарик столкнулся со стеной ID: {wallCollider.WallId} (Fallback, шаг {step + 1}/{numSubSteps})");
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

                            stepCollisionDetected = true;
                            break;
                        }
                    }

                    if (!stepCollisionDetected)
                    {
                        finalPosition = stepNextPosition;
                    }
                    else
                    {
                        break;
                    }

                    Vector2 minBounds = new Vector2(ballCollider.Radius, ballCollider.Radius);
                    Vector2 maxBounds = new Vector2(100 - ballCollider.Radius, 100 - ballCollider.Radius);

                    if (finalPosition.X < minBounds.X)
                    {
                        finalPosition = new Vector2(minBounds.X, finalPosition.Y);
                        finalVelocity = new Vector2(-finalVelocity.X, finalVelocity.Y);
                        break;
                    }
                    else if (finalPosition.X > maxBounds.X)
                    {
                        finalPosition = new Vector2(maxBounds.X, finalPosition.Y);
                        finalVelocity = new Vector2(-finalVelocity.X, finalVelocity.Y);
                        break;
                    }

                    if (finalPosition.Y < minBounds.Y)
                    {
                        finalPosition = new Vector2(finalPosition.X, minBounds.Y);
                        finalVelocity = new Vector2(finalVelocity.X, -finalVelocity.Y);
                        break;
                    }
                    else if (finalPosition.Y > maxBounds.Y)
                    {
                        finalPosition = new Vector2(finalPosition.X, maxBounds.Y);
                        finalVelocity = new Vector2(finalVelocity.X, -finalVelocity.Y);
                        break;
                    }
                }

                ballTransform.Position = finalPosition;
                ballVelocity.Velocity = finalVelocity;

                if (sceneLinkPool.Has(ballEntity))
                {
                    ref var sceneLink = ref sceneLinkPool.Get(ballEntity);
                    if (sceneLink.Reference is Ball sceneBall)
                    {
                        sceneBall.Position = finalPosition;
                        sceneBall.Velocity = finalVelocity;
                    }
                }
            }
        }
    }
}
