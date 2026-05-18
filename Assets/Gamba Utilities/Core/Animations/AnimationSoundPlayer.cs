using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities
{
	public class AnimationSoundPlayer : MonoBehaviour
	{
		[SerializeField]
		[CustomName("Sound {i}")]
		private List<SimpleSound> sounds;

		public void PlaySound(int index)
		{
			sounds[index]?.Play();
		}
	}
}