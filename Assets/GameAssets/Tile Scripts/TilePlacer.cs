using Lean.Touch;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private HexRaycaster raycaster;
    [SerializeField] private Transform tilesHolder;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    [SerializeField] private Tile startTile;

    private Tile currentTile;

    /// <summary>
    /// Tile that has been selected from TileTray and is now on the board
    /// </summary>
    public Tile CurrentTile => currentTile;

    private void Start()
    {
        LeanTouch.OnFingerTap += Touch_TryPlaceTile;

        Invoke(nameof(PlaceStartTile), 1f);
    }

    private void PlaceStartTile()
    {
        var tileObject = Instantiate(startTile, tilesHolder);
        var tile = tileObject.GetComponent<Tile>();

        tile.TryPlace(new Hex(0, 0, 0), tilesHolder, hexGrid);
    }


    private void Update()
    {
        if (currentTile == null) return;

        HandleRotation();
        UpdatePreviewTile();
    }

    private void UpdatePreviewTile()
    {
        var hexPosition = raycaster.HitHexPosition;
        
        var canBePlaced = hexGrid.CanBePlaced(hexPosition, currentTile);

        var visible = !hexGrid.IsPositionOccupied(hexPosition);
        currentTile.gameObject.SetActive(visible);

        if (!visible) return;

        currentTile.ApplyHighlightMaterialToBase(canBePlaced ? highlightMaterial : invalidMaterial);
        SnapToGrid();
    }

    private void SnapToGrid()
    {
        var worldPos = hexGrid.HexToWorld(raycaster.HitHexPosition);
        currentTile.transform.position = worldPos;
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

        var hexPosition = raycaster.HitHexPosition;
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

        TryPlaceTile(currentTile, raycaster.HitHexPosition);
    }

    private bool TryPlaceTile(Tile tile, Hex hex)
    {
        if (tile.TryPlace(hex, transform, hexGrid))
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
            currentTile.transform.SetPositionAndRotation(raycaster.HitWorldPosition, Quaternion.identity);
        }
    }
}
