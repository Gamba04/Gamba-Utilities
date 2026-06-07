using UnityEngine;

namespace GambaUtilities
{
	using Internal;

	public abstract class TouchReceiver : MonoBehaviour
	{

		#region Registration

		private void OnEnable() => TouchManager.Register(this);

		private void OnDisable() => TouchManager.Unregister(this);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Interactions

		public abstract bool Overlap(Vector2 screenPosition);

		public abstract void OnTouch(GameTouch touch);

		#endregion

	}
}