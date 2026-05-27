using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GambaUtilities
{
	public static class CoreUtilities
	{
		public const string scriptableRoot = "Gamba Utilities/";

		#region Lists

		#region Resize

		/// <summary> Resizes the list to have an amount of elements between <paramref name="min"/> and <paramref name="max"/>, while keeping the existing elements. </summary>
		public static void ResizeFrom<T>(this List<T> list, int min, int max)
		{
			list.ResizeFrom(min, max, () => default);
		}

		/// <summary> Resizes the list to have an amount of elements between <paramref name="min"/> and <paramref name="max"/>, while keeping the existing elements. </summary>
		/// <param name="customDefault"> If the list size increases, the new elements will get the value of <paramref name="customDefault"/>(). </param> 
		public static void ResizeFrom<T>(this List<T> list, int min, int max, Func<T> customDefault)
		{
			list.Resize(Mathf.Clamp(list.Count, min, max), customDefault);
		}

		/// <summary> Resizes the list to have a minimum <paramref name="size"/>, while keeping the existing elements. </summary>
		public static void ResizeMin<T>(this List<T> list, int size)
		{
			list.ResizeMin(size, () => default);
		}

		/// <summary> Resizes the list to have a minimum <paramref name="size"/>, while keeping the existing elements. </summary>
		/// <param name="customDefault"> If the list size increases, the new elements will get the value of <paramref name="customDefault"/>(). </param> 
		public static void ResizeMin<T>(this List<T> list, int size, Func<T> customDefault)
		{
			list.Resize(Mathf.Max(list.Count, size), customDefault);
		}

		/// <summary> Resizes the list to have a maximum <paramref name="size"/>, while keeping the existing elements. </summary>
		public static void ResizeMax<T>(this List<T> list, int size)
		{
			list.Resize(Mathf.Min(list.Count, size));
		}

		/// <summary> Resizes the list to match the amount of values in <paramref name="enumType"/>, while keeping the existing elements. </summary>
		public static void Resize<T>(this List<T> list, Type enumType)
		{
			list.Resize(enumType, () => default);
		}

		/// <summary> Resizes the list to match the amount of values in <paramref name="enumType"/>, while keeping the existing elements. </summary>
		/// <param name="customDefault"> If the list size increases, the new elements will get the value of <paramref name="customDefault"/>(). </param> 
		public static void Resize<T>(this List<T> list, Type enumType, Func<T> customDefault)
		{
			int size = GetEnumLength(enumType);

			list.Resize(size, customDefault);
		}

		/// <summary> Resizes the list to <paramref name="size"/>, while keeping the existing elements. </summary>
		public static void Resize<T>(this List<T> list, int size)
		{
			list.Resize(size, () => default);
		}

		/// <summary> Resizes the list to <paramref name="size"/>, while keeping the existing elements. </summary>
		/// <param name="customDefault"> If the list size increases, the new elements will get the value of <paramref name="customDefault"/>(). </param> 
		public static void Resize<T>(this List<T> list, int size, Func<T> customDefault)
		{
			list ??= new List<T>(size);
			int delta = size - list.Count;

			if (delta > 0) Add(delta);
			else if (delta < 0) Remove(-delta);

			void Add(int amount)
			{
				list.Capacity = size;

				for (int i = 0; i < amount; i++)
				{
					list.Add(customDefault());
				}
			}

			void Remove(int amount)
			{
				list.RemoveRange(list.Count - amount, amount);
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Sorting

		/// <summary> QuickSorts the list with its default comparison method. </summary>
		public static void QuickSort<T>(this List<T> list, bool reversed = false)
			where T : IComparable<T>
		{
			list.QuickSort(Comparison);

			int Comparison(T x, T y) => reversed ? y.CompareTo(x) : x.CompareTo(y);
		}

		/// <summary> QuickSorts the list with a custom evaluation method. </summary>
		/// <param name="evaluation"> Custom evaluation method which defines a <see cref="float"/> value to be compared against others. </param>
		public static void QuickSort<T>(this List<T> list, Func<T, float> evaluation, bool reversed = false)
		{
			list.QuickSort(Comparison);

			int Comparison(T x, T y)
			{
				float value = reversed ? evaluation(y) - evaluation(x) : evaluation(x) - evaluation(y);

				return MathUtilities.RoundAwayFromZero(value);
			}
		}

		/// <summary> QuickSorts the list with a custom comparison method for complex operations. </summary>
		/// <param name="comparison">
		/// Custom comparison method which defines whether if element x is greater than y. <br/>
		/// The integer output of <paramref name="comparison"/> should represent the operation "x - y".
		/// </param>
		public static void QuickSort<T>(this List<T> list, Comparison<T> comparison)
		{
			if (list.Count == 0) return;

			int pivot = list.Count - 1;

			// Create partitions
			List<T> left = new List<T>();
			List<T> right = new List<T>();

			for (int i = 0; i < list.Count - 1; i++)
			{
				List<T> partition = comparison(list[i], list[pivot]) < 0 ? left : right;

				partition.Add(list[i]);
			}

			// Recurse
			left.QuickSort(comparison);
			right.QuickSort(comparison);

			// Join partitions
			T pivotElement = list[pivot];

			list.Clear();

			list.AddRange(left);
			list.Add(pivotElement);
			list.AddRange(right);
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Other

		/// <summary> Randomizes the contents of the list. </summary>
		/// <param name="forceChange"> Forces the list to change every time. </param>
		public static void Shuffle<T>(this List<T> list, bool forceChange = false)
		{
			List<T> result = new List<T>(list.Count);
			List<T> pool = new List<T>(list);

			for (int i = 0; i < list.Count; i++)
			{
				if (forceChange && pool.Count > 1) pool.RemoveAt(i);

				int index = Random.Range(0, pool.Count);

				result.Add(pool[index]);
				pool.RemoveAt(index);
			}

			list.Replace(result);
		}

		/// <summary> Iterates each element and calls an <paramref name="action"/> that includes the index. </summary>
		public static void ForEach<T>(this List<T> list, Action<T, int> action)
		{
			for (int i = 0; i < list.Count; i++) action(list[i], i);
		}

		/// <summary> Iterates each element in a reversed loop and calls an <paramref name="action"/>. </summary>
		public static void ForEachReversed<T>(this List<T> list, Action<T> action)
		{
			for (int i = list.Count - 1; i > -1; i--) action(list[i]);
		}

		/// <summary> Iterates each element in a reversed loop and calls an <paramref name="action"/> that includes the index. </summary>
		public static void ForEachReversed<T>(this List<T> list, Action<T, int> action)
		{
			for (int i = list.Count - 1; i > -1; i--) action(list[i], i);
		}

		/// <summary> Replaces the list contents with <paramref name="collection"/>. </summary>
		public static void Replace<T>(this List<T> list, ICollection<T> collection)
		{
			list.Clear();
			list.AddRange(collection);
		}

		/// <summary> Moves <paramref name="element"/> to the start of the list. </summary>
		public static void MoveToStart<T>(this List<T> list, T element)
		{
			list.Remove(element);
			list.Insert(0, element);
		}

		/// <summary> Moves <paramref name="element"/> to the end of the list. </summary>
		public static void MoveToEnd<T>(this List<T> list, T element)
		{
			list.Remove(element);
			list.Add(element);
		}

		/// <summary> Destroys everything in the list. </summary>
		public static void DestroyAll<O>(this List<O> list, bool includeGameObjects = true)
			where O : Object
		{
			for (int i = 0; i < list.Count; i++)
			{
				Object obj = list[i];

				if (includeGameObjects && obj is Component component) obj = component.gameObject;

				Object.Destroy(obj);
			}

			list.Clear();
		}

		#endregion

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Enums

		/// <summary> Gets the amount of values in <typeparamref name="E"/>. </summary>
		public static int GetEnumLength<E>() where E : Enum => GetEnumLength(typeof(E));

		/// <summary> Gets the amount of values in <paramref name="enumType"/>. </summary>
		public static int GetEnumLength(Type enumType) => Enum.GetValues(enumType).Length;

		/// <summary> Indicates whether the <typeparamref name="E"/> is equal to any of these <paramref name="values"/>. </summary>
		public static bool IsEither<E>(this E enumValue, params E[] values)
			where E : Enum
		{
			foreach (E value in values)
			{
				if (enumValue.Equals(value))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Strings

		/// <summary> Indicates whether the value is neither <see langword="null"/> nor empty. </summary>
		public static bool HasContent(this string text) => !string.IsNullOrEmpty(text);

		public static bool Contains(this string text, char value)
		{
			return text.Contains(value.ToString());
		}

		public static string Remove(this string text, params string[] values)
		{
			foreach (string value in values)
			{
				text = text.Replace(value, "");
			}

			return text;
		}

		/// <summary> Converts the text to Proper Case. </summary>
		/// <remarks>
		///		Examples:
		///		<code>
		///			"camelCase"  → "Camel Case"
		///			"PascalCase" → "Pascal Case"
		///			"ABCAcronym" → "ABC Acronym"
		///			"123Numbers" → "123 Numbers"
		///		</code>
		/// </remarks>
		public static string ToProperCase(this string text)
		{
			text = Regex.Replace(text, @"^\p{Ll}", match => match.Value.ToUpper());
			text = Regex.Replace(text, @"(?<!^)(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})|(?<=\p{Ll})(?=\p{N})|(?<=\p{N})(?=\p{L})", " ");

			return text;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Physics

		#region 2D

		#region CheckForComponent

		/// <summary> Checks for an object containing a <typeparamref name="C"/> that collides at <paramref name="point"/>. </summary>
		public static C CheckForComponent2D<C>(Vector2 point, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			return CheckForComponent2D(point, GetComponentAttached<C>, layerMask, ignore);
		}

		/// <summary> Checks for an object containing a <typeparamref name="C"/> that collides at <paramref name="point"/>, with a custom <paramref name="getComponent"/> function. </summary>
		public static C CheckForComponent2D<C>(Vector2 point, Converter<Collider2D, C> getComponent, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			Collider2D[] colliders = Physics2D.OverlapPointAll(point, layerMask);

			foreach (Collider2D collider in colliders)
			{
				C component = getComponent(collider);

				if (!component || ignore.Contains(component)) continue;

				return component;
			}

			return null;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region CheckForNeartestComponent

		/// <summary> Checks for the nearest object containing a <typeparamref name="C"/> that collides at <paramref name="point"/>. </summary>
		public static C CheckForNearestComponent2D<C>(Vector2 point, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			return CheckForNearestComponent2D(point, GetComponentAttached<C>, layerMask, ignore);
		}

		/// <summary> Checks for the nearest object containing a <typeparamref name="C"/> that collides at <paramref name="point"/>, with a custom <paramref name="getComponent"/> function. </summary>
		public static C CheckForNearestComponent2D<C>(Vector2 point, Converter<Collider2D, C> getComponent, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			Collider2D[] colliders = Physics2D.OverlapPointAll(point, layerMask);

			C nearestComponent = null;
			float nearestDistance = Mathf.Infinity;

			foreach (Collider2D collider in colliders)
			{
				C component = getComponent(collider);

				if (!component || ignore.Contains(component)) continue;

				float distance = SqrDistance(point, component.transform.position);

				if (distance < nearestDistance)
				{
					nearestComponent = component;
					nearestDistance = distance;
				}
			}

			return nearestComponent;

			static float SqrDistance(Vector2 a, Vector2 b) => Vector2.SqrMagnitude(a - b);
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region CheckForComponentRaycast

		/// <summary> Checks for an object containing a <typeparamref name="C"/> in the collision result of a raycast. </summary>
		public static C CheckForComponentRaycast2D<C>(Vector2 origin, Vector2 direction, int layerMask = ~0, float maxDistance = Mathf.Infinity)
			where C : Component
		{
			return CheckForComponentRaycast2D(origin, direction, GetComponentAttached<C>, layerMask, maxDistance);
		}

		/// <summary> Checks for an object containing a <typeparamref name="C"/> in the collision result of a raycast, with a custom <paramref name="getComponent"/> function. </summary>
		public static C CheckForComponentRaycast2D<C>(Vector2 origin, Vector2 direction, Converter<Collider2D, C> getComponent, int layerMask = ~0, float maxDistance = Mathf.Infinity)
			where C : Component
		{
			Collider2D collider = Physics2D.Raycast(origin, direction, maxDistance, layerMask).collider;

			return collider ? getComponent(collider) : null;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region CheckForComponents

		/// <summary> Checks for all objects containing a <typeparamref name="C"/> that collide at <paramref name="point"/>. </summary>
		public static List<C> CheckForComponents2D<C>(Vector2 point, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			return CheckForComponents2D(point, GetComponentAttached<C>, layerMask, ignore);
		}

		/// <summary> Checks for all objects containing a <typeparamref name="C"/> that collide at <paramref name="point"/>, with a custom <paramref name="getComponent"/> function. </summary>
		public static List<C> CheckForComponents2D<C>(Vector2 point, Converter<Collider2D, C> getComponent, int layerMask = ~0, params C[] ignore)
			where C : Component
		{
			Collider2D[] colliders = Physics2D.OverlapPointAll(point, layerMask);

			return FindAllComponents(colliders, getComponent, ignore);
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region CheckForComponentsRaycast

		/// <summary> Checks for all objects containing a <typeparamref name="C"/> in the collision results of a raycast. </summary>
		public static List<C> CheckForComponentsRaycast2D<C>(Vector2 origin, Vector2 direction, int layerMask = ~0, float maxDistance = Mathf.Infinity, params C[] ignore)
			where C : Component
		{
			return CheckForComponentsRaycast2D(origin, direction, GetComponentAttached<C>, layerMask, maxDistance, ignore);
		}

		/// <summary> Checks for all objects containing a <typeparamref name="C"/> in the collision results of a raycast, with a custom <paramref name="getComponent"/> function. </summary>
		public static List<C> CheckForComponentsRaycast2D<C>(Vector2 origin, Vector2 direction, Converter<Collider2D, C> getComponent, int layerMask = ~0, float maxDistance = Mathf.Infinity, params C[] ignore)
			where C : Component
		{
			RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance, layerMask);

			return FindAllComponents(hits, hit => getComponent(hit.collider), ignore);
		}

		#endregion

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Other

		private static List<C> FindAllComponents<C, D>(D[] colliders, Converter<D, C> getComponent, C[] ignore)
			where C : Component
		{
			List<C> components = new List<C>();

			foreach (D collider in colliders)
			{
				C component = getComponent(collider);

				if (!component || ignore.Contains(component)) continue;

				components.Add(component);
			}

			return components;
		}

		/// <summary> Finds a component in its <see cref="Collider2D.attachedRigidbody"/> or looks for it in a parent if not available. </summary>
		public static C GetComponentAttached<C>(this Collider2D collider)
			where C : Component
		{
			return collider.GetComponentAttached<C>(collider.attachedRigidbody);
		}

		/// <summary> Finds a component in its <see cref="Collider.attachedRigidbody"/> or looks for it in a parent if not available. </summary>
		public static C GetComponentAttached<C>(this Collider collider)
			where C : Component
		{
			return collider.GetComponentAttached<C>(collider.attachedRigidbody);
		}

		private static C GetComponentAttached<C>(this Component collider, Component rigidbody)
			where C : Component
		{
			return rigidbody ? rigidbody.GetComponent<C>() : collider.GetComponentInParent<C>();
		}

		#endregion

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Scene

		public static void ReloadScene() => SceneManager.LoadScene(GetActiveSceneIndex());

		public static AsyncOperation ReloadSceneAsync() => SceneManager.LoadSceneAsync(GetActiveSceneIndex());

		private static int GetActiveSceneIndex() => SceneManager.GetActiveScene().buildIndex;

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Reset

		/// <summary> Resets the <see cref="Transform"/> to its default values. </summary>
		public static void Reset(this Transform transform)
		{
			transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			transform.localScale = Vector3.one;

			if (transform is RectTransform rectTransform)
			{
				rectTransform.pivot = Vector2.one / 2;
				rectTransform.anchorMin = Vector2.one / 2;
				rectTransform.anchorMax = Vector2.one / 2;
				rectTransform.anchoredPosition = Vector2.zero;
				rectTransform.sizeDelta = Vector2.one * 100;
			}
		}

		/// <summary> Resets the <see cref="AudioSource"/> to its default values. </summary>
		public static void Reset(this AudioSource source, bool playOnAwake = true)
		{
			source.clip = null;
			source.outputAudioMixerGroup = null;
			source.mute = false;
			source.bypassEffects = false;
			source.bypassListenerEffects = false;
			source.bypassReverbZones = false;
			source.playOnAwake = playOnAwake;
			source.loop = false;
			source.priority = 128;
			source.volume = 1;
			source.panStereo = 0;
			source.spatialBlend = 0;
			source.reverbZoneMix = 1;
			source.dopplerLevel = 1;
			source.spread = 0;
			source.rolloffMode = AudioRolloffMode.Logarithmic;
			source.minDistance = 1;
			source.maxDistance = 500;
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Objects

		public static void RecordUndo(Object target, Action action, string description = "")
		{
			SetRecording(true);
			action?.Invoke();
			SetRecording(false);

			void SetRecording(bool enabled)
			{

#if UNITY_EDITOR

				if (!Application.isPlaying)
				{
					if (enabled) Undo.RecordObject(target, description);
					else Undo.FlushUndoRecordObjects();
				}

#endif

			}
		}

		/// <summary> Returns the <typeparamref name="O"/> only if it actually exists. </summary>
		/// <remarks> Operators different than the ones overloaded by Unity don't evaluate to <see langword="null"/> when the <typeparamref name="O"/> is missing or can't be resolved; this fixes that. </remarks>
		public static O ExistingObject<O>(this O obj)
			where O : Object
		{
			return obj ? obj : null;
		}

		#endregion

	}
}