using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    #region SINGLETON
    public static UIManager Instance;
    void Awake() => Instance = this;
    #endregion

    public static int BUTTON_INDEX_WALL = 0;
    public static int BUTTON_INDEX_BRIDGE = 1;
    public static int BUTTON_INDEX_TURRET_DEFAULT = 2;
    public static int BUTTON_INDEX_TURRET_BURST = 3;
    public static int BUTTON_INDEX_TURRET_LASER = 4;
    public static int BUTTON_INDEX_TURRET_MORTAR = 5;

    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Text[] buttonTexts;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;

    [SerializeField] private Text bulletsInSceneText;
    [SerializeField] private Text moneyLabel;
    [SerializeField] private Text roundLabel;

    [SerializeField] private GameObject debugPanel;

    [SerializeField] private Button startRoundButton;

    [SerializeField] private GameObject roundStatsPanel;
    [SerializeField] private GameObject roundStatsHeader;
    [SerializeField] private Text enemiesKilled;
    [SerializeField] private Text shotsFired;
    [SerializeField] private Text roundNumber;

    [SerializeField] private HealthBar healthBar;

    [SerializeField] private Image gameOverPanel;
    [SerializeField] private MaskableGraphic[] gameOverPanelGraphics;

    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private Animator roundNumberAnimation;
    [SerializeField] private GameObject turretsKilledPanel;
    [SerializeField] private TurretIcon turretIcon;

    // player weapons / items to place system
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private GameObject weaponPanel;
    // smg
    [SerializeField] private Image smgImage;
    [SerializeField] private Text smgText;
    // shotgun
    [SerializeField] private Image shotgunImage;
    [SerializeField] private Text shotgunText;

    [SerializeField] private Color IconColorActive;
    [SerializeField] private Color IconColorInactive;
    [SerializeField] private Color TextColorActive;
    [SerializeField] private Color TextColorInactive;

    [SerializeField] private GameObject quickHelpMenu;

    private bool isOverGUI = false;

    private void Start()
    {
        // Make the item costs in the gui reflect the chosen amounts programatically
        UpdateItemCostTexts();

        // makes the game over hidden invisible at the start
        InitGameOverHidden();
        roundNumberAnimation.Play("Base Layer.Rest");
        roundLabel.text = "Round " + (GameManager.Instance.GetCurrentRound() + 1).ToString();
    }

    public void SelectTurret(int turretType)
    {
        // set the build mode to turret since the user selected a turret to place
        GameManager.Instance.SetBuildMode(GameManager.BUILDMODE_TURRET);
        GameManager.Instance.SetTurretType(turretType);
    }

    public void SelectWall()
    {
        GameManager.Instance.SetBuildMode(GameManager.BUILDMODE_WALL);
    }

    public void SelectBridge()
    {
        GameManager.Instance.SetBuildMode(GameManager.BUILDMODE_BRIDGE);
    }

    // change the color of the button to show which is currently selected (avoided built-in selection because that sucked)
    public void SelectButtonPlaceables(Image buttonImage)
    {
        // loop through the buttons, disable 
        foreach (Image image in buttonImages)
        {
            // if the image is the one we've selected, change the color
            if (image == buttonImage)
            {
                image.color = selectedColor;
            }
            // otherwise we want to change the others to their original color
            else
            {
                image.color = normalColor;
            }
        }
    }

    // change the color of the button to show which is currently selected (BASED ON BUTTON INDICES, which are constants) 
    public void SelectButtonPlaceables(int buttonIndex)
    {
        // loop through the buttons, disable 
        for (int i = 0; i < buttonImages.Length; i++)
        {
            var image = buttonImages[i];

            // if the image is the one we've selected, change the color
            if (i == buttonIndex)
            {
                image.color = selectedColor;
            }
            // otherwise we want to change the others to their original color
            else
            {
                image.color = normalColor;
            }
        }
    }

    public void SetBulletsInScene(int count)
    {
        bulletsInSceneText.text = "bullet count: " + count.ToString();
    }

    // toggle the debug panel being visible
    public void ToggleDebugPanel()
    {
        var isActive = debugPanel.activeSelf;

        debugPanel.SetActive(!isActive);
    }

    // toggles the button that is passed in 
    public void ToggleStartRoundButton(bool active)
    {
        // toggle active of the button
        startRoundButton.enabled = active;
        startRoundButton.gameObject.SetActive(active);
    }

    public void UpdateMoneyLabel(int newAmount)
    {
        moneyLabel.text = "Money: " + newAmount.ToString();
    }

    // update the item costs programmatically
    private void UpdateItemCostTexts()
    {
        foreach (Text t in buttonTexts)
        {
            // get the itemUI object from the parent
            // if it exists, we will update this text to reflect the cost of that
            var item = t.GetComponentInParent<ItemUI>();

            if (item != null)
            {
                t.text = item.GetItemCost();
            }
        }
    }

    public void ChangeWeaponType(int gunType)
    {
        Player.Instance.SetGunType(gunType);
        UpdateWeaponPanel(gunType);
    }

    public void UpdateWeaponPanel(int gunType)
    {
        // change the colors of the text/sprite for the active/inactive one
        if (gunType == 0)
        {
            StylizeWeaponIcon(smgImage, smgText, true);
            StylizeWeaponIcon(shotgunImage, shotgunText, false);
        } else if (gunType == 1)
        {
            StylizeWeaponIcon(smgImage, smgText, false);
            StylizeWeaponIcon(shotgunImage, shotgunText, true);
        }
    }

    private void StylizeWeaponIcon(Image image, Text text, bool isActive)
    {
        if (isActive)
        {
            image.color = IconColorActive;
            text.color = TextColorActive;
        } else
        {
            image.color = IconColorInactive;
            text.color = TextColorInactive;
        }
    }

    public void TogglePlayerMode()
    {
        GameManager.Instance.ToggleFireModeEnabled();
        bool enabled = GameManager.Instance.IsFireModeEnabled();
        UpdateWeaponPanel(Player.Instance.GetGunType());

        if (enabled)
        {
            itemPanel.SetActive(false);
            weaponPanel.SetActive(true);
        }
        else
        {
            itemPanel.SetActive(true);
            weaponPanel.SetActive(false);
        }
    }

    public void ShowRoundStatsPanel()
    {
        // grab the stats
        // maybe move to a stats manager?
        var killed = Enemy.GetNumEnemiesKilled();
        var shots = Bullet.GetBulletsFired();
        GameManager.Instance.SetRoundStatsEnabled(true);
        // update the text
        enemiesKilled.text = "Enemies Killed: " + killed.ToString();
        shotsFired.text = "Bullets Fired: " + shots.ToString();
        roundNumber.text = "ROUND " + GameManager.Instance.GetCurrentRound().ToString() +" STATS";

        roundStatsPanel.SetActive(true);
        roundStatsHeader.SetActive(true);
        roundLabel.text = "Round " + (GameManager.Instance.GetCurrentRound() + 1).ToString();
        roundNumberAnimation.Play("Base Layer.Round Number");

        AddTurretsKilled();
    }

    private void AddTurretsKilled()
    {
        // destroy the icons from the previous round
        foreach (Transform icon in turretsKilledPanel.transform)
            Destroy(icon.gameObject);

        int basic = Turret.basic_turrets_killed;
        int burst = Turret.burst_turrets_killed;
        int laser = Turret.laser_turrets_killed;
        int mortar = Turret.mortar_turrets_killed;


        if (basic > 0)
        {
            var icon = Instantiate(turretIcon, turretsKilledPanel.transform);
            icon.Init(basic, Turret.DEFAULT_COLOR);
        }

        if (burst > 0)
        {
            var icon = Instantiate(turretIcon, turretsKilledPanel.transform);
            icon.Init(burst, Turret.BURST_COLOR);
        }

        if (laser > 0)
        {
            var icon = Instantiate(turretIcon, turretsKilledPanel.transform);
            icon.Init(laser, Turret.LASER_COLOR);
        }

        if (mortar > 0)
        {
            var icon = Instantiate(turretIcon, turretsKilledPanel.transform);
            icon.Init(mortar, Turret.MORTAR_COLOR);
        }

        // reset the number of turrets destroyed after every round
        Turret.ResetTurretsDestroyed();
    }

    public void HideRoundStatsPanel()
    {
        roundStatsPanel.SetActive(false);
        roundStatsHeader.SetActive(false);
        GameManager.Instance.SetRoundStatsEnabled(true);

    }

    // for the health bar
    public void AddHealth(float amount)
    {
        healthBar.AddHealth(amount);
    }

    public void RemoveHealth(float amount)
    {
        healthBar.RemoveHealth(amount);
    }

    public void SetHealth(float amount)
    {
        healthBar.SetHealth(amount);
    }

    private void InitGameOverHidden()
    {
        var gP = gameOverPanel.color;

        gameOverPanel.color = new Color(gP.r, gP.g, gP.b, 0);

        
        // loop through the objects in the menu
        foreach (MaskableGraphic g in gameOverPanelGraphics)
        {
            var gT = g.color;

            g.color = new Color(gT.r, gT.g, gT.b, 0);
        }
        
    }

    public void GameOver()
    {
        gameOverPanel.gameObject.SetActive(true);

        FadeColorToVisible(gameOverPanel, 1f);

        // loop through the objects in the menu
        foreach (MaskableGraphic g in gameOverPanelGraphics)
        {
            FadeColorToVisible(g, 2f);
        }
    }

    private void FadeColorToVisible(MaskableGraphic graphic, float fadeTime)
    {
        StartCoroutine(FadeToVisible(graphic, fadeTime));
    }

    private IEnumerator FadeToVisible(MaskableGraphic graphic, float fadeTime)
    {
        float elapsedTime = 0;
        Vector4 origCol = graphic.color;
        Vector4 targetCol = new Vector4(origCol.x, origCol.y, origCol.z, 1);

        // NOTE: Can add check to get player to move on walls only
        while (elapsedTime < fadeTime)
        {
            graphic.color = Vector4.Lerp(origCol, targetCol, (elapsedTime / fadeTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        graphic.color = targetCol;
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame();
        AddTurretsKilled();
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    public void ShowPauseMenu()
    {
        GameManager.Instance.PauseGame();
        pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        GameManager.Instance.ResumeGame();
        pauseMenu.SetActive(false);
    }

    public void TogglePauseMenu()
    {
        var paused = !pauseMenu.activeSelf;

        pauseMenu.SetActive(paused);

        if (paused)
            GameManager.Instance.PauseGame();
        else
            GameManager.Instance.ResumeGame();
    }

    public bool IsPaused()
    {
        return pauseMenu.activeSelf;
    }

    public void ToggleQuickHelp()
    {
        quickHelpMenu.SetActive(!quickHelpMenu.activeSelf);
    }
}
