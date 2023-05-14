using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Item
{
    private Transform _wallHolder;
    // starting health. adjustable
    [SerializeField] private float health = 100.0f;
    private float maxHealth;

    // Start is called before the first frame update
    void Start()
    {
        maxHealth = health;
        _wallHolder = GridManager.Instance.GetWallHolder();

        if (_wallHolder != null)
            transform.SetParent(_wallHolder);
    }

    // returns true if wall is destroyed
    public bool Damage(float damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Destroy();
            return true;
        }
        return false;
    }

    private void Destroy()
    {
		GameManager.Instance.setUpdate();
        Destroy(gameObject);
    }

    public float GetHealth()
    {
        return health;
    }
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
