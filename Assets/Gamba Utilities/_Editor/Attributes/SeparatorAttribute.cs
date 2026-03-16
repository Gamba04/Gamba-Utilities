using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Add a separator line between fields in the Inspector. </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class SeparatorAttribute : PropertyAttribute
	{
		public readonly float thickness;

		public SeparatorAttribute(float thickness = 1)
		{
			this.thickness = thickness;

			order = -1;
		}
	}

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(SeparatorAttribute))]
		public class SeparatorDrawer : DecoratorDrawer
		{
			public override float GetHeight()
			{
				SeparatorAttribute separator = attribute as SeparatorAttribute;

				return EditorGUIUtility.singleLineHeight * 1.5f + separator.thickness;
			}

			public override void OnGUI(Rect position)
			{
				SeparatorAttribute separator = attribute as SeparatorAttribute;

				position.height = separator.thickness;
				position.y += (GetHeight() - position.height) / 2;

				EditorGUI.DrawRect(position, GetColor());
			}

			private Color GetColor()
			{
				float value = EditorGUIUtility.isProSkin ? 0.1f : 0.5f;

				return new Color(value, value, value);
			}
		}
	}

#endif

}