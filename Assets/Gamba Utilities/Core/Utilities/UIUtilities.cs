using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GambaUtilities
{
	public static class UIUtilities 
	{
		private static Canvas canvas;

		public static Canvas Canvas => canvas ? canvas : canvas = Object.FindObjectOfType<Canvas>();

		#region Screen

		public static Vector3 ScreenToCanvasPoint(this Canvas canvas, Vector2 screenPosition)
		{
			return canvas.renderMode switch
			{
				RenderMode.ScreenSpaceOverlay => screenPosition,
				RenderMode.ScreenSpaceCamera => GetScreenSpaceCameraPosition(),
				RenderMode.WorldSpace => GetWorldSpacePosition(),
				_ => throw new InvalidCastException()
			};

			Vector3 GetScreenSpaceCameraPosition()
			{
				Camera camera = canvas.worldCamera;

				return camera ? camera.ScreenToWorldPoint(screenPosition) : (Vector3)screenPosition;
			}

			Vector3 GetWorldSpacePosition()
			{
				RectTransform rectTransform = (RectTransform)canvas.transform;
				Camera camera = GetWorldSpaceCamera(canvas);

				RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPosition, camera, out Vector3 canvasPosition);

				return canvasPosition;
			}
		}

		public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPosition) => rectTransform.ContainsScreenPoint(screenPosition, Canvas);

		public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPosition, Canvas canvas)
		{
			if (canvas)
			{
				Camera camera = canvas.renderMode switch
				{
					RenderMode.ScreenSpaceOverlay => null,
					RenderMode.ScreenSpaceCamera => canvas.worldCamera,
					RenderMode.WorldSpace => GetWorldSpaceCamera(canvas),
					_ => throw new InvalidCastException()
				};

				return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, camera);
			}
			else return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
		}

		private static Camera GetWorldSpaceCamera(Canvas canvas) => canvas.worldCamera.ExistingObject() ?? Camera.main;

		#endregion

	}
}