using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region SINGLETON
    public static Player Instance;
    void Awake() => Instance = this;
    #endregion

    [SerializeField] private float _playerSpeed;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private float timeToMove = 0.2f;
    private Color color;
    private float fireRate = .1f;
    private float bulletSpeed = 750f;
    private static float bulletDamage = 1f;
    private float currentTime;
    private float startTime;
    private int shootMode = 0;
    private Camera cam;
    private bool rClick = false;
    private float[] shootCooldowns = new float[] { .1f, .8f };
    [SerializeField] protected Bullet bullet;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        color = GetComponent<SpriteRenderer>().color;
        cam = MainCam.Instance.GetComponent<Camera>();

        currentTime = Time.time;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        GridManager.Instance.HighlightRadiusFromPoint(GetGridPos(), GameManager.Instance.GetPlayerBuildRadius());

        GetPlayerMovementInput();

        Shoot();
    }

    private void Shoot()
    {
        /*
        if (Input.GetMouseButton(1) && rClick == false)
        {
            shootMode = (shootMode + 1) % 2;
            rClick = true;
        }
        else if (Input.GetMouseButton(1) == false)
        {
            rClick = false;
        }*/

        if (shootMode == 0)
        {
            if (GameManager.Instance.IsFireModeEnabled() && Input.GetMouseButton(0) && currentTime - startTime > shootCooldowns[shootMode] && cam != null)
            {
                Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

                var tempBullet = Instantiate(bullet, transform.position, Quaternion.identity);
                tempBullet.Init(bulletDamage, bulletSpeed, color, 0, true);
                tempBullet.ShootTowards(mousePos, transform.position);

                startTime = Time.time;

            }

            currentTime = Time.time;
        }
        else if (shootMode == 1)
        {
            Shotgun();
        }
    }

    private void Shotgun()
    {
        if (GameManager.Instance.IsFireModeEnabled() && Input.GetMouseButton(0) && currentTime - startTime > shootCooldowns[shootMode] && cam != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SHOTGUN_SOUND);

            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            var direction = mousePos - (Vector2)transform.position;
            var angle = Mathf.PI / 12;


            var leftX = direction.x * Mathf.Cos(angle) - direction.y * Mathf.Sin(angle);
            var leftY = direction.x * Mathf.Sin(angle) + direction.y * Mathf.Cos(angle);
            Vector2 left = new Vector2(leftX, leftY);

            var rightX = direction.x * Mathf.Cos(-1 * angle) - direction.y * Mathf.Sin(-1 * angle);
            var rightY = direction.x * Mathf.Sin(-1 * angle) + direction.y * Mathf.Cos(-1 * angle);
            Vector2 right = new Vector2(rightX, rightY);

            var bulletMiddle = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletMiddle.Init(bulletDamage * 5, bulletSpeed, color, 1, true);
            bulletMiddle.ShootTowards(mousePos, transform.position);

            var bulletLeft = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletLeft.Init(bulletDamage * 2.5f, bulletSpeed, color, 1, true);
            bulletLeft.ShootTowards(left, mousePos, 0);

            var bulletRight = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletRight.Init(bulletDamage * 2.5f, bulletSpeed, color, 1, true);
            bulletRight.ShootTowards(right, mousePos, 0);

            startTime = Time.time;

        }

        currentTime = Time.time;
    }

    // Method gets WASD input to move player
    private void GetPlayerMovementInput()
    {
        if (Input.GetKey(KeyCode.W) && !isMoving)
        {
            var gridPos = GetGridPos();
            var potentialGridPos = new Vector2(gridPos.x, gridPos.y + 1);

            if (IsWalkable(potentialGridPos))
            {
                StartCoroutine(MovePlayer(Vector3.up / GridManager.UNIT_CONVERSION));
                PlayTileParticles(gridPos);
            }
        }

        if (Input.GetKey(KeyCode.A) && !isMoving)
        {
            var gridPos = GetGridPos();
            var potentialGridPos = new Vector2(gridPos.x - 1, gridPos.y);

            if (IsWalkable(potentialGridPos))
            {
                StartCoroutine(MovePlayer(Vector3.left / GridManager.UNIT_CONVERSION));
                PlayTileParticles(gridPos);
            }
        }

        if (Input.GetKey(KeyCode.S) && !isMoving)
        {
            var gridPos = GetGridPos();
            var potentialGridPos = new Vector2(gridPos.x, gridPos.y - 1);

            if (IsWalkable(potentialGridPos))
            {
                StartCoroutine(MovePlayer(Vector3.down / GridManager.UNIT_CONVERSION));
                PlayTileParticles(gridPos);
            }
        }

        if (Input.GetKey(KeyCode.D) && !isMoving)
        {
            var gridPos = GetGridPos();
            var potentialGridPos = new Vector2(gridPos.x + 1, gridPos.y);

            if (IsWalkable(potentialGridPos))
            {
                StartCoroutine(MovePlayer(Vector3.right / GridManager.UNIT_CONVERSION));
                PlayTileParticles(gridPos);
            }
        }
    }

    private void PlayTileParticles(Vector2 gridPos)
    {
        var tile = GridManager.Instance.GetTileAtPosition(gridPos);
        tile.PlayParticles(color);
    }

    // Helper method for GetPlayerMovementInput() that actually moves and animates the player
    private IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        float elapsedTime = 0;
        Vector3 origPos = transform.position;
        Vector3 targetPos = origPos + direction;
        if (GridManager.Instance.GetTileAtPosition(GetGridPos()).ContainsBridge())
            timeToMove = .13f;
        else
            timeToMove = .2f;

        // NOTE: Can add check to get player to move on walls only
        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;

    }

    // NEW
    public Vector2 GetGridPos()
    {
        var worldPos = gameObject.transform.position;

        // This weird * 2 / 2 * 2... etc is just a way to round to nearest .5
        // This is necessary because thats the size of our unit in the world
        var x = Mathf.Round(worldPos.x * 2) / 2 * GridManager.UNIT_CONVERSION;
        var y = Mathf.Round(worldPos.y * 2) / 2 * GridManager.UNIT_CONVERSION;

        return new Vector2(x, y);
    }

    public void HighlightPlayerRadius()
    {
        int playerRadius = GameManager.Instance.GetPlayerBuildRadius();

        GridManager.Instance.HighlightRadiusFromPoint(GetGridPos(), playerRadius);
    }

    // checks if theres a wall 
    private bool IsWalkable(Vector2 gridPos)
    {
        // check if player is trying to go out of bounds
        if (!GridManager.Instance.IsGridPosValid(gridPos))
            return false;

        var tile = GridManager.Instance.GetTileAtPosition(gridPos);

        return !tile.ContainsHole() && (tile.ContainsWall() || tile.ContainsBridge() || tile.ContainsBase());
    }

    public static void IncrementBulletDamage()
    {
        bulletDamage *= 1.1f;
    }

    public int GetGunType()
    {
        return shootMode;
    }

    public void SetGunType(int gunType)
    {
        shootMode = gunType;
        UIManager.Instance.UpdateWeaponPanel(gunType);
    }
}