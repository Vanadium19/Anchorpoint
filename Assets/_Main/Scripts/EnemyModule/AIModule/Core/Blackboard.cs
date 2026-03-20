using System;
using System.Collections.Generic;

namespace EnemyModule
{
    public class Blackboard
    {
        private readonly Dictionary<Type, object> _values = new();
        private readonly Dictionary<BlackboardTag, object> _objectValues = new();

#region Objects

        public bool DeleteValue<T>(BlackboardTag key) where T : struct => GetBucket<T>().Remove(key);

        public void SetObject(BlackboardTag key, object value) => _objectValues[key] = value;

        public bool HasObject(BlackboardTag key) => _objectValues.ContainsKey(key);

        public T GetObject<T>(BlackboardTag key) => (T)_objectValues[key];

        public bool TryGetObject<T>(BlackboardTag key, out T value)
        {
            if (_objectValues.TryGetValue(key, out var result))
            {
                value = (T)result;
                return true;
            }

            value = default;
            return false;
        }

        public bool DeleteObject(BlackboardTag key) => _objectValues.Remove(key);

#endregion

#region Values

        public void SetValue<T>(BlackboardTag key, T value) where T : struct => GetBucket<T>()[key] = value;

        public bool HasValue<T>(BlackboardTag key) where T : struct => GetBucket<T>().ContainsKey(key);

        public T GetValue<T>(BlackboardTag key) where T : struct => GetBucket<T>()[key];

        public bool TryGetValue<T>(BlackboardTag key, out T value) where T : struct
        {
            var bucket = GetBucket<T>();

            if (bucket.TryGetValue(key, out var result))
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }

        private Dictionary<BlackboardTag, T> GetBucket<T>() where T : struct
        {
            var type = typeof(T);

            if (_values.TryGetValue(type, out var boxed))
                return (Dictionary<BlackboardTag, T>)boxed;

            var dictionary = new Dictionary<BlackboardTag, T>();
            _values[type] = dictionary;
            return dictionary;
        }

#endregion
    }
}
