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

		#region Target Data

		#region Structures

		public struct Values
		{
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

			public Target target;
			public int framerate;
			public VSync? vSync;
			public AdaptiveVSync? adaptiveVSync;

			#region Values

			public Values(LimitedTarget target, int framerate) : this((Target)target, framerate, null, null) { }

			public Values(LimitedTarget target, int framerate, VSync? vSync, AdaptiveVSync? adaptiveVSync) : this((Target)target, framerate, vSync, adaptiveVSync) { }

			public Values(Target target, int framerate) : this(target, framerate, null, null) { }

			public Values(Target target, int framerate, VSync? vSync, AdaptiveVSync? adaptiveVSync)
			{
				this.target = target;
				this.framerate = framerate;
				this.vSync = vSync;
				this.adaptiveVSync = adaptiveVSync;
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Set

			public void Set(ref Target target, ref int framerate)
			{
				target = this.target;
				framerate = this.framerate;
			}

			public void Set(ref Target target, ref int framerate, ref VSync vSync, ref AdaptiveVSync adaptiveVSync)
			{
				target = this.target;
				framerate = this.framerate;
				vSync = this.vSync ?? vSync;
				adaptiveVSync = this.adaptiveVSync ?? adaptiveVSync;
			}

			public void Set(ref LimitedTarget target, ref int framerate)
			{
				target = (LimitedTarget)this.target;
				framerate = this.framerate;
			}

			public void Set(ref LimitedTarget target, ref int framerate, ref VSync vSync, ref AdaptiveVSync adaptiveVSync)
			{
				target = (LimitedTarget)this.target;
				framerate = this.framerate;
				vSync = this.vSync ?? vSync;
				adaptiveVSync = this.adaptiveVSync ?? adaptiveVSync;
			}

			#endregion

		}

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

		// ----------------------------------------------------------------------------------------------------

		#region Settings

		private abstract class Settings
		{

			#region Values

			public virtual bool UnlimitedSupported => false;

			public virtual bool VSyncSupported => false;

			public virtual bool AdaptiveVSyncEnabled => false;

			public abstract Values GetValues();

			public abstract void SetValues(Values values);

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public abstract void Apply();

			public abstract void EditorUpdate();

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Utilities

			protected Values.LimitedTarget? GetLimitedTarget(Values.Target target) => target < Values.Target.Unlimited ? (Values.LimitedTarget?)(int)target : null;

			protected void ApplyTarget(Values.Target target, int framerate)
			{
				Application.targetFrameRate = target switch
				{
					Values.Target.RefreshRate => RefreshRate,
					Values.Target.Framerate => framerate,
					Values.Target.Unlimited => -1,
					_ => throw new InvalidCastException()
				};
			}

			protected void ApplyTarget(Values.LimitedTarget target, int framerate, int refreshRate)
			{
				Application.targetFrameRate = target switch
				{
					Values.LimitedTarget.RefreshRate => refreshRate,
					Values.LimitedTarget.Framerate => framerate,
					_ => throw new InvalidCastException()
				};
			}

			protected void ApplyVSync(Values.Target target, Values.VSync vSync)
			{
				if (target == Values.Target.RefreshRate && vSync != Values.VSync.Adaptive)
				{
					QualitySettings.vSyncCount = vSync == Values.VSync.Enabled ? 1 : 0;
				}
			}

			protected void ApplyVSync(Values.LimitedTarget target, Values.VSync vSync) => ApplyVSync((Values.Target)target, vSync);

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
			private Values.Target target;
			[SerializeField]
			private int framerate = 144;

			#region Values

			public override bool UnlimitedSupported => true;

			public override Values GetValues() => new Values(target, framerate);

			public override void SetValues(Values values) => values.Set(ref target, ref framerate);

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
			private Values.Target target;
			[SerializeField]
			private Values.VSync vSync;
			[SerializeField]
			private int framerate = 144;
			[SerializeField]
			private AdaptiveVSync adaptiveVSync;

			#region Values

			public override bool UnlimitedSupported => true;

			public override bool VSyncSupported => true;

			public override bool AdaptiveVSyncEnabled => target == Values.Target.RefreshRate && vSync == Values.VSync.Adaptive;

			public override Values GetValues() => new Values(target, framerate, vSync, adaptiveVSync);

			public override void SetValues(Values values) => values.Set(ref target, ref framerate, ref vSync, ref adaptiveVSync);

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
			private Values.LimitedTarget target;
			[SerializeField]
			private Values.VSync vSync;
			[SerializeField]
			private int framerate = 144;
			[SerializeField]
			private AdaptiveVSync adaptiveVSync;

			#region Values

			public override bool VSyncSupported => true;

			public override bool AdaptiveVSyncEnabled => target == Values.LimitedTarget.RefreshRate && vSync == Values.VSync.Adaptive;

			public override Values GetValues() => new Values(target, framerate, vSync, adaptiveVSync);

			public override void SetValues(Values values) => values.Set(ref target, ref framerate, ref vSync, ref adaptiveVSync);

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
			private Values.LimitedTarget target;
			[SerializeField]
			private int framerate = 120;

			#region Values

			public override Values GetValues() => new Values(target, framerate);

			public override void SetValues(Values values) => values.Set(ref target, ref framerate);

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region General

			public override void Apply() => ApplyTarget(target, framerate, RefreshRate);

			public override void EditorUpdate() => ClampFramerate(ref framerate, 240);

			#endregion

		}

		#endregion

		#endregion

		private enum Platform
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

		private static int RefreshRate => Screen.currentResolution.refreshRate;

		public static bool PlatformSupported => Instance.settings;

		public static bool UnlimitedSupported => Instance.settings?.UnlimitedSupported ?? false;

		public static bool VSyncSupported => Instance.settings?.VSyncSupported ?? false;

		#region Init

		protected override void Init()
		{
			if (TryGetSettings(out settings))
			{
				settings.Apply();

				if (settings.GetValues().adaptiveVSync is AdaptiveVSync adaptiveVSync)
				{
					StartCoroutine(AdaptiveVSyncUpdate(adaptiveVSync));
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

		#region Target

		/// <summary> Retrieves the current set of <see cref="Values"/> applied. </summary>
		/// <returns> True if the platform is supported. </returns>
		public static bool GetTargetValues(out Values values)
		{
			Settings settings = Instance.settings;

			values = settings?.GetValues() ?? default;

			return settings;
		}

		/// <summary> Sets a new set of <see cref="Values"/> and applies it. </summary>
		/// <returns> True if the platform is supported. </returns>
		public static bool SetTargetValues(Values values)
		{
			Settings settings = Instance.settings;

			settings?.SetValues(values);
			settings?.Apply();

			return settings;
		}

		/// <summary> Sets the target framerate to match the screen's refresh rate. </summary>
		/// <returns> True if the platform is supported. </returns>
		public static bool SetTargetRefreshRate() => SetTargetRefreshRate(Values.VSync.Disabled, null);

		/// <summary> Sets the target framerate to match the screen's refresh rate and attempts to set the specified <see cref="Values.VSync"/> if the platform supports it. </summary>
		/// <returns> True if the platform is supported and the attempted <see cref="Values.VSync"/> is supported. </returns>
		public static bool SetTargetRefreshRate(Values.VSync vSync) => SetTargetRefreshRate(vSync, null);

		/// <summary> Sets the target framerate to match the screen's refresh rate and attempts to set the specified <see cref="Values.VSync"/> if the platform supports it. </summary>
		/// <param name="adaptiveVSync"> Custom settings for adaptive VSync. Pass <see cref="Values.VSync.Adaptive"/> as the argument for <paramref name="vSync"/> to enable adaptive VSync. </param>
		/// <returns> True if the platform is supported and the attempted <see cref="Values.VSync"/> is supported. </returns>
		public static bool SetTargetRefreshRate(Values.VSync vSync, AdaptiveVSync adaptiveVSync) => SetTargetRefreshRate(vSync, (AdaptiveVSync?)adaptiveVSync);

		private static bool SetTargetRefreshRate(Values.VSync vSync, AdaptiveVSync? adaptiveVSync)
		{
			bool vSyncSuccess = VSyncSupported || vSync == Values.VSync.Disabled;

			if (GetTargetValues(out Values values))
			{
				values.target = Values.Target.RefreshRate;
				values.vSync = vSync;
				values.adaptiveVSync = adaptiveVSync;
			}

			return SetTargetValues(values) && vSyncSuccess;
		}

		/// <summary> Sets the target framerate to match the specified <paramref name="framerate"/>. </summary>
		/// <returns> True if the platform is supported. </returns>
		public static bool SetTargetFramerate(int framerate)
		{
			if (GetTargetValues(out Values values))
			{
				values.target = Values.Target.Framerate;
				values.framerate = framerate;
			}

			return SetTargetValues(values);
		}

		/// <summary> Sets the target framerate to be unlimited if the platform supports it. </summary>
		/// <returns> True if the platform is supported and ultimited framerate is supported. </returns>
		public static bool SetTargetUnlimited()
		{
			if (UnlimitedSupported)
			{
				if (GetTargetValues(out Values values))
				{
					values.target = Values.Target.Unlimited;
				}

				return SetTargetValues(values);
			}

			return false;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Adaptive VSync

		private IEnumerator AdaptiveVSyncUpdate(AdaptiveVSync adaptiveVSync)
		{
			QualitySettings.vSyncCount = adaptiveVSync.initialState ? 1 : 0;

			yield return WaitForFrames(3);

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

		private static IEnumerator WaitForFrames(int frames)
		{
			for (int f = 0; f < frames; f++) yield return null;
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

				DrawTarget(settings, out Values.Target target);
				DrawFramerate(settings, target);
				DrawVSync(settings, target);
			}

			private void DrawTarget(SerializedProperty settings, out Values.Target value)
			{
				SerializedProperty target = settings.FindPropertyRelative(nameof(Values.target));

				EditorGUILayout.PropertyField(target);

				value = (Values.Target)target.intValue;
			}

			private void DrawFramerate(SerializedProperty settings, Values.Target target)
			{
				if (target == Values.Target.Framerate)
				{
					SerializedProperty framerate = settings.FindPropertyRelative(nameof(Values.framerate));

					GUI.SetNextControlName("Framerate");
					EditorGUILayout.PropertyField(framerate);
				}
			}

			private void DrawVSync(SerializedProperty settings, Values.Target target)
			{
				if (target == Values.Target.RefreshRate)
				{
					SerializedProperty vSync = settings.FindPropertyRelative(nameof(Values.vSync));

					if (vSync != null)
					{
						EditorGUILayout.PropertyField(vSync, new GUIContent("VSync"));

						Values.VSync value = (Values.VSync)vSync.intValue;

						DrawAdaptiveVSync(settings, value);
					}
				}
			}

			private void DrawAdaptiveVSync(SerializedProperty settings, Values.VSync vSync)
			{
				if (vSync == Values.VSync.Adaptive)
				{
					if (DrawButton())
					{
						SerializedProperty adaptiveVSync = settings.FindPropertyRelative(nameof(Values.adaptiveVSync));

						DrawProperty(nameof(AdaptiveVSync.initialState));
						DrawProperty(nameof(AdaptiveVSync.delay), "Delay", " (s)");
						DrawProperty(nameof(AdaptiveVSync.threshold), "Threshold", " (%)");

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