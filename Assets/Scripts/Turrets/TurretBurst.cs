using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurretBurst : Turret
{
    [SerializeField] private int burstNumber = 3; // number of bullets in the burst
    [SerializeField] private float timeBetweenShots = .5f;

    protected override void Start()
    {
        base.Start();
        BURST_COLOR = GetColor();
        // hard code ui color 
        color = new Color32(153, 0, 255, 195);

        // upgrade shit 
        upgradeDamageCost[0] = 500;
        upgradeDamageCost[1] = 625;
        upgradeDamageCost[2] = 750;

        upgradeDamageAmounts[0] = 5;
        upgradeDamageAmounts[1] = 10;
        upgradeDamageAmounts[2] = 15;
    }

    // shoot burstNumber of times, waiting timeBetweenShots seconds to fire
    private IEnumerator ShootBurst()
    {
		Debug.Log("Shooting");
		Vector2 tr = new Vector2(0.0f, 0.0f);
        if(targetList.Count > 0 && targetList[0] != null)tr = (Vector2)targetList[0].transform.position;
		
        for (int i = 0; i < burstNumber; i++)
        {
			if(tr != new Vector2(0.0f, 0.0f))
			{
				Debug.Log("Shooting " + i);
                var tempBullet = Instantiate(bullet, transform.position, Quaternion.identity);
                tempBullet.Init(damageAmount, color, this, effect);
                tempBullet.ShootTowards(tr);

				yield return new WaitForSeconds(timeBetweenShots); // wait till the next round
			}
			else if(targetList.Count > 0)
            {
                targetList.Clear();
            }
        }
    }


    protected override void Shoot()
    {
        // same as base turret
        if (IsEnemyInRange() && currentTime - startTime > shootWaitSecond)
        {
            // begin the burst as a coroutine
            StartCoroutine(ShootBurst());
            //StartCoroutine(Waiter());
            startTime = Time.time;
        }


        currentTime = Time.time;
    }

    void OnDestroy()
    {
        if (!playerRemoved)
            burst_turrets_killed++;
    }


    private IEnumerator Waiter()
    {
        var waitTime = Random.Range(0f, 1f);
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ShootBurst());
    }
}
