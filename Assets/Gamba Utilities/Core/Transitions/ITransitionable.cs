namespace GambaUtilities
{
	public interface ITransitionable<T>
	{
		T Lerp(T a, T b, float t);
	}
}