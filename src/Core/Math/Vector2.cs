using System;

namespace BHS.Engine.Core.Math
{
    /// <summary>
    /// 2D вектор для позиций и скоростей
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        /// <summary>
        /// Длина вектора
        /// </summary>
        public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Квадрат длины вектора (для оптимизации)
        /// </summary>
        public float SqrMagnitude => X * X + Y * Y;

        /// <summary>
        /// Нормализованный вектор
        /// </summary>
        public Vector2 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                if (magnitude > 0.0001f)
                    return new Vector2(X / magnitude, Y / magnitude);
                return Zero;
            }
        }

        /// <summary>
        /// Сложение векторов
        /// </summary>
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// Вычитание векторов
        /// </summary>
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

        /// <summary>
        /// Умножение на скаляр
        /// </summary>
        public static Vector2 operator *(Vector2 vector, float scalar) => new Vector2(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Умножение скаляра на вектор
        /// </summary>
        public static Vector2 operator *(float scalar, Vector2 vector) => new Vector2(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Деление на скаляр
        /// </summary>
        public static Vector2 operator /(Vector2 vector, float scalar) => new Vector2(vector.X / scalar, vector.Y / scalar);

        /// <summary>
        /// Унарный минус
        /// </summary>
        public static Vector2 operator -(Vector2 vector) => new Vector2(-vector.X, -vector.Y);

        /// <summary>
        /// Скалярное произведение
        /// </summary>
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// Отражение вектора от нормали
        /// </summary>
        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            float dot = Dot(vector, normal);
            return vector - 2 * dot * normal;
        }

        /// <summary>
        /// Расстояние между двумя точками
        /// </summary>
        public static float Distance(Vector2 a, Vector2 b) => (a - b).Magnitude;

        /// <summary>
        /// Квадрат расстояния между двумя точками (для оптимизации)
        /// </summary>
        public static float SqrDistance(Vector2 a, Vector2 b) => (a - b).SqrMagnitude;

        public bool Equals(Vector2 other)
        {
            return System.Math.Abs(X - other.X) < 0.0001f && System.Math.Abs(Y - other.Y) < 0.0001f;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }
    }
}
