using System;
using Leopotam.EcsLite;
using BHS.Engine.ECS.Components;
using BHS.Engine.ECS.Systems;
using BHS.Engine.Scene;
using BHS.Engine.Core.Math;

namespace BHS.Engine.ECS
{
    /// <summary>
    /// Менеджер ECS мира - создаю сущности и управляю системами
    /// </summary>
    public class EcsWorldManager : IDisposable
    {
        private EcsWorld _world;
        private EcsSystems _systems;
        private bool _disposed = false;

        public EcsWorld World => _world;
        public EcsSystems Systems => _systems;

        public EcsWorldManager()
        {
            Initialize();
        }

        /// <summary>
        /// Инициализирую ECS мир и регистрирую системы
        /// </summary>
        private void Initialize()
        {
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            _systems.Add(new MovementSystem());
            _systems.Add(new AdvancedCollisionSystem());

            _systems.Init();
        }

        /// <summary>
        /// Создаю ECS сущность для шарика
        /// </summary>
        public int CreateBallEntity(Ball ball)
        {
            if (ball == null)
                throw new ArgumentNullException(nameof(ball));

            int entity = _world.NewEntity();

            _world.GetPool<TransformComponent>().Add(entity) = new TransformComponent(ball.Position);
            _world.GetPool<VelocityComponent>().Add(entity) = new VelocityComponent(ball.Velocity);
            _world.GetPool<CircleColliderComponent>().Add(entity) = new CircleColliderComponent(ball.Radius, ball.Mass);
            _world.GetPool<SceneObjectLink>().Add(entity) = new SceneObjectLink(ball);

            return entity;
        }

        /// <summary>
        /// Создаю ECS сущность для стены
        /// </summary>
        public int CreateWallEntity(Wall wall, int wallId = 0)
        {
            if (wall == null)
                throw new ArgumentNullException(nameof(wall));

            int entity = _world.NewEntity();

            _world.GetPool<TransformComponent>().Add(entity) = new TransformComponent(wall.Position);
            _world.GetPool<WallColliderComponent>().Add(entity) = new WallColliderComponent(wall.Start, wall.End, wallId);
            _world.GetPool<SceneObjectLink>().Add(entity) = new SceneObjectLink(wall);

            return entity;
        }

        /// <summary>
        /// Инициализирую сцену в ECS - создаю сущности для всех объектов
        /// </summary>
        public void InitializeScene(GameScene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            int wallId = 0;
            foreach (var wall in scene.Walls)
            {
                CreateWallEntity(wall, wallId++);
            }

            foreach (var ball in scene.Balls)
            {
                CreateBallEntity(ball);
            }
        }

        /// <summary>
        /// Обновляю ECS мир каждый кадр
        /// </summary>
        public void Update()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EcsWorldManager));

            _systems.Run();
        }

        /// <summary>
        /// Получаю информацию о количестве сущностей
        /// </summary>
        public string GetWorldInfo()
        {
            if (_disposed)
                return "World disposed";

            var entities = new int[1000];
            int count = _world.GetAllEntities(ref entities);
            return $"Entities: {count}";
        }

        /// <summary>
        /// Создаю простую тестовую сцену с коробочкой и шариком
        /// </summary>
        public GameScene CreateTestScene()
        {
            var scene = new GameScene();

            Vector2 min = new Vector2(0, 0);
            Vector2 max = new Vector2(50, 50);
            scene.CreateBox(min, max);

            Vector2 ballPosition = new Vector2(25, 25);
            float ballRadius = 5f;
            Vector2 ballVelocity = new Vector2(2, 1.5f);
            scene.CreateBall(ballPosition, ballRadius, ballVelocity);

            return scene;
        }

        /// <summary>
        /// Создаю продвинутую сцену только с коробочкой для GUI режима
        /// </summary>
        public GameScene CreateAdvancedTestScene()
        {
            var scene = new GameScene();

            Vector2 min = new Vector2(0, 0);
            Vector2 max = new Vector2(100, 100);
            scene.CreateBox(min, max);

            return scene;
        }

        /// <summary>
        /// Создаю сцену с рандомной стеной внутри коробки (фиксированного размера)
        /// </summary>
        public GameScene CreateSceneWithRandomWall()
        {
            var scene = new GameScene();

            Vector2 min = new Vector2(0, 0);
            Vector2 max = new Vector2(100, 100);
            scene.CreateBox(min, max);

            Random random = new Random();

            float wallLength = 40f;

            Vector2 start = new Vector2(
                (float)(random.NextDouble() * 50 + 25), // между 25 и 75
                (float)(random.NextDouble() * 50 + 25)
            );

            float angle = (float)(random.NextDouble() * 2 * Math.PI);

            Vector2 end = new Vector2(
                start.X + (float)(Math.Cos(angle) * wallLength),
                start.Y + (float)(Math.Sin(angle) * wallLength)
            );

            var randomWall = new Wall(start, end);
            scene.AddObject(randomWall);

            return scene;
        }

        /// <summary>
        /// Создаю сцену для консольного режима с неподвижным шариком
        /// </summary>
        public GameScene CreateConsoleTestScene()
        {
            var scene = new GameScene();

            Vector2 min = new Vector2(0, 0);
            Vector2 max = new Vector2(100, 100);
            scene.CreateBox(min, max);

            Vector2 ballPosition = new Vector2(50, 50);
            float ballRadius = 3f;
            Vector2 ballVelocity = Vector2.Zero;
            scene.CreateBall(ballPosition, ballRadius, ballVelocity);

            return scene;
        }

        /// <summary>
        /// Добавляет шарик в ECS мир
        /// </summary>
        public void AddBallToEcs(Ball ball)
        {
            if (_world == null) return;

            var ballEntity = _world.NewEntity();

            var transformPool = _world.GetPool<TransformComponent>();
            var velocityPool = _world.GetPool<VelocityComponent>();
            var circleColliderPool = _world.GetPool<CircleColliderComponent>();
            var sceneLinkPool = _world.GetPool<SceneObjectLink>();

            transformPool.Add(ballEntity) = new TransformComponent { Position = ball.Position };
            velocityPool.Add(ballEntity) = new VelocityComponent { Velocity = ball.Velocity };
            circleColliderPool.Add(ballEntity) = new CircleColliderComponent { Radius = ball.Radius };
            sceneLinkPool.Add(ballEntity) = new SceneObjectLink { Reference = ball };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _systems?.Destroy();
                _world?.Destroy();
                _disposed = true;
            }
        }
    }
}
