# AddressablePools
A generalized system to create object pools directly from Addressable AssetReferences. Includes a Zenject factory implementation as well.

`**AddressableAssetPool**` is the primary pooling mechanism. It takes an `**AssetReference**`, initial size, factory implementaiton of your choice, and a transform parent. Make sure it is initialized before spawning objects. You can check initialization status via `**AsyncInitializationHandle**`, or `**IsInitialized**` property. Alternatively you can also assign a callback method to `**OnInitialized**`.

The asset you want to pool needs to have a `**PooledObject**` monobehaviour component. This is the component that tracks what pool the object is instantiated under, and handles spawning and despawning mechanisms.

If you want any other components on the asset to execute logic during spawn and despawn phases, you can implement the `**IPoolable**` interface for those behaviours. 

`**AddressableAssetFactory**` is a sample implementation of the `**IAddressableAssetFactory**` interface, written for use along Zenject/Extenject Dependency Injection framework.

`**PooledParticle**` is an example class derived from **PooledObject**. When attached to particle systems, it allows the particle system to be returned to the pool when the effect has stopped playing or after a specified amount of time.
