using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDefault : Turret
{
    protected override void Start()
    {
        base.Start();
        isEffectUpgradeable = true;
        DEFAULT_COLOR = GetColor();
    }

    void OnDestroy()
    {
        if (!playerRemoved)
            basic_turrets_killed++;
    }
}
