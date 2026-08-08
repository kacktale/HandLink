using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyDefeatEffects : MonoBehaviour
{
    private const int InitialPoolSize = 16;
    private const int ParticleCount = 12;
    private const string ParticleMaterialResourcePath = "Materials/EnemyDefeatParticle";

    private readonly List<ParticleSystem> pool = new List<ParticleSystem>(InitialPoolSize);
    private Material particleMaterial;

    private void Awake()
    {
        particleMaterial = Resources.Load<Material>(ParticleMaterialResourcePath);
        if (particleMaterial == null)
        {
            Debug.LogError(
                $"Missing particle material at Resources/{ParticleMaterialResourcePath}.",
                this);
            enabled = false;
            return;
        }

        for (int index = 0; index < InitialPoolSize; index++)
        {
            pool.Add(Create());
        }
    }

    private void Update()
    {
        foreach (ParticleSystem effect in pool)
        {
            if (effect.gameObject.activeSelf && !effect.IsAlive(true))
            {
                effect.gameObject.SetActive(false);
            }
        }
    }

    public void Play(Vector3 position, Color color)
    {
        ParticleSystem effect = Rent();
        ParticleSystem.MainModule main = effect.main;
        main.startColor = color;
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.Play(true);
    }

    private ParticleSystem Rent()
    {
        foreach (ParticleSystem effect in pool)
        {
            if (!effect.gameObject.activeSelf)
            {
                return effect;
            }
        }

        ParticleSystem expandedEffect = Create();
        pool.Add(expandedEffect);
        return expandedEffect;
    }

    private ParticleSystem Create()
    {
        GameObject effectObject = new GameObject("EnemyDefeatParticle", typeof(ParticleSystem));
        effectObject.transform.SetParent(transform, false);
        ParticleSystem effect = effectObject.GetComponent<ParticleSystem>();
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = effect.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.05f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 2.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.11f);
        main.maxParticles = ParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = effect.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, ParticleCount) });

        ParticleSystem.ShapeModule shape = effect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.03f;
        shape.randomDirectionAmount = 1f;

        ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = particleMaterial;
        renderer.sortingOrder = 10;

        effectObject.SetActive(false);
        return effect;
    }
}
