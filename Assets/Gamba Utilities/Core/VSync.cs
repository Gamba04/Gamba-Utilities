using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	using Editor;

	[HideMonoScript]
	public class VSync : SingletonBehaviour<VSync>
	{

		#region Settings

		public abstract class Settings
		{

			#region Adaptive VSync

			[Serializable]
			public struct AdaptiveVSync : ISerializationCallbackReceiver
			{
				[SerializeField, HideInInspector]
				private bool initialized;

				[Tooltip("The initial VSync state before sampling the first framerate average")]
				public bool initialState;
				[Tooltip("The delay between samples of the average framerate")]
				[Range(0.1f, 2)]
				public float delay;
				[Tooltip("Percentage of the refresh rate where VSync becomes enabled\n\nWhen VSync is enabled, the framerate drops down to the screen's refresh rate. However, even if the device can sustain framerates well above that, the reported framerate will always have some fluctuations which prevent it from matching the refresh rate value exactly. This threshold acts as a small margin of error in order to detect VSync eligibility reliably\n\nRecommended: 99%")]
				[Range(90, 99.9f)]
				public float threshold;

				#region ISerializationCallbackReceiver

				void ISerializationCallbackReceiver.OnBeforeSerialize() { }

				void ISerializationCallbackReceiver.OnAfterDeserialize()
				{
					if (!initialized)
					{
						initialized = true;

						delay = 1;
						threshold = 99;
					}
				}

				#endregion

			}

			#endregion

			public enum Target
			{
				RefreshRate,
				Framerate,
				Unlimited
			}

			public enum LimitedTarget
			{
				RefreshRate,
				Framerate
			}

			public enum VSync
			{
				Disabled,
				Enabled,
				[Tooltip("Adaptive VSync dynamically enables VSync only when the average framerate is about the same or higher than the screen's refresh rate")]
				Adaptive
			}

			#region Values

			public virtual bool UnlimitedSupported => false;

			public virtual bool VSyncSupported => false;

			public virtual bool AdaptiveVSyncEnabled => false;

			public abstract Target TargetValue { get; set; }

			public abstract int FramerateValue { get; set; }

			public virtual VSync? VSyncValue { get => null; set { } }

			public virtual AdaptiveVSync? AdaptiveVSyncValue { get => null; set { } }

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public abstract void Apply();

			public abstract void EditorUpdate();

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Utilities

			protected LimitedTarget? GetLimitedTarget(Target target) => target < Target.Unlimited ? (LimitedTarget?)(int)target : null;

			protected void ApplyTarget(Target target, int framerate)
			{
				Application.targetFrameRate = target switch
				{
					Target.RefreshRate => RefreshRate,
					Target.Framerate => framerate,
					Target.Unlimited => -1,
					_ => throw new InvalidCastException()
				};
			}

			protected void ApplyTarget(LimitedTarget target, int framerate, int refreshRate)
			{
				Application.targetFrameRate = target switch
				{
					LimitedTarget.RefreshRate => refreshRate,
					LimitedTarget.Framerate => framerate,
					_ => throw new InvalidCastException()
				};
			}

			protected void ApplyVSync(Target target, VSync vSync)
			{
				if (target == Target.RefreshRate && vSync != VSync.Adaptive)
				{
					QualitySettings.vSyncCount = vSync == VSync.Enabled ? 1 : 0;
				}
			}

			protected void ApplyVSync(LimitedTarget target, VSync vSync) => ApplyVSync((Target)target, vSync);

			protected void ClampFramerate(ref int framerate, int max = 1000) => framerate = Mathf.Clamp(framerate, 1, max);

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Other

			public static implicit operator bool(Settings settings) => settings != null;

			#endregion

		}

		[Serializable]
		private class EditorSettings : Settings
		{
			[SerializeField]
			private Target target;
			[SerializeField]
			private int framerate = 144;

			#region Values

			public override bool UnlimitedSupported => true;

			public override Target TargetValue { get => target; set => target = value; }

			public override int FramerateValue { get => framerate; set => framerate = value; }

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public override void Apply() => ApplyTarget(target, framerate);

			public override void EditorUpdate() => ClampFramerate(ref framerate);

			#endregion

		}

		[Serializable]
		private class DesktopSettings : Settings
		{
			[SerializeField]
			private Target target;
			[SerializeField]
			private VSync vSync;
			[SerializeField]
			private int framerate = 144;
			[SerializeField]
			private AdaptiveVSync adaptiveVSync;

			#region Values

			public override bool UnlimitedSupported => true;

			public override bool VSyncSupported => true;

			public override bool AdaptiveVSyncEnabled => target == Target.RefreshRate && vSync == VSync.Adaptive;

			public override Target TargetValue { get => target; set => target = value; }

			public override VSync? VSyncValue { get => vSync; set => vSync = value.Value; }

			public override int FramerateValue { get => framerate; set => framerate = value; }

			public override AdaptiveVSync? AdaptiveVSyncValue { get => adaptiveVSync; set => adaptiveVSync = value.Value; }

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public override void Apply()
			{
				ApplyTarget(target, framerate);
				ApplyVSync(target, vSync);
			}

			public override void EditorUpdate() => ClampFramerate(ref framerate);

			#endregion

		}

		[Serializable]
		private class WebSettings : Settings
		{
			[SerializeField]
			private LimitedTarget target;
			[SerializeField]
			private VSync vSync;
			[SerializeField]
			private int framerate = 144;
			[SerializeField]
			private AdaptiveVSync adaptiveVSync;

			#region Values

			public override bool VSyncSupported => true;

			public override bool AdaptiveVSyncEnabled => target == LimitedTarget.RefreshRate && vSync == VSync.Adaptive;

			public override Target TargetValue { get => (Target)target; set => target = (LimitedTarget)value; }

			public override VSync? VSyncValue { get => vSync; set => vSync = value.Value; }

			public override int FramerateValue { get => framerate; set => framerate = value; }

			public override AdaptiveVSync? AdaptiveVSyncValue { get => adaptiveVSync; set => adaptiveVSync = value.Value; }

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public override void Apply()
			{
				ApplyTarget(target, framerate, -1);
				ApplyVSync(target, vSync);
			}

			public override void EditorUpdate() => ClampFramerate(ref framerate);

			#endregion

		}

		[Serializable]
		private class MobileSettings : Settings
		{
			[SerializeField]
			private LimitedTarget target;
			[SerializeField]
			private int framerate = 120;

			#region Values

			public override Target TargetValue { get => (Target)target; set => target = (LimitedTarget)value; }

			public override int FramerateValue { get => framerate; set => framerate = value; }

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public override void Apply() => ApplyTarget(target, framerate, RefreshRate);

			public override void EditorUpdate() => ClampFramerate(ref framerate, 240);

			#endregion

		}

		#endregion

		public enum Platform
		{
			Editor,
			Desktop,
			Web,
			Mobile
		}

		[SerializeField]
		private Platform platform;

		[SerializeField]
		private EditorSettings editor;
		[SerializeField]
		private DesktopSettings desktop;
		[SerializeField]
		private WebSettings web;
		[SerializeField]
		private MobileSettings mobile;

		private Settings settings;

		public static int RefreshRate => Screen.currentResolution.refreshRate;

		public static Settings RuntimeSettings => Instance.settings;

		#region Init

		protected override void Init()
		{
			if (TryGetSettings(out settings))
			{
				settings.Apply();

				if (settings.AdaptiveVSyncValue is Settings.AdaptiveVSync adaptiveVSync)
				{
					StartCoroutine(AdaptiveVSync(adaptiveVSync));
				}
			}
		}

		private bool TryGetSettings(out Settings settings) => settings = IsEditor() ? editor : IsDesktop() ? desktop : IsWeb() ? web : IsMobile() ? mobile : (Settings)null;

		private bool IsEditor() => Application.isEditor;

		private bool IsDesktop() => Application.platform.IsEither(RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer, RuntimePlatform.OSXPlayer);

		private bool IsWeb() => Application.platform == RuntimePlatform.WebGLPlayer;

		private bool IsMobile() => Application.isMobilePlatform;

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Adaptive VSync

		private IEnumerator AdaptiveVSync(Settings.AdaptiveVSync adaptiveVSync)
		{
			QualitySettings.vSyncCount = adaptiveVSync.initialState ? 1 : 0;

			yield return InitDelay();

			WaitForSecondsRealtime delay = new WaitForSecondsRealtime(adaptiveVSync.delay);

			while (true)
			{
				if (settings.AdaptiveVSyncEnabled)
				{
					int frame = Time.frameCount;
					double time = Time.unscaledTimeAsDouble;

					yield return delay;

					int deltaFrames = Time.frameCount - frame;
					double deltaTime = Time.unscaledTimeAsDouble - time;

					double framerate = deltaFrames / deltaTime;

					QualitySettings.vSyncCount = framerate < RefreshRate * adaptiveVSync.threshold / 100 ? 0 : 1;
				}
				else yield return null;
			}
		}

		private IEnumerator InitDelay()
		{
			for (int f = 0; f < 3; f++) yield return null;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Editor

#if UNITY_EDITOR

		#region General

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
			EditorUpdate();
			RuntimeEditorUpdate();
		}

		private void EditorUpdate()
		{
			Settings[] settings = { editor, desktop, web, mobile };

			foreach (Settings current in settings)
			{
				current.EditorUpdate();
			}
		}

		private void RuntimeEditorUpdate()
		{
			if (Application.isPlaying && platform == Platform.Editor)
			{
				editor.Apply();
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Inspector

		[CustomEditor(typeof(VSync))]
		private class VSyncDrawer : UnityEditor.Editor
		{
			private static bool buttonState;

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				DrawPlatform(out Platform platform);
				DrawSettings(platform);

				UpdateFocus();

				serializedObject.ApplyModifiedProperties();
			}

			private void DrawPlatform(out Platform value)
			{
				SerializedProperty platform = serializedObject.FindProperty(nameof(VSync.platform));

				EditorGUILayout.PropertyField(platform);

				value = platform.GetValueOfType<Platform>();
			}

			private void DrawSettings(Platform platform)
			{
				SerializedProperty settings = platform switch
				{
					Platform.Editor => serializedObject.FindProperty(nameof(editor)),
					Platform.Desktop => serializedObject.FindProperty(nameof(desktop)),
					Platform.Web => serializedObject.FindProperty(nameof(web)),
					Platform.Mobile => serializedObject.FindProperty(nameof(mobile)),
					_ => throw new InvalidCastException()
				};

				DrawTarget(settings, out Settings.Target target);
				DrawFramerate(settings, target);
				DrawVSync(settings, target);
			}

			private void DrawTarget(SerializedProperty settings, out Settings.Target value)
			{
				SerializedProperty target = settings.FindPropertyRelative("target");

				EditorGUILayout.PropertyField(target);

				value = (Settings.Target)target.intValue;
			}

			private void DrawFramerate(SerializedProperty settings, Settings.Target target)
			{
				if (target == Settings.Target.Framerate)
				{
					SerializedProperty framerate = settings.FindPropertyRelative("framerate");

					GUI.SetNextControlName("Framerate");
					EditorGUILayout.PropertyField(framerate);
				}
			}

			private void DrawVSync(SerializedProperty settings, Settings.Target target)
			{
				if (target == Settings.Target.RefreshRate)
				{
					SerializedProperty vSync = settings.FindPropertyRelative("vSync");

					if (vSync != null)
					{
						EditorGUILayout.PropertyField(vSync, new GUIContent("VSync"));

						Settings.VSync value = (Settings.VSync)vSync.intValue;

						DrawAdaptiveVSync(settings, value);
					}
				}
			}

			private void DrawAdaptiveVSync(SerializedProperty settings, Settings.VSync vSync)
			{
				if (vSync == Settings.VSync.Adaptive)
				{
					if (DrawButton())
					{
						SerializedProperty adaptiveVSync = settings.FindPropertyRelative("adaptiveVSync");

						DrawProperty(nameof(Settings.AdaptiveVSync.initialState));
						DrawProperty(nameof(Settings.AdaptiveVSync.delay), "Delay");
						DrawProperty(nameof(Settings.AdaptiveVSync.threshold), "Threshold", " (%)");

						void DrawProperty(string name, string controlName = null, string suffix = null)
						{
							SerializedProperty property = adaptiveVSync.FindPropertyRelative(name);

							if (controlName != null) GUI.SetNextControlName(controlName);

							GUIContent label = new GUIContent(property.displayName + suffix, property.tooltip);

							EditorGUILayout.PropertyField(property, label);
						}
					}
				}
			}

			private bool DrawButton()
			{
				Rect position = EditorGUILayout.GetControlRect(true, -EditorGUIUtility.standardVerticalSpacing);

				position.x = EditorGUIUtility.labelWidth;
				position.y -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				position.width = EditorGUIUtility.singleLineHeight;
				position.height = EditorGUIUtility.singleLineHeight;

				GUIContent content = new GUIContent(buttonState ? "−" : "+");
				GUIStyle style = new GUIStyle(EditorStyles.miniButton)
				{
					alignment = TextAnchor.MiddleCenter,
					clipping = TextClipping.Overflow,
					fontSize = 15
				};

				if (GUI.Button(position, content, style))
				{
					buttonState = !buttonState;
				}

				return buttonState;
			}

			private void UpdateFocus()
			{
				if (GUI.GetNameOfFocusedControl() == "")
				{
					GUI.FocusControl(null);
				}
			}
		}

		#endregion

#endif

		#endregion

	}
}