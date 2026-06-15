using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Displays the field in the Inspector only if a condition is met. </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ShowIfAttribute : PropertyAttribute
	{
		public readonly bool? condition;
		public readonly string field;
		public readonly object value;

		/// <summary> Evaluates a compile-time condition to be met. </summary>
		public ShowIfAttribute(bool condition) => this.condition = condition;

		/// <summary> Evaluates the runtime value of <paramref name="field"/> and compares it with <paramref name="value"/>. </summary>
		/// <param name="field"> Name or relative path of the sibling field to evaluate. </param>
		/// <param name="value"> Constant value to be evaluated against. </param>
		public ShowIfAttribute(string field, object value)
		{
			this.field = field;
			this.value = value;
		}
	}

	#region Editor

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(ShowIfAttribute))]
		public class ShowIfDrawer : PropertyDrawer
		{
			private bool isVisible;

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return IsVisible(property) ? EditorGUIUtility.singleLineHeight : 0;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				if (isVisible) EditorGUI.PropertyField(position, property, label, true);
			}

			private bool IsVisible(SerializedProperty property)
			{
				ShowIfAttribute showIf = attribute as ShowIfAttribute;

				return isVisible = showIf.condition ?? Evaluate(property, showIf.field, showIf.value);
			}

			private bool Evaluate(SerializedProperty property, string field, object value)
			{
				SerializedProperty fieldProperty = property.FindSiblingProperty(field);

				return fieldProperty.GetValueOfType<object>().Equals(value);
			}
		}
	}

#endif

	#endregion

}