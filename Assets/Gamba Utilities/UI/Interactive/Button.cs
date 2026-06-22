using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;

namespace GambaUtilities.UI
{
	using Editor;

	[SelectionBase]
	[DisallowMultipleComponent]
	[HideMonoScript]
	public class Button : TouchReceiver
	{

		#region Target

		private enum TargetSpace
		{
			UI,
			World
		}

		private enum ButtonState
		{
			None,
			Normal,
			Hover,
			Pressed
		}

		[Serializable]
		private class Target : SerializableElement
		{
			[SerializeField, HideInInspector]
			public bool overridePosition;

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

			public void SetState(ButtonState state)
			{
				TargetState target = GetTarget(state);

				if (transition.Start(target, true))
				{
					graphic.Apply(target);
				}
			}

			public void ApplyState(ButtonState state)
			{
				TargetState target = GetTarget(state);

				graphic.Apply(target);
			}

			private TargetState GetTarget(ButtonState state)
			{
				return state switch
				{
					ButtonState.None => TargetState.Default,
					ButtonState.Normal => normal,
					ButtonState.Hover => hover,
					ButtonState.Pressed => pressed,
					_ => throw new InvalidCastException()
				};
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
					transition.Start(normal, true);
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

			public void EditorUpdate(int index, TargetSpace space, ButtonState preview)
			{
				UpdateValues(index, space);
				ApplyState(preview);
			}

			private void UpdateValues(int index, TargetSpace space)
			{
				name = $"Target {index + 1}";

				graphic.space = space;
				transition.value = normal;
				transition.duration = duration;
			}

			public void TogglePosition()
			{
				overridePosition = !overridePosition;

				normal.overridePosition = overridePosition;
				hover.overridePosition = overridePosition;
				pressed.overridePosition = overridePosition;
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
							graphic.canvasRenderer.SetColor(state.color);
							graphic.rectTransform.localScale = state.scale.GetScale2D();

							if (state.overridePosition) graphic.rectTransform.anchoredPosition = state.position;
						}

						break;

					case TargetSpace.World:

						if (sprite)
						{
							sprite.color = state.color;
							sprite.transform.localScale = state.scale.GetScale2D();

							if (state.overridePosition) sprite.transform.localPosition = state.position.WithDepth(sprite.transform.localPosition.z);
						}

						break;
				}
			}
		}

		[Serializable]
		private struct TargetState : ITransitionable<TargetState>
		{
			[HideInInspector]
			public bool overridePosition;

			public Color color;
			[ShowIf(nameof(overridePosition), true)]
			public Vector2 position;
			public Vector2 scale;

			public readonly static TargetState Default = new TargetState(Color.white, Vector2.zero, Vector2.one);

			public TargetState(Color color, Vector2 position, Vector2 scale) : this()
			{
				this.color = color;
				this.position = position;
				this.scale = scale;
			}

			public TargetState Lerp(TargetState a, TargetState b, float t) => new TargetState
			(
				color: Color.LerpUnclamped(a.color, b.color, t),
				position: Vector2.LerpUnclamped(a.position, b.position, t),
				scale: Vector2.LerpUnclamped(a.scale, b.scale, t)
			);

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
		[Tooltip("Right-click to toggle position")]
		private List<Target> targets;

		[Space]
		[SerializeField]
		private UnityEvent onClick;

		[Space]
		[SerializeField]
		private ButtonState preview;

		public event Action onHoverEnter;
		public event Action onHoverExit;
		public event Action onPress;
		public event Action onAbort;

		public UnityEvent OnClick => onClick;

		public static bool interactions = true;

		#region Init

		protected override void Init()
		{
			preview = ButtonState.None;
			targets.ForEach(target => target.ApplyState(ButtonState.Normal));
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
				ButtonState state = ButtonState.None;

				switch (touch.state)
				{
					case TouchState.HoverEnter: OnHoverEnter(); break;
					case TouchState.HoverExit: OnHoverExit(); break;
					case TouchState.Press: OnPress(); break;
					case TouchState.Hold: OnHold(); break;
					case TouchState.Release: OnRelease(); break;
					case TouchState.Cancel: OnCancel(); break;
				}

				if (state > ButtonState.None)
				{
					foreach (Target target in targets)
					{
						target.SetState(state);
					}
				}

				void OnHoverEnter()
				{
					state = ButtonState.Hover;

					onHoverEnter?.Invoke();
				}

				void OnHoverExit()
				{
					state = ButtonState.Normal;

					onHoverExit?.Invoke();
				}

				void OnPress()
				{
					state = ButtonState.Pressed;

					onPress?.Invoke();
				}

				void OnHold()
				{
					if (!isCurrent)
					{
						state = ButtonState.Normal;

						Discard(touch);

						onAbort?.Invoke();
					}
				}

				void OnRelease()
				{
					state = ButtonState.Normal;

					if (isCurrent) onClick?.Invoke();
					else onAbort?.Invoke();
				}

				void OnCancel()
				{
					state = ButtonState.Normal;
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

		#region Context Menu

		[InitializeOnLoadMethod]
		private static void InitContextMenu()
		{
			EditorApplication.contextualPropertyMenu += OnContextMenu;
		}

		private static void OnContextMenu(GenericMenu menu, SerializedProperty property)
		{
			if (Validate(property, out Target target))
			{
				menu.AddItem(new GUIContent("Override Position"), target.overridePosition, TogglePosition);
			}

			void TogglePosition() => Button.TogglePosition(property, target);
		}

		private static bool Validate(SerializedProperty property, out Target target)
		{
			target = null;

			bool isValid = true

			&& property.serializedObject.targetObject.GetType() == typeof(Button)
			&& property.type == nameof(Target)
			&& !property.isArray
			&& property.TryGetValueOfType(out target);

			return isValid;
		}

		private static void TogglePosition(SerializedProperty property, Target target)
		{
			target.TogglePosition();

			EditorUtility.SetDirty(property.serializedObject.targetObject);
			ActiveEditorTracker.sharedTracker.ForceRebuild();
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Target Graphic

		[CustomPropertyDrawer(typeof(TargetGraphic))]
		private class TargetGraphicDrawer : PropertyDrawer
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

		[CustomPropertyDrawer(typeof(ButtonState))]
		private class PreviewStateDrawer : PropertyDrawer
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