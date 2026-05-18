using UnityEngine;

namespace GambaUtilities
{
	public class VSync : MonoBehaviour
	{
		[SerializeField]
		private new bool enabled = true;
		[SerializeField]
		private ushort target = 144;

		private void Awake()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = enabled ? target : -1;
		}
	}
}