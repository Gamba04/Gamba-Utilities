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
		HoverExit,
		Press,
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

		public static implicit operator int(GameTouch touch) => touch.id;

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Activity

		#region Initial

		/// <summary> Retrieves a copy of the receivers overlapped at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public TouchReceiver[] GetInitialOverlaps() => activity.initial.ToArray();

		/// <summary> Fills <paramref name="receivers"/> with the receivers overlapped at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public void GetInitialOverlaps(List<TouchReceiver> receivers) => receivers.AddRange(activity.initial);

		/// <summary> Checks whether there were any overlaps at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped() => activity.initial.Count > 0;

		/// <summary> Checks if <paramref name="receiver"/> was overlapped with at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped(TouchReceiver receiver) => activity.initial.Contains(receiver);

		/// <summary> Checks whether there were any overlaps with <paramref name="layer"/> at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped(int layer) => InitiallyOverlapped(receiver => receiver.gameObject.layer == layer);

		/// <summary> Checks whether there were any overlaps with a layer contained by <paramref name="mask"/> at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped(LayerMask mask) => InitiallyOverlapped(receiver => mask.Contains(receiver.gameObject.layer));

		/// <summary> Checks whether there were any overlaps with <typeparamref name="R"/> at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped<R>() where R : TouchReceiver => InitiallyOverlapped(receiver => receiver is R);

		/// <summary> Checks whether there were any overlaps that <paramref name="match"/> the specific condition at the <see cref="TouchState.Press"/> state of this touch. </summary>
		public bool InitiallyOverlapped(Predicate<TouchReceiver> match) => activity.initial.Exists(match);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Current

		/// <summary> Retrieves a copy of the currently overlapping receivers. </summary>
		public TouchReceiver[] GetOverlaps() => activity.current.ToArray();

		/// <summary> Fills <paramref name="receivers"/> with the currently overlapping receivers. </summary>
		public void GetOverlaps(List<TouchReceiver> receivers) => receivers.AddRange(activity.current);

		/// <summary> Checks whether there are currently any overlaps. </summary>
		public bool IsOverlapping() => activity.current.Count > 0;

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
			public readonly List<TouchReceiver> history;

			public TouchActivity(int capacity)
			{
				initial = new List<TouchReceiver>(capacity);
				current = new List<TouchReceiver>(capacity);
				history = new List<TouchReceiver>(capacity);
			}

			public void Clear()
			{
				initial.Clear();
				current.Clear();
				history.Clear();
			}

			public void ClearCurrent() => current.Clear();

			public void Record(TouchReceiver receiver, TouchState state)
			{
				if (state == TouchState.Press) initial.Add(receiver);

				current.Add(receiver);
				
				if (!history.Contains(receiver)) history.Add(receiver);
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
				touches.Capacity = Input.touchSupported ? maxTouches : 1;

				InitActivity();
			}

			private void InitActivity()
			{
				int capacity = FindObjectsOfType<TouchReceiver>(true).Length;

				if (Input.touchSupported)
				{
					for (int id = 0; id < maxTouches; id++)
					{
						Add(id);
					}
				}
				else if (Input.mousePresent) Add(-1);

				void Add(int id) => activities.Add(id, new TouchActivity(capacity));
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region LateUpdate

			private void LateUpdate()
			{
				UpdateTouches();
				UpdateInteractions();
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
						activities[touch].Clear();
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
					if (Input.GetMouseButtonDown(0)) return TouchState.Press;
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
						TouchPhase.Began => TouchState.Press,
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
				GameTouch touch = new GameTouch(id, state, screenPosition, activities[id]);

				touches.Add(touch);
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Registration

			public static void Register(TouchReceiver receiver) => GetReceivers(true).Add(receiver);

			public static void Unregister(TouchReceiver receiver) => GetReceivers(false)?.Remove(receiver);

			private static List<TouchReceiver> GetReceivers(bool guaranteed) => guaranteed ? Instance.receivers : instance?.ExistingObject()?.receivers;

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Interactions

			private void UpdateInteractions()
			{
				RecordInteractions();
				ProcessInteractions();
			}

			private void RecordInteractions()
			{
				foreach (GameTouch touch in touches)
				{
					TouchActivity activity = activities[touch];

					activity.ClearCurrent();

					foreach (TouchReceiver receiver in receivers)
					{
						if (receiver.Validate(touch) && receiver.Overlap(touch.screenPosition))
						{
							activity.Record(receiver, touch.state);
						}
					}
				}
			}

			private void ProcessInteractions()
			{
				foreach (GameTouch touch in touches)
				{
					List<TouchReceiver> history = activities[touch].history;

					for (int i = history.Count - 1; i > -1; i--)
					{
						SendInteraction(touch, history[i], history);
					}
				}
			}

			private void SendInteraction(GameTouch touch, TouchReceiver receiver, List<TouchReceiver> history)
			{
				bool isInitial = touch.InitiallyOverlapped(receiver);
				bool isCurrent = touch.IsOverlapping(receiver);

				if (touch.state == TouchState.Hover && !isCurrent)
				{
					touch.state = TouchState.HoverExit;

					history.Remove(receiver);
				}

				receiver.ReceiveTouch(touch, isInitial, isCurrent);
			}

			#endregion

		}
	}
}