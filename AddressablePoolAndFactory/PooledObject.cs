using System;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private Transform m_cachedTransform;
    private AddressableAssetPool m_pool;
    private Dictionary<Type, Component> cachedComponents = new Dictionary<Type, Component>();
    private IPoolable<Vector3>[] m_poolables;
    [SerializeField] private bool destroyIfNotPooled = true;

    private void Awake()
    {
        m_poolables = GetComponentsInChildren<IPoolable<Vector3>>(true);
    }

    public new T GetComponent<T>() where T : Component
    {
        if (cachedComponents.TryGetValue(typeof(T), out Component comp))
        {
            if (comp == null)
            {
                cachedComponents.Remove(typeof(T));
                return null;
            }
            else
            {
                return (T)comp;
            }
        }
        else
        {
            var result = base.GetComponent<T>();
            cachedComponents.Add(typeof(T), result);
            return result;
        }
    }

    public Transform CachedTransform
    {
        get
        {
            if (m_cachedTransform == null)
            {
                m_cachedTransform = transform;
            }

            return m_cachedTransform;
        }
    }

    public void SetParent(Transform parent)
    {
        CachedTransform.SetParent(parent);
    }

    public void SetPool(AddressableAssetPool pool)
    {
        m_pool = pool;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void OnDespawned()
    {
        foreach (var poolable in m_poolables)
        {
            poolable.OnDespawned();
        }
    }

    public void OnSpawned(Vector3 position)
    {
        foreach (var poolable in m_poolables)
        {
            poolable.OnSpawned(position);
        }
    }

    public void Despawn()
    {
        if (m_pool == null && destroyIfNotPooled)
        {
            Destroy(gameObject);
            return;
        }

        m_pool?.Despawn(this);
    }
}