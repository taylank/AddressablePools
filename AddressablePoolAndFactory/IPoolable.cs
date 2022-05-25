public interface IPoolable<in TParam1>
{
    void OnDespawned();
    void OnSpawned(TParam1 p1);
}