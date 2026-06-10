using UnityEngine;

namespace GambaUtilities
{
	/// <summary> Ensures that <typeparamref name="T"/> can only have one active instance. </summary>
	[DisallowMultipleComponent]
	public abstract class SingletonBehaviour<T> : MonoBehaviour
		where T : SingletonBehaviour<T>
	{
		protected static T instance;

		public static T Instance => instance.ExistingObject() ?? FindInstance() ?? BuildInstance();

		#region Instance

		private static T FindInstance()
		{
			return instance = FindObjectOfType<T>();
		}

		private static T BuildInstance()
		{
			GameObject gameObject = new GameObject(typeof(T).Name.ToProperCase());

			return instance = gameObject.AddComponent<T>();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Init

		private void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
			}
			else if (this is T && instance != this)
			{
				Destroy(gameObject);

				return;
			}

			Init();
		}

		protected virtual void Init() { }

		#endregion

	}
}