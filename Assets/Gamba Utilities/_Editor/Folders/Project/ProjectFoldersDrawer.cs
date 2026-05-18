using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

using UnityEditorInternal;

namespace GambaUtilities.Editor.Folders
{
	[InitializeOnLoad]
	[CreateAssetMenu(fileName = name, menuName = CoreUtilities.scriptableRoot + name)]
	public class ProjectFoldersDrawer : ScriptableObject
	{
		private new const string name = "Project Folders";

		#region Folder

		[Serializable]
		private class Folder
		{
			[SerializeField, HideInInspector]
			private string name;
			[SerializeField]
			private string path;
			[SerializeField]
			private Color color = Color.white;

			public string Path => $"Assets/{path}";

			public Color Color => color;

			public void EditorUpdate()
			{
				ProcessPath();

				name = Path;
			}

			private void ProcessPath()
			{
				path = path.Remove("\\");
				path = path.TrimEnd('/');
			}

			public static implicit operator bool(Folder folder) => folder != null;
		}

		#endregion

		private const float oneColumnHeight = 16;
		private const char separator = '/';

		private static EditorApplication.ProjectWindowItemCallback onProcessItem;

		[SerializeField]
		private List<Folder> folders = new List<Folder>();

		#region Init

		static ProjectFoldersDrawer()
		{
			EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

			EditorApplication.delayCall += () =>
			{
				EditorUtilities.FindAssetOfType<ProjectFoldersDrawer>();
				InternalEditorUtility.RepaintAllViews();
			};
		}

		private static void OnProjectWindowItemGUI(string guid, Rect area) => onProcessItem?.Invoke(guid, area);

		private void Hook() => onProcessItem = ProcessItem;

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Processing

		private void ProcessItem(string guid, Rect area)
		{
			if (area.height > oneColumnHeight) return;

			string path = AssetDatabase.GUIDToAssetPath(guid);

			ProcessPath(path, area);
		}

		private void ProcessPath(string path, Rect area)
		{
			for (int level = 0; ValidatePath(path); level++)
			{
				if (TryGetFolder(path, out Folder folder))
				{
					FolderDrawer.DrawFolder(area, folder.Color, level);
				}

				TrimPath(ref path);
			}
		}

		private bool ValidatePath(string path)
		{
			return path.Contains(separator);
		}

		private bool TryGetFolder(string path, out Folder folder)
		{
			return folder = folders.Find(folder => folder.Path == path);
		}

		private void TrimPath(ref string path)
		{
			int index = path.LastIndexOf(separator);

			path = path.Remove(index);
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Inspector

		private void OnValidate()
		{
			Hook();

			folders.ForEach(folder => folder.EditorUpdate());
		}

		#endregion

	}
}

#endif