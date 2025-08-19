using System;

namespace GambaUtilities
{
    using Internal;

    #region Transition<A, B, C, D>

    [Serializable]
    public class Transition<A, B, C, D> : TransitionTemplate
        where A : struct
        where B : struct
        where C : struct
        where D : struct
    {
        private readonly Transition<A> transitionA = new Transition<A>();
        private readonly Transition<B> transitionB = new Transition<B>();
        private readonly Transition<C> transitionC = new Transition<C>();
        private readonly Transition<D> transitionD = new Transition<D>();

        public override event Action onTransitionEnd;

        public A ValueA { get => transitionA.value; set => transitionA.value = value; }

        public B ValueB { get => transitionB.value; set => transitionB.value = value; }

        public C ValueC { get => transitionC.value; set => transitionC.value = value; }

        public D ValueD { get => transitionD.value; set => transitionD.value = value; }

        public override float Time => transitionA.Time;

        public override bool IsInTransition => transitionA.IsInTransition;

        #region Start

        public void Start(A targetA, B targetB, C targetC, D targetD, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, targetD, isUnscaled, isReversed, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, D targetD, bool isUnscaled, Action onTransitionEnd = null) => Start(targetA, targetB, targetC, targetD, isUnscaled, isReversed, onTransitionEnd);

        public void Start(A targetA, B targetB, C targetC, D targetD, bool isUnscaled, bool isReversed, Action onTransitionEnd = null)
        {
            transitionA.Start(targetA, isUnscaled, isReversed, onTransitionEnd);
            transitionB.Start(targetB, isUnscaled, isReversed);
            transitionC.Start(targetC, isUnscaled, isReversed);
            transitionD.Start(targetD, isUnscaled, isReversed);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Update

        public bool Update(out A valueA, out B valueB, out C valueC, out D valueD)
        {
            bool updateValue = true;

            updateValue |= transitionA.Update(out valueA);
            updateValue |= transitionB.Update(out valueB);
            updateValue |= transitionC.Update(out valueC);
            updateValue |= transitionD.Update(out valueD);

            return updateValue;
        }

        public void Update(Action<A, B, C, D> onTransitionUpdate)
        {
            if (Update(out A valueA, out B valueB, out C valueC, out D valueD))
            {
                onTransitionUpdate?.Invoke(valueA, valueB, valueC, valueD);
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Termination

        public override void Cancel() => ForEachTransition(transition => transition.Cancel());

        public override void End() => ForEachTransition(transition => transition.End());

        public override void Complete() => ForEachTransition(transition => transition.Complete());

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Other

        private void ForEachTransition(Action<TransitionTemplate> action)
        {
            action(transitionA);
            action(transitionB);
            action(transitionC);
            action(transitionD);
        }

        #endregion

    }

    #endregion

}