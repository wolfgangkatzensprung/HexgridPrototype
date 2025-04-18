using Lean.Touch;
using UnityEngine;
using static HexUtils;

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
        tile.Place(startHex);
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
        currentTile.AddRotation(rotationAngle);
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
            AudioManager.Instance.PlaySound(AudioManager.SoundType.Place);
        }
        else if (finger.Age < 1)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.Error);
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
        Debug.Log($"Try Place {tile} on {hex} with rotation {tile.CurrentRotation}.");
        if (tile.TryPlace(hex, hexGrid))
        {
            currentTile = null;
            FindAnyObjectByType<TileTray>().NextTile();
            Debug.Log($"{tile} has been placed on {hex}");
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
