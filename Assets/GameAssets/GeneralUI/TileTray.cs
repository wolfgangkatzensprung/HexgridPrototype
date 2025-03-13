using UnityEngine;

public class TileTray : MonoBehaviour
{
    const int MAX_TILES = 3;
    const string RESOURCE_FOLDER = "TilePrefabs/";

    [SerializeField] private Transform trayContainer; // UI Panel
    [SerializeField] private GameObject tilePreviewPrefab; // UI element with RawImage
    [SerializeField] private Camera tilePreviewCamera;

    [SerializeField] private Transform[] tileSpawnPoints; // 3 possible spawn positions
    private Tile[] tilesOnTray = new Tile[3];

    private GameObject[] tilePrefabs;   // load from resources

    private GameObject RndTilePrefab => tilePrefabs[Random.Range(0, tilePrefabs.Length)];

    private void Awake()
    {
        tilePrefabs = Resources.LoadAll<GameObject>(RESOURCE_FOLDER);
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
        for (int i = 0; i < MAX_TILES; i++) SpawnTileOnTray(i);
    }

    private void SpawnTileOnTray(int index)
    {
        if (tilesOnTray[index] != null)
        {
            Destroy(tilesOnTray[index]);
        }

        Tile tile = Instantiate(RndTilePrefab, tileSpawnPoints[index].position, tileSpawnPoints[index].rotation).GetComponent<Tile>();
        tilesOnTray[index] = tile;
    }

    public void SpawnNextTile()
    {
        for (int i = 0; i < MAX_TILES; i++)
        {
            if (tilesOnTray[i] == null)
            {
                SpawnTileOnTray(i);
                return;
            } 
        }
    }

    public void Button_SelectTileFromTray(int index)
    {
        var tile = tilesOnTray[index];

        if (tile == null) return;

        Debug.Log($"Tile {index} selected from TileTray: {tile}");

        var tilePlacer = FindAnyObjectByType<TilePlacer>();
        if (tilePlacer.CurrentTile != null)
        {
            PutTileBackOnTray(tilePlacer.CurrentTile);
        }
        tilePlacer.SetCurrentTile(tilesOnTray[index]);

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
