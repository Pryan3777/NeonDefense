using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMortar : Turret
{
    protected override void Start()
    {
        base.Start();

        // upgrade shit 
        upgradeDamageCost[0] = 1000;
        upgradeDamageCost[1] = 1500;
        upgradeDamageCost[2] = 2000;

        upgradeDamageAmounts[0] = 5;
        upgradeDamageAmounts[1] = 10;
        upgradeDamageAmounts[2] = 15;

        MORTAR_COLOR = GetColor();
    }

    void OnDestroy()
    {
        if (!playerRemoved)
            mortar_turrets_killed++;
    }
}
