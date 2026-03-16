using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Modify the field's display name with custom text in the Inspector. </summary>
	/// <remarks>
	/// You may use the following macros to insert specific data in the name:
	/// <code>
	/// {name} : Original display name of the field
	/// {var}  : Actual name of the field
	/// {i}    : (Lists/Arrays) index
	/// {i+1}  : (Lists/Arrays) index + 1
	/// {i-1}  : (Lists/Arrays) index - 1
	/// </code>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class CustomNameAttribute : PropertyAttribute
	{
		public readonly string name;

		public CustomNameAttribute(string name) => this.name = name;
	}

#if UNITY_EDITOR 

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(CustomNameAttribute))]
		public class CustomNameDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return EditorGUI.GetPropertyHeight(property, true);
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				CustomNameAttribute customName = attribute as CustomNameAttribute;

				label.text = GetDisplayName(property, customName.name);

				EditorGUI.PropertyField(position, property, label, true);
			}

			private string GetDisplayName(SerializedProperty property, string name)
			{
				bool isElement = property.TryGetIndex(out int index);

				Dictionary<string, string> replacements = new Dictionary<string, string>()
				{
					{ "name", property.displayName },
					{ "var", property.name },
					{ "i",    GetIndexValue() },
					{ "i+1",  GetIndexValue(1) },
					{ "i-1",  GetIndexValue(-1) }
				};

				foreach (KeyValuePair<string, string> replacement in replacements)
				{
					name = name.Replace($"{{{replacement.Key}}}", replacement.Value);
				}

				return name;

				string GetIndexValue(int offset = 0) => isElement ? (index + offset).ToString() : "";
			}
		}
	}
		
#endif

}