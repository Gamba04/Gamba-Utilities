using System;
using UnityEngine;
using static UnityEngine.Time;

namespace GambaUtilities
{
    [Serializable]
    public class Transition : Transition<float> { }

    [Serializable]
    public class Transition<T>
        where T : struct
    {
        [Range(0, 5)]
        public float duration = 1;
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [HideInInspector]
        public T value;
        [HideInInspector]
        public bool isUnscaled;
        [HideInInspector]
        public bool isReversed;

        private T startValue;
        private T targetValue;

        private float time;
        private bool isInTransition;

        public event Action onTransitionEnd;

        private float DeltaTime => isUnscaled ? unscaledDeltaTime : deltaTime;

        private float Progress => isReversed ? time : 1 - time;

        public float Time => time;

        public bool IsInTransition => isInTransition;

        #region Start

        public void Start(T target, Action onTransitionEnd = null) => Start(target, isUnscaled, isReversed, onTransitionEnd);

        public void Start(T target, bool isUnscaled, Action onTransitionEnd = null) => Start(target, isUnscaled, isReversed, onTransitionEnd);

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

        public bool Update(out T value)
        {
            bool updateValue = isInTransition;

            if (time > 0)
            {
                if (duration > 0)
                {
                    time -= DeltaTime / duration;

                    if (time <= 0) End();

                    float interpolator = curve.Evaluate(Progress);

                    updateValue = Lerp(interpolator);
                }
                else Complete();
            }

            value = this.value;
            return updateValue;
        }

        public void Update(Action<T> onTransitionUpdate)
        {
            if (Update(out T value))
            {
                onTransitionUpdate?.Invoke(value);
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Interpolation

        private delegate S LerpFunction<S>(S a, S b, float t);

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

            void TryLerp<S>(LerpFunction<S> lerp)
            {
                if (!success && typeof(S).IsAssignableFrom(typeof(T)))
                {
                    S start = (S)(object)startValue;
                    S target = (S)(object)targetValue;

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

        /// <summary> Ends the transition at the current value. </summary>
        public void Cancel()
        {
            time = default;
            isInTransition = default;
            onTransitionEnd = default;

            startValue = value;
            targetValue = value;
        }

        /// <summary> Ends the transition and triggers the callback. </summary>
        public void End()
        {
            onTransitionEnd?.Invoke();

            Cancel();
        }

        /// <summary> Completes the transition in the target value and triggers the callback. </summary>
        public void Complete()
        {
            value = targetValue;

            End();

            isInTransition = true;
        }

        #endregion

    }
}