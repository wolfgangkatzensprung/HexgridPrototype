using Unity.VisualScripting;
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
    [SerializeField] private Material invalidMaterial;

    private void Awake()
    {
        tilePrefabs = Resources.LoadAll<GameObject>(RESOURCE_FOLDER);
    }

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
        currentTile = SpawnTile(Vector3.zero);
    }

    private void Update()
    {
        HandleInput();

        if (currentTile == null) return;

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
                if (CanTileBePlaced(hexPosition, currentTile))
                PlaceTile(currentTile, hexPosition);
            }
        }
    }

    private void UpdatePreviewTile()
    {
        var hexPosition = raycaster.HexPosition;
        if (hexGrid.IsPositionOccupied(hexPosition))
        {
            currentTile.gameObject.SetActive(false);  // Disable if the tile is already occupied
            return;
        }

        SnapToGrid();

        bool canBePlaced = CanTileBePlaced(hexPosition, currentTile);

        currentTile.ApplyHighlightMaterialToBase(canBePlaced ? highlightMaterial : invalidMaterial);

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

    /// <summary>
    /// Returns true when Tile follows all rules and can be placed
    /// </summary>
    private bool CanTileBePlaced(Hex hex, Tile tile)
    {
        return TileMatchesNeighbors(hex, tile);
    }

    private bool TileMatchesNeighbors(Hex hex, Tile tile)
    {
        foreach (var neighborHex in hexGrid.GetAllNeighbours(hex))
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

    private Tile SpawnTile(Vector3 worldPosition) => Instantiate(RndTilePrefab, worldPosition, Quaternion.identity).GetComponent<Tile>();

    private void PlaceTile(Tile tile, Hex hex)
    {
        Vector3 worldPosition = hexGrid.HexToWorld(hex);

        tile.Place(hex, worldPosition, transform);

        // Spawn next Tile
        currentTile = SpawnTile(worldPosition);
    }
}
