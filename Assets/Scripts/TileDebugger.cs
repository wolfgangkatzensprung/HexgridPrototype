using TMPro;
using UnityEngine;
using static HexUtils;

public class TileDebugger : Singleton<TileDebugger>
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] TilePlacer tilePlacer;

    private void OnEnable()
    {
        debugText.enabled = true;
    }

    private void OnDisable()
    {
        if (debugText == null) return;

        debugText.enabled = false;
    }

    private void OnGUI()
    {
        if (debugText == null) return;
        if (tilePlacer == null) return;
        if (tilePlacer.CurrentTile == null) return;

        DebugTileEdges(tilePlacer.CurrentTile);
    }

    public void DebugTileEdges(Tile currentTile)
    {
        string debugInfo = "";

        for (int i = 0; i < currentTile.Edges.Length; i++)
        {
            Edge edge = currentTile.Edges[i];
            string edgeType = edge.Type.ToString();

            Hex direction = Directions[i];
            DirectionType dir = (DirectionType)((i + currentTile.CurrentRotation) % 6);

            debugInfo += $"{dir} ({i}) is {edgeType}, Direction: {direction}\n";
        }

        debugText.text = debugInfo;
    }
}
