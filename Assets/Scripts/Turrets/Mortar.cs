using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : Bullet
{
    private List<GameObject> targetList = new List<GameObject>();
    private BoxCollider2D boxColl;
    private CircleCollider2D circleColl;
    [SerializeField] private float blastRadius = 1;


    protected override void Start()
    {
        base.Start();

        // grab the colliders
        boxColl = GetComponent<BoxCollider2D>();
        circleColl = GetComponent<CircleCollider2D>();

        // disable the box collider
        boxColl.enabled = false;
    }

    protected override void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            // TODO: NEED TO ADD FUNCTIONALITY OF BULLET DISAPPEARING
            // AND ADD THE FUNCTIONALITY OF DAMAGING ALL ENEMIES IN TARGETLIST

            // disable components besides collider
            // disable the circle collider
            DisablePhysics();

            // move the gameobject to the center of the tile
            var enemyPos = col.gameObject.transform.position;

            // enable the box collider

            var layerMask = LayerMask.GetMask("Enemy");

            var enemyRoundedPos = GridManager.RoundToTileInWorld(enemyPos);

            Collider2D[] enemyColliders = Physics2D.OverlapBoxAll(enemyRoundedPos, new Vector2(blastRadius, blastRadius), 0, layerMask);

            // get enemies in box collider
            // affect enemies in the box collider
            // destroy mortar object

            AudioManager.Instance.PlaySound(AudioManager.MORTAR_SOUND);
            // col.GetComponent<Enemy>().damage(damageAmount);


            foreach (Collider2D coll in enemyColliders)
            {
                // damage here

                // checks that the object has the enemy script
                if (coll.gameObject.TryGetComponent<Enemy>(out var enemy))
                {
                    // apply the damage
                    // checks if it was a kill
                    if (enemy != null && enemy.damage(damageAmount))
                    {
                        IncrementKills();
                    }
                }
            }

            Destroy();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        // Debug.Log(col.gameObject);
        targetList.Remove(col.gameObject);
    }

    protected virtual void DisablePhysics()
    {
        // disable the movement of the mortar
        rb.simulated = false;
        circleColl.enabled = false;
    }

    private Vector2 CenterAt(Vector2 pos)
    {
        return GridManager.RoundToTileInWorld(pos);
    }
}
