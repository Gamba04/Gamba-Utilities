using System;
using UnityEngine;

namespace GambaUtilities
{
    public static class MathUtilities
    {

        #region Scalars

        /// <summary> Clamps negative values to zero. </summary>
        public static void Ramp(ref int value) => value = Ramp(value);

        /// <summary> Clamps negative values to zero. </summary>
        public static int Ramp(int value) => Mathf.Max(0, value);

        /// <summary> Clamps negative values to zero. </summary>
        public static void Ramp(ref float value) => value = Ramp(value);

        /// <summary> Clamps negative values to zero. </summary>
        public static float Ramp(float value) => Mathf.Max(0, value);

        /// <summary> Rounds <paramref name="value"/> to the nearest multiple of <paramref name="step"/>. </summary>
        public static void RoundToMultiple(ref float value, float step) => value = RoundToMultiple(value, step);

        /// <summary> Rounds <paramref name="value"/> to the nearest multiple of <paramref name="step"/>. </summary>
        public static float RoundToMultiple(float value, float step) => Mathf.Round(value / step) * step;

        /// <summary> Rounds <paramref name="value"/> to the nearest integrer pointing away from zero. </summary>
        public static void RoundAwayFromZero(ref float value) => value = RoundAwayFromZero(value);

        /// <summary> Rounds <paramref name="value"/> to the nearest integrer pointing away from zero. </summary>
        public static int RoundAwayFromZero(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Vectors

        public static Vector3 GetScaleOf(float size) => new Vector3(size, size, 1);

        public static Vector2 GetDirection(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        public static float GetAngle(this Vector2 point)
        {
            const float pi = Mathf.PI;

            float x = point.x;
            float y = point.y;

            float r;

            if (x > 0)
            {
                if (y > 0) // Cuadrant: 1
                {
                    r = Mathf.Atan(y / x);
                }
                else if (y < 0) // Cuadrant: 4
                {
                    r = pi * 3 / 2f + (pi * 3 / 2f - (pi - Mathf.Atan(y / x)));
                }
                else // Right
                {
                    r = 0;
                }
            }
            else if (x < 0)
            {
                if (y > 0) // Cuadrant: 2
                {
                    r = pi * 1 / 2f + (pi * 1 / 2f + Mathf.Atan(y / x));

                }
                else if (y < 0) // Cuadrant: 3
                {
                    r = pi + Mathf.Atan(y / x);
                }
                else // Left
                {
                    r = pi;
                }
            }
            else
            {
                if (y > 0) // Up
                {
                    r = pi * 1 / 2f;
                }
                else if (y < 0) // Down
                {
                    r = pi * 3 / 2f;
                }
                else // Zero
                {
                    r = 0;
                }
            }

            return r;
        }

        public static Vector2 Perpendicular(this Vector2 vector) => new Vector2(vector.y, -vector.x);

        public static Vector3 Perpendicular(this Vector3 a, Vector3 b) => Vector3.Cross(a, b);

        public static Vector2 MultipliedBy(this Vector2 a, Vector2 b) => a.MultipliedBy(b.x, b.y);

        public static Vector2 MultipliedBy(this Vector2 a, float x, float y) => new Vector2(a.x * x, a.y * y);

        public static Vector3 MultipliedBy(this Vector3 a, Vector3 b) => a.MultipliedBy(b.x, b.y, b.z);

        public static Vector3 MultipliedBy(this Vector3 a, float x, float y, float z) => new Vector3(a.x * x, a.y * y, a.z * z);

        public static Vector2 DividedBy(this Vector2 a, Vector2 b) => a.DividedBy(b.x, b.y);

        public static Vector2 DividedBy(this Vector2 a, float x, float y) => new Vector2(a.x / x, a.y / y);

        public static Vector3 DividedBy(this Vector3 a, Vector3 b) => a.DividedBy(b.x, b.y, b.z);

        public static Vector3 DividedBy(this Vector3 a, float x, float y, float z) => new Vector3(a.x / x, a.y / y, a.z / z);

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Colors

        public static Color WithAlpha(this Color color, float value) => new Color(color.r, color.g, color.b, value);

        /// <summary> Linearly interpolates between colors <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/> in HSV mode. </summary>
        public static Color ColorLerp(Color a, Color b, float t) // Change to LCH
        {
            Vector3 aHSV = GetHSV(a);
            Vector3 bHSV = GetHSV(b);

            Vector3 targetHSV = Vector3.Lerp(aHSV, bHSV, t);

            return Color.HSVToRGB(targetHSV.x, targetHSV.y, targetHSV.z);

            static Vector3 GetHSV(Color color)
            {
                Color.RGBToHSV(color, out float h, out float s, out float v);

                return new Vector3(h, s, v);
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Other

        public static bool RandomBool() => UnityEngine.Random.Range(0, 2) == 0;

        public static float VolumeToDB(float volume) => volume > 0 ? Mathf.Log10(volume) * 20 : float.MinValue;

        public static float DBToVolume(float dB) => Mathf.Pow(10, dB / 20);

        public static bool Contains(this LayerMask mask, int layer) => mask == (mask | (1 << layer));

        #endregion

    }
}