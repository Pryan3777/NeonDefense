using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurretLaser : Turret
{
    private LineRenderer lr;
    private bool isFirstShot = true;
    private float laserRate = .5f;
    private float adjDamage;

    protected override void Start()
    {
        base.Start();

        // upgrade shit 
        upgradeDamageCost[0] = 750;
        upgradeDamageCost[1] = 1250;
        upgradeDamageCost[2] = 1750;

        upgradeDamageAmounts[0] = 10;
        upgradeDamageAmounts[1] = 15;
        upgradeDamageAmounts[2] = 25;

        lr = GetComponent<LineRenderer>();
        currentTime = Time.time + shootWaitSecond;
        startTime = Time.time;
        adjDamage = damageAmount / 50;

		LASER_COLOR = GetColor();
    }

    // Overrides the shoot method to implement the laser itself
    protected override void Shoot()
    {
        // TODO: Need to implement damaging the target when the laser is fired
        if (IsEnemyInRange())
        {
            if (currentTime - startTime > shootWaitSecond || lr.enabled || isFirstShot)
            {
                isFirstShot = false;
                lr.enabled = true;
                lr.SetPosition(0, transform.position);

                if (targetList.Count > 0 && targetList.First() != null)
                {
                    lr.SetPosition(1, targetList.First().transform.position);

                    if (targetList.First().TryGetComponent<Enemy>(out var enemy))
                    {
                        // apply the damage
                        // checks if it was a kill
                        if (enemy.damage(damageAmount / 50))
                        {
                            IncrementKills();
                        }
                    }
                }
                else
                {
                    lr.enabled = false;
                }



                startTime = Time.time;
            }
            else
            {
                lr.enabled = false;
            }


            currentTime = Time.time;
        }
        else
        {
            lr.enabled = false;
            currentTime = Time.time;
        }
    }

    void OnDestroy()
    {
        if (!playerRemoved)
            laser_turrets_killed++;
    }
}
