using System;
using UnityEngine;

namespace GambaUtilities
{
	public static class Log
	{

		#region Scopes

		[Flags]
		public enum Scope
		{
			Editor = 1,
			DevelopmentBuild = 2,
			ReleaseBuild = 4,
			Everything = Editor | DevelopmentBuild | ReleaseBuild
		}

		#endregion

		public static bool enabled = true;
		public static Scope scope = Scope.Editor | Scope.DevelopmentBuild;

		#region Logs

		#region Info

		/// <summary> Logs info to the Unity Console. </summary>
		public static void Info(object message) => Execute(message, Debug.Log);

		/// <summary> Logs info to the Unity Console with a custom <see cref="Color"/>. </summary>
		public static void Info(object message, Color color) => Execute(message, Debug.Log, color);

		/// <summary> Logs info to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
		public static void Info(object message, FontStyle style) => Execute(message, Debug.Log, style: style);

		/// <summary> Logs info to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
		public static void Info(object message, Color color, FontStyle style) => Execute(message, Debug.Log, color, style);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Warning

		/// <summary> Logs a warning to the Unity Console. </summary>
		public static void Warning(object message) => Execute(message, Debug.LogWarning);

		/// <summary> Logs a warning to the Unity Console with a custom <see cref="Color"/>. </summary>
		public static void Warning(object message, Color color) => Execute(message, Debug.LogWarning, color);

		/// <summary> Logs a warning to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
		public static void Warning(object message, FontStyle style) => Execute(message, Debug.LogWarning, style: style);

		/// <summary> Logs a warning to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
		public static void Warning(object message, Color color, FontStyle style) => Execute(message, Debug.LogWarning, color, style);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Error

		/// <summary> Logs an error to the Unity Console. </summary>
		public static void Error(object message) => Execute(message, Debug.LogError);

		/// <summary> Logs an error to the Unity Console with a custom <see cref="Color"/>. </summary>
		public static void Error(object message, Color color) => Execute(message, Debug.LogError, color);

		/// <summary> Logs an error to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
		public static void Error(object message, FontStyle style) => Execute(message, Debug.LogError, style: style);

		/// <summary> Logs an error to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
		public static void Error(object message, Color color, FontStyle style) => Execute(message, Debug.LogError, color, style);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Other

		public static void Empty() => Execute("", Debug.Log);

		#endregion

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Processing

		#region Execution

		private static void Execute(object message, Action<string> method, Color? color = null, FontStyle style = FontStyle.Normal)
		{
			if (Validate())
			{
				message ??= "Null";
				string text = message.ToString();

				ApplyColor(ref text, color);
				ApplyStyle(ref text, style);

				method(text + "\n");
			}
		}

		public static bool Validate()
		{
			bool isValid = enabled;

			isValid &= scope.HasFlag(Scope.Editor) || !Application.isEditor;
			isValid &= scope.HasFlag(Scope.DevelopmentBuild) || !Debug.isDebugBuild;
			isValid &= scope.HasFlag(Scope.ReleaseBuild) || Application.isEditor || Debug.isDebugBuild;

			return isValid;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Color

		private static void ApplyColor(ref string text, Color? color)
		{
			if (color.HasValue)
			{
				string hex = ColorUtility.ToHtmlStringRGBA(color.Value);

				text = $"<color=#{hex}>{text}</color>";
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Style

		private static void ApplyStyle(ref string text, FontStyle style)
		{
			text = style switch
			{
				FontStyle.Bold => Bold(text),
				FontStyle.Italic => Italic(text),
				FontStyle.BoldAndItalic => Bold(Italic(text)),
				_ => text
			};

			static string Bold(string text) => $"<b>{text}</b>";

			static string Italic(string text) => $"<i>{text}</i>";
		}

		#endregion

		#endregion

	}
}