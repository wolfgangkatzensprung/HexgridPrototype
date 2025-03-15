using UnityEngine;

public class TileTray : MonoBehaviour
{
    const int MAX_TILES = 3;
    const string RESOURCE_FOLDER = "TilePrefabs/";
    const string STAGE_1 = "Stage1/";

    [SerializeField] private Transform trayContainer; // UI Panel
    [SerializeField] private GameObject tilePreviewPrefab; // UI element with RawImage
    [SerializeField] private Camera tilePreviewCamera;
    [SerializeField] private Transform tilesHolder;
    [SerializeField] private Transform[] tileSpawnPoints; // 3 possible spawn positions
    private Tile[] tilesOnTray = new Tile[3];

    private GameObject[] tilePrefabs;   // load from resources

    public GameObject RndTilePrefab => tilePrefabs[Random.Range(0, tilePrefabs.Length)];

    private void Awake()
    {
        var folderToLoad = RESOURCE_FOLDER + STAGE_1;
        tilePrefabs = Resources.LoadAll<GameObject>(folderToLoad);
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            Button_SelectTileFromTray(0);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            Button_SelectTileFromTray(1);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            Button_SelectTileFromTray(2);
        }
    }

    public void Initialize()
    {
        for (int i = 0; i < MAX_TILES; i++) TrySpawnTileOnTray(i);
    }

    private bool TrySpawnTileOnTray(int index)
    {
        if (!Game.Instance.HasTiles) return false;

        if (tilesOnTray[index] != null)
        {
            Debug.LogWarning($"Tile spawn on already existing {tilesOnTray[index]} Tile in Tray");
            Destroy(tilesOnTray[index]);
        }

        Tile tile = Instantiate(RndTilePrefab, tileSpawnPoints[index].position, tileSpawnPoints[index].rotation, tilesHolder).GetComponent<Tile>();
        tile.transform.SetParent(transform);
        tilesOnTray[index] = tile;
        return true;
    }

    public void NextTile()
    {
        for (int i = 0; i < MAX_TILES; i++)
        {
            if (tilesOnTray[i] == null)
            {
                var tileSpawned = TrySpawnTileOnTray(i);

                if (tileSpawned)
                {
                    Game.Instance.TileCount--;
                }
                else
                {
                    Game.Instance.Restart();
                }

                return;
            }
        }
    }

    public void Button_SelectTileFromTray(int index)
    {
        var tile = tilesOnTray[index];

        if (tile == null) return;

        var tilePlacer = FindAnyObjectByType<TilePlacer>();
        if (tilePlacer.CurrentTile != null)
        {
            PutTileBackOnTray(tilePlacer.CurrentTile);
        }
        tilePlacer.SetCurrentTile(tile);
        tile.transform.SetParent(FindAnyObjectByType<HexGrid>().transform);

        tilesOnTray[index] = null;

        AudioManager.Instance.PlaySound(AudioManager.SoundType.Select);
    }

    private void PutTileBackOnTray(Tile currentTile)
    {
        for (int i = 0; i < MAX_TILES; i++)
        {
            if (tilesOnTray[i] == null)
            {
                tilesOnTray[i] = currentTile;
                currentTile.transform.SetPositionAndRotation(tileSpawnPoints[i].position, tileSpawnPoints[i].rotation);
                currentTile.ResetRotation();
                currentTile.ResetMaterials();
            }
        }
    }
}
