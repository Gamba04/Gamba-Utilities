using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace GambaUtilities.Audio
{

	#region Audio Track

	[Serializable]
	public class AudioTrack : SerializableElement
	{
		[SerializeField, HideInInspector]
		private AudioMixerGroup mixer;
		[CustomName("Name")]
		[SerializeField]
		private string trackName;
		[SerializeField]
		[CustomName("Clip {i+1}")]
		[Tooltip("Plays one of these clips randomly every time the track is played")]
		private List<AudioClip> clips;
		[SerializeField]
		[Range(0, 1)]
		private float volume;
		[SerializeField]
		[Range(-3, 3)]
		private float pitch;
		[SerializeField]
		[Range(0, 1)]
		private float spatialBlend;

		public string Name => name;

		#region Init

		public override void Init()
		{
			volume = 1;
			pitch = 1;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Setup

		public void Setup(AudioSource source)
		{
			source.clip = GetClip();
			source.outputAudioMixerGroup = mixer;
			source.playOnAwake = false;
			source.volume = volume;
			source.pitch = pitch;
			source.spatialBlend = spatialBlend;
		}

		public AudioClip GetClip()
		{
			return clips[Random.Range(0, clips.Count)];
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Inspector

		public void EditorUpdate(int index, AudioMixerGroup mixer)
		{
			name = trackName.HasContent() ? trackName : $"Track {index + 1}";
			this.mixer = mixer;

			clips.ResizeMin(1);
		}

		#endregion

	}

	#endregion

	[CreateAssetMenu(fileName = name, menuName = CoreUtilities.scriptableRoot + name)]
	public class AudioLibrary : ScriptableObject
	{
		private new const string name = "Audio Library";

		[SerializeField]
		private AudioMixerGroup mixer;
		[SerializeField]
		private List<AudioTrack> tracks = new List<AudioTrack>();

		public int Count => tracks.Count;

		#region Tracks

		public AudioTrack GetTrack(int index) => tracks[index];

		public List<string> GetTrackNames() => tracks.ConvertAll(track => track.Name);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Inspector

		private void OnValidate()
		{
			tracks.ForEach((track, index) => track.EditorUpdate(index, mixer));
		}

		#endregion

	}
}