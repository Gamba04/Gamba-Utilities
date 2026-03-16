using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Extend the field's name with additional text in the Inspector. </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ExtendNameAttribute : PropertyAttribute
	{
		public readonly string text;

		public ExtendNameAttribute(string text) => this.text = text;
	}

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(ExtendNameAttribute))]
		public class ExtendNameDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return EditorGUI.GetPropertyHeight(property, true);
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				ExtendNameAttribute extendName = attribute as ExtendNameAttribute;

				label.text += extendName.text;

                EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}

#endif

}