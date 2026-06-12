using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GambaUtilities
{
	public class AnimationEventsBridge : MonoBehaviour
	{
		[SerializeField]
		private List<UnityEvent> events = new List<UnityEvent>();

		public void TriggerEvent(int index)
		{
			if (index >= 0 && index < events.Count) events[index]?.Invoke();
			else Debug.LogError($"Event index {index} is out of bounds");
		}
	}
}