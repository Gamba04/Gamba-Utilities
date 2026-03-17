using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace GambaUtilities.Editor
{
    [InitializeOnLoad]
    public static class HierarchyFoldersDrawer
    {
        private const float offset = 21;
        private const float levelSeparation = 14;

        private const float parentHeight = 12;
        private const float parentWidth = 6;

        private const float childWidth = 2;

        #region Processing

        static HierarchyFoldersDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect area)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            Transform transform = gameObject?.transform;

			for (int level = 0; transform != null; level++)
			{
                if (transform.TryGetComponent(out HierarchyFolder folder))
                {
                    DrawFolder(area, folder.Color, level);
                }

                transform = transform.parent;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Drawing

        private static void DrawFolder(Rect area, Color color, int level)
        {
            if (level == 0) DrawParent(area, color);
            else DrawChild(area, color, level);
        }

        private static void DrawParent(Rect area, Color color)
        {
            float centeringOffset = (area.height - parentHeight) / 2;

            area.height = parentHeight;
            area.y += centeringOffset;

            area.width = parentWidth;
            area.x -= offset;

            EditorGUI.DrawRect(area, color);
        }

        private static void DrawChild(Rect area, Color color, int level)
        {
            float centeringOffset = (parentWidth - childWidth) / 2;

            area.width = childWidth;
            area.x -= offset - centeringOffset + level * levelSeparation;

            EditorGUI.DrawRect(area, color);
        }

        #endregion

    }
}

#endif