using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    HexGrid hexGrid;
    TMP_Text text;

    [Title("Hexagonal Enum Selector")]
    [EnumToggleButtons]
    public Edge[] Edges = new Edge[6];

    public Hex hexPosition { get; private set; }
    private int _currentRotation = 0;
    public int CurrentRotation
    {
        get => _currentRotation;
        private set
        {
            if (_currentRotation != value)
            {
                _currentRotation = (value + 6) % 6;
            }
        }
    }

    [Header("Debug Colors")]
    public Color qColor = new Color(1f, 0.5f, 0.5f); // Default red
    public Color rColor = new Color(0.5f, 0.62f, 1f); // Default blue
    public Color sColor = new Color(0.5f, 1f, 0.5f); // Default green

    /// <summary>
    /// Setup the Tile
    /// </summary>
    /// <param name="hex">Hex Coordinates</param>
    /// <param name="worldPosition">Transform Coordinates</param>
    /// <param name="parent">Tiles Holder</param>
    /// <param name="defaultMaterial">Material to switch to, when placed</param>
    public void Place(Hex hex, Vector3 worldPosition, Transform parent, Material defaultMaterial)
    {
        hexGrid = FindAnyObjectByType<HexGrid>();
        text = GetComponentInChildren<TMP_Text>();

        this.hexPosition = hex;

        transform.position = worldPosition;
        transform.parent = parent;
        GetComponentInChildren<Renderer>().material = defaultMaterial;

        hexGrid.AddTile(hex, this);

        for (int i = 0; i < Edges.Length; i++)
        {
            Debug.Log($"Edge{i} is {Edges[i].ToString()}");
        }

        SetupCoordinatesText(hex);
        if (TrySetupScoreTexts(hex, 100, out int neighbourCount))
        {
            Game.Instance.Score += 100 * neighbourCount;
        }
    }

    private void SetupCoordinatesText(Hex hex)
    {
        string coloredText =
        $"<color=#{ColorToHex(qColor)}>{hex.Q}</color>, " +
        $"<color=#{ColorToHex(rColor)}>{hex.R}</color>, " +
        $"<color=#{ColorToHex(sColor)}>{hex.S}</color>";

        text.text = coloredText;
    }

    private bool TrySetupScoreTexts(Hex hex, int scorePerEdge, out int neighbourCount)
    {
        var scores = new List<(Vector3,int)>();
        var neighbours = hexGrid.GetOccupiedNeighbours(hex);
        neighbourCount = neighbours.Length;
        foreach (var neighbor in neighbours)
        {
            var dir = HexUtils.GetDirectionToNeighbor(hex, neighbor);
            var edgePosition = (FractionalHex)hex + (FractionalHex)dir * Mathf.Sqrt(3f) * .5f;
            var offset = (FractionalHex)dir * -0.2f; // slight offset inwards
            var textPosition = hexGrid.FractionalHexToWorld(edgePosition + offset);
            textPosition += Vector3.up * .5f;
            var score = (textPosition, scorePerEdge);
            scores.Add(score);
        }

        GetComponentInChildren<TileScoreDisplay>().ShowScores(scores);
        return neighbourCount > 0;
    }

    public void Rotate(float angle)
    {
        transform.Rotate(0, angle, 0);
        CurrentRotation -= (int)Mathf.Sign(angle);
    }

    private string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public bool Matches(Tile other, int sharedEdgeIndex)
    {
        var edgeIndex = (sharedEdgeIndex + CurrentRotation + 3) % 6;
        Edge thisEdge = this.Edges[edgeIndex];

        var otherEdgeIndex = (sharedEdgeIndex + other.CurrentRotation) % 6;
        Edge otherEdge = other.Edges[otherEdgeIndex]; // opposite edge

        Debug.Log($"Shared Edge: {edgeIndex} is {thisEdge.ToString()}, Other Edge: {otherEdgeIndex} is {otherEdge.ToString()}");
        Debug.Log($"Rotation: {CurrentRotation}, Other Rotation: {other.CurrentRotation}");

        return thisEdge.Type == otherEdge.Type;
    }
}

[System.Serializable]
public class Edge
{
    [EnumToggleButtons]
    public EdgeType Type;

    [ShowIf("HasRegion"), LabelText("Region ID")]
    public int Region;

    private bool HasRegion => Type != EdgeType.None;

    public override string ToString()
    {
        return $"{Type},{Region}";
    }
}

public enum EdgeType
{
    None = 0,
    Street = 1,
    Castle = 2,
    Forest = 3,
    River = 4
}


#if UNITY_EDITOR
[CustomEditor(typeof(Tile))]
public class TileEditor : OdinEditor
{
    private readonly Vector2[] hexOffsets = new Vector2[]
        {
        new Vector2(0, -1),   // 0 - Top
        new Vector2(0.87f, -0.5f),  // 1 - Top-right
        new Vector2(0.87f, 0.5f),  // 2 - Bottom-right
        new Vector2(0, 1),   // 3 - Bottom
        new Vector2(-0.87f, 0.5f),  // 4 - Bottom-left
        new Vector2(-0.87f, -0.5f)   // 5 - Top-left
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw default inspector with Odin

        Tile tile = (Tile)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Hex Edge Selector", EditorStyles.boldLabel);

        float size = 50f;
        float rightOffset = 100f; // Move UI further right
        Vector2 center = new Vector2(EditorGUIUtility.currentViewWidth / 2 + rightOffset, 100);

        GUILayout.BeginVertical("box");
        GUILayout.Space(120); // Reserve space for the hex grid
        Rect rect = GUILayoutUtility.GetLastRect();

        Handles.BeginGUI();
        for (int i = 0; i < tile.Edges.Length; i++)
        {
            Vector2 pos = hexOffsets[i] * size + new Vector2(rect.center.x + rightOffset, rect.y + 60);

            Handles.EndGUI(); // End GUI to prevent layout issues

            tile.Edges[i].Type = (EdgeType)EditorGUI.EnumPopup(
                new Rect(pos.x - 25, pos.y - 15, 80f, 20f),
                tile.Edges[i].Type
            );

            Handles.BeginGUI(); // Restart GUI for next loop
        }
        Handles.EndGUI();

        GUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tile); // Mark object as changed
        }
    }
}
#endif
