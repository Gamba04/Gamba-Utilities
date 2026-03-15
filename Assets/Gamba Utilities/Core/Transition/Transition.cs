using System;
using UnityEngine;
using static UnityEngine.Time;

namespace GambaUtilities
{
	using Internal;

	#region TransitionBase

	namespace Internal
	{
		public abstract class TransitionBase
		{
			[Range(0, 5)]
			public float duration = 1;
			public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

			public abstract float Time { get; }

			public abstract bool IsInTransition { get; }

			/// <summary> Silently stops the transition at the current value. </summary>
			public abstract void Cancel();

			/// <summary> Stops the transition at the current value and triggers the callback. </summary>
			/// <param name="pendingUpdate"> Leave a pending update for syncing to the latest value. </param>
			public abstract void Stop(bool pendingUpdate);

			/// <summary> Stops the transition at the target value and triggers the callback. </summary>
			/// <param name="pendingUpdate"> Leave a pending update for syncing to the latest value. </param>
			public abstract void Complete(bool pendingUpdate);
		}
	}

	#endregion

	// ----------------------------------------------------------------------------------------------------

	#region Transition

	[Serializable]
	public class Transition : Transition<float> { }

	[Serializable]
	public class Transition<T> : TransitionBase
		where T : struct
	{
		private T value;

		private T startValue;
		private T targetValue;

		private bool isUnscaled;
		private bool isReversed;

		private float time;
		private bool isInTransition;

		private Action onTransitionEnd;

		private float DeltaTime => isUnscaled ? unscaledDeltaTime : deltaTime;

		private float Progress => isReversed ? time : 1 - time;

		public override float Time => time;

		public override bool IsInTransition => isInTransition;

		public T Value { get => value; set => this.value = value; }

		#region Start

		public void Start(T target, Action onTransitionEnd = null) => Start(target, default, default, onTransitionEnd);

		public void Start(T target, bool isUnscaled, Action onTransitionEnd = null) => Start(target, isUnscaled, default, onTransitionEnd);

		public void Start(T target, bool isUnscaled, bool isReversed, Action onTransitionEnd = null)
		{
			this.isUnscaled = isUnscaled;
			this.isReversed = isReversed;
			this.onTransitionEnd = onTransitionEnd;

			startValue = value;
			targetValue = target;
			isInTransition = true;

			if (duration > 0 && !value.Equals(target))
			{
				time = 1;
			}
			else Complete();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Update

		public void Update(Action<T> onTransitionUpdate)
		{
			if (Update(out T value))
			{
				onTransitionUpdate?.Invoke(value);
			}
		}

		public bool Update(out T value)
		{
			bool updateValue = isInTransition;

			if (time > 0)
			{
				if (duration > 0)
				{
					time -= DeltaTime / duration;

					if (time <= 0) Stop(false);

					float interpolator = curve.Evaluate(Progress);

					updateValue = Lerp(interpolator);
				}
				else Complete(false);
			}
			else isInTransition = false;

			value = this.value;
			return updateValue;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Interpolation

		private delegate L LerpFunction<L>(L a, L b, float t);

		private bool Lerp(float interpolator)
		{
			bool success = false;

			TryLerp<float>(Mathf.LerpUnclamped);
			TryLerp<Vector2>(Vector2.LerpUnclamped);
			TryLerp<Vector3>(Vector3.LerpUnclamped);
			TryLerp<Vector4>(Vector4.LerpUnclamped);
			TryLerp<Quaternion>(Quaternion.SlerpUnclamped);
			TryLerp<Color>(Color.LerpUnclamped);
			TryLerpTransitionable();

			return success;

			void TryLerp<L>(LerpFunction<L> lerp)
			{
				if (!success && typeof(L).IsAssignableFrom(typeof(T)))
				{
					L start = (L)(object)startValue;
					L target = (L)(object)targetValue;

					value = (T)(object)lerp(start, target, interpolator);

					success = true;
				}
			}

			void TryLerpTransitionable()
			{
				if (!success && default(T) is ITransitionable<T> transitionable)
				{
					value = transitionable.Lerp(startValue, targetValue, interpolator);

					success = true;
				}
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Termination

		public override void Cancel()
		{
			time = default;
			isInTransition = default;
			onTransitionEnd = default;

			startValue = value;
			targetValue = value;
		}

		public override void Stop(bool pendingUpdate = true)
		{
			onTransitionEnd?.Invoke();

			Cancel();

			if (pendingUpdate) isInTransition = true;
		}

		public override void Complete(bool pendingUpdate = true)
		{
			value = targetValue;

			Stop(pendingUpdate);
		}

		#endregion

	}

	#endregion

}