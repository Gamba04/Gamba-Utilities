using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace GambaUtilities.Editor.Folders
{
	public static class FolderDrawer
	{
		private const float offset = 21;
		private const float levelSeparation = 14;

		private const float folderHeight = 12;
		private const float folderWidth = 6;

		private const float subfolderWidth = 2;

		public static void DrawFolder(Rect area, Color color, int level)
		{
			if (level == 0) DrawFolder(area, color);
			else DrawSubfolder(area, color, level);
		}

		private static void DrawFolder(Rect area, Color color)
		{
			float centeringOffset = (area.height - folderHeight) / 2;

			area.height = folderHeight;
			area.y += centeringOffset;

			area.width = folderWidth;
			area.x -= offset;

			EditorGUI.DrawRect(area, color);
		}

		private static void DrawSubfolder(Rect area, Color color, int level)
		{
			float centeringOffset = (folderWidth - subfolderWidth) / 2;

			area.width = subfolderWidth;
			area.x -= offset - centeringOffset + level * levelSeparation;

			EditorGUI.DrawRect(area, color);
		}
	}
}

#endif