using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] private float BULLET_FORCE = 50.0f;
    protected Rigidbody2D rb;
    private Transform _bulletHolder;
    public static int bulletsInScene = 0;
    [SerializeField] protected float damageAmount = 0;
    private static int bulletsFired = 0;
    protected SpriteRenderer sprite;
    protected float startTime;
    protected Turret turretFrom;
    protected string effect = "None";
    protected float poisonDamage = .1f;
    private bool isFade = false;
    private Vector2 firedFromPos = Vector2.zero;
	private bool isPlayer = false;
    private float maxDistance = 15;
    private float fadeRate = 0.015f;
    private int gunType = 0;

    public void Init(float damageAmount, Color color, Turret turretFrom, string effect)
    {
        this.turretFrom = turretFrom;
        this.damageAmount = damageAmount;
        this.effect = effect;

        sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.color = color;
    }

    public void Init(float damageAmount, float bulletSpeed, Color color, int gunType, bool player = false)
    {
        this.damageAmount = damageAmount;
        BULLET_FORCE = bulletSpeed;

        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = color;

        isFade = true;
		if(player)
		{
			isPlayer = true;
		}

        //shotgun
        if (gunType == 1)
        {
            this.gunType = gunType;
            maxDistance = 2f;
            fadeRate = 0.040f;
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        bulletsInScene++;
        bulletsFired++;
        UpdateCountText();
        startTime = Time.time;
        
        _bulletHolder = GridManager.Instance.GetBulletHolder();

        if (_bulletHolder != null)
            transform.SetParent(_bulletHolder);

        firedFromPos = transform.position;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        // fixes bug where bullet stays still
        if (Time.fixedTime - startTime > 3 && rb.velocity == Vector2.zero)
        {
            Destroy(gameObject);
        }

        if (isFade && sprite != null && sprite.color.a > .01f)
        {
            var col = sprite.color;
            sprite.color = new Color(col.r, col.g, col.b, col.a - fadeRate);
        }

        // if shotgun bullet out of range, destroy it
        if (gunType == 1 && Vector2.Distance(transform.position, firedFromPos) > maxDistance)
            Destroy();
    }

    // Given a target's position, shoot the bullet towards it
    public void ShootTowards(Transform tran)
    {
        var direction = (Vector2)tran.position - (Vector2)this.transform.position;
        rb.AddForce(direction.normalized * BULLET_FORCE);
    }
	
	public void ShootTowards(Vector2 vec)
    {
        var direction = vec - (Vector2)this.transform.position;
        rb.AddForce(direction.normalized * BULLET_FORCE);
    }

    public void ShootTowards(Vector2 vec, Vector2 playerPos)
    {
        var direction = vec - playerPos;
        rb.AddForce(direction.normalized * BULLET_FORCE);
    }

    public void ShootTowards(Vector2 vec, Vector2 playerPos, float angle)
    {
    
        rb.AddForce(vec.normalized * BULLET_FORCE);
    }

    public virtual void Destroy()
    {
        bulletsInScene--;
        UpdateCountText();
        Destroy(gameObject);
    }

    private void UpdateCountText()
    {
        UIManager.Instance.SetBulletsInScene(bulletsInScene);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            // checks that the object has the enemy script
            if (col.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                // apply the damage
                // checks if it was a kill
                if (isFade)
                {
                    var distance = Vector2.Distance(transform.position, firedFromPos);
                    var damageModifier = 1f;

                    if (gunType == 0)
                        damageModifier = 1 - distance * distance / maxDistance;

                    else if (gunType == 1)
                        damageModifier = 1 - distance / maxDistance;

                    if (damageModifier < 0)
                        damageModifier = 0.1f;

                    damageAmount *= damageModifier;
                }

                if (!enemy.damage(damageAmount, isPlayer))
                {
                    if (turretFrom != null)
                        IncrementKills();
                }

                if (effect == "Freeze")
                {
                    enemy.Freeze(turretFrom.GetFreezeDuration());
                }
                else if (effect == "Poison")
                {
                    enemy.Poison(turretFrom.GetPoisonDuration(), poisonDamage);
                }
            }

            AudioManager.Instance.PlaySound(AudioManager.BULLET_SOUND);

            // changed to private Destroy method.
            Destroy();
        }
    }

    public static int GetBulletsFired()
    {
        return bulletsFired;
    }

    public static void ResetBulletsFired()
    {
        bulletsFired = 0;
    }

    protected void IncrementKills()
    {
        turretFrom.IncrementKills();
    } 

    public bool IsStuck()
    {
        return rb.velocity == Vector2.zero;
    }
}
