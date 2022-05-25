using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableAssetPool
{
    private Transform m_parent;
    public Transform Parent => m_parent;
    private readonly Stack<PooledObject> m_pooledObjects = new Stack<PooledObject>();
    private readonly AssetReference m_assetReference;
    public string Id => m_assetReference.AssetGUID;
    private IAddressableAssetFactory m_factory;
    private bool m_isInitialized = false;
    public Action OnInitialized;

    public bool IsInitialized
    {
        get => m_isInitialized;
        private set
        {
            switch (m_isInitialized)
            {
                case true when !value:
                    throw new InvalidOperationException("Cannot uninitialize an initialized pool.");
                case false when value && OnInitialized != null:
                    OnInitialized();
                    break;
            }

            m_isInitialized = value;
        }
    }

    public AsyncOperationHandle AsyncInitializationHandle { get; private set; }

    public AddressableAssetPool(Transform parent, AssetReference reference, IAddressableAssetFactory factory,
        int size = 5)
    {
        m_parent = parent ??
                   throw new ArgumentNullException("Cannot pass a null transform for AssetPool parent transform.");
        m_assetReference = reference ?? throw new ArgumentNullException("AssetReference cannot be null for AssetPool.");
        m_factory = factory ?? throw new ArgumentNullException("IAddressableFactory cannot be null for AssetPool.");

        if (m_assetReference.Asset == null &&
            !(m_assetReference.OperationHandle.IsValid() && !m_assetReference.OperationHandle.IsDone))
        {
            AsyncInitializationHandle = m_assetReference.LoadAssetAsync<UnityEngine.Object>();
        }
        
        InitializePool(size);
    }

    private async Task InitializePool(int size)
    {
        await AsyncInitializationHandle.Task;
        if (AsyncInitializationHandle.Status == AsyncOperationStatus.Failed) throw AsyncInitializationHandle.OperationException;
        
        for (int i = 0; i < size; i++)
        {
            var item = m_factory.CreateSync(m_assetReference);
            item.SetParent(m_parent);
            item.SetPool(this);
            item.SetActive(false);
            m_pooledObjects.Push(item);
        }

        IsInitialized = true;
    }

    public void Despawn(PooledObject item)
    {
        if (item == null) return;
        item.SetActive(false);
        item.OnDespawned();
        m_pooledObjects.Push(item);
    }

    public PooledObject Spawn(Vector3 position)
    {
        if (!IsInitialized)
            throw new InvalidOperationException(
                "Cannot spawn an object before the pool is initialized. Either check to see if IsInitialized is true, or assign a callback for OnInitialized.");

        if (m_pooledObjects.Count <= 0)
        {
            var newItem = m_factory.CreateSync(m_assetReference);
            newItem.SetParent(m_parent);
            newItem.SetPool(this);
            m_pooledObjects.Push(newItem);
        }

        var instance = m_pooledObjects.Pop();
        instance.SetActive(true);
        instance.CachedTransform.position = position;
        instance.OnSpawned(position);
        return instance;
    }

    public void ReleasePool()
    {
        Addressables.Release(AsyncInitializationHandle);
        while (m_pooledObjects.Count > 0)
        {
            var item = m_pooledObjects.Pop();
            GameObject.Destroy(item.gameObject);
        }
    }
}