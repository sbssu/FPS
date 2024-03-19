using UnityEngine;
using UnityEngine.Pool;

public interface IReturnPool<T> where T : Component
{
    event System.Action<T> release;
}

public class ObjectPool<T> where T : Component, IReturnPool<T>
{
    T prefab;
    IObjectPool<T> pool;
    Transform parent;

    public void Setup(GameObject owner, T prefab, int initCount)
    {
        this.prefab = prefab;
        pool = new UnityEngine.Pool.ObjectPool<T>(Create, OnTakeFromPool, OnReleaseToPool, OnDestroyObject, true, initCount);
        parent = new GameObject("pool storage").transform;
        parent.SetParent(owner.transform);
    }

    public T Get()
    {
        return pool.Get();
    }

    private T Create()
    {
        T newObject = Object.Instantiate(prefab);
        newObject.transform.SetParent(parent);
        newObject.release += (target) => pool.Release(target);      // 되돌아오는 함수 연결.
        
        return newObject;
    }
    private void OnTakeFromPool(T target)
    {
        target.gameObject.SetActive(true);
        target.transform.SetParent(null);
    }
    private void OnReleaseToPool(T target)
    {
        target.gameObject.SetActive(false);
        target.transform.SetParent(parent);
    }
    private void OnDestroyObject(T target)
    {
        Object.Destroy(target.gameObject);
    }
}

