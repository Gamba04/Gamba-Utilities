using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;

namespace GambaUtilities.UI
{
	[SelectionBase]
	[DisallowMultipleComponent]
	public class Button : TouchReceiver
	{

		#region Target

		private enum TargetSpace
		{
			UI,
			World
		}

		private enum PreviewState
		{
			None,
			Normal,
			Hover,
			Pressed
		}

		[Serializable]
		private class Target : SerializableElement
		{
			[SerializeField]
			private TargetGraphic graphic;
			[SerializeField]
			private TargetState normal;
			[SerializeField]
			private TargetState hover;
			[SerializeField]
			private TargetState pressed;
			[SerializeField]
			[Min(0)]
			private float duration;

			[SerializeField, HideInInspector]
			private Transition<TargetState> transition = new Transition<TargetState>();

			private bool interactions = true;

			#region Init

			public Target() => Init();

			public override void Init()
			{
				normal = TargetState.Default;
				hover = TargetState.Default;
				pressed = TargetState.Default;
				duration = 0.1f;
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region States

			public void ProcessState(TouchState state, bool isMouse, bool isCurrent)
			{
				TargetState? target = state switch
				{
					TouchState.HoverEnter => hover,
					TouchState.HoverExit => normal,
					TouchState.Press => pressed,
					TouchState.Release => !isMouse || !isCurrent ? normal : (TargetState?)null,
					TouchState.Cancel => normal,
					_ => null
				};

				if (target.HasValue) transition.Start(target.Value);
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Update

			public void Update(bool interactable)
			{
				UpdateInteractions(interactable);
				UpdateTransition();
			}

			private void UpdateInteractions(bool interactable)
			{
				if (!interactable && interactions)
				{
					transition.Start(normal);
				}

				interactions = interactable;
			}

			private void UpdateTransition()
			{
				if (transition.Update(out TargetState state))
				{
					graphic.Apply(state);
				}
			}

			#endregion

			// ----------------------------------------------------------------------------------------------------

			#region Editor

			public void EditorUpdate(int index, TargetSpace space, PreviewState preview)
			{
				UpdateValues(index, space);
				UpdatePreview(preview);
			}

			private void UpdateValues(int index, TargetSpace space)
			{
				name = $"Target {index + 1}";

				graphic.space = space;
				transition.value = normal;
				transition.duration = duration;
			}

			public void UpdatePreview(PreviewState preview)
			{
				TargetState state = preview switch
				{
					PreviewState.None => TargetState.Default,
					PreviewState.Normal => normal,
					PreviewState.Hover => hover,
					PreviewState.Pressed => pressed,
					_ => throw new InvalidCastException()
				};

				graphic.Apply(state);
			}

			#endregion

		}

		[Serializable]
		private struct TargetGraphic
		{
			[HideInInspector]
			public TargetSpace space;
			public Graphic graphic;
			public SpriteRenderer sprite;

			public void Apply(TargetState state)
			{
				switch (space)
				{
					case TargetSpace.UI:

						if (graphic)
						{
							graphic.color = state.color;
							graphic.rectTransform.anchoredPosition = state.position;
							graphic.rectTransform.localScale = state.scale.GetScale2D();
						}

						break;

					case TargetSpace.World:

						if (sprite)
						{
							sprite.color = state.color;
							sprite.transform.localPosition = state.position.WithDepth(sprite.transform.localPosition.z);
							sprite.transform.localScale = state.scale.GetScale2D();
						}

						break;
				}
			}
		}

		[Serializable]
		private struct TargetState : ITransitionable<TargetState>
		{
			public Color color;
			public Vector2 position;
			public Vector2 scale;

			public readonly static TargetState Default = new TargetState() { color = Color.white, scale = Vector2.one };

			public TargetState Lerp(TargetState a, TargetState b, float t)
			{
				return new TargetState()
				{
					color = Color.LerpUnclamped(a.color, b.color, t),
					position = Vector2.LerpUnclamped(a.position, b.position, t),
					scale = Vector2.LerpUnclamped(a.scale, b.scale, t)
				};
			}

			public bool Equals(TargetState state) => (color, position, scale) == (state.color, state.position, state.scale);
		}

		#endregion

		[SerializeField, HideInInspector]
		private Canvas canvas;

		public bool interactable = true;

		[Space]
		[SerializeField]
		private TargetSpace space;
		[SerializeField]
		[ShowIf(nameof(space), TargetSpace.World)]
		private new Collider2D collider;
		[SerializeField]
		private List<Target> targets;

		[Space]
		[SerializeField]
		private UnityEvent onClick;

		[Space]
		[SerializeField]
		private PreviewState preview;

		public event Action onHoverEnter;
		public event Action onHoverExit;
		public event Action onPress;
		public event Action onRelease;
		public event Action onAbort;

		public UnityEvent OnClick => onClick;

		public static bool interactions = true;

		#region Start

		private void Start()
		{
			preview = PreviewState.None;
			targets.ForEach(target => target.UpdatePreview(PreviewState.Normal));
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Interactions

		public override bool Validate(GameTouch touch)
		{
			bool interactable = this.interactable && interactions;

			return interactable && base.Validate(touch);
		}

		public override bool Overlap(Vector2 screenPosition)
		{
			return space switch
			{
				TargetSpace.UI => OverlapUI(),
				TargetSpace.World => OverlapWorld(),
				_ => false
			};

			bool OverlapUI() => canvas ? this.OverlapUI(screenPosition, canvas) : false;

			bool OverlapWorld() => collider ? Overlap2D(screenPosition, collider) : false;
		}

		public override void ReceiveTouch(GameTouch touch, bool isInitial, bool isCurrent)
		{
			if (isInitial || touch.IsHover)
			{
				foreach (Target target in targets)
				{
					target.ProcessState(touch.state, touch.IsMouse, isCurrent);
				}

				Action callback = touch.state switch
				{
					TouchState.HoverEnter => onHoverEnter,
					TouchState.HoverExit => onHoverExit,
					TouchState.Press => onPress,
					TouchState.Release => onRelease,
					_ => null
				};

				callback?.Invoke();

				if (touch.state == TouchState.Release)
				{
					if (isCurrent) onClick.Invoke();
					else onAbort?.Invoke();
				}
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Update

		private void Update()
		{
			bool interactable = this.interactable && interactions;

			foreach (Target target in targets)
			{
				target.Update(interactable);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Editor

#if UNITY_EDITOR

		#region Button

		private void Reset()
		{
			space = transform is RectTransform ? TargetSpace.UI : TargetSpace.World;
			targets = new List<Target>() { new Target() };

			OnValidate();
		}

		private void OnValidate()
		{
			UpdateCanvas();
			UpdateTargets();
		}

		private void UpdateCanvas()
		{
			if (IsUI() && !IsCached()) canvas = GetComponentInParent<Canvas>();

			bool IsUI() => space == TargetSpace.UI && transform is RectTransform;

			bool IsCached() => canvas && transform.IsChildOf(canvas.transform);
		}

		private void UpdateTargets()
		{
			targets.ForEach((target, index) => target.EditorUpdate(index, space, preview));
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Target Graphic

		[CustomPropertyDrawer(typeof(TargetGraphic))]
		public class TargetGraphicDrawer : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				TargetSpace space = GetSpace(property);

				DrawGraphic(position, property, space);
			}

			private TargetSpace GetSpace(SerializedProperty property)
			{
				SerializedProperty space = property.FindPropertyRelative(nameof(TargetGraphic.space));

				return (TargetSpace)Enum.GetValues(typeof(TargetSpace)).GetValue(space.enumValueIndex);
			}

			private void DrawGraphic(Rect position, SerializedProperty property, TargetSpace space)
			{
				string name = space switch
				{
					TargetSpace.UI => nameof(TargetGraphic.graphic),
					TargetSpace.World => nameof(TargetGraphic.sprite),
					_ => throw new InvalidCastException()
				};

				SerializedProperty graphic = property.FindPropertyRelative(name);

				EditorGUI.PropertyField(position, graphic);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Preview State

		[CustomPropertyDrawer(typeof(PreviewState))]
		public class PreviewStateDrawer : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.PropertyField(position, property, label);

				DrawButton(position, property);
			}

			private void DrawButton(Rect position, SerializedProperty property)
			{
				position.x = EditorGUIUtility.labelWidth - 2;
				position.width = 20;

				if (GUI.Button(position, GUIContent.none, GUIStyle.none)) Refresh(property);

				GUIContent content = new GUIContent("↻", "Refresh");
				GUIStyle style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };

				EditorGUI.LabelField(position, content, style);
			}

			private void Refresh(SerializedProperty property)
			{
				if (property.serializedObject.targetObject is Button button)
				{
					button.OnValidate();

					EditorApplication.QueuePlayerLoopUpdate();
				}
			}
		}

		#endregion

#endif

		#endregion

	}
}