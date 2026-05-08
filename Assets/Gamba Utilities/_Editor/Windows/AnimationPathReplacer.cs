using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace GambaUtilities.Editor
{
	public class AnimationPathReplacer : EditorWindow
	{

		#region Match Types

		private enum MatchType
		{
			Any,
			StartsWith,
			EndsWith
		}

		#endregion

		[SerializeField]
		private AnimationClip animation;
		[SerializeField]
		private MatchType matchType;
		[SerializeField]
		private string search;
		[SerializeField]
		private string replace;

		private new const string title = "Animation Path Replacer";

		private SerializedObject serializedObject;

		#region Window

		[MenuItem(EditorUtilities.windowRoot + title)]
		public static void Open()
		{
			GetWindow<AnimationPathReplacer>(title, true);
		}

		private void OnEnable()
		{
			serializedObject = new SerializedObject(this);

			minSize = new Vector2(350, GetHeight());
		}

		private float GetHeight()
		{
			const float inspectorMargin = 4;
			const float propertyHeight = 18;
			const float propertySeparation = 2;
			const float spaceHeight = 6;
			const float buttonHeight = 25;

			const int margins = 3;
			const int properties = 4;
			const int spaces = 1;
			const int buttons = 1;

			float height = default;

			height += inspectorMargin * margins;
			height += propertyHeight * properties + propertySeparation * Mathf.Max(0, properties - 1);
			height += spaceHeight * spaces;
			height += buttonHeight * buttons;

			return height;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------------------------------

		#region OnGUI

		private void OnGUI()
		{
			serializedObject.Update();

			DrawProperties();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawProperties()
		{
			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.4f;

			DrawProperty(nameof(animation));
			DrawProperty(nameof(matchType));
			DrawProperty(nameof(search));
			DrawProperty(nameof(replace));

			DrawSwapButton();

			EditorGUILayout.EndVertical();

			DrawButton("Replace", Replace);
		}

		private void DrawProperty(string name)
		{
			SerializedProperty property = serializedObject.FindProperty(name);

			EditorGUILayout.PropertyField(property);
		}

		private void DrawButton(string name, Action onTrigger)
		{
			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(name, GUILayout.Height(25), GUILayout.Width(100)))
			{
				onTrigger?.Invoke();
			}

			GUILayout.EndHorizontal();
		}

		private void DrawSwapButton()
		{
			Rect rect = GUILayoutUtility.GetLastRect();

			rect.width = 22;
			rect.x += EditorGUIUtility.labelWidth - rect.width - EditorGUIUtility.standardVerticalSpacing;
			rect.y -= (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) / 2;

			GUIContent content = new GUIContent("⇅", "Swap values");

			GUIStyle style = new GUIStyle(EditorStyles.miniButton)
			{
				fontSize = 15
			};

			if (GUI.Button(rect, content, style)) Swap();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------------------------------

		#region Animation

		private void Replace()
		{
			if (animation == null || search == "") return;

			Dictionary<EditorCurveBinding, AnimationCurve> curves = new Dictionary<EditorCurveBinding, AnimationCurve>();
			List<EditorCurveBinding> bindings = AnimationUtility.GetCurveBindings(animation).ToList();

			bindings.ForEach(ReplaceBinding);

			AnimationUtility.SetEditorCurves(animation, curves.Keys.ToArray(), curves.Values.ToArray());

			Debug.Log($"Replaced {curves.Count / 2} animation properties");

			void ReplaceBinding(EditorCurveBinding binding)
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, binding);

				EditorCurveBinding result = binding;
				result.path = Replace(binding.path);

				if (result == binding || bindings.Contains(result)) return;

				curves.Add(binding, null);
				curves.Add(result, curve);
			}
		}

		private string Replace(string path)
		{
			List<char> chars = path.ToList();

			List<int> indices = matchType switch
			{
				MatchType.Any => Regex.Matches(path, search).Cast<Match>().Select(match => match.Index).ToList(),
				MatchType.StartsWith => GetIndexIf(path.StartsWith(search), 0),
				MatchType.EndsWith => GetIndexIf(path.EndsWith(search), path.Length - search.Length),

				_ => throw new InvalidCastException($"The enum value {(int)matchType} is not a valid {nameof(MatchType)}")
			};

			for (int i = indices.Count - 1; i > -1; i--)
			{
				int index = indices[i];

				chars.RemoveRange(index, search.Length);
				chars.InsertRange(index, replace);
			}

			static List<int> GetIndexIf(bool condition, int index)
			{
				List<int> indices = new List<int>();

				if (condition) indices.Add(index);

				return indices;
			}

			return string.Concat(chars);
		}

		private void Swap()
		{
			string oldFind = search;

			search = replace;
			replace = oldFind;
		}

		#endregion

	}
}

#endif