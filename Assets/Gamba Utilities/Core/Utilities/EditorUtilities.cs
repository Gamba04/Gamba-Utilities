using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

#if UNITY_EDITOR

namespace GambaUtilities.Editor
{
	public static class EditorUtilities
	{
		public const string scriptableRoot = "Gamba Utilities/";
		public const string windowRoot = "Window/Gamba Utilities/";

		#region Serialized Properties

		/// <summary> Attempts to find the element index of the property in case of being part of a <see cref="List{T}"/> or <see cref="Array"/>. </summary>
		public static bool TryGetIndex(this SerializedProperty property, out int index)
		{
			string path = property.propertyPath;

			if (path.EndsWith("]"))
			{
				int start = path.LastIndexOf('[') + 1;
				int length = path.Length - 1 - start;

				index = int.Parse(path.Substring(start, length));
				return true;
			}

			index = -1;
			return false;
		}

		/// <summary> Attempts to find the value of the property as <typeparamref name="T"/>. </summary>
		public static bool TryGetValueOfType<T>(this SerializedProperty property, out T value)
		{
			try
			{
				value = property.GetValueOfType<T>();
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		/// <summary> Finds the value of the property as <typeparamref name="T"/>. </summary>
		public static T GetValueOfType<T>(this SerializedProperty property)
		{
			string path = property.propertyPath;
			string[] names = path.Remove("Array.data[", "]").Split('.');

			object currentObject = property.serializedObject.targetObject;

			foreach (string name in names)
			{
				currentObject = int.TryParse(name, out int index) ? GetElementValue(index) : GetFieldValue(name);

				if (currentObject == null) throw new Exception($"Could not retrieve next field or element '{name}'");
			}

			return currentObject is T value ? value : throw new InvalidCastException($"Could not assign {currentObject.GetType().Name} '{names[names.Length - 1]}' as {typeof(T).Name}");

			object GetElementValue(int index)
			{
				IEnumerable enumerable = (IEnumerable)currentObject;

				List<object> list = new List<object>(enumerable.Cast<object>());

				return list?[index];
			}

			object GetFieldValue(string name)
			{
				Type type = currentObject.GetType();
				FieldInfo field;

				do
				{
					field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					type = type.BaseType;
				}
				while (field == null && type != null);

				return field?.GetValue(currentObject);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Assets

		public static T FindAssetOfType<T>()
			where T : Object
		{
			string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

			if (guids.Length > 0)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[0]);

				return AssetDatabase.LoadAssetAtPath<T>(path);
			}

			return null;
		}

		public static T[] FindAssetsOfType<T>()
			where T : Object
		{
			string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

			return Array.ConvertAll(guids, Load);

			static T Load(string guid)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				return AssetDatabase.LoadAssetAtPath<T>(path);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Other

		/// <summary> Destroys an <see cref="Object"/> properly while in Edit Mode. </summary>
		/// <param name="canUndo"> Specifies whether the action can be undone in the Editor. </param>
		public static void DestroyInEditMode(Object obj, bool canUndo = true)
		{
			if (Application.isPlaying) return;

			EditorApplication.delayCall += DestroyObject;

			void DestroyObject()
			{
				if (canUndo)
				{
					Undo.DestroyObjectImmediate(obj);
				}
				else
				{
					EditorUtility.SetDirty(obj);

					Object.DestroyImmediate(obj);
				}

				EditorApplication.delayCall -= DestroyObject;
			}
		}

		#endregion

	}
}

#endif