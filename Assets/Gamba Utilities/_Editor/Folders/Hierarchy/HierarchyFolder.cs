using UnityEngine;

namespace GambaUtilities.Editor.Folders
{
    [AddComponentMenu("​Hierarchy Folder")]
    [DisallowMultipleComponent]
    public class HierarchyFolder : MonoBehaviour
    {
        [SerializeField]
        private Color color = Color.white;

        public Color Color => color;
    }
}