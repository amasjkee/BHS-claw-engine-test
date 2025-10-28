\mainpage BHS Claw Engine Test

# Структура проекта

## src/Core
- **Math/** — математические утилиты (Vector2, MathHelper)
- **Interfaces/** — интерфейсы (ISceneObject)
- **Events/** — события (CollisionEvent)

## src/ECS
- **Components/** — компоненты данных (Transform, Velocity, Colliders)
- **Systems/** — системы логики (Movement, AdvancedCollision)
- **EcsWorldManager.cs** — менеджер ECS мира

## src/Physics
- **AdvancedCollisionDetector.cs** — обнаружение коллизий
- **AdvancedPhysicsCalculator.cs** — расчеты физики

## src/Scene
- **SceneObject.cs** — базовый объект сцены
- **Ball.cs** — шарик
- **Wall.cs** — стена
- **Scene.cs** — игровая сцена

## src/UI
- **MainWindow.cs** — главное окно Avalonia
- **App.axaml.cs** — приложение
- **Program.cs** — точка входа UI