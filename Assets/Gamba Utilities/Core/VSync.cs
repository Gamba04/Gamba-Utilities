using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	[HideMonoScript]
	public class VSync : SingletonBehaviour<VSync>
	{
		public enum Target
		{
			RefreshRate,
			Framerate,
			Unlimited
		}

		[SerializeField]
		private Target target;
		[SerializeField]
		[ShowIf(nameof(target), Target.Framerate)]
		private int framerate = 144;

		#region Init

		protected override void Init()
		{
			Application.targetFrameRate = target switch
			{
				Target.RefreshRate => Screen.currentResolution.refreshRate,
				Target.Framerate => framerate,
				Target.Unlimited => -1,
				_ => throw new InvalidCastException()
			};
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Target

		public static void SetTarget(Target target) => Instance.SetTarget(target);

		public static void SetTarget(int framerate) => Instance.SetTarget(Target.Framerate, Mathf.Max(framerate, 1));

		private void SetTarget(Target target, int? framerate = null)
		{
			this.target = target;
			this.framerate = framerate ?? this.framerate;

			Init();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Editor

#if UNITY_EDITOR

		[InitializeOnLoadMethod]
		private static void EditorInit()
		{
			EditorApplication.playModeStateChanged += state =>
			{
				if (state == PlayModeStateChange.EnteredEditMode)
				{
					Application.targetFrameRate = -1;
				}
			};
		}

		private void OnValidate()
		{
			QualitySettings.vSyncCount = 0;

			framerate = Mathf.Clamp(framerate, 1, 1000);
		}

#endif

		#endregion

	}
}