using Lean.Touch;
using UnityEngine;
using static CW.Common.CwInputManager;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private HexRaycaster raycaster;
    [SerializeField] private Transform tilesHolder;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material invalidMaterial;

    [SerializeField] private Tile startTile;

    [SerializeField] private AudioSource placeSound;
    [SerializeField] private AudioSource errorSound;

    private Tile currentTile;

    /// <summary>
    /// Tile that has been selected from TileTray and is now the preview tile on the board
    /// </summary>
    public Tile CurrentTile => currentTile;

    private void Start()
    {
        LeanTouch.OnFingerTap += Touch_Tap;
        LeanTouch.OnFingerUpdate += Touch_Update;

        Invoke(nameof(PlaceStartTile), 1f);
    }

    private void PlaceStartTile()
    {
        var tile = Instantiate(startTile, tilesHolder).GetComponent<Tile>();

        var startHex = new Hex(0, 0, 0);

        tile.ForceSetup(startHex, hexGrid);
        tile.Place(startHex, tilesHolder);
    }


    private void Update()
    {
        if (currentTile == null) return;

        HandleRotation();
        UpdatePreviewTile();
    }

    private void UpdatePreviewTile()
    {
        if (LeanTouch.GuiInUse) return;

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
    }

    public void Button_Rotate()
    {
        if (currentTile == null || !currentTile.gameObject.activeSelf) return;

        RotateTile(60f);
    }

    private void Touch_Tap(LeanFinger finger)
    {
        if (LeanTouch.GuiInUse || currentTile == null) return;

        if (TryPlaceTile(currentTile, raycaster.HitHexPosition))
        {
            placeSound.Play();
        }
        else
        {
            errorSound.Play();
            Button_Rotate();
        }
    }

    private void Touch_Update(LeanFinger finger)
    {
        var l = FindAnyObjectByType<LeanSelectByFinger>();
        l.SelectScreenPosition(finger);
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
        Debug.Log($"Set Current Tile = {tile}");
        currentTile = tile;

        if (currentTile != null)
        {
            currentTile.transform.SetPositionAndRotation(raycaster.HitWorldPosition, Quaternion.identity);
        }
        else
        {
            Debug.Log($"Assigned null to {nameof(currentTile)}");
        }

        // Reset Fingers
        foreach (var finger in LeanTouch.Fingers)
        {
            finger.StartedOverGui = false;
        }
    }
}
