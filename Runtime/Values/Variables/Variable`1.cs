namespace GenericScriptableArchitecture
{
    using System;
    using System.Collections.Generic;
    using GenericUnityObjects;
    using SolidUtilities;
    using UnityEngine;
    using Object = UnityEngine.Object;

#if UNIRX
    using UniRx;
#endif

    [Serializable]
    [CreateGenericAssetMenu(FileName = "New Variable", MenuName = Config.PackageName + "Variable")]
    public class Variable<T> : BaseVariable, IVariable<T>
    {
        public IEqualityComparer<T> EqualityComparer = _defaultEqualityComparer;
        private static readonly IEqualityComparer<T> _defaultEqualityComparer = UnityEqualityComparer.GetDefault<T>();

        [SerializeField] internal T _initialValue;
        [SerializeField] internal T _value;
        [SerializeField] internal bool ListenersExpanded;

        internal EventHelperWithDefaultValue<T> _eventHelper;

        public T InitialValue => _initialValue;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer.Equals(_value, value))
                    SetValue(value);
            }
        }

        internal override List<Object> Listeners => _eventHelper?.Listeners ?? ListHelper.Empty<Object>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _eventHelper = new EventHelperWithDefaultValue<T>(this, () => _value);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _eventHelper.Dispose();
        }

        public void SetValueAndForceNotify(T value) => SetValue(value);

        #region Adding Removing Listeners

        public void AddListener(IListener<T> listener, bool notifyCurrentValue = false) => _eventHelper.AddListener(listener, notifyCurrentValue);

        public void RemoveListener(IListener<T> listener) => _eventHelper.RemoveListener(listener);

        public void AddListener(Action<T> listener, bool notifyCurrentValue = false) => _eventHelper.AddListener(listener, notifyCurrentValue);

        public void RemoveListener(Action<T> listener) => _eventHelper.RemoveListener(listener);

        #endregion

        protected override void InitializeValues()
        {
            _value = SerializedCopyInEditor(_initialValue);
        }

        protected virtual void SetValue(T value)
        {
            _value = value;
            AddStackTrace(_value);
            InvokeValueChangedEvents();
        }

        internal override void InvokeValueChangedEvents()
        {
            if ( ! CanBeInvoked())
                return;

            _eventHelper.NotifyListeners(_value);
        }

        #region Operator Overloads

        public static implicit operator T(Variable<T> variable) => variable.Value;

        public override string ToString() => $"Variable{{{Value}}}";

        public bool Equals(IVariable<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _value.Equals(other.Value);
        }

        public bool Equals(T other)
        {
            if (ReferenceEquals(_value, other))
                return true;

            return Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj is IVariable<T> typedObj)
                return Equals(typedObj);

            if (obj is T tObj)
                return Equals(tObj);

            return false;
        }

        /// <summary>
        /// Use with caution. The value contained by a Variable instance can be changed through inspector.
        /// </summary>
        /// <returns>Hash code of the instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _value?.GetHashCode() ?? 0;
                return hash;
            }
        }

        public static bool operator ==(Variable<T> lhs, Variable<T> rhs)
        {
            if ((Object)lhs == null)
            {
                return (Object)rhs == null;
            }

            return lhs.Equals((IVariable<T>)rhs);
        }

        public static bool operator !=(Variable<T> lhs, Variable<T> rhs)
        {
            return ! (lhs == rhs);
        }

        public static bool operator ==(Variable<T> lhs, T rhs)
        {
            if ((Object)lhs == null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Variable<T> lhs, T rhs)
        {
            return ! (lhs == rhs);
        }

        public static Variable<T> operator +(Variable<T> variable, Action<T> listener)
        {
            if (variable == null)
                return null;

            variable.AddListener(listener);
            return variable;
        }

        public static Variable<T> operator +(Variable<T> variable, (Action<T> Listener, bool NotifyCurrentValue) args)
        {
            if (variable == null)
                return null;

            variable.AddListener(args.Listener, args.NotifyCurrentValue);
            return variable;
        }

        public static Variable<T> operator -(Variable<T> variable, Action<T> listener)
        {
            if (variable == null)
                return null;

            variable.RemoveListener(listener);
            return variable;
        }

        public static Variable<T> operator +(Variable<T> variable, IListener<T> listener)
        {
            if (variable == null)
                return null;

            variable.AddListener(listener);
            return variable;
        }

        public static Variable<T> operator +(Variable<T> variable, (IListener<T> Listener, bool NotifyCurrentValue) args)
        {
            if (variable == null)
                return null;

            variable.AddListener(args.Listener, args.NotifyCurrentValue);
            return variable;
        }

        public static Variable<T> operator -(Variable<T> variable, IListener<T> listener)
        {
            if (variable == null)
                return null;

            variable.RemoveListener(listener);
            return variable;
        }

        #endregion

#if UNIRX
        bool IReadOnlyReactiveProperty<T>.HasValue => true;

        public IDisposable Subscribe(IObserver<T> observer) => _eventHelper.Subscribe(observer);
#endif
    }
}