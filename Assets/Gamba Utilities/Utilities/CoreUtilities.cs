using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace GambaUtilities
{
    public static class CoreUtilities
    {

        #region Collections

        #region Resize

        /// <summary> Resizes the list to match the amount of values in <paramref name="enumType"/>, without deleting the existing elements. </summary>
        public static void Resize<T>(this List<T> list, Type enumType)
        {
            list.Resize(enumType, () => default);
        }

        /// <summary>
        /// Resizes the list to match the amount of values in <paramref name="enumType"/>, without deleting the existing elements. <para/>
        /// If the list size increases, the new elements will get the value of <paramref name="customDefault"/>().
        /// </summary>
        public static void Resize<T>(this List<T> list, Type enumType, Func<T> customDefault)
        {
            int length = GetEnumLength(enumType);

            list.Resize(length, customDefault);
        }

        /// <summary> Resizes the list to <paramref name="length"/>, without deleting the existing elements. </summary>
        public static void Resize<T>(this List<T> list, int length)
        {
            list.Resize(length, () => default);
        }

        /// <summary>
        /// Resizes the list to <paramref name="length"/>, without deleting the existing elements. <para/>
        /// If the list size increases, the new elements will get the value of <paramref name="customDefault"/>().
        /// </summary>
        public static void Resize<T>(this List<T> list, int length, Func<T> customDefault)
        {
            if (list.Count == length) return;

            List<T> result = new List<T>(length);

            for (int i = 0; i < length; i++)
            {
                result.Add(i < list.Count ? list[i] : customDefault());
            }

            list.Replace(result);
        }

        /// <summary> Gets the amount of values in <typeparamref name="E"/>. </summary>
        public static int GetEnumLength<E>() where E : Enum => GetEnumLength(typeof(E));

        /// <summary> Gets the amount of values in <paramref name="enumType"/>. </summary>
        public static int GetEnumLength(Type enumType) => Enum.GetValues(enumType).Length;

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

                int index = UnityEngine.Random.Range(0, pool.Count);

                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            list.Replace(result);
        }

        /// <summary> Iterates each element in an <paramref name="action"/> that includes the index. </summary>
        public static void ForEach<T>(this List<T> list, Action<T, int> action)
        {
            if (action == null) return;

            for (int i = 0; i < list.Count; i++) action(list[i], i);
        }

        /// <summary> Replaces the list contents with <paramref name="collection"/>. </summary>
        public static void Replace<T>(this List<T> list, ICollection<T> collection)
        {
            list.Clear();
            list.AddRange(collection);
        }

        /// <summary> Destroys everything in the list. </summary>
        public static void DestroyAll<O>(this List<O> list, bool includeGameObjects = true)
            where O : UnityEngine.Object
        {
            for (int i = 0; i < list.Count; i++)
            {
                UnityEngine.Object obj = list[i];

                if (includeGameObjects && obj is Component component) obj = component.gameObject;

                UnityEngine.Object.Destroy(obj);
            }

            list.Clear();
        }

        public static void Clear<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = default;
            }
        }

        #endregion

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

        #region Reflection

        public static T GetValueOfType<T>(this SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            string path = property.propertyPath.Replace("Array.data", "").Replace("]", "");
            string[] names = path.Split('.');

            object currentObject = property.serializedObject.targetObject;

            foreach (string name in names)
            {
                if (name[0] == '[') // Array
                {
                    int index = int.Parse(name.Substring(1));

                    List<object> list = new List<object>(currentObject as IEnumerable<object>);

                    currentObject = list[index];
                }
                else // Object
                {
                    FieldInfo field = GetField(currentObject.GetType(), name);

                    if (field == null) return default;

                    currentObject = field.GetValue(currentObject);
                }
            }

            return (T)currentObject;

            static FieldInfo GetField(Type type, string name)
            {
                FieldInfo field;

                do
                {
                    field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    type = type.BaseType;
                }
                while (field == null && type != null);

                return field;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Other

        /// <summary> Resets the <see cref="Transform"/> to its default values. </summary>
        public static void Reset(this Transform transform)
        {
            SetUndoRecording(true);
            ApplyReset();
            SetUndoRecording(false);

            void ApplyReset()
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

            void SetUndoRecording(bool enabled)
            {
#if UNITY_EDITOR
                if (Application.isPlaying) return;

                if (enabled) Undo.RecordObject(transform, "Reset Transform");
                else Undo.FlushUndoRecordObjects();
#endif
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------

        #region Editor

#if UNITY_EDITOR

        /// <summary> Destroys an <see cref="UnityEngine.Object"/> properly while in Edit Mode. </summary>
        /// <param name="canUndo"> Specifies whether the action can be undone in the Editor. </param>
        public static void DestroyInEditMode(UnityEngine.Object obj, bool canUndo = true)
        {
            if (Application.isPlaying) return;

            EditorApplication.delayCall += DestroyObject;

            void DestroyObject()
            {
                if (canUndo) Undo.DestroyObjectImmediate(obj);
                else
                {
                    EditorUtility.SetDirty(obj);

                    UnityEngine.Object.DestroyImmediate(obj);
                }

                EditorApplication.delayCall -= DestroyObject;
            }
        }

#endif

        #endregion

    }
}