using System;

namespace BHS.Engine.Core.Math
{
    /// <summary>
    /// Математические утилиты для физических расчетов
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Константа для сравнения с плавающей точкой
        /// </summary>
        public const float Epsilon = 0.0001f;

        /// <summary>
        /// Константа Пи
        /// </summary>
        public const float PI = (float)System.Math.PI;

        /// <summary>
        /// Константа 2*Пи
        /// </summary>
        public const float TwoPI = 2f * PI;

        /// <summary>
        /// Константа Пи/2
        /// </summary>
        public const float HalfPI = PI / 2f;

        /// <summary>
        /// Преобразование градусов в радианы
        /// </summary>
        public static float DegreesToRadians(float degrees) => degrees * PI / 180f;

        /// <summary>
        /// Преобразование радианов в градусы
        /// </summary>
        public static float RadiansToDegrees(float radians) => radians * 180f / PI;

        /// <summary>
        /// Ограничение значения между min и max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Линейная интерполяция между двумя значениями
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp(t, 0f, 1f);
        }

        /// <summary>
        /// Проверка на равенство с учетом погрешности
        /// </summary>
        public static bool Approximately(float a, float b, float epsilon = Epsilon)
        {
            return System.Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Проверка на равенство векторов с учетом погрешности
        /// </summary>
        public static bool Approximately(Vector2 a, Vector2 b, float epsilon = Epsilon)
        {
            return Approximately(a.X, b.X, epsilon) && Approximately(a.Y, b.Y, epsilon);
        }

        /// <summary>
        /// Минимальное значение из двух
        /// </summary>
        public static float Min(float a, float b) => a < b ? a : b;

        /// <summary>
        /// Максимальное значение из двух
        /// </summary>
        public static float Max(float a, float b) => a > b ? a : b;

        /// <summary>
        /// Абсолютное значение
        /// </summary>
        public static float Abs(float value) => System.Math.Abs(value);

        /// <summary>
        /// Квадратный корень
        /// </summary>
        public static float Sqrt(float value) => (float)System.Math.Sqrt(value);

        /// <summary>
        /// Синус
        /// </summary>
        public static float Sin(float radians) => (float)System.Math.Sin(radians);

        /// <summary>
        /// Косинус
        /// </summary>
        public static float Cos(float radians) => (float)System.Math.Cos(radians);

        /// <summary>
        /// Арктангенс
        /// </summary>
        public static float Atan2(float y, float x) => (float)System.Math.Atan2(y, x);
    }
}
