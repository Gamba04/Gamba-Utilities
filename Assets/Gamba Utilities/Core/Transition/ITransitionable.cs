namespace GambaUtilities
{
    public interface ITransitionable<T>
        where T : ITransitionable<T>
    {
        T Lerp(T a, T b, float t);
    }
}