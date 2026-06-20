using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TDG0407.Core.Utils
{

    public class ObjectPooler<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly IObjectPool<T> _pool;
        private readonly Transform _parent;

        public ObjectPooler(T prefab, int capacity = 10, int maxSize = 50, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            
            // 풀 생성 로직 정의
            _pool = new UnityEngine.Pool.ObjectPool<T>(
                createFunc: () => Object.Instantiate(_prefab, _parent),
                actionOnGet: (obj) => obj.gameObject.SetActive(true),
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                actionOnDestroy: (obj) => Object.Destroy(obj.gameObject),
                defaultCapacity: capacity,
                maxSize: maxSize
            );
        }

        public T Get() => _pool.Get();
        public void Release(T obj) => _pool.Release(obj);
    }
}