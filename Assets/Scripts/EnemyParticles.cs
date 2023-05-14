using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyParticles : MonoBehaviour
{
    private ParticleSystem particles;

    private Vector2 startScale;
    private float scaleDownFactor = .8f;
    private bool isExplode = false;
    private Gradient grad;

    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        startScale = transform.localScale;

        grad = new Gradient();
        GradientColorKey[] keys = new GradientColorKey[2];
        keys[0].color = Color.yellow;
        keys[1].color = Color.yellow;
        grad.mode = GradientMode.Blend;

        grad.colorKeys = keys;
    }

    public void PlayParticlesDeath(Vector2 enemyPos, Color color)
    {
        // move the particles to the position of the enemy
        transform.position = enemyPos;
        transform.localScale = startScale;
		var main = particles.main;

        if (!isExplode)
            main.startColor = color;
		else
			main.startColor = new Color(255, 40, 0);
        

        particles.Play();

        Destroy(gameObject, 3f);
    }

    public void PlayParticlesHit(Vector2 enemyPos, Color color)
    {
        // move the particles to the position of the enemy
        transform.position = enemyPos;

        // make the particle size smaller
        transform.localScale = startScale * scaleDownFactor;
		var main = particles.main;
		main.startColor = color;
        particles.Play();

        // make the particle size its orig size
    }

    public void SetExplode()
    {
        isExplode = true;

        var emission = particles.emission;
        emission.rateOverTime = 200f;
    }
}
