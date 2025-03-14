using TMPro;
using UnityEngine;
using static HexUtils;

public class TileDebugger : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;

    private void OnGUI()
    {
        var tilePlacer = FindAnyObjectByType<TilePlacer>();

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
            DirectionType directionEnum = (DirectionType)i;

            debugInfo += $"{directionEnum} ({i}) is {edgeType}, Direction: {direction}\n";
        }

        debugText.text = debugInfo;
    }
}
