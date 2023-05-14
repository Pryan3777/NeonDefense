using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// SUPERCLASS FOR ALL TURRETS

public abstract class Turret : Item
{
    public static int TURRET_DEFAULT = 0;
    public static int TURRET_BURST = 1;
    public static int TURRET_LASER = 2;
    public static int TURRET_MORTAR = 3;

    public static Color DEFAULT_COLOR = Color.black;
    public static Color BURST_COLOR = Color.black;
    public static Color LASER_COLOR = Color.black;
    public static Color MORTAR_COLOR = Color.black;
    public static Color FREEZE_COLOR = new Color32(0, 50, 255, 255); // blue
    public static Color POISON_COLOR = new Color32(72, 255, 0, 255); // green

    public static int basic_turrets_killed = 0;
    public static int burst_turrets_killed = 0;
    public static int laser_turrets_killed = 0;
    public static int mortar_turrets_killed = 0;

    protected Transform _turretHolder;
    // FIXME: Need to adjust datatype from GameObject to Enemy
    protected List<GameObject> targetList = new List<GameObject>();
    [SerializeField] protected Bullet bullet;
    [SerializeField] protected float shootWaitSecond;
    protected float currentTime;
    protected float startTime;
    protected int numKills = 0;

    [SerializeField] protected Color color;
    [SerializeField] protected float damageAmount;

    protected string effect = "No Effect";
    protected float freezeDuration = 0;
    protected float poisonDuration = 0;
    private static int DURATION_START_TIME = 1; // start the durations at 1 second


    protected int damageLevel = 0;
    protected int freezeLevel = 0;
    protected int poisonLevel = 0;
    protected int maxUpgradeLevel = 3;

    // damage upgrades 
    protected int[] upgradeDamageCost = {100, 125, 150};
    protected int[] upgradeDamageAmounts = {15, 20, 25};

    // damage upgrades 
    protected int[] upgradeFreezeCost = {250, 375, 500};
    protected float[] upgradeFreezeAmounts = {1, .25f, .5f};

    // damage upgrades
    protected int[] upgradePoisonCost = {250, 375, 500};
    protected float[] upgradePoisonAmounts = {1, .25f, .5f};

    protected bool isEffectUpgradeable = false;
    protected bool firedThisRound = false;
    protected bool playerRemoved = true;

    public static float UPGRADE_RATIO = 1.2f;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        _turretHolder = GridManager.Instance.GetTurretHolder();

        if (_turretHolder != null)
            transform.SetParent(_turretHolder);

        currentTime = Time.time;
        startTime = Time.time;

        // store the color of this turret for other scripts
        color = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    protected void Update()
    {
        Shoot();
    }

    // Shoot a bullet towards our target if we detect one and\
    // We waited enough to fire
    protected virtual void Shoot()
    {
        if (IsEnemyInRange() && currentTime - startTime > shootWaitSecond)
        {
            if (targetList.Count > 0)
            {
                Transform tr = null;
                if (targetList.Count > 0 && targetList[0] != null) tr = targetList[0].transform;

                if (targetList.Count > 0 && tr != null)
                {
                    var tempBullet = Instantiate(bullet, transform.position, Quaternion.identity);
                    tempBullet.Init(damageAmount, color, this, effect);
                    tempBullet.ShootTowards(tr);
                }
                else
                {
                    targetList.Clear();
                }
            }

            startTime = Time.time;
        }

        currentTime = Time.time;
    }

    protected bool IsEnemyInRange()
    {
        return targetList.Count > 0;
    }

    // Listener event that adds an enemy to our targetList if 
    // it enters the turret range
    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            targetList.Add(col.gameObject);
        }
    }

    // Listener event that removes an enemy to our targetList if 
    // it leaves the turret range
    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Enemy")
            targetList.Remove(col.gameObject);
    }

    public Color GetColor()
    {
        return color;
    }

    public float GetDamage()
    {
        return damageAmount;
    }

    public string GetDamageString()
    {
        return damageAmount + " (" + damageLevel + "/" + maxUpgradeLevel + ")";
    }

    public int GetKills()
    {
        return numKills;
    }

    public void IncrementKills()
    {
        numKills++;
    }

    public void UpgradeDamage()
    {
        if (damageLevel > 2) return;

        if (!EconomyManager.Instance.UpgradeDamage(this)) return;

        damageAmount += GetUpgradeDamageAmount();
        damageLevel++;
    }

    public bool UpgradeFreeze()
    {
        if (freezeLevel > 2) return false;

        if (!EconomyManager.Instance.UpgradeFreeze(this)) return false;

        if (effect == "Poison") return false;

        freezeDuration += GetUpgradeFreezeAmount();
        effect = "Freeze";
        ChangeColor(FREEZE_COLOR);
        freezeLevel++;

        return true;
    }

    public bool UpgradePoison()
    {
        if (poisonLevel > 2) return false;

        if (!EconomyManager.Instance.UpgradePoison(this)) return false;

        if (effect == "Freeze") return false;

        poisonDuration += GetUpgradePoisonAmount();
        effect = "Poison";
        ChangeColor(POISON_COLOR);
        poisonLevel++;

        return true;
    }

    public string GetTurretEffect()
    {
        return effect;
    }

    public string GetTurretEffectString()
    {
        if (effect == "Freeze")
            return freezeDuration.ToString("n2") + "s " + "(" + freezeLevel + "/" + maxUpgradeLevel+")";
        else if (effect == "Poison")
            return poisonDuration.ToString("n2") + "s " + "(" + poisonLevel + "/" + maxUpgradeLevel + ")";
        else 
            return "";
    }

    public float GetFreezeDuration()
    {
        return freezeDuration;
    }

    public float GetPoisonDuration()
    {
        return poisonDuration;
    }

    public int GetUpgradeDamageCost()
    {
        if (damageLevel < 3)
            return upgradeDamageCost[damageLevel];

        return -1;
    }

    public int GetUpgradeFreezeCost()
    {
        if (freezeLevel < 3)
            return upgradeFreezeCost[freezeLevel];

        return -1;
    }

    public int GetUpgradePoisonCost()
    {
        if (poisonLevel < 3)
            return upgradePoisonCost[poisonLevel];

        return -1;
    }

    public int GetUpgradeDamageAmount()
    {
        if (damageLevel < 3)
            return upgradeDamageAmounts[damageLevel];

        return -1;
    }

    public float GetUpgradeFreezeAmount()
    {
        if (freezeLevel < 3)
            return upgradeFreezeAmounts[freezeLevel];

        return -1;
    }

    public float GetUpgradePoisonAmount()
    {
        if (poisonLevel < 3)
            return upgradePoisonAmounts[poisonLevel];

        return -1;
    }

    public bool IsEffectUpgradeable()
    {
        return isEffectUpgradeable;
    }

    public void ResetFiredThisRound()
    {
        firedThisRound = false;
    }

    public bool GetFiredThisRound()
    {
        return firedThisRound;
    }

    public static void ResetTurretsDestroyed()
    {
        basic_turrets_killed = 0;
        burst_turrets_killed = 0;
        laser_turrets_killed = 0;
        mortar_turrets_killed = 0;
    }

    public void SetPlayerRemoved(bool playerRemoved)
    {
        this.playerRemoved = playerRemoved;
    }

    protected void ChangeColor(Color col)
    {
        color = col;
        GetComponent<SpriteRenderer>().color = col;
    }
}
