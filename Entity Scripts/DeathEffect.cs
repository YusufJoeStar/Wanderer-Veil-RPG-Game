using System.Collections;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private ParticleSystem dissolveParticles;
    private Collider2D col;

    [Header("Death Effect Settings")]
    public float deathDuration = 3f;
    public float fadeStartDelay = 0.8f;
    public float particleLaunchDelay = 0.5f;
    public float launchForce = 50f;
    public Color dissolveColor = new Color(0.8f, 0.8f, 1f, 1f);

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        dissolveParticles = transform.Find("DissolveParticles")?.GetComponent<ParticleSystem>();
        if (dissolveParticles == null)
        {
            CreateDissolveParticles();
        }
    }

    private void CreateDissolveParticles()
    {
        GameObject particleObject = new GameObject("DissolveParticles");
        particleObject.transform.SetParent(transform);
        particleObject.transform.localPosition = Vector3.zero;

        dissolveParticles = particleObject.AddComponent<ParticleSystem>();


        var main = dissolveParticles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.12f);
        main.startColor = dissolveColor;
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = false;


        var shape = dissolveParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        if (spriteRenderer != null)
        {
            shape.scale = spriteRenderer.bounds.size;
        }
        else
        {
            shape.scale = Vector3.one;
        }


        var emission = dissolveParticles.emission;
        emission.enabled = false;


        var velocityOverLifetime = dissolveParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;


        AnimationCurve launchCurve = new AnimationCurve();
        launchCurve.AddKey(0f, 0f);
        launchCurve.AddKey(0.2f, 0f);
        launchCurve.AddKey(0.3f, 30f);
        launchCurve.AddKey(1f, 60f);

        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(5f, 15f); // Random between 5-15 upward
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);


        var sizeOverLifetime = dissolveParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.8f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(0.4f, 1.2f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);


        var colorOverLifetime = dissolveParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(dissolveColor, 0.0f),
                new GradientColorKey(Color.white, 0.2f),
                new GradientColorKey(dissolveColor, 0.3f),
                new GradientColorKey(new Color(0.5f, 0.5f, 1f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.2f),
                new GradientAlphaKey(0.8f, 0.4f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;


        var noise = dissolveParticles.noise;
        noise.enabled = true;
        noise.strength = 0.8f;
        noise.frequency = 1f;
        noise.damping = true;


        var renderer = dissolveParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));


        Texture2D circleTexture = CreateCircleTexture(32);
        renderer.material.mainTexture = circleTexture;


        dissolveParticles.Stop();
    }


    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {

                    float alpha = 1f - (distance / radius);
                    alpha = Mathf.Pow(alpha, 2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    public void StartDeathEffect()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {

        if (col != null)
        {
            col.enabled = false;
        }


        var playerMovement = GetComponent<PlayerMovement>();
        var enemyMovement = GetComponent<EnemyMove>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }


        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Changed from linearVelocity
            rb.isKinematic = true;
        }


        if (dissolveParticles != null)
        {
            var emission = dissolveParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 40f;
            dissolveParticles.Play();
        }


        yield return new WaitForSeconds(fadeStartDelay);


        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float fadeTime = deathDuration - fadeStartDelay;
            float timer = 0;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }


            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }


        if (dissolveParticles != null)
        {
            var emission = dissolveParticles.emission;
            emission.enabled = false;
        }


        yield return new WaitForSeconds(4f);

        Destroy(gameObject);
    }
}