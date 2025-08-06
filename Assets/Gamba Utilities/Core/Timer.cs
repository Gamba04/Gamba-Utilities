using System;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    #region Serializable

    [Serializable]
    private class Request
    {
        [SerializeField, HideInInspector] private string name;

        public float timer;
        public Action action;
        public bool unscaled;
        public bool abort;

        private event Action<float> onUpdateValue;

        public Request(float timer, Action action, Action<float> onUpdateValue, bool unscaled)
        {
            this.timer = timer;
            this.action = action;
            this.unscaled = unscaled;
            this.onUpdateValue = onUpdateValue;
        }

        public void OnUpdateValue() => onUpdateValue?.Invoke(timer);

        public void SetCancelRequest(CancelRequest cancel) => cancel.onCancel += Cancel;

        private void Cancel() => abort = true;

        public void SetName(string name) => this.name = name;

    }

    [Serializable]
    public class CancelRequest
    {
        public event Action onCancel;

        public void Cancel()
        {
            onCancel?.Invoke();
            onCancel = null;
        }
    }

    #endregion

    [SerializeField]
    private List<Request> requests = new List<Request>();

    #region Singleton

    private static Timer instance = null;

    public static Timer Instance
    {
        get
        {
            if (instance == null)
            {
                var sceneResult = FindObjectOfType<Timer>();

                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else
                {
                    GameObject obj = new GameObject($"{GetTypeName(instance)}_Instance");

                    instance = obj.AddComponent<Timer>();
                }
            }

            return instance;
        }
    }

    private static string GetTypeName<T>(T obj) => typeof(T).Name;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        UpdateRequests();
        CleanRequests();
    }

    private void UpdateRequests()
    {
        foreach (Request request in requests.ToArray())
        {
            if (request.abort) continue;

            ReduceCooldownInternal(ref request.timer, request.unscaled, request.action);
            request.OnUpdateValue();
        }
    }

    private void CleanRequests() => requests.RemoveAll(request => request.timer <= 0 || request.abort);

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    #region Public

    /// <summary> Call an Action after a period of time. </summary>
    public static void CallOnDelay(Action action, float delay, string optionalName = "") => CallOnDelayInternal(action, false, delay, null, optionalName);

    /// <summary> Call an Action after a period of time, with a cancellation Action. </summary>
    public static void CallOnDelay(Action action, float delay, CancelRequest cancelRequest, string optionalName = "") => CallOnDelayInternal(action, false, delay, cancelRequest, null, optionalName);

    /// <summary> Call an Action after a period of time, with a cancellation Action. </summary>
    public static void CallOnDelay(Action action, float delay, CancelRequest cancelRequest, Action<float> onUpdateValue, string optionalName = "") => CallOnDelayInternal(action, false, delay, cancelRequest, onUpdateValue, optionalName);

    /// <summary> Call an Action after a period of unscaled time. </summary>
    public static void CallOnDelayUnscaled(Action action, float delay, string optionalName = "") => CallOnDelayInternal(action, true, delay, null, optionalName);

    /// <summary> Call an Action after a period of unscaled time, with a cancellation Action. </summary>
    public static void CallOnDelayUnscaled(Action action, float delay, CancelRequest cancelRequest, string optionalName = "") => CallOnDelayInternal(action, true, delay, cancelRequest, null, optionalName);

    /// <summary> Call an Action after a period of unscaled time, with a cancellation Action. </summary>
    public static void CallOnDelayUnscaled(Action action, float delay, CancelRequest cancelRequest, Action<float> onUpdateValue, string optionalName = "") => CallOnDelayInternal(action, true, delay, cancelRequest, onUpdateValue, optionalName);

    /// <summary> Reduce a variable over time and call an Action if it reaches 0. </summary>
    public static void ReduceCooldown(ref float value, Action endingAction = null) => ReduceCooldownInternal(ref value, false, endingAction);

    /// <summary> Reduce a variable over unscaled time and call an Action if it reaches 0. </summary>
    public static void ReduceCooldownUnscaled(ref float value, Action endingAction = null) => ReduceCooldownInternal(ref value, true, endingAction);

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Private

    private static void CallOnDelayInternal(Action action, bool unscaled, float delay, Action<float> onUpdateValue, string optionalName)
    {
        CallOnDelayInternalBase(action, unscaled, delay, null, onUpdateValue, optionalName);
    }

    private static void CallOnDelayInternal(Action action, bool unscaled, float delay, CancelRequest cancelRequest, Action<float> onUpdateValue, string optionalName)
    {
        CallOnDelayInternalBase(action, unscaled, delay, request => request.SetCancelRequest(cancelRequest), onUpdateValue, optionalName);
    }

    private static void CallOnDelayInternalBase(Action action, bool unscaled, float delay, Action<Request> requestSetup, Action<float> onUpdateValue, string optionalName)
    {
        if (delay > 0)
        {
            Request request = new Request(delay, action, onUpdateValue, unscaled);

            request.SetName(optionalName);
            requestSetup?.Invoke(request);

            Instance.requests.Add(request);
        }
        else
        {
            action?.Invoke();
        }
    }

    private static void ReduceCooldownInternal(ref float value, bool unscaled, Action endingAction)
    {
        if (value > 0)
        {
            value -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            if (value <= 0)
            {
                value = 0;

                endingAction?.Invoke();
            }
        }
        else if (value < 0)
        {
            value = 0;
        }
    }

    #endregion

    #endregion

}