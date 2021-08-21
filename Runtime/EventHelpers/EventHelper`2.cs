﻿namespace GenericScriptableArchitecture
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SolidUtilities.Extensions;
    using Object = UnityEngine.Object;

    public class EventHelper<T1, T2> : IEventHelper<T1, T2>, IDisposable
    {
        private readonly IEvent<T1, T2> _parentEvent;
        private readonly List<ScriptableEventListener<T1, T2>> _scriptableListeners = new List<ScriptableEventListener<T1, T2>>();
        private readonly List<IEventListener<T1, T2>> _singleEventListeners = new List<IEventListener<T1, T2>>();
        private readonly List<IMultipleEventsListener<T1, T2>> _multipleEventsListeners = new List<IMultipleEventsListener<T1, T2>>();
        private readonly List<Action<T1, T2>> _responses = new List<Action<T1, T2>>();

        public List<Object> Listeners => _responses
            .Select(response => response.Target)
            .Concat(_scriptableListeners)
            .Concat(_singleEventListeners)
            .Concat(_multipleEventsListeners)
            .OfType<Object>()
            .ToList();

        public EventHelper() { }

        public EventHelper(IEvent<T1, T2> parentEvent)
        {
            _parentEvent = parentEvent;
        }

        public void AddListener(IListener<T1, T2> listener)
        {
            if (listener == null)
                return;

            if (listener is ScriptableEventListener<T1, T2> scriptableListener)
            {
                _scriptableListeners.Add(scriptableListener);
            }
            else if (listener is IEventListener<T1, T2> eventListener)
            {
                _singleEventListeners.AddIfMissing(eventListener);
            }
            else if (listener is IMultipleEventsListener<T1, T2> multipleEventsListener)
            {
                _multipleEventsListeners.AddIfMissing(multipleEventsListener);
            }
        }

        public void RemoveListener(IListener<T1, T2> listener)
        {
            if (listener is ScriptableEventListener<T1, T2> scriptableListener)
            {
                _scriptableListeners.Remove(scriptableListener);
            }
            else if (listener is IEventListener<T1, T2> eventListener)
            {
                _singleEventListeners.Remove(eventListener);
            }
            else if (listener is IMultipleEventsListener<T1, T2> multipleEventsListener)
            {
                _multipleEventsListeners.Remove(multipleEventsListener);
            }
        }

        public void AddListener(Action<T1, T2> listener)
        {
            if (listener == null)
                return;

            _responses.AddIfMissing(listener);
        }

        public void RemoveListener(Action<T1, T2> listener) => _responses.Remove(listener);

        public void NotifyListeners(T1 arg0, T2 arg1)
        {
            for (int i = _scriptableListeners.Count - 1; i != -1; i--)
            {
                _scriptableListeners[i].OnEventInvoked(arg0, arg1);
            }

            for (int i = _singleEventListeners.Count - 1; i != -1; i--)
            {
                _singleEventListeners[i].OnEventInvoked(arg0, arg1);
            }

            for (int i = _multipleEventsListeners.Count - 1; i != -1; i--)
            {
                _multipleEventsListeners[i].OnEventInvoked(_parentEvent ?? this, arg0, arg1);
            }

            for (int i = _responses.Count - 1; i != -1; i--)
            {
                _responses[i].Invoke(arg0, arg1);
            }

#if UNIRX
            _observableHelper?.RaiseOnNext((arg0, arg1));
#endif
        }

        #region UniRx
#if UNIRX
        private ObservableHelper<(T1, T2)> _observableHelper;

        public IDisposable Subscribe(IObserver<(T1, T2)> observer)
        {
            _observableHelper ??= new ObservableHelper<(T1, T2)>();
            return _observableHelper.Subscribe(observer);
        }
#endif

        public void Dispose()
        {
#if UNIRX
            _observableHelper?.Dispose();
#endif
        }
        #endregion
    }
}