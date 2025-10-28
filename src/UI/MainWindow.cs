using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using BHS.Engine.ECS;
using BHS.Engine.Scene;
using BHS.Engine.Core.Math;

namespace BHS.Engine.UI
{
    public partial class MainWindow : Window
    {
        private Canvas _gameCanvas;
        private TextBlock _infoText;
        private Slider _speedSlider;
        private TextBlock _speedValueText;
        private EcsWorldManager _ecsManager;
        private GameScene _scene;
        private DispatcherTimer _gameTimer;
        private bool _isRunning = false;
        private int _collisionCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _gameCanvas = this.FindControl<Canvas>("GameCanvas");
            _infoText = this.FindControl<TextBlock>("InfoText");
            _speedSlider = this.FindControl<Slider>("SpeedSlider");
            _speedValueText = this.FindControl<TextBlock>("SpeedValueText");

            _speedSlider.ValueChanged += OnSpeedSliderChanged;
        }

        private void InitializeGame()
        {
            _ecsManager = new EcsWorldManager();

            _scene = _ecsManager.CreateAdvancedTestScene();

            Vector2 ballPosition = new Vector2(50, 50);
            float ballRadius = 3f;
            Vector2 ballVelocity = Vector2.Zero; // Шарик не двигается по умолчанию
            var ball = _scene.CreateBall(ballPosition, ballRadius, ballVelocity);

            _ecsManager.InitializeScene(_scene);

            BHS.Engine.ECS.Systems.AdvancedCollisionSystem.OnCollision += OnCollisionDetected;

            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _gameTimer.Tick += OnGameTimerTick;

            UpdateInfo();

            DrawScene();

            _isRunning = false;
        }

        private void OnGameTimerTick(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            _ecsManager.Update();

            _scene.Update(1f / 60f);

            DrawScene();

            UpdateInfo();
        }

        private void DrawScene()
        {
            _gameCanvas.Children.Clear();

            foreach (var wall in _scene.Walls)
            {
                DrawWall(wall);
            }

            foreach (var ball in _scene.Balls)
            {
                DrawBall(ball);
            }

            _gameCanvas.InvalidateVisual();
        }

        private void DrawWall(Wall wall)
        {
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = new Avalonia.Point(wall.Start.X * 4, wall.Start.Y * 4),
                EndPoint = new Avalonia.Point(wall.End.X * 4, wall.End.Y * 4),
                Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(200, 200, 200)),
                StrokeThickness = 2
            };

            _gameCanvas.Children.Add(line);
        }

        private void DrawBall(Ball ball)
        {
            var ellipse = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = ball.Radius * 8,
                Height = ball.Radius * 8,
                Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 255, 255)),
                Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(150, 150, 150)),
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, (ball.Position.X - ball.Radius) * 4);
            Canvas.SetTop(ellipse, (ball.Position.Y - ball.Radius) * 4);

            _gameCanvas.Children.Add(ellipse);
        }

        private void UpdateInfo()
        {
            string status = _isRunning ? "Запущено" : "Остановлено";

            if (_scene.Balls.Count > 0)
            {
                var ball = _scene.Balls[0];
                _infoText.Text = $"Позиция шарика: ({ball.Position.X:F1}, {ball.Position.Y:F1})\n" +
                                $"Скорость шарика: ({ball.Velocity.X:F1}, {ball.Velocity.Y:F1})\n" +
                                $"Статус: {status}\n" +
                                $"Столкновения: {_collisionCount}";
            }
            else
            {
                _infoText.Text = $"Позиция шарика: (0, 0)\n" +
                                $"Скорость шарика: (0, 0)\n" +
                                $"Статус: {status}\n" +
                                $"Столкновения: {_collisionCount}";
            }
        }

        private void OnSpeedSliderChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            _speedValueText.Text = ((int)e.NewValue).ToString();
        }

        private void OnCollisionDetected(BHS.Engine.Core.Events.CollisionEvent collisionEvent)
        {
            _collisionCount++;
            UpdateInfo();
        }

        private void StartSimulationButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                if (_scene.Balls.Count > 0)
                {
                    var ball = _scene.Balls[0];
                    float speed = (float)_speedSlider.Value;

                    Random random = new Random();
                    float angle = (float)(random.NextDouble() * 2 * Math.PI);
                    Vector2 velocity = new Vector2(
                        (float)(Math.Cos(angle) * speed),
                        (float)(Math.Sin(angle) * speed)
                    );

                    ball.SetVelocity(velocity);

                    var world = _ecsManager.World;
                    var velocityPool = world.GetPool<BHS.Engine.ECS.Components.VelocityComponent>();
                    var sceneLinkPool = world.GetPool<BHS.Engine.ECS.Components.SceneObjectLink>();

                    var ballFilter = world.Filter<BHS.Engine.ECS.Components.VelocityComponent>()
                        .Inc<BHS.Engine.ECS.Components.SceneObjectLink>()
                        .End();

                    foreach (var entity in ballFilter)
                    {
                        ref var sceneLink = ref sceneLinkPool.Get(entity);
                        if (sceneLink.Reference == ball)
                        {
                            ref var velocityComponent = ref velocityPool.Get(entity);
                            velocityComponent.Velocity = velocity;
                            break;
                        }
                    }
                }

                _isRunning = true;
                _gameTimer.Start();
            }
        }

        private void StopButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _isRunning = false;
            _gameTimer.Stop();
        }

        private void ResetButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _isRunning = false;
            _gameTimer.Stop();

            _ecsManager.Dispose();
            _ecsManager = new EcsWorldManager();
            _scene = _ecsManager.CreateAdvancedTestScene();

            Vector2 ballPosition = new Vector2(50, 50);
            float ballRadius = 3f;
            Vector2 ballVelocity = Vector2.Zero;
            var ball = _scene.CreateBall(ballPosition, ballRadius, ballVelocity);

            _ecsManager.InitializeScene(_scene);

            BHS.Engine.ECS.Systems.AdvancedCollisionSystem.OnCollision += OnCollisionDetected;

            DrawScene();
            UpdateInfo();
        }

        private void ClearBallsButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _scene.ClearBalls();

            _ecsManager.Dispose();
            _ecsManager = new EcsWorldManager();
            _ecsManager.InitializeScene(_scene);

            DrawScene();
            UpdateInfo();
        }

        private void RandomWallButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _gameTimer.Stop();
                _isRunning = false;
            }

            Vector2? ballPos = null;
            float? ballRadius = null;
            Vector2? ballVel = null;

            if (_scene.Balls.Count > 0)
            {
                var existingBall = _scene.Balls[0];
                ballPos = existingBall.Position;
                ballRadius = existingBall.Radius;
                ballVel = existingBall.Velocity;
            }

            _ecsManager.Dispose();
            _ecsManager = new EcsWorldManager();
            _scene = _ecsManager.CreateSceneWithRandomWall();

            if (ballPos.HasValue)
            {
                _scene.CreateBall(ballPos.Value, ballRadius.Value, ballVel ?? Vector2.Zero);
            }
            else
            {
                Vector2 ballPosition = new Vector2(50, 50);
                float newBallRadius = 3f;
                Vector2 ballVelocity = Vector2.Zero;
                _scene.CreateBall(ballPosition, newBallRadius, ballVelocity);
            }

            _ecsManager.InitializeScene(_scene);

            DrawScene();
            UpdateInfo();
        }

        private void HighSpeedButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_scene.Balls.Count > 0)
            {
                var ball = _scene.Balls[0];
                float speed = (float)_speedSlider.Value * 10f; // Увеличиваем скорость в 10 раз

                Vector2 velocity;
                if (ball.Velocity.SqrMagnitude > 0.01f)
                {
                    velocity = ball.Velocity * 2f;
                }
                else
                {
                    Random random = new Random();
                    float angle = (float)(random.NextDouble() * 2 * Math.PI);
                    velocity = new Vector2(
                        (float)(Math.Cos(angle) * speed),
                        (float)(Math.Sin(angle) * speed)
                    );
                }

                ball.SetVelocity(velocity);

                var world = _ecsManager.World;
                var velocityPool = world.GetPool<BHS.Engine.ECS.Components.VelocityComponent>();
                var sceneLinkPool = world.GetPool<BHS.Engine.ECS.Components.SceneObjectLink>();

                var ballFilter = world.Filter<BHS.Engine.ECS.Components.VelocityComponent>()
                    .Inc<BHS.Engine.ECS.Components.SceneObjectLink>()
                    .End();

                foreach (var entity in ballFilter)
                {
                    ref var sceneLink = ref sceneLinkPool.Get(entity);
                    if (sceneLink.Reference == ball)
                    {
                        ref var velocityComponent = ref velocityPool.Get(entity);
                        velocityComponent.Velocity = velocity;
                        break;
                    }
                }
            }

            if (!_isRunning)
            {
                _isRunning = true;
                _gameTimer.Start();
            }
        }

        private void GameCanvas_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
        }


        protected override void OnClosed(EventArgs e)
        {
            _gameTimer?.Stop();
            _ecsManager?.Dispose();
            base.OnClosed(e);
        }
    }
}
