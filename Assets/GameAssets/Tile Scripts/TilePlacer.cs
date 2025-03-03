using Lean.Touch;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private HexRaycaster raycaster;
    [SerializeField] private Transform tilesHolder;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    private Tile currentTile;

    /// <summary>
    /// Tile that has been selected from TileTray and is now on the board
    /// </summary>
    public Tile CurrentTile => currentTile;

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
        LeanTouch.OnFingerTap += Touch_TryPlaceTile;
    }

    private void Update()
    {
        if (currentTile == null) return;

        UpdatePreviewTile();
        HandleRotation();
    }

    private void UpdatePreviewTile()
    {
        var hexPosition = raycaster.HexPosition;

        var occupied = hexGrid.IsPositionOccupied(hexPosition);
        currentTile.gameObject.SetActive(!occupied);

        if (occupied) return;

        SnapToGrid();
        bool canBePlaced = CanTileBePlaced(hexPosition, currentTile);
        currentTile.ApplyHighlightMaterialToBase(canBePlaced ? highlightMaterial : invalidMaterial);
    }

    private void SnapToGrid()
    {
        var worldPos = hexGrid.HexToWorld(raycaster.HexPosition);
        currentTile.transform.position = worldPos;
    }

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
                if (!tile.Matches(neighborTile, sharedEdge)) return false;
            }
        }
        return true;
    }

    private void HandleRotation()
    {
        if (currentTile == null || !currentTile.gameObject.activeSelf) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) RotateTile(scroll > 0 ? 60f : -60f);
    }

    private void RotateTile(float rotationAngle)
    {
        currentTile.Rotate(rotationAngle);

        var hexPosition = raycaster.HexPosition;
        if (!hexGrid.HasNeighbours(hexPosition) || hexGrid.IsPositionOccupied(hexPosition))
        {
            currentTile.Rotate(-rotationAngle);
        }
    }

    public void Button_Rotate()
    {
        if (currentTile == null || !currentTile.gameObject.activeSelf) return;

        RotateTile(60f);
    }

    private void Touch_TryPlaceTile(LeanFinger finger)
    {
        if (LeanTouch.GuiInUse || currentTile == null) return;

        TryPlaceTile(currentTile, raycaster.HexPosition);
    }

    private bool TryPlaceTile(Tile tile, Hex hex)
    {
        if (!CanTileBePlaced(hex, tile)) return false;

        if (tile.TryPlace(hex, transform))
        {
            currentTile = null;
            FindAnyObjectByType<TileTray>().SpawnNextTile();
            Debug.Log($"{tile} placed on {hex}");
            return true;
        }

        return false;
    }

    public void SetCurrentTile(Tile tile)
    {
        currentTile = tile;

        if (currentTile != null)
        {
            currentTile.transform.SetPositionAndRotation(raycaster.WorldPosition, Quaternion.identity);
        }
    }
}
