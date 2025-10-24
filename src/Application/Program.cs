using System;
using System.Threading;
using BHS.Engine.ECS;
using BHS.Engine.Scene;
using BHS.Engine.Core.Math;

namespace BHS.Engine.Application
{
    /// <summary>
    /// Консольное приложение для демонстрации физического движка
    /// </summary>
    class Program
    {
        private static EcsWorldManager _ecsManager;
        private static GameScene _scene;
        private static bool _running = true;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== BHS Claw Engine Test ===");
            Console.WriteLine("Инициализация физического движка с ECS...");
            Console.WriteLine();

            try
            {
                Initialize();
                RunMainLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                Cleanup();
            }

            Console.WriteLine("Приложение завершено.");
        }

        /// <summary>
        /// Инициализирую ECS менеджер и создаю тестовую сцену
        /// </summary>
        private static void Initialize()
        {
            _ecsManager = new EcsWorldManager();
            _scene = _ecsManager.CreateConsoleTestScene();
            _ecsManager.InitializeScene(_scene);

            Console.WriteLine("Сцена инициализирована:");
            Console.WriteLine($"  Стены: {_scene.Walls.Count}");
            Console.WriteLine($"  Шарики: {_scene.Balls.Count}");
            Console.WriteLine($"  ECS сущности: {_ecsManager.GetWorldInfo()}");
            Console.WriteLine();

            if (_scene.Balls.Count > 0)
            {
                var ball = _scene.Balls[0];
                Console.WriteLine($"Начальная позиция шарика: {ball.Position}");
                Console.WriteLine($"Начальная скорость шарика: {ball.Velocity}");
                Console.WriteLine($"Радиус шарика: {ball.Radius}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Главный цикл приложения - обновляю ECS и вывожу позицию шарика
        /// </summary>
        private static void RunMainLoop()
        {
            Console.WriteLine("Запуск главного цикла...");
            Console.WriteLine("Нажмите 'q' для выхода, 's' для остановки/запуска");
            Console.WriteLine();

            int frameCount = 0;
            const int maxFrames = 1000;

            while (_running && frameCount < maxFrames)
            {
                _ecsManager.Update();
                _scene.Update(1f / 60f);

                if (frameCount % 10 == 0 && _scene.Balls.Count > 0)
                {
                    var ball = _scene.Balls[0];
                    Console.WriteLine($"Кадр {frameCount}: Позиция шарика = {ball.Position}, Скорость = {ball.Velocity}");
                }

                if (frameCount >= 500)
                {
                    _running = false;
                    Console.WriteLine("Демонстрация завершена");
                }

                frameCount++;
                Thread.Sleep(16);
            }

            if (frameCount >= maxFrames)
            {
                Console.WriteLine($"Достигнуто максимальное количество кадров ({maxFrames})");
            }
        }

        /// <summary>
        /// Очищаю ресурсы ECS
        /// </summary>
        private static void Cleanup()
        {
            _ecsManager?.Dispose();
            Console.WriteLine("Ресурсы очищены");
        }
    }
}
