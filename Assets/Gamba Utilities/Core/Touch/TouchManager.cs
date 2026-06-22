using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities
{
	using Internal;

	#region Game Touch

	public enum TouchState
	{
		HoverEnter,
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

		public bool IsHover => state < TouchState.Press;

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

		public static implicit operator sbyte(GameTouch touch) => (sbyte)touch.id;

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
		public bool InitiallyOverlapped(Predicate<TouchReceiver> match) => activity.initial.Any(match.Invoke);

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
		public bool IsOverlapping(Predicate<TouchReceiver> match) => activity.current.Any(match.Invoke);

		#endregion

		#endregion

	}

	#endregion

	namespace Internal
	{

		#region Touch Activity

		public readonly struct TouchActivity
		{
			public readonly HashSet<TouchReceiver> current;
			public readonly HashSet<TouchReceiver> history;
			public readonly HashSet<TouchReceiver> hovered;
			public readonly HashSet<TouchReceiver> initial;

			public TouchActivity(int capacity)
			{
				TouchReceiver[] collection = new TouchReceiver[capacity];

				current = new HashSet<TouchReceiver>(collection);
				history = new HashSet<TouchReceiver>(collection);
				hovered = new HashSet<TouchReceiver>(collection);
				initial = new HashSet<TouchReceiver>(collection);

				Clear();
			}

			public void Clear()
			{
				current.Clear();
				history.Clear();
				hovered.Clear();
				initial.Clear();
			}

			public void ClearFrame(TouchState state)
			{
				current.Clear();

				if (state == TouchState.Press) hovered.Clear();
			}

			public void Record(TouchReceiver receiver, TouchState state)
			{
				current.Add(receiver);
				history.Add(receiver);

				if (state == TouchState.Press) initial.Add(receiver);
			}

			public void Remove(TouchReceiver receiver)
			{
				if (history.Contains(receiver))
				{
					history.Remove(receiver);
					hovered.Remove(receiver);
					initial.Remove(receiver);
				}
			}
		}

		#endregion

		[DisallowMultipleComponent]
		[HideMonoScript]
		public class TouchManager : SingletonBehaviour<TouchManager>
		{
			[Header("Settings")]
			[SerializeField]
			[Range(1, 10)]
			private int maxTouches = 10;

			[Header("Info")]
			[ReadOnly, SerializeField]
			private List<GameTouch> touches = new List<GameTouch>();

			private readonly List<TouchReceiver> receivers = new List<TouchReceiver>();
			private readonly List<TouchReceiver> historyBuffer = new List<TouchReceiver>();

			private Dictionary<int, TouchActivity> activities;

			private Vector2 lastMousePosition;

			public static int MaxTouches => Input.touchSupported ? Instance.maxTouches : Input.mousePresent ? 1 : 0;

			#region Init

			protected override void Init()
			{
				int maxTouches = MaxTouches;
				int maxReceivers = FindObjectsOfType<TouchReceiver>(true).Length;

				touches.Capacity = maxTouches;
				receivers.Capacity = maxReceivers;
				historyBuffer.Capacity = maxReceivers;

				InitActivities(maxTouches, maxReceivers);
			}

			private void InitActivities(int maxTouches, int maxReceivers)
			{
				activities = new Dictionary<int, TouchActivity>(maxTouches);

				if (Input.touchSupported)
				{
					for (int id = 0; id < maxTouches; id++)
					{
						Add(id);
					}
				}
				else if (Input.mousePresent) Add(-1);

				void Add(int id) => activities.Add(id, new TouchActivity(maxReceivers));
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

			private static List<TouchReceiver> GetReceivers(bool guaranteed) => guaranteed ? Instance.receivers : instance.ExistingObject()?.receivers;

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

					activity.ClearFrame(touch.state);

					foreach (TouchReceiver receiver in receivers)
					{
						receiver.PreValidate(touch);

						if (receiver.Validate(touch))
						{
							if (receiver.Overlap(touch.screenPosition))
							{
								activity.Record(receiver, touch.state);
							}
						}
						else activity.Remove(receiver);
					}
				}
			}

			private void ProcessInteractions()
			{
				foreach (GameTouch touch in touches)
				{
					TouchActivity activity = activities[touch];

					HashSet<TouchReceiver> history = activity.history;
					HashSet<TouchReceiver> hovered = activity.hovered;

					historyBuffer.Replace(history);

					foreach (TouchReceiver receiver in historyBuffer)
					{
						SendInteraction(touch, receiver, history, hovered);
					}
				}
			}

			private static void SendInteraction(GameTouch touch, TouchReceiver receiver, HashSet<TouchReceiver> history, HashSet<TouchReceiver> hovered)
			{
				bool isInitial = touch.InitiallyOverlapped(receiver);
				bool isCurrent = touch.IsOverlapping(receiver);

				if (touch.state <= TouchState.Press) ProcessHoverInteraction(isCurrent, receiver, history, hovered, ref touch.state);

				receiver.PreReceiveTouch(touch);
				receiver.ReceiveTouch(touch, isInitial, isCurrent);
			}

			private static void ProcessHoverInteraction(bool isCurrent, TouchReceiver receiver, HashSet<TouchReceiver> history, HashSet<TouchReceiver> hovered, ref TouchState state)
			{
				if (isCurrent)
				{
					if (state == TouchState.Hover && !hovered.Contains(receiver))
					{
						hovered.Add(receiver);

						state = TouchState.HoverEnter;
					}
				}
				else
				{
					history.Remove(receiver);
					hovered.Remove(receiver);

					state = TouchState.HoverExit;
				}
			}

			#endregion

		}
	}
}