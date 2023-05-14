using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region SINGLETON
    public static GridManager Instance;
    void Awake() => Instance = this;
    #endregion

    private const int CAMERA_Z = -10;

    // Our gameunit = .5 Unity units => 1 Unity unit = 2 gameunits
    public const int UNIT_CONVERSION = 2;

    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;

    [SerializeField] private static int _numHoles;

    [SerializeField] public Tile _tilePrefab;
    [SerializeField] public Wall _wallPrefab;
    [SerializeField] public Base _basePrefab;
    [SerializeField] public Bridge _bridgePrefab;
    [SerializeField] private Hole _holePrefab;
    [SerializeField] public Turret[] _turretPrefab; // DO NOT MODIFY, THAT MEANS YOU JOSEPH

    [SerializeField] private Transform _cam;

    [SerializeField] private Transform _tileHolder; // Changed from gameobject to transform
    [SerializeField] private Transform _wallHolder; // Changed from gameobject to transform
    [SerializeField] private Transform _turretHolder; // Changed from gameobject to transform
    [SerializeField] private Transform _bulletHolder;
	[SerializeField] private Transform _enemyHolder;

    private bool isInMultiPlace = false;
    // two tiles for the multiplace, fill in the tiles between
    private Tile multiPlaceFirst = null;
    private Tile multiPlaceSecond = null;
    private Vector2 highlightFrom;
    private Vector2 highlightTo;
    private static Tile firstTile = null;

    private bool isWall = true;
    private Camera mainCam;

    private Dictionary<Vector2, Tile> _tiles;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();

        GenerateBase();

        GenerateHoles(_numHoles);

        mainCam = MainCam.Instance.GetComponent<Camera>();
    }

    // Generates a grid of tile objects 
    void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * .5f, y * .5f), Quaternion.identity);

                Vector2 gridPos = new Vector2(x, y);

                spawnedTile.Init(gridPos, _tileHolder);
                _tiles[gridPos] = spawnedTile;
            }
        }

        // Used to convert camera position because of difference in world and grid units
        
        // OLD NEWS
        //var camConversion = UNIT_CONVERSION * UNIT_CONVERSION;

        //_cam.transform.position = new Vector3((float)_gridWidth / camConversion - 0.5f, (float)_gridHeight / camConversion - 0.5f, CAMERA_Z);
    }

    void GenerateBase()
    {
        // get tile at the base spawn location
        // hard coded for now, maybe let the player choose the position l8r
        // HARD CODE
        var spawnLocation = new Vector2(33, 7); // HARD CODE

        var spawnTile = GetTileAtPosition(spawnLocation);

        spawnTile.SetBase();
    }

    void GenerateHoles(int numHoles)
    {
        Dictionary<Vector2, bool> map = new Dictionary<Vector2, bool>();

        for (int i = 0; i < numHoles; i++)
        {
            // generate a hole at random position
            var rand = GetRandomHolePositionGrid();

            while (map.ContainsKey(rand))
            {
                rand = GetRandomHolePositionGrid();
            }

            map[rand] = true;

            var hole = Instantiate(_holePrefab, rand, Quaternion.identity);

            hole.Init();

            SetTilesAsHole(hole);
        }
    }

    Vector2 GetRandomHolePositionGrid()
    {
        var xMax = _gridWidth - 6;
        var yMax = _gridHeight - 3;
        var xMin = 6;
        var yMin = 0;

        var x = Random.Range(xMin, xMax);
        var y = Random.Range(yMin, yMax);

        return GridToWorld(new Vector2(x, y));
    }

    void SetTilesAsHole(Hole hole)
    {
        var dimensions = hole.GetDimensions();
        var gridPos = hole.GetGridPos();


        // go through the tiles contained by the generated black hole
        // mark these tiles as being occupied by the hole
        // so that the user cant place, walk, etc.

        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                var tileHolePos = new Vector2(gridPos.x + x, gridPos.y + y);

                var tile = GetTileAtPosition(tileHolePos);

                tile.SetHole();
            }
        }
    }

    // return whether a grid pos is within bounds of the grid
    public bool IsGridPosValid(Vector2 gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < _gridWidth && gridPos.y >= 0 && gridPos.y < _gridHeight;
    }

    // Return a tile at pos (x, y)
    public Tile GetTileAtPosition(Vector2 gridPos)
    {
        if (_tiles.TryGetValue(gridPos, out var tile))
        {
            return tile;
        }

        return null;
    }

    // Converts the grid pos to the world pos
    public static Vector2 GridToWorld(Vector2 gridPos)
    {
        return gridPos / UNIT_CONVERSION;
    }

    // Converts the world pos to the grid pos
    public static Vector2 WorldToGrid(Vector2 worldPos)
    {
        // This weird * 2 / 2 * 2... etc is just a way to round to nearest .5
        // This is necessary because thats the size of our unit in the world
        var x = Mathf.Round(worldPos.x * 2) / 2 * GridManager.UNIT_CONVERSION;
        var y = Mathf.Round(worldPos.y * 2) / 2 * GridManager.UNIT_CONVERSION;

        return new Vector2(x, y);
    }

    // Round to nearest tile position in world units
    public static Vector2 RoundToTileInWorld(Vector2 worldPos)
    {
        var gridPos = WorldToGrid(worldPos);

        return GridToWorld(gridPos);
    }

    // NEW
    public Transform GetWallHolder()
    {
        return _wallHolder;
    }

    // NEW
    public Transform GetTurretHolder()
    {
        return _turretHolder;
    }

    public Transform GetBulletHolder()
    {
        return _bulletHolder;
    }

    // Highlights tiles around a given x, y
    public void HighlightRadiusFromPoint(Vector2 gridPos, int radius)
    {
        // Unhighlight to remove old radius visual
        UnhighlightGrid();

        // check that the player is in battle mode
        if (GameManager.Instance.GetGameMode() == GameManager.GAMEMODE_BATTLE)
        {
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    var x = gridPos.x + i;
                    var y = gridPos.y + j;

                    var tile = GetTileAtPosition(new Vector2(x, y));

                    if (tile == null)
                        continue;

                    tile.RadiusHighlight(true);
                }
            }
        }
    }

    private void UnhighlightGrid()
    {
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                var tile = GetTileAtPosition(new Vector2(i, j));

                if (tile == null)
                    continue;

                tile.RadiusHighlight(false);
            }
        }
    }

    public static void SetNumBlackHoles(int num)
    {
        _numHoles = num;
    }

    public static int GetNumBlackHoles()
    {
        return _numHoles;
    }

    public void SetIsInMultiPlace(bool b)
    {
        isInMultiPlace = b;
    }

    public bool IsInMultiPlace()
    {
        return isInMultiPlace;
    }

    public void SetFirstTile(Tile tile, bool isWall)
    {
        if (firstTile != null)
            firstTile.MultiPlaceHighlight(false);

        multiPlaceFirst = tile;
        firstTile = tile;
        this.isWall = isWall;
    }

    public void SetSecondTile(Tile tile)
    {
        multiPlaceSecond = tile;

        if (firstTile != null)
            firstTile.MultiPlaceHighlight(false);
    }

    public void ClearMultiFill()
    {
        if (multiPlaceFirst != null)
            multiPlaceFirst.MultiPlaceHighlight(false);

        if (firstTile != null)
            firstTile.MultiPlaceHighlight(false);

        multiPlaceFirst = null;
        multiPlaceSecond = null;
        firstTile = null;
        isInMultiPlace = false;
    }

    public void MultiFill(int turret)
    {
        if (multiPlaceFirst == null || multiPlaceSecond == null)
            return;

        var firstPos = multiPlaceFirst.GetGridPos();
        var secondPos = multiPlaceSecond.GetGridPos();

        // vertical line
        if (firstPos.x == secondPos.x)
        {
            var x = firstPos.x;
            var startY = (int)Mathf.Min(firstPos.y, secondPos.y);
            var endY = (int)Mathf.Max(firstPos.y, secondPos.y);

            for (int y = startY; y <= endY; y++)
            {
                if (turret == -1)
                    _tiles[new Vector2(x, y)].SetWall();
                else
                    _tiles[new Vector2(x, y)].SetTurret(turret);
            }
        }

        // horizontal line
        if (firstPos.y == secondPos.y)
        {
            var y = firstPos.y;
            var startX = (int)Mathf.Min(firstPos.x, secondPos.x);
            var endX = (int)Mathf.Max(firstPos.x, secondPos.x);

            for (int x = startX; x <= endX; x++)
            {
                if (turret == -1)
                    _tiles[new Vector2(x, y)].SetWall();
                else
                    _tiles[new Vector2(x, y)].SetTurret(turret);
            }
        }

        SetIsInMultiPlace(false);
    }

    public void HighlightMultiplace(Tile to)
    {
        // Unhighlight to remove old radius visual

        highlightTo = WorldToGrid(mainCam.ScreenToWorldPoint(Input.mousePosition));
        highlightFrom = multiPlaceFirst.GetGridPos();

        // vertical line
        if (highlightTo.x == highlightFrom.x && highlightTo.y != highlightFrom.y)
        {
            for (int y = (int)highlightTo.y; y <= multiPlaceFirst.GetGridPos().y; y++)
            {
                _tiles[new Vector2(highlightTo.x, y)].MultiPlaceHighlight(true);
                _tiles[new Vector2(highlightTo.x, y)].MouseHighlight(false);
                _tiles[new Vector2(highlightTo.x, y)].MultiPlaceHighlight(true);
            }
        }

        // horizontal line
        if (highlightTo.y == highlightFrom.y && highlightTo.x != highlightFrom.x)
        {
            for (int x = (int)highlightTo.x; x <= multiPlaceFirst.GetGridPos().x; x++)
            {
                _tiles[new Vector2(x, highlightTo.y)].MultiPlaceHighlight(true);
                _tiles[new Vector2(x, highlightTo.y)].MouseHighlight(false);
                _tiles[new Vector2(x, highlightTo.y)].MultiPlaceHighlight(true);
            }
        }
    }

    public void UnhighlightMultiplace()
    {
        if (highlightTo == null || highlightFrom == null)
            return;

        // vertical line
        if (highlightTo.x == highlightFrom.x)
        {
            var x = highlightTo.x;
            var startY = (int)Mathf.Min(highlightTo.y, highlightFrom.y);
            var endY = (int)Mathf.Max(highlightTo.y, highlightFrom.y);

            for (int y = startY; y <= endY; y++)
            {
                _tiles[new Vector2(x, y)].MultiPlaceHighlight(false);
            }
        }

        // horizontal line
        if (highlightTo.y == highlightFrom.y)
        {
            var y = highlightTo.y;
            var startX = (int)Mathf.Min(highlightTo.x, highlightFrom.x);
            var endX = (int)Mathf.Max(highlightTo.x, highlightFrom.x);

            for (int x = startX; x <= endX; x++)
            {
                _tiles[new Vector2(x, y)].MultiPlaceHighlight(false);
            }
        }
    }

}
