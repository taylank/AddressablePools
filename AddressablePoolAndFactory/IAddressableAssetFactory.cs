using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public interface IAddressableAssetFactory
{
    PooledObject CreateSync(AssetReference assetReference);
    Task<PooledObject> CreateAsync(AssetReference assetReference);
}