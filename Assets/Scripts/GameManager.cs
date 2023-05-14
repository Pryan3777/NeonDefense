using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region SINGLETON
    public static GameManager Instance;
    void Awake() => Instance = this;
    #endregion

    #region Game Modes
    public static readonly int GAMEMODE_BATTLE = 0;
    public static readonly int GAMEMODE_REST = 1;
    public static readonly int GAMEMODE_LOSE = 2;
    #endregion

    #region Build Modes
    public static readonly int BUILDMODE_NONE = 0;
    public static readonly int BUILDMODE_WALL = 1;
    public static readonly int BUILDMODE_TURRET = 2;
    public static readonly int BUILDMODE_REMOVE = 3;
    public static readonly int BUILDMODE_BRIDGE = 4;
    #endregion

    public static readonly KeyCode KEY_DEBUG_PANEL = KeyCode.F4;
    public static readonly KeyCode KEY_PAUSE_MENU = KeyCode.Escape;

    [SerializeField] private int buildMode = BUILDMODE_NONE;
    [SerializeField] private int gameMode = GAMEMODE_REST;

    [SerializeField] private Player player; // NEW

    [SerializeField] private int playerBuildRadius = 3; // NEW

    private int turretTypeSelected = Turret.TURRET_DEFAULT;
	private int gridUpdate;

    private int currentRound = 0;
    private float currentBaseHealth = 100; // default for now!! we can move this if we want.
    private float maxBaseHealth = 100; // default for now!! we can move this if we want.
    private bool playerLostRegistered = false; // so the loss state is only called once
    private bool newRound = false;
    private bool fireModeEnabled = false;
    private bool roundStatsEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        gridUpdate = 0;
    }

    // Update is called once per frame
    void Update()
    {
        

        // IDEA: Input manager maybe later?
        // handles the user input for switching between building / turrets etc
        if (fireModeEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Player.Instance.SetGunType(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Player.Instance.SetGunType(1);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // set to wall build mode
                SetBuildMode(BUILDMODE_WALL);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_WALL;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // set to bridge build mode
                SetBuildMode(BUILDMODE_BRIDGE);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_BRIDGE;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                // set to turret build mode with designated turret
                SetBuildMode(BUILDMODE_TURRET);
                SetTurretType(Turret.TURRET_DEFAULT);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_TURRET_DEFAULT;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                // set to turret build mode with designated turret
                SetBuildMode(BUILDMODE_TURRET);
                SetTurretType(Turret.TURRET_BURST);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_TURRET_BURST;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                // set to turret build mode with designated turret
                SetBuildMode(BUILDMODE_TURRET);
                SetTurretType(Turret.TURRET_LASER);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_TURRET_LASER;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                // set to turret build mode with designated turret
                SetBuildMode(BUILDMODE_TURRET);
                SetTurretType(Turret.TURRET_MORTAR);

                // store the index (more readable code for next line)
                var index = UIManager.BUTTON_INDEX_TURRET_MORTAR;

                // show this as the current placeable selected in the UI
                UIManager.Instance.SelectButtonPlaceables(index);
            }
        }
        

        // toggle fire mode  : )
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UIManager.Instance.TogglePlayerMode();
        }

        // View the debug panel
        if (Input.GetKeyDown(KEY_DEBUG_PANEL))
            UIManager.Instance.ToggleDebugPanel();

        if (Input.GetKeyDown(KEY_PAUSE_MENU))
            UIManager.Instance.TogglePauseMenu();

        // clear multifill
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            GridManager.Instance.SetIsInMultiPlace(false);
            GridManager.Instance.ClearMultiFill();
        }

        if (roundStatsEnabled && Input.GetMouseButtonDown(0))
        {
            UIManager.Instance.HideRoundStatsPanel();
            roundStatsEnabled = false;
        }

        if (Input.GetKeyDown(KeyCode.H))
            UIManager.Instance.ToggleQuickHelp();

        #region OLD CODE
        /*
        // Temporary way to switch between the 'Build Modes'
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetBuildMode(BUILDMODE_WALL);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetBuildMode(BUILDMODE_TURRET);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetBuildMode(BUILDMODE_REMOVE);
        }


        // Temporary way to switch between the 'Turret to be placed'
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetTurretType(Turret.TURRET_DEFAULT);
        }
        else if (Input.GetKeyDown(KeyCode.X)) 
        {
            SetTurretType(Turret.TURRET_LASER);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            SetTurretType(Turret.TURRET_BURST);
        }
        */

        #endregion
    }

    public void SetBuildMode(int buildMode)
    {
        this.buildMode = buildMode;
    }

    public int GetBuildMode()
    {
        return buildMode;
    }

    public void SetGameMode(int gameMode)
    {
        this.gameMode = gameMode;

        if (gameMode == GAMEMODE_BATTLE)
        {
            EnemyManager.Instance.RoundStart(); // starts spawning shit
        }
    }

    public int GetGameMode()
    {
        return gameMode;
    }

    public void SetTurretType(int turretType)
    {
        turretTypeSelected = turretType;
    }

    public int GetTurretType()
    {
        return turretTypeSelected;
    }

    // NEW
    public int GetPlayerBuildRadius()
    {
        return playerBuildRadius;
    }

    // NEW : GRID DISTANCE!
    public double GetDistanceFromPlayer(Vector2 gridPos)
    {
        var playerPos = player.GetGridPos();

        var xDist = Mathf.Abs(gridPos.x - playerPos.x);
        var yDist = Mathf.Abs(gridPos.y - playerPos.y);

        if (xDist == 0)
        {
            return yDist;
        }
        else if (yDist == 0)
        {
            return xDist;
        }

        return Mathf.Max(xDist, yDist);
    }
	
	public int getUpdate()
	{
		return gridUpdate;
	}
	
	public void setUpdate()
	{
		gridUpdate++;
		return;
	}
     
    public void IncrementRound()
    {
        Enemy.PrintHeatMap();
        if (currentRound++ > 2)
            Player.IncrementBulletDamage();
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public float SetHealth(float amount)
    {
        currentBaseHealth = amount;

        if (currentBaseHealth > maxBaseHealth)
            currentBaseHealth = maxBaseHealth;
        if (currentBaseHealth < 0)
        {
            currentBaseHealth = 0;
            // player LOSES : : : DEATH()
        }

        UIManager.Instance.SetHealth(currentBaseHealth);

        return currentBaseHealth;
    }

    public float AddHealth(float amount)
    {
        currentBaseHealth += amount;

        if (currentBaseHealth > maxBaseHealth)
            currentBaseHealth = maxBaseHealth;

        UIManager.Instance.AddHealth(amount);

        return currentBaseHealth;
    }

    public float RemoveHealth(float amount)
    {
        currentBaseHealth -= amount;

        if (currentBaseHealth < 0)
        {
            currentBaseHealth = 0;
            PlayerLose();
        }

        UIManager.Instance.RemoveHealth(amount);

        return currentBaseHealth;
    }

    private void PlayerLose()
    {
        if (playerLostRegistered)
            return;

        playerLostRegistered = true;

        // maybe pause the game as it is ... effects and shit (think WASTED in GTA when u die)

        GameOver();
    }

    private void GameOver()
    {
        // slow down the game
        // effects?
        //HealthBarUI.Instance.gameObject.SetActive(false);
        UIManager.Instance.GameOver();
    }

    public void RestartGame()
    {
        ResumeGame();
        // this will reset any stats and stuff necessary but for now just reload..
        SceneManager.LoadScene("main");
    }

    public void ReturnToMainMenu()
    {
        // this will reset any stats and stuff necessary but for now just reload..
        SceneManager.LoadScene("menu");
        ResumeGame();
    }

    public bool IsPaused()
    {
        return UIManager.Instance.IsPaused();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public bool IsNewRound()
    {
        return newRound;
    }

    public bool IsFireModeEnabled()
    {
        return fireModeEnabled;
    }

    public void ToggleFireModeEnabled()
    {
        fireModeEnabled = !fireModeEnabled;
    }

    public void SetRoundStatsEnabled(bool isEnabled)
    {
        roundStatsEnabled = isEnabled;
    }

    public bool GetRoundStatsEnabled()
    {
        return roundStatsEnabled;
    }


}
