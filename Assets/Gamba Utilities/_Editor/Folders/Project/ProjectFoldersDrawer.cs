using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace GambaUtilities.Editor.Folders
{
	[InitializeOnLoad]
	[CreateAssetMenu(fileName = name, menuName = EditorUtilities.scriptableRoot + name)]
	public class ProjectFoldersDrawer : ScriptableObject
	{
		private new const string name = "Project Folders";

		#region Serializable

		[Serializable]
		private class Folder
		{
			[SerializeField, HideInInspector]
			private string name;
			[SerializeField]
			private string path;
			public Color color = Color.white;

			public string Path => $"Assets/{path}";

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

		private static EditorApplication.ProjectWindowItemCallback onProjectWindowItemGUI;

		[SerializeField]
		private List<Folder> folders = new List<Folder>();

		#region Init

		static ProjectFoldersDrawer()
		{
			EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

			EditorApplication.delayCall += Init;

			static void OnProjectWindowItemGUI(string guid, Rect area) => onProjectWindowItemGUI?.Invoke(guid, area);

			static void Init() => EditorUtilities.FindAssetOfType<ProjectFoldersDrawer>();
		}

		private void Hook() => onProjectWindowItemGUI = OnProjectWindowItemGUI;

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Processing

		private void OnProjectWindowItemGUI(string guid, Rect area)
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
					FolderDrawer.DrawFolder(area, folder.color, level);
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