using UnityEngine;

namespace GambaUtilities
{
	using Internal;

	public abstract class TouchReceiver : MonoBehaviour
	{
		private int? currentTouch;

		/// <summary> Allow multiple touches to interact with this object simultaneously. </summary>
		/// <remarks> For single-touch systems such as buttons, it is recommended to leave this as default. </remarks>
		protected virtual bool MultiTouch => false;

		#region Registration

		private void OnEnable() => TouchManager.Register(this);

		private void OnDisable() => TouchManager.Unregister(this);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Interactions

		/// <summary> Checks if the object is currently available for interactions. </summary>
		/// <remarks> Default implementation contains single-touch validation. </remarks>
		public virtual bool Validate(GameTouch touch)
		{
			if (!MultiTouch)
			{
				if (currentTouch == touch || !currentTouch.HasValue)
				{
					currentTouch = !touch.IsFinished ? touch : (int?)null;

					return true;
				}

				return false;
			}

			return true;
		}

		/// <summary> Checks if the object is overlapped by <paramref name="screenPosition"/>. </summary>
		public abstract bool Overlap(Vector2 screenPosition);

		/// <summary> Receives an update of a <paramref name="touch"/> that is currently or has interacted with this object. </summary>
		/// <param name="isInitial"> Indicates whether this object was overlapped by the <paramref name="touch"/> when it was initially pressed (if it has been). </param>
		/// <param name="isCurrent"> Indicates whether this object is currently being overlapped by the <paramref name="touch"/>. </param>
		public abstract void ReceiveTouch(GameTouch touch, bool isInitial, bool isCurrent);

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Overlap Utilities

		/// <summary> Checks if <paramref name="collider"/> is overlapped by the world-converted <paramref name="screenPosition"/>. </summary>
		protected bool Overlap2D(Vector2 screenPosition, Collider2D collider) => Overlap2D(screenPosition, collider, Camera.main);

		/// <summary> Checks if <paramref name="collider"/> is overlapped by the world-converted <paramref name="screenPosition"/>. </summary>
		/// <param name="camera"> Custom world-conversion camera. </param>
		protected bool Overlap2D(Vector2 screenPosition, Collider2D collider, Camera camera)
		{
			Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);

			return collider.OverlapPoint(worldPosition);
		}

		/// <summary> Checks if <paramref name="collider"/> is intersected by the ray-converted <paramref name="screenPosition"/>. </summary>
		protected bool Overlap3D(Vector2 screenPosition, Collider collider) => Overlap3D(screenPosition, collider, Mathf.Infinity, Camera.main);

		/// <summary> Checks if <paramref name="collider"/> is intersected by the ray-converted <paramref name="screenPosition"/>. </summary>
		/// <param name="distance"> Max distance from the camera. </param>
		protected bool Overlap3D(Vector2 screenPosition, Collider collider, float distance) => Overlap3D(screenPosition, collider, distance, Camera.main);

		/// <summary> Checks if <paramref name="collider"/> is intersected by the ray-converted <paramref name="screenPosition"/>. </summary>
		/// <param name="camera"> Custom ray-conversion camera. </param>
		protected bool Overlap3D(Vector2 screenPosition, Collider collider, Camera camera) => Overlap3D(screenPosition, collider, Mathf.Infinity, camera);

		/// <summary> Checks if <paramref name="collider"/> is intersected by the ray-converted <paramref name="screenPosition"/>. </summary>
		/// <param name="distance"> Max distance from the <paramref name="camera"/>. </param>
		/// <param name="camera"> Custom ray-conversion camera. </param>
		protected bool Overlap3D(Vector2 screenPosition, Collider collider, float distance, Camera camera)
		{
			Ray ray = camera.ScreenPointToRay(screenPosition);

			return collider.Raycast(ray, out _, distance);
		}

		/// <summary> Assumes that <see cref="Component.transform"/> is a <see cref="RectTransform"/> and checks if it is overlapped by <paramref name="screenPosition"/>. </summary>
		protected bool OverlapUI(Vector2 screenPosition) => OverlapUI(screenPosition, (RectTransform)transform);

		/// <summary> Checks if <paramref name="rectTransform"/> is overlapped by <paramref name="screenPosition"/>. </summary>
		protected bool OverlapUI(Vector2 screenPosition, RectTransform rectTransform) => rectTransform.ContainsScreenPoint(screenPosition);

		/// <summary> Checks if <paramref name="rectTransform"/> is overlapped by <paramref name="screenPosition"/>. </summary>
		/// <param name="canvas"> Custom canvas to be based on. </param>
		protected bool OverlapUI(Vector2 screenPosition, RectTransform rectTransform, Canvas canvas) => rectTransform.ContainsScreenPoint(screenPosition, canvas);

		/// <summary> Checks if <paramref name="collider"/> is overlapped by the canvas-converted <paramref name="screenPosition"/>. </summary>
		/// <remarks> It is not recommended to use this variant for UI detection, unless a custom <paramref name="collider"/> shape is necessary. </remarks>
		protected bool OverlapUI(Vector2 screenPosition, Collider2D collider) => OverlapUI(screenPosition, collider, UIUtilities.Canvas);

		/// <summary> Checks if <paramref name="collider"/> is overlapped by the canvas-converted <paramref name="screenPosition"/>. </summary>
		/// <remarks> It is not recommended to use this variant for UI detection, unless a custom <paramref name="collider"/> shape is necessary. </remarks>
		/// <param name="canvas"> Custom conversion canvas. </param>
		protected bool OverlapUI(Vector2 screenPosition, Collider2D collider, Canvas canvas)
		{
			Vector3 canvasPosition = canvas.ScreenToCanvasPoint(screenPosition);

			return collider.OverlapPoint(canvasPosition);
		}

		#endregion

	}
}