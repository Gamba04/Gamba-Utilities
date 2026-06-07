using System;
using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities
{
	using Internal;

	#region Game Touch

	public enum TouchState
	{
		Hover,
		Start,
		Hold,
		Release,
		Cancel
	}

	[Serializable]
	public struct GameTouch
	{
		[SerializeField, HideInInspector]
		private string name;

		public int id;
		public TouchState state;
		public Vector2 screenPosition;
		public Vector2 deltaPosition;

		private readonly TouchActivity activity;

		public bool IsMouse => id == -1;

		public bool IsFinished => state > TouchState.Hold;

		#region Values

		public GameTouch(int id, TouchState state, Vector2 screenPosition, TouchActivity activity) : this()
		{
			this.id = id;
			this.state = state;
			this.screenPosition = screenPosition;
			this.activity = activity;

			name = IsMouse ? "Mouse" : $"Touch {id}";
		}

		public void Update(TouchState state, Vector2 screenPosition, Vector2 deltaPosition)
		{
			this.state = state;
			this.screenPosition = screenPosition;
			this.deltaPosition = deltaPosition;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Activity

		#region Initial

		/// <summary> Retrieves a copy of the receivers overlapped at the <see cref="TouchState.Start"/> of the touch. </summary>
		public TouchReceiver[] GetInitialOverlaps() => activity.initial.ToArray();

		/// <summary> Fills <paramref name="receivers"/> with the receivers overlapped at the <see cref="TouchState.Start"/> of the touch. </summary>
		public void GetInitialOverlaps(List<TouchReceiver> receivers) => receivers.AddRange(activity.initial);

		/// <summary> Checks whether there were any overlaps at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped() => activity.initial.Count > 0;

		/// <summary> Checks if <paramref name="receiver"/> was overlapped with at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped(TouchReceiver receiver) => activity.initial.Contains(receiver);

		/// <summary> Checks whether there were any overlaps with <paramref name="layer"/> at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped(int layer) => InitiallyOverlapped(receiver => receiver.gameObject.layer == layer);

		/// <summary> Checks whether there were any overlaps with a layer contained by <paramref name="mask"/> at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped(LayerMask mask) => InitiallyOverlapped(receiver => mask.Contains(receiver.gameObject.layer));

		/// <summary> Checks whether there were any overlaps with <typeparamref name="R"/> at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped<R>() where R : TouchReceiver => InitiallyOverlapped(receiver => receiver is R);

		/// <summary> Checks whether there were any overlaps that <paramref name="match"/> the specific condition at the <see cref="TouchState.Start"/> of the touch. </summary>
		public bool InitiallyOverlapped(Predicate<TouchReceiver> match) => activity.initial.Exists(match);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Current

		/// <summary> Retrieves a copy of the currently overlapping receivers. </summary>
		public TouchReceiver[] GetOverlaps() => activity.current.ToArray();

		/// <summary> Fills <paramref name="receivers"/> with the currently overlapping receivers. </summary>
		public void GetOverlaps(List<TouchReceiver> receivers) => receivers.AddRange(activity.current);

		/// <summary> Checks if <paramref name="receiver"/> is currently being overlapped. </summary>
		public bool IsOverlapping(TouchReceiver receiver) => activity.current.Contains(receiver);

		/// <summary> Checks whether there are currently any overlaps with <paramref name="layer"/>. </summary>
		public bool IsOverlapping(int layer) => IsOverlapping(receiver => receiver.gameObject.layer == layer);

		/// <summary> Checks whether there are currently any overlaps with a layer contained by <paramref name="mask"/>. </summary>
		public bool IsOverlapping(LayerMask mask) => IsOverlapping(receiver => mask.Contains(receiver.gameObject.layer));

		/// <summary> Checks whether there are currently any overlaps with <typeparamref name="R"/>. </summary>
		public bool IsOverlapping<R>() where R : TouchReceiver => IsOverlapping(receiver => receiver is R);

		/// <summary> Checks whether there are currently any overlaps that <paramref name="match"/> the specific condition. </summary>
		public bool IsOverlapping(Predicate<TouchReceiver> match) => activity.current.Exists(match);

		#endregion

		#endregion

	}

	#endregion

	namespace Internal
	{

		#region Touch Activity

		public struct TouchActivity
		{
			public readonly List<TouchReceiver> initial;
			public readonly List<TouchReceiver> current;

			public TouchActivity(int capacity)
			{
				initial = new List<TouchReceiver>(capacity);
				current = new List<TouchReceiver>(capacity);
			}

			public void Record(TouchReceiver receiver, bool isInitial)
			{
				if (isInitial) initial.Add(receiver);

				current.Add(receiver);
			}
		}

		#endregion

		public class TouchManager : SingletonBehaviour<TouchManager>
		{
			[Header("Settings")]
			[SerializeField]
			[Range(1, 10)]
			private int maxTouches = 10;

			[Header("Info")]
			[ReadOnly, SerializeField]
			private List<GameTouch> touches = new List<GameTouch>();

			private Vector2 lastMousePosition;

			private readonly List<TouchReceiver> receivers = new List<TouchReceiver>();
			private readonly Dictionary<int, TouchActivity> activities = new Dictionary<int, TouchActivity>();

			#region Init

			protected override void Init()
			{
				touches.Capacity = maxTouches;
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Update

			private void Update()
			{
				UpdateTouches();
				UpdateReceivers();
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Touches

			private void UpdateTouches()
			{
				CleanTouches();
				ProcessMouse();
				ProcessTouches();
			}

			private void CleanTouches()
			{
				for (int i = touches.Count - 1; i > -1; i--)
				{
					GameTouch touch = touches[i];

					if (touch.IsFinished)
					{
						activities[touch.id].initial.Clear();

						touches.RemoveAt(i);
					}
				}
			}

			private void ProcessMouse()
			{
				if (!Input.mousePresent) return;

				TouchState state = GetState();

				Vector2 screenPosition = Input.mousePosition;
				Vector2 deltaPosition = GetDeltaPosition();

				SetTouch(-1, state, screenPosition, deltaPosition);

				Vector2 GetDeltaPosition()
				{
					Vector2 delta = screenPosition - lastMousePosition;
					lastMousePosition = screenPosition;

					return delta;
				}

				TouchState GetState()
				{
					if (Input.GetMouseButtonDown(0)) return TouchState.Start;
					if (Input.GetMouseButton(0)) return TouchState.Hold;
					if (Input.GetMouseButtonUp(0)) return TouchState.Release;

					return TouchState.Hover;
				}
			}

			private void ProcessTouches()
			{
				if (!Input.touchSupported) return;

				int touchCount = Mathf.Min(Input.touchCount, maxTouches);

				for (int i = 0; i < touchCount; i++)
				{
					ProcessTouch(Input.GetTouch(i));
				}
			}

			private void ProcessTouch(Touch touch)
			{
				SetTouch(touch.fingerId, GetState(), touch.position, touch.deltaPosition);

				TouchState GetState()
				{
					return touch.phase switch
					{
						TouchPhase.Began => TouchState.Start,
						TouchPhase.Moved => TouchState.Hold,
						TouchPhase.Stationary => TouchState.Hold,
						TouchPhase.Ended => TouchState.Release,
						TouchPhase.Canceled => TouchState.Cancel,
						_ => throw new InvalidCastException()
					};
				}
			}

			private void SetTouch(int id, TouchState state, Vector2 screenPosition, Vector2 deltaPosition)
			{
				if (TryGetTouch(id, out GameTouch touch, out int index))
				{
					UpdateTouch(touch, index, state, screenPosition, deltaPosition);
				}
				else CreateTouch(id, state, screenPosition);
			}

			private bool TryGetTouch(int id, out GameTouch touch, out int index)
			{
				for (index = 0; index < touches.Count; index++)
				{
					touch = touches[index];

					if (touch.id == id) return true;
				}

				touch = default;
				return false;
			}

			private void UpdateTouch(GameTouch touch, int index, TouchState state, Vector2 screenPosition, Vector2 deltaPosition)
			{
				touch.Update(state, screenPosition, deltaPosition);

				touches[index] = touch;
			}

			private void CreateTouch(int id, TouchState state, Vector2 screenPosition)
			{
				TouchActivity activity = GetActivity(id);
				GameTouch touch = new GameTouch(id, state, screenPosition, activity);

				touches.Add(touch);
			}

			private TouchActivity GetActivity(int id)
			{
				bool exists = activities.ContainsKey(id);
				TouchActivity activity = exists ? activities[id] : new TouchActivity(receivers.Count);

				if (!exists) activities.Add(id, activity);

				return activity;
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Receivers

			public static void Register(TouchReceiver receiver) => Instance.receivers.Add(receiver);

			public static void Unregister(TouchReceiver receiver) => Instance.receivers.Remove(receiver);

			private void UpdateReceivers()
			{
				UpdateInteractions();
				ProcessInteractions();
			}

			private void UpdateInteractions()
			{
				foreach (GameTouch touch in touches)
				{
					TouchActivity activity = activities[touch.id];

					activity.current.Clear();

					foreach (TouchReceiver receiver in receivers)
					{
						if (receiver.Overlap(touch.screenPosition))
						{
							bool isInitial = touch.state == TouchState.Start;

							activity.Record(receiver, isInitial);
						}
					}
				}
			}

			private void ProcessInteractions()
			{
				foreach (GameTouch touch in touches)
				{
					foreach (TouchReceiver receiver in activities[touch.id].current)
					{
						receiver.OnTouch(touch);
					}
				}
			}

			#endregion

		}
	}
}