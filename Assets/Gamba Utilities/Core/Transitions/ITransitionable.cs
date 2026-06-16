using System;

namespace GambaUtilities
{
	public interface ITransitionable<T> : IEquatable<T>
	{
		T Lerp(T a, T b, float t);
	}
}