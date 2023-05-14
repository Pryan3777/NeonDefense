using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{	
    private static int enemiesKilled = 0; // added to keep track if all enemies killed
    private static int enemiesAlive = 0; // added to keep track if all enemies killed
	private float Speed;
	private float Health;
	private int state;
	private int wallPenalty;
	public bool gridChange;
	private bool flag;
	private float secondsPerAttack;
	private float timer;
	private float health;
	private float maxHealth;
	private bool attacking;
	private int countedUpdates;
	private int holePenalty;
	private Rigidbody2D rb;
	private int speedStat;
	private int healthStat;
	private int bruteStat;
	private float damagePerHit;
	public Color color;
	public float hpMult;
	public float playerDamageMult;
	[SerializeField] public int newSpeedStat;
	[SerializeField] public int newHealthStat;
	[SerializeField] public int newBruteStat;
	[SerializeField] private bool isSplitter;
	[SerializeField] private bool isExplode;
	
	private Vector2 gridDim;
	private Vector2 currentLocation;
	private Vector2 nextLocation;
	private Vector2 targetLocation;
	public bool isCounted;
	
	private Vector2 tempPlayerStart;

    [SerializeField] private int enemyKillReward;
    private float enemyDamageToBase = 10; // default just a value for testing ! 
    [SerializeField] private GameObject particles;
	private EnemyParticles enemyParticles;
    private ParticleSystem exploderParticles;
    [SerializeField] private HealthBar healthBar;
    private HealthBar hb;
    private bool isFrozen = false;
    private bool isPoisoned = false;
    private bool deathLatch = false;
    private float poisonDamage = 0;
    private float updateTimer = .5f;
    private IEnumerator freezeCoroutine;
    private IEnumerator poisonCoroutine;

    // [Calculated Distance from Start,
    //  Absolute Distance from End,
    //  Penalty,
    //  Score]
    private Dictionary<Vector2, int[]> map;
	private List<Vector2> path;
    private static Dictionary<Vector2, int> heatmap = new Dictionary<Vector2, int>();
    private Dictionary<Vector2, bool> visited = new Dictionary<Vector2, bool>(); // whether or not enemy has visited this (x,y) tile

    // Start is called before the first frame update
    void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		map = null;
        currentLocation = GridManager.WorldToGrid(gameObject.transform.position);
		nextLocation = GridManager.WorldToGrid(gameObject.transform.position);
		targetLocation = new Vector2(33, 7);
		gridChange = false;
		gridDim = new Vector2(34, 16);
		wallPenalty = 100;
		holePenalty = 10000000;
		secondsPerAttack = 1.0f;
		timer = 0.0f;
		health = 100.0f;
		flag = false;
		countedUpdates = 0;
		playerDamageMult = 1.0f;

        particles = Instantiate(particles);
        enemyParticles = particles.GetComponent<EnemyParticles>();
        exploderParticles = GetComponent<ParticleSystem>();
		speedStat = 3;
		bruteStat = 1;
		healthStat = 3;
		damagePerHit = 30.0f;
		isCounted = true;
		GetComponent<SpriteRenderer>().color = color;

        // set the health bar parent to the UI canvas
        hb = Instantiate(healthBar, OffScreenLoader.Instance.transform); // load off screen at first
        enemiesAlive++;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHeatMap();

        if(speedStat != newSpeedStat) setSpeedStat(newSpeedStat);
		if(healthStat != newHealthStat) setHealthStat(newHealthStat);
		if(bruteStat != newBruteStat) setBruteStat(newBruteStat);

		if(flag == false)
		{
			var vec = new Vector2(gridDim.x - 1, gridDim.y - 1);
			if(GridManager.Instance.GetTileAtPosition(vec) != null)
			{
				flag = true;
				updateDictionary();
			}
		}
		
		if(flag)
        {
			if(isFrozen)return;
			float moveMult = 1.0f;
			timer -= Time.deltaTime;
			if(timer <= 0)
			{
                int round = GameManager.Instance.GetCurrentRound();

                if (countedUpdates != GameManager.Instance.getUpdate() && updateTimer <= 0 && bruteStat == 1 && round % 2 != 0)
				{
					countedUpdates = GameManager.Instance.getUpdate();
					gridChange = true;
                    

                    // progressive
                    // later on... players will not notice the changes in the pathing

                    if (round < 6)
                        updateTimer = .05f;
                    else if (round < 10)
                        updateTimer = .1f;
                    else if (round < 15)
                        updateTimer = .2f;
                    else
                        updateTimer = .3f;
                }
				if(attacking)
				{
					if(GridManager.Instance.GetTileAtPosition(nextLocation).ContainsWall())
					{
						if(GridManager.Instance.GetTileAtPosition(nextLocation).DamageWall(damagePerHit))
						{
							if(GridManager.Instance.GetTileAtPosition(nextLocation).ContainsTurret())
							{
								GridManager.Instance.GetTileAtPosition(nextLocation).RemoveTurret(false);
							}
							attacking = false;
							timer = getSecondsPerBlock();
						}
					}
					else
					{
						attacking = false;
						timer = getSecondsPerBlock();
					}
				}					
				else 
				{
					currentLocation = nextLocation;
					if(gridChange)
					{
						gridChange = false;
						updateDictionary();
					}
					nextLocation = getNext();
					timer = nextLocation.x == -1 ? 1000 : GridManager.Instance.GetTileAtPosition(nextLocation).ContainsWall() ? secondsPerAttack * moveMult : getSecondsPerBlock() *  moveMult;
					attacking = nextLocation.x == -1 ? false : GridManager.Instance.GetTileAtPosition(nextLocation).ContainsWall();
					if(nextLocation.x == -1)
					{/*
						var startVec = new Vector2(2,2);
						gameObject.transform.position = GridManager.GridToWorld(startVec);
						currentLocation = startVec;
						nextLocation = startVec;
						gridChange = true;
						timer = 0;
                        */
					}
				}
			}
			else
			{
				// Debug.Log("Move/Attack");
				if(attacking == false)
				{
					gameObject.transform.position = ((timer / (getSecondsPerBlock() * moveMult)) * GridManager.GridToWorld(currentLocation)) + ((1.0f - (timer / (getSecondsPerBlock() * moveMult))) * GridManager.GridToWorld(nextLocation));
				}
				else
				{
					
				}
			}
		}
        updateTimer -= Time.deltaTime;

        // POISON DAMAGE
        if (isPoisoned)
        {
            damage(maxHealth / 100);
        }
    }

    private void OnDestroy()
    {
        // update num enemies alive
        if(isCounted) enemiesAlive--;
        enemiesKilled++;
    }

    private void UpdateHeatMap()
    {
        // we havent visited yet, increment the heatmap now that we are at this pos
        if (!visited.ContainsKey(currentLocation))
        {
            visited[currentLocation] = true;

            if (!heatmap.ContainsKey(currentLocation))
                heatmap[currentLocation] = 1;
            else 
                heatmap[currentLocation]++;
        }
    }

    public static void PrintHeatMap()
    {
        string map = "";

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 34; j++)
            {
                var pos = new Vector2(j, i);

                if (!heatmap.ContainsKey(pos))
                    map += "0 ";
                else
                    map += (heatmap[pos]) + " ";
            }
            map += "\n";
        }

        Debug.Log(map);
    }

    public static Dictionary<Vector2, int> GetHeatMap()
    {
        return heatmap;
    }

    public void setSpeedStat(int a)
	{
		speedStat = a;

	}
	public void setHealthStat(int a)
	{
		healthStat = a;
		health = getHealth();
		maxHealth = health;
        hb.Init(this, health); // ADDED to update healthbar stats
    }
	public void setBruteStat(int a)
	{
		countedUpdates--;
		bruteStat = a;
	}
	public void setExplode(bool a)
	{
		isExplode = a;
	}
	public void setSplitter(bool a)
	{
		isSplitter = a;
	}
	private float getSecondsPerBlock() // 1 - 5
	{
		switch(speedStat)
		{
			case 1:
			return 0.7f;
			//break;
			case 2:
			return 0.6f;
			//break;
			case 3:
			return 0.5f;
			//break;
			case 4:
			return 0.4f;
			//break;
			case 5:
			return 0.2f;
			//break;
		}
		return .20f;
	}
	private float getHealth() // 1 - 5
	{
		switch(healthStat)
		{
			case 1:
			return 20.0f * hpMult;
			break;
			case 2:
			return 30.0f * hpMult;
			break;
			case 3:
			return 50.0f * hpMult;
			break;
			case 4:
			return 100.0f * hpMult;
			break;
			case 5:
			return 300.0f * hpMult;
			break;
		}
		return 50.0f;
	}
	private int getWallPenalty() // 1 - 5
	{
		switch(bruteStat)
		{
			case 1:
			return 1000;
			break;
			case 2:
			return 100;
			break;
			case 3:
			return 50;
			break;
			case 4:
			return 1;
			break;
			case 5:
			return 0;
			break;
		}
		return 10000;
	}
	
	private Vector2 getNext()
	{
		if(path.Count == 0)
		{
			GameManager.Instance.RemoveHealth(34.0f);
			damage(1000000000.0f);
			var vec = new Vector2(-1, -1);
			return vec;
		}
		var cur = path[0];
		path.RemoveAt(0);
		return cur;
	}
	
	private void printList()
	{
		//Debug.Log("LIST LEN: " + path.Count);
		for(int x = 0; x < path.Count; x++)
		{
			//Debug.Log("PATH TO: " + path[x].x + " " + path[x].y + " " + map[path[x]][3]);
			GridManager.Instance.GetTileAtPosition(path[x]).SetWall();
		}
	}
	
	public bool damage(float dmg, bool player = false)
	{
		if(player)
		{
			dmg = dmg * playerDamageMult;
			playerDamageMult = playerDamageMult * (1.0f + dmg * 0.03f);
		}
		health -= dmg;
        if (hb != null) hb.RemoveHealth(dmg);
        
        if (health <= 0.0f)
		{
			if(isExplode)
			{
                if (enemyParticles != null) enemyParticles.SetExplode();
                //Debug.Log("Exploding");
				//Debug.Log(currentLocation[0]);
				//Debug.Log(currentLocation[1]);
				if(GridManager.Instance.GetTileAtPosition(new Vector2(nextLocation[0],nextLocation[1])).ContainsWall())
				{
					//Debug.Log("Exploding 1");
					var vec = new Vector2(nextLocation[0], nextLocation[1]);
					GridManager.Instance.GetTileAtPosition(vec).DamageWall(10000.0f);
					
				}
				if(nextLocation[0] > 0 && GridManager.Instance.GetTileAtPosition(new Vector2(nextLocation[0] - 1,nextLocation[1])).ContainsWall())
				{
					//Debug.Log("Exploding 1");
					var vec = new Vector2(nextLocation[0] - 1, nextLocation[1]);
					GridManager.Instance.GetTileAtPosition(vec).DamageWall(10000.0f);
					
				}
				if(nextLocation[1] > 0 && GridManager.Instance.GetTileAtPosition(new Vector2(nextLocation[0],nextLocation[1] - 1)).ContainsWall())
				{
					//Debug.Log("Exploding 2");
					var vec = new Vector2(nextLocation[0], nextLocation[1] - 1);
					GridManager.Instance.GetTileAtPosition(vec).DamageWall(10000.0f);
				}
				if(nextLocation[0] < gridDim[0] - 1 && GridManager.Instance.GetTileAtPosition(new Vector2(nextLocation[0] + 1,nextLocation[1])).ContainsWall())
				{
					//Debug.Log("Exploding 3");
					var vec = new Vector2(nextLocation[0] + 1, nextLocation[1]);
					GridManager.Instance.GetTileAtPosition(vec).DamageWall(10000.0f);
				}
				if(nextLocation[1] < gridDim[1] - 1 && GridManager.Instance.GetTileAtPosition(new Vector2(nextLocation[0],nextLocation[1] + 1)).ContainsWall())
				{
					//Debug.Log("Exploding 4");
					var vec = new Vector2(nextLocation[0], nextLocation[1] + 1);
					GridManager.Instance.GetTileAtPosition(vec).DamageWall(10000.0f);
				}
			}
			if(isSplitter)
			{
				//Debug.Log("Splitting");
				//Debug.Log(currentLocation[0]);
				//Debug.Log(currentLocation[1]);
				if(currentLocation[0] > 0 && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0] - 1,currentLocation[1])).ContainsHole() && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0] - 1,currentLocation[1])).ContainsWall())
				{
					//Debug.Log("Splitting 1");
					var vec = new Vector2(currentLocation[0] - 1, currentLocation[1]);
					var enemy = Instantiate(EnemyManager.Instance.enemyPrefab, GridManager.GridToWorld(vec), Quaternion.identity);
					
					enemy.hpMult = hpMult;
					enemy.newBruteStat = bruteStat;
					enemy.newSpeedStat = speedStat;
					enemy.newHealthStat = healthStat;
					enemy.setSplitter(false);
					enemy.setExplode(isExplode);
					enemy.color = color;
					enemy.isCounted = false;
				}
				if(currentLocation[1] > 0 && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0],currentLocation[1] - 1)).ContainsHole() && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0],currentLocation[1] - 1)).ContainsWall())
				{
					//Debug.Log("Splitting 2");
					var vec = new Vector2(currentLocation[0], currentLocation[1] - 1);
					var enemy = Instantiate(EnemyManager.Instance.enemyPrefab, GridManager.GridToWorld(vec), Quaternion.identity);
					
					enemy.hpMult = hpMult;
					enemy.newBruteStat = bruteStat;
					enemy.newSpeedStat = speedStat;
					enemy.newHealthStat = healthStat;
					enemy.setSplitter(false);
					enemy.setExplode(isExplode);
					enemy.color = color;
					enemy.isCounted = false;
				}
				if(currentLocation[0] < gridDim[0] - 1 && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0] + 1,currentLocation[1])).ContainsHole() && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0] + 1,currentLocation[1])).ContainsWall())
				{
					//Debug.Log("Splitting 3");
					var vec = new Vector2(currentLocation[0] + 1, currentLocation[1]);
					var enemy = Instantiate(EnemyManager.Instance.enemyPrefab, GridManager.GridToWorld(vec), Quaternion.identity);
					
					enemy.hpMult = hpMult;
					enemy.newBruteStat = bruteStat;
					enemy.newSpeedStat = speedStat;
					enemy.newHealthStat = healthStat;
					enemy.setSplitter(false);
					enemy.setExplode(isExplode);
					enemy.color = color;
					enemy.isCounted = false;
				}
				if(currentLocation[1] < gridDim[1] - 1 && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0],currentLocation[1] + 1)).ContainsHole() && !GridManager.Instance.GetTileAtPosition(new Vector2(currentLocation[0],currentLocation[1] + 1)).ContainsWall())
				{
					//Debug.Log("Splitting 4");
					var vec = new Vector2(currentLocation[0], currentLocation[1] + 1);
					var enemy = Instantiate(EnemyManager.Instance.enemyPrefab, GridManager.GridToWorld(vec), Quaternion.identity);
					
					enemy.hpMult = hpMult;
					enemy.newBruteStat = bruteStat;
					enemy.newSpeedStat = speedStat;
					enemy.newHealthStat = healthStat;
					enemy.setSplitter(false);
					enemy.setExplode(isExplode);
					enemy.color = color;
					enemy.isCounted = false;
				}
			}
            if (enemyParticles != null && transform != null)
                enemyParticles.PlayParticlesDeath(transform.position, color); // play particles

            if (hb != null && hb.gameObject != null)
                Destroy(hb.gameObject);

			Destroy(gameObject);

            return true;
		}

        enemyParticles.PlayParticlesHit(transform.position, color); // play particles

        return false;
	}

    // freeze the enemy movement for duration
    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
             freezeCoroutine = FreezeWait(duration);
             StartCoroutine(freezeCoroutine);
        } else
        {
            StopCoroutine(freezeCoroutine);
            isFrozen = false;

            freezeCoroutine = FreezeWait(duration);
            StartCoroutine(freezeCoroutine);
        }
    }

    // damage the enemy for duration (poison effect)
    public void Poison(float duration, float poisonDamage)
    {
        this.poisonDamage = poisonDamage;

        if (!isPoisoned)
        {
            poisonCoroutine = PoisonWait(duration);
            StartCoroutine(poisonCoroutine);
        }
        else
        {
            StopCoroutine(poisonCoroutine);
            isPoisoned = false;

            poisonCoroutine = PoisonWait(duration);
            StartCoroutine(poisonCoroutine);
        }
    }

    IEnumerator FreezeWait(float duration)
    {
        // freeze 
        isFrozen = true;

        yield return new WaitForSeconds(duration);

        isFrozen = false;
    }

    IEnumerator PoisonWait(float duration)
    {
        // freeze 
        isPoisoned = true;

        yield return new WaitForSeconds(duration);

        isPoisoned = false;
    }

    private void updateDictionary()
	{
		//Debug.Log("Calculating Grd");
		var pos = new Vector2();;
		map = new Dictionary<Vector2, int[]>();
		for(int x = 0; x < gridDim.x; x++)
		{
			for(int y = 0; y < gridDim.y; y++)
			{
				pos = new Vector2(x, y);
				map[pos] = new int[7]{-1, -1, -1, -1, 0, 0, 0};
			}
		}
		
		Vector2 c = new Vector2(currentLocation.x, currentLocation.y); 
		
		map[currentLocation][0] = 0;
		map[currentLocation][1] = distance(c, targetLocation);
		map[currentLocation][2] = 0;
		map[currentLocation][3] = map[c][0] + map[c][1] + map[c][2];
		map[currentLocation][4] = 0;
		map[currentLocation][5] = 0;
		map[currentLocation][6] = 0;
		
		List<Vector2> l = new List<Vector2>();
		var tem = -1;
		while(tem-- != 0)
		{
			if(c.x == targetLocation.x && c.y == targetLocation.y)
			{
				//Debug.Log("Writing Path");
				path = new List<Vector2>();
				while(true)
				{
					if(c.x == currentLocation.x && c.y == currentLocation.y)
					{
						break;
					}
					path.Insert(0, c);
					c = new Vector2(c.x - map[c][4], c.y - map[c][5]);
				}
				//Debug.Log("Grid Calculated");
				break;
			}
			if(map[c][6] != 1)
			{
				//Debug.Log("Checking " + c.x + " " + c.y);
				// Calculate 4 Neighbors
				map[c][6] = 1;
				// Left
				pos.x = c.x - 1;
				pos.y = c.y;
				if(c.x > 0 && map[pos][6] == 0)
				{
					//Debug.Log("LEFT");
					if(map[pos][0] == -1)
					{
						l.Add(pos);
					}
					if(map[pos][0] == -1 || map[pos][0] > map[c][0] + map[c][2] + 10)
					{
					map[pos][0] = map[c][0] + map[c][2] + 10;
					map[pos][1] = distance(pos, targetLocation);
					map[pos][2] = GridManager.Instance.GetTileAtPosition(pos).ContainsHole() ? holePenalty : GridManager.Instance.GetTileAtPosition(pos).ContainsWall() ? getWallPenalty() : 0;
					map[pos][3] = map[pos][0] + map[pos][1] + map[pos][2];
					map[pos][4] = -1;
					map[pos][5] = 0;
					}
				}
				// Right
				pos.x = c.x + 1;
				if(c.x < gridDim.x - 1 && map[pos][6] == 0)
				{
					//Debug.Log("RIGHT");
					if(map[pos][0] == -1)
					{
						l.Add(pos);
					}
					if(map[pos][0] == -1 || map[pos][0] > map[c][0] + map[c][2] + 10)
					{
					map[pos][0] = map[c][0] + map[c][2] + 10;
					map[pos][1] = distance(pos, targetLocation);
					map[pos][2] = GridManager.Instance.GetTileAtPosition(pos).ContainsHole() ? holePenalty : GridManager.Instance.GetTileAtPosition(pos).ContainsWall() ? getWallPenalty() : 0;
					map[pos][3] = map[pos][0] + map[pos][1] + map[pos][2];
					map[pos][4] = 1;
					map[pos][5] = 0;
					}
				}
				// Down
				pos.x = c.x;
				pos.y = c.y - 1;
				if(c.y > 0 && map[pos][6] == 0)
				{
					//Debug.Log("DOWN");
					if(map[pos][0] == -1)
					{
						l.Add(pos);
					}
					if(map[pos][0] == -1 || map[pos][0] > map[c][0] + map[c][2] + 10)
					{
					map[pos][0] = map[c][0] + map[c][2] + 10;
					map[pos][1] = distance(pos, targetLocation);
					map[pos][2] = GridManager.Instance.GetTileAtPosition(pos).ContainsHole() ? holePenalty : GridManager.Instance.GetTileAtPosition(pos).ContainsWall() ? getWallPenalty() : 0;
					map[pos][3] = map[pos][0] + map[pos][1] + map[pos][2];
					map[pos][4] = 0;
					map[pos][5] = -1;
					}
				}
				// Up
				pos.y = c.y + 1;
				if(c.y < gridDim.y - 1 && map[pos][6] == 0)
				{
					//Debug.Log("UP");
					if(map[pos][0] == -1)
					{
						l.Add(pos);
					}
					if(map[pos][0] == -1 || map[pos][0] > map[c][0] + map[c][2] + 10)
					{
					map[pos][0] = map[c][0] + map[c][2] + 10;
					map[pos][1] = distance(pos, targetLocation);
					map[pos][2] = GridManager.Instance.GetTileAtPosition(pos).ContainsHole() ? holePenalty : GridManager.Instance.GetTileAtPosition(pos).ContainsWall() ? getWallPenalty() : 0;
					map[pos][3] = map[pos][0] + map[pos][1] + map[pos][2];
					map[pos][4] = 0;
					map[pos][5] = 1;
					}
				}
			}
			
			// Sort Queue
			var count = l.Count;
			var last = count - 1;
			for(var i = 0; i < last; i++)
			{
				var r = UnityEngine.Random.Range(i, count);
				var temp = l[i];
				l[i] = l[r];
				l[r] = temp;
			}
			l.Sort((a, b) => map[a][3].CompareTo(map[b][3]));
			
			// Get Next in Queue
			c = l[0];
			l.RemoveAt(0);
		}
	}
	
	private int distance(Vector2 a, Vector2 b)
	{
		// Manhatten Distance
		return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
	}

    public static int GetNumEnemiesKilled()
    {
        return enemiesKilled;
    }

    public static void ResetNumEnemiesKilled()
    {
        Debug.Log("Reset");
        enemiesKilled = 0;
        enemiesAlive = 0;
    }
	
    public static int GetNumEnemiesAlive()
    {
        return enemiesAlive;
    }

	public void Randomize()
	{
		newBruteStat = UnityEngine.Random.Range(1,6); // 1-5
		newSpeedStat = UnityEngine.Random.Range(1,6);
		newHealthStat = UnityEngine.Random.Range(1,6);
		if(newBruteStat < 5)
		{
			newBruteStat = 1;
		}
		if(newSpeedStat < 5)
		{
			newSpeedStat = 1;
		}
		if(newHealthStat < 5)
		{
			newHealthStat = 1;
		}
	}
}
