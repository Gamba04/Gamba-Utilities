using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GambaUtilities
{
	using Audio;

	#region Sound

	namespace Audio
	{
		public abstract class Sound
		{
			[SerializeField]
			private TrackSelector track;

			protected AudioTrack Track => track.GetTrack();
		}
	}

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Simple Sound

	/// <summary> Sound to be played and forgotten about. </summary>
	[Serializable]
	public class SimpleSound : Sound
	{
		private Transform transform;

		#region Init

		public void Init(Transform transform)
		{
			this.transform = transform;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Play

		/// <summary> Plays the sound independently. </summary>
		/// <param name="volume"> This value is multiplied by the track's configured volume. </param>
		public void Play(float volume = 1)
		{
			if (transform) AudioPlayer.Play(Track, transform.position, volume);
			else AudioPlayer.Play(Track, volume);
		}

		/// <summary> Plays the sound independently at the specified position. </summary>
		/// <param name="volume"> This value is multiplied by the track's configured volume. </param>
		public void Play(Vector3 position, float volume = 1)
		{
			AudioPlayer.Play(Track, position, volume);
		}

		#endregion

	}

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Local Sound

	/// <summary> Sound to be played and controlled locally. </summary>
	[Serializable]
	public class LocalSound : Sound
	{
		public AudioSource source;

		public bool IsPlaying => source.isPlaying;

		#region Play

		/// <summary> Starts a new playback of the sound. </summary>
		/// <param name="volume"> This value is multiplied by the track's configured volume. </param>
		public void Play(float volume = 1) => Play(false, volume);

		/// <summary> Starts a new playback of the sound. </summary>
		/// <param name="volume"> This value is multiplied by the track's configured volume. </param>
		public void Play(bool loop, float volume = 1)
		{
			Track.Setup(source);

			source.loop = loop;
			source.volume *= volume;

			source.Play();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Other

		/// <summary> Pauses the current playback. </summary>
		public void Pause()
		{
			source.Pause();
		}

		/// <summary> Resumes the current playback. </summary>
		public void Resume()
		{
			source.UnPause();
		}

		#endregion

	}

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Track Selector

	namespace Audio
	{
		[Serializable]
		public struct TrackSelector
		{
			public AudioLibrary library;
			public int track;

			public AudioTrack GetTrack()
			{
				if (library)
				{
					if (library.Count > 0) return library.GetTrack(track);
					else throw new Exception($"The sound's library '{library.name}' has no tracks");
				}
				else if (Equals(library, null)) throw new UnassignedReferenceException("The sound's library is not assigned");
				else throw new MissingReferenceException("The sound's library is missing");
			}
		}
	}

	#region Editor

#if UNITY_EDITOR

	namespace Editor
	{
		[CustomPropertyDrawer(typeof(TrackSelector))]
		public class TrackSelectorDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				position.height = EditorGUIUtility.singleLineHeight;

				DrawLibrary(ref position, property, out List<string> tracks);
				DrawTrack(position, property, tracks);
			}

			#region Library

			private void DrawLibrary(ref Rect position, SerializedProperty property, out List<string> tracks)
			{
				SerializedProperty library = property.FindPropertyRelative(nameof(TrackSelector.library));

				EditorGUI.PropertyField(position, library);

				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

				tracks = GetTracks(library);
			}

			private List<string> GetTracks(SerializedProperty library)
			{
				return library.TryGetValueOfType(out AudioLibrary value) ? value.GetTrackNames() : new List<string>();
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Track

			private void DrawTrack(Rect position, SerializedProperty property, List<string> tracks)
			{
				SerializedProperty track = property.FindPropertyRelative(nameof(TrackSelector.track));

				GUIContent[] contents = GetContents(tracks);
				int[] values = Enumerable.Range(0, tracks.Count).ToArray();

				track.intValue = Mathf.Clamp(track.intValue, 0, tracks.Count - 1);
				EditorGUI.IntPopup(position, track, contents, values);
			}

			private GUIContent[] GetContents(List<string> tracks)
			{
				List<GUIContent> contents = new List<GUIContent>(tracks.Count);

				foreach (string track in tracks)
				{
					const string zeroWidthSpace = "\u200B";

					string text = !string.IsNullOrWhiteSpace(track) ? track : zeroWidthSpace;

					while (contents.Exists(content => content.text == text)) text += zeroWidthSpace;

					contents.Add(new GUIContent(text));
				}

				return contents.ToArray();
			}

			#endregion

		}
	}

#endif

	#endregion

	#endregion

}