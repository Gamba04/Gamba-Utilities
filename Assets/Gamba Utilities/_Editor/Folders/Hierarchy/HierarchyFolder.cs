using UnityEngine;
using UnityEditor;

namespace GambaUtilities.Editor.Folders
{
	[AddComponentMenu("​Hierarchy Folder")]
	[DisallowMultipleComponent]
	[HideMonoScript]
	public class HierarchyFolder : MonoBehaviour
	{
		[SerializeField]
		private Color color = Color.white;

		public Color Color => color;

		#region Editor

#if UNITY_EDITOR

		private void OnValidate()
		{
			EditorApplication.RepaintHierarchyWindow();
		}

#endif

		#endregion

	}
}