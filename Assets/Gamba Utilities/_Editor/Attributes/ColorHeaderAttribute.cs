using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Add a custom color header in the Inspector. </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class ColorHeaderAttribute : HeaderAttribute
	{
		public readonly Color? color;

		private ColorHeaderAttribute(string header, Color? color) : base(header) => this.color = color;

		/// <param name="color"> Hexadecimal color code formatted as <c>#RRGGBB</c>. </param>
		public ColorHeaderAttribute(string header, string color) : this(header, ParseColor(color)) { }

		public ColorHeaderAttribute(string header, byte r, byte g, byte b, byte a = 255) : this(header, new Color32(r, g, b, a)) { }

		public ColorHeaderAttribute(string header, float r, float g, float b, float a = 1) : this(header, new Color(r, g, b, a)) { }

		private static Color? ParseColor(string hex) => ColorUtility.TryParseHtmlString(hex, out Color color) ? color : (Color?)null;
	}

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(ColorHeaderAttribute))]
		public class ColorHeaderDrawer : DecoratorDrawer
		{
			public override float GetHeight() => EditorGUIUtility.singleLineHeight * 1.5f;

			public override void OnGUI(Rect position)
			{
				position.height += 9;

				ColorHeaderAttribute header = attribute as ColorHeaderAttribute;

				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				style.normal.textColor = header.color ?? style.normal.textColor;

				GUI.Label(position, header.header, style);
			}
		}
	}

#endif

}