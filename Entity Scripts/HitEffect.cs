using System.Collections;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ParticleSystem hitParticles;

    [Header("Hit Effect Settings")]
    public Color hitColor = Color.red;
    public float hitDuration = 0.2f;

    [Header("Particle Effect Settings")]
    public bool enableParticles = true;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Try to find existing HIT particle system, if not create one
        hitParticles = transform.Find("HitParticles")?.GetComponent<ParticleSystem>();
        if (hitParticles == null && enableParticles)
        {
            CreateHitParticles();
        }
    }

    private void CreateHitParticles()
    {
        GameObject particleObject = new GameObject("HitParticles");
        particleObject.transform.SetParent(transform);
        particleObject.transform.localPosition = Vector3.zero;

        hitParticles = particleObject.AddComponent<ParticleSystem>();

        // Configure particle system for sword hit effect
        var main = hitParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 8f;
        main.startSize = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.3f, 1f); // Orange/yellow sparks
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Shape - burst outward
        var shape = hitParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // Emission - burst on hit
        var emission = hitParticles.emission;
        emission.enabled = false; // We'll trigger manually

        // Velocity over lifetime - sparks fly outward then fall
        var velocityOverLifetime = hitParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(5f, 10f);

        // Gravity
        var forceOverLifetime = hitParticles.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = -15f;

        // Size over lifetime - shrink
        var sizeOverLifetime = hitParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime - fade out
        var colorOverLifetime = hitParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        // Material - use default sprite material for bright sparks
        var renderer = hitParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void PlayHitEffect()
    {
        // Flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(HitFlash());
        }

        // Particle effect
        if (hitParticles != null && enableParticles)
        {
            hitParticles.Emit(15); // Emit 15 spark particles
        }
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColor;
    }
}