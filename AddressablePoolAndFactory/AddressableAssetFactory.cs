using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class AddressableAssetFactory : PlaceholderFactory<AssetReference, PooledObject>, IAddressableAssetFactory
{
    [Inject] private DiContainer m_container;
    private Dictionary<string, PooledObject> m_assetDict = new Dictionary<string, PooledObject>();

    public PooledObject CreateSync(AssetReference assetReference)
    {
        // if we have already loaded this asset before, instantiate it
        if (m_assetDict.TryGetValue(assetReference.AssetGUID, out var asset))
        {
            return m_container.InstantiatePrefabForComponent<PooledObject>(asset);
        }
        
        // if the asset is not loaded, use the async load but wait for completion for synchronous workflow
        if (assetReference.Asset == null)
        {
            var handle = assetReference.LoadAssetAsync<UnityEngine.Object>();
            handle.WaitForCompletion();
            if (handle.Status == AsyncOperationStatus.Failed) throw handle.OperationException;
        }

        return InstantiatePrefabForComponent(assetReference);
    }

    public async Task<PooledObject> CreateAsync(AssetReference assetReference)
    {
        // if we have already loaded this asset before, instantiate it
        if (m_assetDict.TryGetValue(assetReference.AssetGUID, out var asset))
        {
            return m_container.InstantiatePrefabForComponent<PooledObject>(asset);
        }
        
        // if the asset is not loaded, use the async load and await for async workflow
        if (assetReference.Asset == null)
        {
            var handle = assetReference.LoadAssetAsync<UnityEngine.Object>();
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Failed) throw handle.OperationException;
        }
        
        return InstantiatePrefabForComponent(assetReference);
    }
    
    private PooledObject InstantiatePrefabForComponent(AssetReference assetReference)
    {
        var po = (assetReference.Asset as GameObject)?.GetComponent<PooledObject>();
        if (po == null)
            throw new System.Exception(
                $"Asset Reference {assetReference.Asset.name} is either not a GameObject or does not have a component deriving from PooledObject.");
        m_assetDict.Add(assetReference.AssetGUID, po);
        return m_container.InstantiatePrefabForComponent<PooledObject>(assetReference.Asset);
    }
}