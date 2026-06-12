using System;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Restrict editing of the field in the Inspector. </summary>
	/// <remarks> Does not work for children inside Serializable types. </remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ReadOnlyAttribute : PropertyAttribute
	{
		public readonly bool toggleable;

		public bool Editable { get; private set; }

		/// <param name="toggleable"> Allows the ability to toggle editing on or off. </param>
		public ReadOnlyAttribute(bool toggleable = false)
		{
			this.toggleable = toggleable;

			order = int.MaxValue;
		}

		public void Toggle() => Editable = !Editable;
	}

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
		public class ReadOnlyDrawer : DecoratorDrawer
		{
			public override float GetHeight() => 0;

			public override void OnGUI(Rect position)
			{
				ReadOnlyAttribute readOnly = attribute as ReadOnlyAttribute;

				if (readOnly.toggleable) DrawButton(position, readOnly);

				GUI.enabled = readOnly.Editable;
			}

			private void DrawButton(Rect position, ReadOnlyAttribute readOnly)
			{
				position.position -= new Vector2(16, 1);
				position.size = new Vector2(16, 21);

				if (GUI.Button(position, GUIContent.none, GUIStyle.none)) readOnly.Toggle();

				GUI.Label(position, new GUIContent(readOnly.Editable ? "✎" : "✐", "ReadOnly made by Gamba"));
			}
		}
	}

#endif

}