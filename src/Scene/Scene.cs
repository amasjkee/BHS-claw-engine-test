using System;
using System.Collections.Generic;
using System.Linq;
using BHS.Engine.Core.Interfaces;
using BHS.Engine.Core.Math;

namespace BHS.Engine.Scene
{
    /// <summary>
    /// Контейнер для управления объектами сцены - стенами и шариками
    /// </summary>
    public class GameScene
    {
        private readonly List<ISceneObject> _objects = new List<ISceneObject>();
        private readonly List<Wall> _walls = new List<Wall>();
        private readonly List<Ball> _balls = new List<Ball>();

        public IReadOnlyList<ISceneObject> Objects => _objects.AsReadOnly();
        public IReadOnlyList<Wall> Walls => _walls.AsReadOnly();
        public IReadOnlyList<Ball> Balls => _balls.AsReadOnly();
        public int ObjectCount => _objects.Count;

        /// <summary>
        /// Добавляю объект в сцену
        /// </summary>
        public void AddObject(ISceneObject sceneObject)
        {
            if (sceneObject == null)
                throw new ArgumentNullException(nameof(sceneObject));

            _objects.Add(sceneObject);

            switch (sceneObject)
            {
                case Wall wall:
                    _walls.Add(wall);
                    break;
                case Ball ball:
                    _balls.Add(ball);
                    break;
            }
        }

        /// <summary>
        /// Удаляю объект из сцены
        /// </summary>
        public bool RemoveObject(ISceneObject sceneObject)
        {
            if (sceneObject == null)
                return false;

            bool removed = _objects.Remove(sceneObject);

            if (removed)
            {
                switch (sceneObject)
                {
                    case Wall wall:
                        _walls.Remove(wall);
                        break;
                    case Ball ball:
                        _balls.Remove(ball);
                        break;
                }
            }

            return removed;
        }

        /// <summary>
        /// Удаляю объект по ID
        /// </summary>
        public bool RemoveObject(Guid id)
        {
            var obj = _objects.FirstOrDefault(o => o.Id == id);
            return obj != null && RemoveObject(obj);
        }

        /// <summary>
        /// Нахожу объект по ID
        /// </summary>
        public ISceneObject FindObject(Guid id)
        {
            return _objects.FirstOrDefault(o => o.Id == id);
        }

        /// <summary>
        /// Нахожу объект по типу
        /// </summary>
        public T FindObject<T>() where T : class, ISceneObject
        {
            return _objects.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Нахожу все объекты определенного типа
        /// </summary>
        public IEnumerable<T> FindObjects<T>() where T : class, ISceneObject
        {
            return _objects.OfType<T>();
        }

        /// <summary>
        /// Обновляю все объекты сцены
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var obj in _objects)
            {
                if (obj is SceneObject sceneObj)
                {
                    sceneObj.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Очищаю всю сцену
        /// </summary>
        public void Clear()
        {
            _objects.Clear();
            _walls.Clear();
            _balls.Clear();
        }

        /// <summary>
        /// Очищаю только шарики
        /// </summary>
        public void ClearBalls()
        {
            _balls.Clear();
            _objects.RemoveAll(obj => obj is Ball);
        }

        /// <summary>
        /// Создаю прямоугольную коробочку из стен
        /// </summary>
        public void CreateBox(Vector2 min, Vector2 max)
        {
            AddObject(new Wall(new Vector2(min.X, min.Y), new Vector2(max.X, min.Y)));
            AddObject(new Wall(new Vector2(max.X, min.Y), new Vector2(max.X, max.Y)));
            AddObject(new Wall(new Vector2(max.X, max.Y), new Vector2(min.X, max.Y)));
            AddObject(new Wall(new Vector2(min.X, max.Y), new Vector2(min.X, min.Y)));
        }

        /// <summary>
        /// Создаю шарик в сцене
        /// </summary>
        public Ball CreateBall(Vector2 position, float radius, Vector2 velocity)
        {
            var ball = new Ball(position, radius, velocity);
            AddObject(ball);
            return ball;
        }

        public override string ToString()
        {
            return $"Scene (Objects: {ObjectCount}, Walls: {_walls.Count}, Balls: {_balls.Count})";
        }
    }
}
