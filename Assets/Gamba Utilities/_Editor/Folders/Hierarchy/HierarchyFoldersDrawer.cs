using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace GambaUtilities.Editor.Folders
{
    [InitializeOnLoad]
    public static class HierarchyFoldersDrawer
    {
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
                    FolderDrawer.DrawFolder(area, folder.Color, level);
                }

                transform = transform.parent;
            }
        }
    }
}

#endif