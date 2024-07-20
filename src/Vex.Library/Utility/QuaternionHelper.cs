using System;
using System.Numerics;

namespace Vex.Library
{
    /// <summary>
    /// Class to provide helper methods for working with <see cref="Quaternion"/>s
    /// </summary>
    public static class QuaternionHelper
    {
        /// <summary>
        /// Converts the provided <see cref="Quaternion"/> to Degrees
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        /// <returns>Result as Degrees</returns>
        public static Vector3 ToDegrees(Quaternion q) => ToEulerAngles(q) * (180 / MathF.PI);

        /// <summary>
        /// Converts the provided <see cref="Quaternion"/> to Euler
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        /// <returns>Result as Euler</returns>
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            Vector3 result = Vector3.Zero;

            // Roll
            result.X = MathF.Atan2(
                2.0f * (q.W * q.X + q.Y * q.Z),
                1.0f - 2.0f * (q.X * q.X + q.Y * q.Y));

            // Pitch
            var sinp = 2.0f * (q.W * q.Y - q.Z * q.X);
            if (MathF.Abs(sinp) >= 1)
                result.Y = MathF.CopySign(MathF.PI / 2, sinp);
            else
                result.Y = MathF.Asin(sinp);

            // Yaw
            result.Z = MathF.Atan2(
                2.0f * (q.W * q.Z + q.X * q.Y),
                1.0f - 2.0f * (q.Y * q.Y + q.Z * q.Z));

            return result;
        }

        /// <summary>
        /// Creates a <see cref="Quaternion"/> from the provided degrees
        /// </summary>
        /// <param name="degrees">Degrees to convert</param>
        /// <returns>Resulting Quaternion</returns>
        public static Quaternion CreateFromDegrees(Vector3 degrees) => CreateFromEuler(degrees / (180 / MathF.PI));

        /// <summary>
        /// Creates a <see cref="Quaternion"/> from the provided euler angles
        /// </summary>
        /// <param name="degrees">Euler value to convert</param>
        /// <returns>Resulting Quaternion</returns>
        public static Quaternion CreateFromEuler(Vector3 vec)
        {
            // Abbreviations for the various angular functions
            var cy = MathF.Cos(vec.Z * 0.5f);
            var sy = MathF.Sin(vec.Z * 0.5f);
            var cp = MathF.Cos(vec.Y * 0.5f);
            var sp = MathF.Sin(vec.Y * 0.5f);
            var cr = MathF.Cos(vec.X * 0.5f);
            var sr = MathF.Sin(vec.X * 0.5f);

            Quaternion q;
            q.W = cr * cp * cy + sr * sp * sy;
            q.X = sr * cp * cy - cr * sp * sy;
            q.Y = cr * sp * cy + sr * cp * sy;
            q.Z = cr * cp * sy - sr * sp * cy;

            return q;
        }

        public static Quaternion QuatPackingA(ulong packedData)
        {
            // Load and shift 2 bits
            ulong packedQuatData = packedData;
            int axis = (int)(packedQuatData & 3);
            int wSign = (int)((packedQuatData >> 63) & 1);
            packedQuatData >>= 2;

            // Calculate XYZ
            int ix = (int)(packedQuatData & 0xfffff);
            if (ix > 0x7ffff) ix -= 0x100000;
            int iy = (int)((packedQuatData >> 20) & 0xfffff);
            if (iy > 0x7ffff) iy -= 0x100000;
            int iz = (int)((packedQuatData >> 40) & 0xfffff);
            if (iz > 0x7ffff) iz -= 0x100000;
            float x = ix / 1048575.0f;
            float y = iy / 1048575.0f;
            float z = iz / 1048575.0f;

            // Mod all values
            x *= 1.41421f;
            y *= 1.41421f;
            z *= 1.41421f;

            // Calculate W
            float w = (float)Math.Pow(1 - x * x - y * y - z * z, 0.5f);

            // Determine sign of W
            if (wSign == 1)
            {
                w = -w;
            }

            // Determine axis
            return axis switch
            {
                0 => new Quaternion(w, x, y, z),
                1 => new Quaternion(x, y, z, w),
                2 => new Quaternion(y, z, w, x),
                3 => new Quaternion(z, w, x, y),
                _ => Quaternion.Identity,
            };
        }

        public static Quaternion QuatPacking2DA(uint packedData)
        {
            // Load data, calculate WSign, mask off bits
            uint packedQuatData = packedData;
            int wSign = (int)((packedQuatData >> 30) & 1);
            packedQuatData &= 0xBFFFFFFF;

            // Calculate Z W
            float z = BitConverter.ToSingle(BitConverter.GetBytes(packedQuatData), 0);
            float w = (float)Math.Sqrt(1.0f - Math.Pow(z, 2.0f));

            // Determine sign of W
            if (wSign == 1)
            {
                w = -w;
            }

            // Return it
            return new Quaternion(0.0f, 0.0f, z, w);
        }
    }
}
