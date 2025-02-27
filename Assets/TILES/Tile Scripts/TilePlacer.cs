using UnityEngine;

[RequireComponent(typeof(HexGrid))]
public class TilePlacer : MonoBehaviour
{
    const string RESOURCE_FOLDER = "TilePrefabs/";
    GameObject[] tilePrefabs;

    private HexGrid hexGrid;
    GameObject RndTilePrefab => tilePrefabs[Random.Range(0, tilePrefabs.Length)];

    [SerializeField] HexRaycaster raycaster;
    private Tile currentTile;
    [SerializeField] private Transform tilesHolder;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material invalidMaterial;

    private void Awake()
    {
        tilePrefabs = Resources.LoadAll<GameObject>(RESOURCE_FOLDER);
        currentTile = SpawnTile(Vector3.zero, new Hex(0,0));
    }

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    private void Update()
    {
        HandleInput();
        UpdatePreviewTile();
        HandleRotation();
    }

    private void HandleInput()
    {
        var hexPosition = raycaster.HexPosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (!hexGrid.IsPositionOccupied(hexPosition) && hexGrid.HasNeighbours(hexPosition))
            {
                PlaceTile(hexPosition);
            }
        }
    }

    private void UpdatePreviewTile()
    {
        if (currentTile == null) return;

        var hexPosition = raycaster.HexPosition;
        if (hexGrid.IsPositionOccupied(hexPosition))
        {
            currentTile.gameObject.SetActive(false);  // Disable if the tile is already occupied
            return;
        }

        SnapToGrid();

        bool canBePlaced = CanTileBePlaced(hexPosition, currentTile);

        UpdateMaterial(canBePlaced);

        if (!currentTile.gameObject.activeSelf)
        {
            currentTile.gameObject.SetActive(true);
        }
    }

    private void SnapToGrid()
    {
        var worldPos = hexGrid.HexToWorld(raycaster.HexPosition);
        currentTile.transform.position = worldPos;
    }

    private void UpdateMaterial(bool isValidPlacement)
    {
        var renderer = currentTile.GetComponentInChildren<Renderer>();

        if (isValidPlacement)
        {
            renderer.material = highlightMaterial;
        }
        else
        {
            renderer.material = invalidMaterial;
        }
    }

    /// <summary>
    /// Returns true when Tile follows all rules and can be placed
    /// </summary>
    private bool CanTileBePlaced(Hex hex, Tile tile)
    {
        return TileMatchesNeighbors(hex, tile);
    }

    private bool TileMatchesNeighbors(Hex hex, Tile tile)
    {
        foreach (var neighborHex in hexGrid.GetNeighbours(hex))
        {
            if (hexGrid.TilesByHex.TryGetValue(neighborHex, out var neighborTile))
            {
                int sharedEdge = HexUtils.GetSharedEdgeIndex(hex, neighborHex);

                if (!tile.Matches(neighborTile, sharedEdge))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void HandleRotation()
    {
        if (currentTile == null || !currentTile.gameObject.activeSelf)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float rotationAngle = scroll > 0 ? 60f : -60f;
            currentTile.Rotate(rotationAngle);

            var hexPosition = raycaster.HexPosition;
            if ((!hexGrid.HasNeighbours(hexPosition) || hexGrid.IsPositionOccupied(hexPosition)))
            {
                currentTile.Rotate(-rotationAngle);
            }
        }
    }

    private void PlaceTile(Hex hex)
    {
        Vector3 worldPosition = hexGrid.HexToWorld(hex);
        var tile = currentTile;
        tile.transform.position = worldPosition;
        tile.transform.parent = tilesHolder;
        tile.GetComponentInChildren<Renderer>().material = defaultMaterial;
        hexGrid.AddTile(hex, currentTile.GetComponent<Tile>());
        currentTile = SpawnTile(worldPosition, hex);
    }

    private Tile SpawnTile(Vector3 worldPosition, Hex hex)
    {
        var tile = Instantiate(RndTilePrefab, worldPosition, Quaternion.identity).GetComponent<Tile>();
        tile.Setup(hex);
        return tile;
    }
}
