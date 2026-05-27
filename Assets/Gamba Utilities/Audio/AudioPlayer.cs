using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities.Audio
{
	public class AudioPlayer : SingletonBehaviour<AudioPlayer>
	{
		[SerializeField]
		[Range(1, 128)]
		private int poolSize = 32;

		private const string defaultName = "Source";

		private readonly List<AudioSource> sources = new List<AudioSource>();
		private readonly List<AudioSource> activeSources = new List<AudioSource>();

		#region Init

		protected override void Init()
		{
			InitPool();
		}

		private void InitPool()
		{
			sources.Capacity = poolSize;
			activeSources.Capacity = poolSize;

			for (int i = 0; i < poolSize; i++)
			{
				GameObject obj = new GameObject(defaultName);
				obj.transform.parent = transform;
				obj.SetActive(false);

				AudioSource source = obj.AddComponent<AudioSource>();
				source.Reset(false);

				sources.Add(source);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Play

		public static void Play(AudioTrack track, float volume = 1)
		{
			Play(track, default, volume);
		}

		public static void Play(AudioTrack track, Vector3 position, float volume = 1)
		{
			AudioSource source = Instance.GetSource();

			track.Setup(source);

			source.name = track.Name;
			source.transform.position = position;
			source.volume *= volume;

			source.Play();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Sources

		private AudioSource GetSource()
		{
			AudioSource source = sources[0];

			SetSource(source, true);

			return source;
		}

		private void SetSource(AudioSource source, bool active)
		{
			source.gameObject.SetActive(active);

			if (active)
			{
				source.transform.SetAsFirstSibling();
				sources.MoveToEnd(source);
				activeSources.Add(source);
			}
			else
			{
				source.name = defaultName;
				source.transform.position = default;
				source.transform.SetAsLastSibling();
				source.Reset(false);
				activeSources.Remove(source);
			}
		}

		private void Update()
		{
			for (int i = activeSources.Count - 1; i > -1; i--)
			{
				AudioSource source = activeSources[i];

				if (!source.isPlaying)
				{
					SetSource(source, false);
				}
			}
		}

		#endregion

	}
}