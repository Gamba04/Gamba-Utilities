using System;
using System.Collections.Generic;

namespace GambaUtilities
{
    using Internal;

    #region MultiTransition

    public abstract class MultiTransition : TransitionBase
    {
        private TransitionBase referenceTransition;

        public override float Time => referenceTransition != null ? referenceTransition.Time : default;

        public override bool IsInTransition => referenceTransition != null && referenceTransition.IsInTransition;

        protected abstract List<TransitionBase> Transitions { get; }

        #region Start

        protected void StartTransition<T>(Transition<T> transition, T target, bool isUnscaled, bool isReversed, ref Action onTransitionEnd)
            where T : struct
        {
            if (duration > 0)
            {
                if (!transition.value.Equals(target))
                {
                    referenceTransition = transition;

                    transition.Start(target, isUnscaled, isReversed, onTransitionEnd);
                    onTransitionEnd = null;
                }
            }
            else
            {
                transition.value = target;

                onTransitionEnd?.Invoke();
                onTransitionEnd = null;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Update

        protected void UpdateTransition<T>(Transition<T> transition, ref bool updateValue, out T value)
            where T : struct
        {
            transition.duration = duration;
            transition.curve = curve;

            updateValue |= transition.Update(out value);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Termination

        public override void Cancel() => Transitions.ForEach(transition => transition.Cancel());

        public override void Stop(bool pendingUpdate = true) => Transitions.ForEach(transition => transition.Stop(pendingUpdate));

        public override void Complete(bool pendingUpdate = true) => Transitions.ForEach(transition => transition.Complete(pendingUpdate));

        #endregion

    }

    #endregion

    // ----------------------------------------------------------------------------------------------------

    #region Transition<A, B>

    [Serializable]
    public class Transition<A, B> : MultiTransition
        where A : struct
        where B : struct
    {
        private readonly Transition<A> transitionA = new Transition<A>();
        private readonly Transition<B> transitionB = new Transition<B>();

        public A ValueA { get => transitionA.value; set => transitionA.value = value; }

        public B ValueB { get => transitionB.value; set => transitionB.value = value; }

        protected override List<TransitionBase> Transitions => new List<TransitionBase>()
        {
            transitionA,
            transitionB
        };

        #region Start

        public void Start(A targetA, B targetB, Action onTransitionEnd = null) => Start(targetA, targetB, default, default, onTransitionEnd);

        public void Start(A targetA, B targetB, bool isUnscaled, Action onTransitionEnd = null) => Start(targetA, targetB, isUnscaled, default, onTransitionEnd);

        public void Start(A targetA, B targetB, bool isUnscaled, bool isReversed, Action onTransitionEnd = null)
        {
            StartTransition(transitionA, targetA, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionB, targetB, isUnscaled, isReversed, ref onTransitionEnd);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Update

        public void Update(Action<A, B> onTransitionUpdate)
        {
            if (Update(out A valueA, out B valueB))
            {
                onTransitionUpdate?.Invoke(valueA, valueB);
            }
        }

        public bool Update(out A valueA, out B valueB)
        {
            bool updateValue = false;

            UpdateTransition(transitionA, ref updateValue, out valueA);
            UpdateTransition(transitionB, ref updateValue, out valueB);

            return updateValue;
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Public

        public void SetValues(A valueA, B valueB)
        {
            ValueA = valueA;
            ValueB = valueB;
        }

        #endregion

    }

    #endregion

    // ----------------------------------------------------------------------------------------------------

    #region Transition<A, B, C>

    [Serializable]
    public class Transition<A, B, C> : MultiTransition
        where A : struct
        where B : struct
        where C : struct
    {
        private readonly Transition<A> transitionA = new Transition<A>();
        private readonly Transition<B> transitionB = new Transition<B>();
        private readonly Transition<C> transitionC = new Transition<C>();

        public A ValueA { get => transitionA.value; set => transitionA.value = value; }

        public B ValueB { get => transitionB.value; set => transitionB.value = value; }

        public C ValueC { get => transitionC.value; set => transitionC.value = value; }

        protected override List<TransitionBase> Transitions => new List<TransitionBase>()
        {
            transitionA,
            transitionB,
            transitionC
        };

        #region Start

        public void Start(A targetA, B targetB, C targetC, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, default, default, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, bool isUnscaled, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, isUnscaled, default, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, bool isUnscaled, bool isReversed, Action onTransitionEnd = null)
        {
            StartTransition(transitionA, targetA, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionB, targetB, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionC, targetC, isUnscaled, isReversed, ref onTransitionEnd);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Update

        public void Update(Action<A, B, C> onTransitionUpdate)
        {
            if (Update(out A valueA, out B valueB, out C valueC))
            {
                onTransitionUpdate?.Invoke(valueA, valueB, valueC);
            }
        }

        public bool Update(out A valueA, out B valueB, out C valueC)
        {
            bool updateValue = false;

            UpdateTransition(transitionA, ref updateValue, out valueA);
            UpdateTransition(transitionB, ref updateValue, out valueB);
            UpdateTransition(transitionC, ref updateValue, out valueC);

            return updateValue;
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Public

        public void SetValues(A valueA, B valueB, C valueC)
        {
            ValueA = valueA;
            ValueB = valueB;
            ValueC = valueC;
        }

        #endregion

    }

    #endregion

    // ----------------------------------------------------------------------------------------------------

    #region Transition<A, B, C, D>

    [Serializable]
    public class Transition<A, B, C, D> : MultiTransition
        where A : struct
        where B : struct
        where C : struct
        where D : struct
    {
        private readonly Transition<A> transitionA = new Transition<A>();
        private readonly Transition<B> transitionB = new Transition<B>();
        private readonly Transition<C> transitionC = new Transition<C>();
        private readonly Transition<D> transitionD = new Transition<D>();

        public A ValueA { get => transitionA.value; set => transitionA.value = value; }

        public B ValueB { get => transitionB.value; set => transitionB.value = value; }

        public C ValueC { get => transitionC.value; set => transitionC.value = value; }

        public D ValueD { get => transitionD.value; set => transitionD.value = value; }

        protected override List<TransitionBase> Transitions => new List<TransitionBase>()
        {
            transitionA,
            transitionB,
            transitionC,
            transitionD
        };

        #region Start

        public void Start(A targetA, B targetB, C targetC, D targetD, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, targetD, default, default, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, D targetD, bool isUnscaled, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, targetD, isUnscaled, default, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, D targetD, bool isUnscaled, bool isReversed, Action onTransitionEnd = null)
        {
            StartTransition(transitionA, targetA, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionB, targetB, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionC, targetC, isUnscaled, isReversed, ref onTransitionEnd);
            StartTransition(transitionD, targetD, isUnscaled, isReversed, ref onTransitionEnd);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Update

        public void Update(Action<A, B, C, D> onTransitionUpdate)
        {
            if (Update(out A valueA, out B valueB, out C valueC, out D valueD))
            {
                onTransitionUpdate?.Invoke(valueA, valueB, valueC, valueD);
            }
        }

        public bool Update(out A valueA, out B valueB, out C valueC, out D valueD)
        {
            bool updateValue = false;

            UpdateTransition(transitionA, ref updateValue, out valueA);
            UpdateTransition(transitionB, ref updateValue, out valueB);
            UpdateTransition(transitionC, ref updateValue, out valueC);
            UpdateTransition(transitionD, ref updateValue, out valueD);

            return updateValue;
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Public

        public void SetValues(A valueA, B valueB, C valueC, D valueD)
        {
            ValueA = valueA;
            ValueB = valueB;
            ValueC = valueC;
            ValueD = valueD;
        }

        #endregion

    }

    #endregion

}