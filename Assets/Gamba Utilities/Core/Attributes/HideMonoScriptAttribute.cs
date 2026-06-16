using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	/// <summary> Hides the readonly Script field at the beginning of the <see cref="MonoBehaviour"/> in the Inspector. </summary>
	/// <remarks>
	///		You may also define a custom scripting symbol as <c>HIDE_MONOSCRIPT</c> to apply this feature to the entire project.<br/><br/>
	///		Custom scripting symbols can be defined from <b>Project Settings → Player → Other Settings → Script Compilation</b>.
	/// </remarks>
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
				ApplyDefinedSymbol();

				hideMonoScript = hideMonoScript || target.GetType().GetCustomAttribute<HideMonoScriptAttribute>(true) != null;
			}

			[Conditional("HIDE_MONOSCRIPT")]
			private void ApplyDefinedSymbol()
			{
				hideMonoScript = true;
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