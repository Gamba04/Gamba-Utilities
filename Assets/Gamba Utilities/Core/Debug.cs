using System;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

public static class Debug
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

	#region Log

	/// <summary> Logs a message to the Unity Console. </summary>
	public static void Log(object message) => Execute(message, UnityDebug.Log);

	/// <summary> Logs a message to the Unity Console with a custom <see cref="Color"/>. </summary>
	public static void Log(object message, Color color) => Execute(message, UnityDebug.Log, color);

	/// <summary> Logs a message to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
	public static void Log(object message, FontStyle style) => Execute(message, UnityDebug.Log, style: style);

	/// <summary> Logs a message to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
	public static void Log(object message, Color color, FontStyle style) => Execute(message, UnityDebug.Log, color, style);

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Warning

	/// <summary> Logs a warning to the Unity Console. </summary>
	public static void LogWarning(object message) => Execute(message, UnityDebug.LogWarning);

	/// <summary> Logs a warning to the Unity Console with a custom <see cref="Color"/>. </summary>
	public static void LogWarning(object message, Color color) => Execute(message, UnityDebug.LogWarning, color);

	/// <summary> Logs a warning to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
	public static void LogWarning(object message, FontStyle style) => Execute(message, UnityDebug.LogWarning, style: style);

	/// <summary> Logs a warning to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
	public static void LogWarning(object message, Color color, FontStyle style) => Execute(message, UnityDebug.LogWarning, color, style);

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Error

	/// <summary> Logs an error to the Unity Console. </summary>
	public static void LogError(object message) => Execute(message, UnityDebug.LogError);

	/// <summary> Logs an error to the Unity Console with a custom <see cref="Color"/>. </summary>
	public static void LogError(object message, Color color) => Execute(message, UnityDebug.LogError, color);

	/// <summary> Logs an error to the Unity Console with a custom <see cref="FontStyle"/>. </summary>
	public static void LogError(object message, FontStyle style) => Execute(message, UnityDebug.LogError, style: style);

	/// <summary> Logs an error to the Unity Console with a custom <see cref="Color"/> and <see cref="FontStyle"/>. </summary>
	public static void LogError(object message, Color color, FontStyle style) => Execute(message, UnityDebug.LogError, color, style);

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Other

	/// <summary> Logs an empty message to the Unity Console. </summary>
	public static void LogEmpty() => Execute("", UnityDebug.Log);

	#endregion

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Processing

	#region Execution

	private static void Execute(object message, Action<string> method, Color? color = null, FontStyle style = FontStyle.Normal)
	{
		if (IsValid())
		{
			message ??= "Null";

			string text = message.ToString();
			string[] lines = text.Split('\n');

			for (int i = 0; i < lines.Length; i++)
			{
				ApplyColor(ref lines[i], color);
				ApplyStyle(ref lines[i], style);
			}

			text = string.Join("\n", lines);

			method(text + "\n");
		}
	}

	private static bool IsValid()
	{
		bool isValid = enabled;

		isValid &= scope.HasFlag(Scope.Editor) || !Application.isEditor;
		isValid &= scope.HasFlag(Scope.DevelopmentBuild) || !UnityDebug.isDebugBuild;
		isValid &= scope.HasFlag(Scope.ReleaseBuild) || Application.isEditor || UnityDebug.isDebugBuild;

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