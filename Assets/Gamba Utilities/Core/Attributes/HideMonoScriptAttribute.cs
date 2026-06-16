using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Hides the readonly Script field at the beginning of the <see cref="MonoBehaviour"/> in the Inspector. </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class HideMonoScriptAttribute : Attribute { }

	#region Editor

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomEditor(typeof(MonoBehaviour), true, isFallback = true), CanEditMultipleObjects]
		public class MonoBehaviourDrawer : HideMonoScriptDrawer { }

		[CustomEditor(typeof(ScriptableObject), true, isFallback = true), CanEditMultipleObjects]
		public class ScriptableObjectDrawer : HideMonoScriptDrawer { }

		public abstract class HideMonoScriptDrawer : UnityEditor.Editor
		{
			private const string fieldName = "m_Script";

			#region OnInspectorGUI

			private bool hideMonoScript;

			private void OnEnable()
			{
				hideMonoScript = target.GetType().GetCustomAttribute<HideMonoScriptAttribute>(true) != null;
			}

			public override void OnInspectorGUI()
			{
				if (hideMonoScript)
				{
					serializedObject.Update();

					DrawPropertiesExcluding(serializedObject, fieldName);

					serializedObject.ApplyModifiedProperties();
				}
				else DrawDefaultInspector();
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Context Menu

			[MenuItem("CONTEXT/" + nameof(MonoBehaviour) + "/Select Script", priority = 601)]
			private static void OnSelectScript(MenuCommand command)
			{
				using SerializedObject serializedObject = new SerializedObject(command.context);
				SerializedProperty property = serializedObject.FindProperty(fieldName);

				if (property != null) EditorGUIUtility.PingObject(property.objectReferenceValue);
			}

			#endregion

		}
	}

#endif

	#endregion

}