using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{

#if UNITY_EDITOR

    public static class GizmoUtilities
    {

        #region 2D

        #region Arrow

        public static void DrawArrow2D(Vector2 position, Vector2 direction) => DrawArrow2D(position, direction, Gizmos.color);

        public static void DrawArrow2D(Vector2 position, Vector2 direction, Color color) => DrawArrow2D(position, direction, 0.25f, color);

        public static void DrawArrow2D(Vector2 position, Vector2 direction, float headSize) => DrawArrow2D(position, direction, headSize, Gizmos.color);

        public static void DrawArrow2D(Vector2 position, Vector2 direction, float headSize, Color color)
        {
            if (direction == Vector2.zero) return;

            Gizmos.color = color;

            Vector2 head = position + direction;

            direction.Normalize();

            const float angle = 30;
            float height = Mathf.Tan(angle.ToRadians()) * headSize;

            Vector2 headDirection = -direction * headSize;
            Vector2 perpendicular = direction.Perpendicular() * height;

            Gizmos.DrawLine(position, head);
            Gizmos.DrawRay(head, headDirection + perpendicular);
            Gizmos.DrawRay(head, headDirection - perpendicular);
        }

        #endregion

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region 3D

        #region Arrow

        public static void DrawArrow(Vector3 position, Vector3 direction) => DrawArrow(position, direction, Handles.color);

        public static void DrawArrow(Vector3 position, Vector3 direction, Color color)
        {
            Handles.color = color;

            Quaternion rotation = Quaternion.LookRotation(direction);
            float size = direction.magnitude * 0.8771929f;

            Handles.ArrowHandleCap(0, position, rotation, size, EventType.Repaint);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Dashed Line

        public static void DrawDashedLine(Vector3 from, Vector3 to) => DrawDashedLine(from, to, Handles.color);

        public static void DrawDashedLine(Vector3 from, Vector3 to, Color color) => DrawDashedLine(from, to, 1, 0.25f, color);

        public static void DrawDashedLine(Vector3 from, Vector3 to, float thickness, float separation, Color color)
        {
            Handles.color = color;

            if (separation > 0)
            {
                Vector3 position = from;
                Vector3 vector = to - from;
                Vector3 direction = vector.normalized;

                float distance = vector.magnitude;
                float value = 0;

                bool isDrawing = true;

                while (value < distance)
                {
                    float increment = Mathf.Min(separation, distance - value);

                    Vector3 target = position + direction * increment;

                    if (isDrawing) Handles.DrawLine(position, target, thickness);

                    position = target;
                    value += increment;
                    isDrawing = !isDrawing;
                }
            }
            else Handles.DrawLine(from, to, thickness);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Cube

        public static void DrawCube(Transform transform) => DrawCube(transform, Vector3.one);

        public static void DrawCube(Vector3 center, Vector3 scale) => DrawCube(center, scale, Handles.color);

        public static void DrawCube(Transform transform, Vector3 scale) => DrawCube(transform, scale, Handles.color);

        public static void DrawCube(Vector3 center, Vector3 scale, Color color) => DrawCube(center, scale, 1, color);

        public static void DrawCube(Transform transform, Vector3 scale, Color color) => DrawCube(transform, scale, 1, color);

        public static void DrawCube(Vector3 center, Vector3 scale, float thickness, Color color) => DrawCube(coord => center + coord, scale, thickness, color);

        public static void DrawCube(Transform transform, Vector3 scale, float thickness, Color color) => DrawCube(transform.TransformPoint, scale, thickness, color);

        private static void DrawCube(Func<Vector3, Vector3> transformCoord, Vector3 scale, float thickness, Color color)
        {
            Handles.color = color;

            scale /= 2;

            DrawVertex(0, 0, 0);
            DrawVertex(0, 0, 1);
            DrawVertex(0, 1, 0);
            DrawVertex(0, 1, 1);
            DrawVertex(1, 0, 0);
            DrawVertex(1, 0, 1);
            DrawVertex(1, 1, 0);
            DrawVertex(1, 1, 1);

            void DrawVertex(float x, float y, float z)
            {
                Vector3 coord = new Vector3(x, y, z) * 2 - Vector3.one;

                DrawEdge(coord, coord.MultipliedBy(1, 1, -1));
                DrawEdge(coord, coord.MultipliedBy(1, -1, 1));
                DrawEdge(coord, coord.MultipliedBy(-1, 1, 1));
            }

            void DrawEdge(Vector3 coordA, Vector3 coordB)
            {
                Vector3 positionA = transformCoord(scale.MultipliedBy(coordA));
                Vector3 positionB = transformCoord(scale.MultipliedBy(coordB));

                Handles.DrawLine(positionA, positionB, thickness);
            }
        }

        #endregion

        #endregion

    }

#endif

}