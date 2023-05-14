using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    #region SINGLETON
    public static ItemView Instance;
    void Awake() => Instance = this;
    #endregion

    [SerializeField] private Image preview;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Text maxHealth;
    [SerializeField] private Text maxHealthLabel;
    [SerializeField] private Text damage;
    [SerializeField] private Text damageLabel;
    [SerializeField] private Text effect;
    [SerializeField] private Text effectLabel;
    [SerializeField] private Text kills;
    [SerializeField] private Text killsLabel;
    [SerializeField] private GameObject itemViewPanel;
    [SerializeField] private Text damageUpgrade;
    [SerializeField] private Text poisonUpgrade;
    [SerializeField] private Text freezeUpgrade;
    [SerializeField] private GameObject damagePanel;
    [SerializeField] private GameObject poisonPanel;
    [SerializeField] private GameObject freezePanel;
    [SerializeField] private Text damageBuy;
    [SerializeField] private Text poisonBuy;
    [SerializeField] private Text freezeBuy;
    private Turret turret;
    private Wall wall;

    // Start is called before the first frame update
    void Start()
    {
        SetInactive();
    }

    // Update is called once per frame
    void Update()
    {
        if (wall != null)
        {
            var health = wall.GetHealth();
            var max = wall.GetMaxHealth();

            healthBar.SetHealth(health);
            healthBar.SetMaxHealth(max);
            maxHealth.text = health.ToString() + "/" + max.ToString();
            preview.color = Color.white;
        }

        if (turret != null)
        {
            preview.color = turret.GetColor();

            damage.text = turret.GetDamageString();
            // effect from turret
            var eff = GetTurretEffectString();

            UpdateUpgradePanel(turret.GetTurretEffect());
            effect.text = eff;

            kills.text = turret.GetKills().ToString();

            UpdateBuyPrices();
        }

    }

    private void UpdateBuyPrices()
    {
        damageBuy.text = "Buy (" + turret.GetUpgradeDamageCost() + ")";
        freezeBuy.text = "Buy (" + turret.GetUpgradeFreezeCost() + ")";
        poisonBuy.text = "Buy (" + turret.GetUpgradePoisonCost() + ")";
    }

    public void SetInactive()
    {
        itemViewPanel.SetActive(false);
    }

    private void UpdateUpgradePanel(string effect)
    {
        var damageAmount = turret.GetUpgradeDamageAmount();
        var freezeAmount = turret.GetUpgradeFreezeAmount();
        var poisonAmount = turret.GetUpgradePoisonAmount();

        damageUpgrade.text = string.Format("Damage (+{0})", damageAmount);
        freezeUpgrade.text = string.Format("Freeze (+{0}s)", freezeAmount.ToString("n2"));
        poisonUpgrade.text = string.Format("Poison (+{0}s)", poisonAmount.ToString("n2"));

        UpdateEffectLabel();

        // no effects, we want user to be able to buy either
        if (turret.IsEffectUpgradeable() && effect == "No Effect")
        {
            poisonPanel.SetActive(true);
            freezePanel.SetActive(true);
        }
        else if (effect == "Poison")
        {
            effectLabel.text = effect;
            poisonPanel.SetActive(true);
            freezePanel.SetActive(false);
        }
        else if (effect == "Freeze")
        {
            effectLabel.text = effect;
            poisonPanel.SetActive(false);
            freezePanel.SetActive(true);
        }
        else
        {
            poisonPanel.SetActive(false);
            freezePanel.SetActive(false);
        }

        if (damageAmount == -1)
            damagePanel.SetActive(false);

        if (freezeAmount == -1)
            freezePanel.SetActive(false);

        if (poisonAmount == -1)
            poisonPanel.SetActive(false);
    }

    public void UpdateView(Turret turret, Wall wall)
    {
        this.wall = wall;
        this.turret = turret;

        if (turret == null && wall == null)
        {
            itemViewPanel.SetActive(false);
            return;
        }

        itemViewPanel.SetActive(true);
        // showing wall
        if (turret == null)
        {
            damage.gameObject.SetActive(false);
            damagePanel.gameObject.SetActive(false);
            freezePanel.gameObject.SetActive(false);
            poisonPanel.gameObject.SetActive(false);
            damageLabel.gameObject.SetActive(false);
            effect.gameObject.SetActive(false);
            effectLabel.gameObject.SetActive(false);
            kills.gameObject.SetActive(false);
            killsLabel.gameObject.SetActive(false);
        }
        // showing turret
        else
        {
            damageUpgrade.text = string.Format("Damage (+{0})", turret.GetUpgradeDamageAmount());
            freezeUpgrade.text = string.Format("Freeze (+{0}s)", turret.GetUpgradeFreezeAmount().ToString("n1"));
            poisonUpgrade.text = string.Format("Poison (+{0}s)", turret.GetUpgradePoisonAmount().ToString("n1"));

            damage.gameObject.SetActive(true);
            damagePanel.gameObject.SetActive(true);
            freezePanel.gameObject.SetActive(true);
            poisonPanel.gameObject.SetActive(true);
            damageLabel.gameObject.SetActive(true);
            effect.gameObject.SetActive(true);
            effectLabel.gameObject.SetActive(true);
            kills.gameObject.SetActive(true);
            killsLabel.gameObject.SetActive(true);
            UpdateEffectLabel();

        }
    }

    private string GetTurretEffectString()
    {
        if (turret != null)
            return turret.GetTurretEffectString();

        return "";
    }

    public void UpgradeDamage()
    {
        if (turret != null)
            turret.UpgradeDamage();
    }

    public void UpgradeFreeze()
    {
        if (turret != null)
            // user already has an effect upgrade on this turret
            if (turret.UpgradeFreeze())
                poisonPanel.SetActive(false);

        // clean up panel, remove unnecessary
    }

    public void UpgradePoison()
    {
        if (turret != null)
            if (turret.UpgradePoison())
                freezePanel.SetActive(false);

        // clean up panel, remove unnecessary
    }

    private void UpdateEffectLabel()
    {
        effectLabel.text = turret.GetTurretEffect() + ":";
    }
}
