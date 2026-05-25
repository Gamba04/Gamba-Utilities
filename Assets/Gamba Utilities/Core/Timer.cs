using System;
using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities
{
	public class Timer : SingletonBehaviour<Timer>
	{

		#region Request

		[Serializable]
		private class Request
		{
			[SerializeField, HideInInspector]
			private string name;
			[SerializeField]
			private float time;
			[SerializeField]
			private bool unscaled;

			private readonly Action action;
			private readonly Action<float> onUpdate;
			private readonly Action onExpire;

			public Request(Action action, float time, bool unscaled, CancelRequest cancel, Action<float> onUpdate, Action<Request> onExpire, string name)
			{
				this.action = action;
				this.time = time;
				this.unscaled = unscaled;
				this.onUpdate = onUpdate;
				this.onExpire = Expire;
				this.name = name ?? "Request";

				cancel?.Register(Cancel);

				void Expire()
				{
					cancel?.Expire();
					onExpire(this);
				}
			}

			public void Update()
			{
				if (Decrease(ref time, unscaled)) Complete();

				onUpdate?.Invoke(time);
			}

			private void Cancel()
			{
				onExpire?.Invoke();
			}

			private void Complete()
			{
				action?.Invoke();
				onExpire?.Invoke();
			}
		}

		public class CancelRequest
		{
			private Action onCancel;

			public void Register(Action cancel)
			{
				onCancel += cancel;
			}

			public void Cancel()
			{
				onCancel?.Invoke();
			}

			public void Expire()
			{
				onCancel = null;
			}
		}

		#endregion

		[SerializeField]
		private List<Request> requests = new List<Request>();

		#region Delay

		/// <summary> Delay the execution of an <paramref name="action"/>. </summary>
		public static void Delay(Action action, float delay, string name = null) => Delay(action, delay, false, null, null, name);

		/// <summary> Delay the execution of an <paramref name="action"/>. </summary>
		public static void Delay(Action action, float delay, bool unscaled, string name = null) => Delay(action, delay, unscaled, null, null, name);

		/// <summary> Delay the execution of an <paramref name="action"/> with a way to <paramref name="cancel"/> the request. </summary>
		public static void Delay(Action action, float delay, bool unscaled, CancelRequest cancel, string name = null) => Delay(action, delay, unscaled, cancel, null, name);

		/// <summary> Delay the execution of an <paramref name="action"/> with a way to <paramref name="cancel"/> the request. </summary>
		/// <param name="onUpdate"> Receive updates of the remaining time of the request. </param>
		public static void Delay(Action action, float delay, bool unscaled, CancelRequest cancel, Action<float> onUpdate, string name = null)
		{
			if (delay > 0) Instance.CreateRequest(action, delay, unscaled, cancel, onUpdate, name);
			else action?.Invoke();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Decrease

		/// <summary> Decreases a positive <paramref name="value"/> over time and triggers an <paramref name="action"/> when it reaches zero. </summary>
		public static void Decrease(ref float value, Action action = null) => Decrease(ref value, false, action);

		/// <summary> Decreases a positive <paramref name="value"/> over time and triggers an <paramref name="action"/> when it reaches zero. </summary>
		public static void Decrease(ref float value, bool unscaled, Action action = null)
		{
			if (Decrease(ref value, unscaled)) action?.Invoke();
		}

		/// <summary> Decreases a positive <paramref name="value"/> over time and returns <see langword="true"/> when it reaches zero. </summary>
		public static bool Decrease(ref float value) => Decrease(ref value, false);

		/// <summary> Decreases a positive <paramref name="value"/> over time and returns <see langword="true"/> when it reaches zero. </summary>
		public static bool Decrease(ref float value, bool unscaled)
		{
			if (value > 0)
			{
				value -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

				if (value <= 0)
				{
					value = 0;

					return true;
				}
			}
			else value = 0;

			return false;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Requests

		private void CreateRequest(Action action, float delay, bool unscaled, CancelRequest cancel, Action<float> onUpdate, string name)
		{
			requests.Add(new Request(action, delay, unscaled, cancel, onUpdate, Expire, name));

			void Expire(Request request) => requests.Remove(request);
		}

		private void Update()
		{
			for (int i = requests.Count - 1; i > -1; i--)
			{
				requests[i].Update();
			}
		}

		#endregion

	}
}