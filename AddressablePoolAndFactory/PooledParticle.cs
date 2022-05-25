using Sirenix.OdinInspector;
using UnityEngine;

public class PooledParticle : PooledObject
{
    private ParticleSystem m_particleSystem;
    [SerializeField] private bool hasLifetime;

    [SerializeField, ShowIf(nameof(hasLifetime))]
    private float lifetime;

    private float timer = 0;
    public ParticleSystem ParticleSystem
    {
        get
        {
            if (m_particleSystem == null)
            {
                m_particleSystem = GetComponentInChildren<ParticleSystem>(true);
            }

            return m_particleSystem;
        }
    }

    private void Update()
    {
        if (!ParticleSystem.IsAlive(true))
        {
            CachedTransform.localScale = Vector3.one;
            Despawn();
        }

        if (hasLifetime)
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                timer = 0;
                Despawn();
            }
        }
    }
}