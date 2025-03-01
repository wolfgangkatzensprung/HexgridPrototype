#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Tile : MonoBehaviour
{
    HexGrid hexGrid;
    TMP_Text text;

    MeshRenderer[] renderers;
    Material[][] startMaterials;

    [HideInInspector]
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

    private void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();

        startMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            startMaterials[i] = renderers[i].materials;
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
    public void Place(Hex hex, Vector3 worldPosition, Transform parent)
    {
        hexGrid = FindAnyObjectByType<HexGrid>();
        text = GetComponentInChildren<TMP_Text>();

        this.hexPosition = hex;

        transform.position = worldPosition;
        transform.parent = parent;

        ResetMaterials();

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

    private void ResetMaterials()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                renderers[i].SetMaterials(startMaterials[i].ToList());
            }
        }
    }

    public void ApplyHighlightMaterialToBase(Material mat)
    {
        if (renderers == null) return;

        Material[] mats = renderers[0].materials;
        for (int j = 0; j < mats.Length; j++)
        {
            mats[j] = mat;
        }
        renderers[0].SetMaterials(mats.ToList());
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
        var scores = new List<(Vector3, int)>();
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
    public EdgeType Type;

    public int Region;

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
    new Vector2(-0.5f, -0.87f),     // 0 = top
    new Vector2(0.5f, -0.87f),      // 1 = top right
    new Vector2(1, 0),              // 2 = bottom right
    new Vector2(0.5f, 0.87f),       // 3 = bottom
    new Vector2(-0.5f, 0.87f),      // 4 = bottom left
    new Vector2(-1, 0)              // 5 = top left
    };


    private Color GetEdgeColor(EdgeType type)
    {
        switch (type)
        {
            case EdgeType.River: return Color.blue;
            case EdgeType.Street: return new Color(0.5f, 0.25f, 0f);  // Brown color for street
            case EdgeType.Castle: return Color.red;
            case EdgeType.Forest: return Color.green;
            default: return Color.white;
        }
    }

    // Helper to determine the color for the inside edge visual element
    private Color GetEdgeVisualColor(EdgeType type)
    {
        switch (type)
        {
            case EdgeType.River: return Color.cyan;  // Light blue for river
            case EdgeType.Street: return new Color(0.7f, 0.4f, 0f);  // Darker brown for street
            case EdgeType.Castle: return Color.yellow;  // Bright yellow for castle
            case EdgeType.Forest: return Color.green;  // Green for forest
            default: return Color.gray;  // Default neutral color
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Tile tile = (Tile)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Hex Edge Selector", EditorStyles.boldLabel);

        float size = 50f;
        float rightOffset = 100f;

        GUILayout.BeginVertical("box");
        GUILayout.Space(180);
        Rect rect = GUILayoutUtility.GetLastRect();

        Handles.BeginGUI();

        Vector2[] worldPositions = DrawPreviewHexagon(tile, size, rightOffset, rect);

        DrawEdgeTypeSelectionHexagon(tile, worldPositions);

        DrawEdgeTypeSelectionToggles(tile, worldPositions);

        Handles.EndGUI();

        GUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tile);
        }
    }

    private void DrawEdgeTypeSelectionToggles(Tile tile, Vector2[] worldPositions)
    {
        // Edge type and region selection buttons with index
        for (int i = 0; i < tile.Edges.Length; i++)
        {
            Vector2 pos = worldPositions[i];
            EdgeType[] edgeValues = (EdgeType[])System.Enum.GetValues(typeof(EdgeType));
            GUILayout.BeginHorizontal();

            // Edge Type Toggle Buttons
            foreach (EdgeType type in edgeValues)
            {
                Color prevColor = GUI.backgroundColor;
                if (tile.Edges[i].Type == type)
                {
                    GUI.backgroundColor = GetEdgeColor(type);
                }

                if (GUILayout.Toggle(tile.Edges[i].Type == type, type.ToString(), "Button", GUILayout.Width(80)))
                {
                    tile.Edges[i].Type = type;
                }

                GUI.backgroundColor = prevColor;
            }

            GUILayout.Space(10); // Add space between edge type and region field

            // Region selection input field
            GUILayout.Label($"Region {i}", GUILayout.Width(60));  // Label for the region field
            tile.Edges[i].Region = EditorGUILayout.IntField(tile.Edges[i].Region, GUILayout.Width(60));

            GUILayout.EndHorizontal();
        }
    }

    private Vector2[] DrawPreviewHexagon(Tile tile, float size, float rightOffset, Rect rect)
    {
        Vector2[] worldPositions = new Vector2[hexOffsets.Length];
        for (int i = 0; i < hexOffsets.Length; i++)
        {
            worldPositions[i] = hexOffsets[i] * size + new Vector2(rect.center.x + rightOffset, rect.y + 80);
        }

        // Glow effect - drawing a thick hexagon outline
        Handles.color = new Color(0f, 0f, 0f, 0.2f);  // Transparent black for glow
        for (int i = 0; i < worldPositions.Length; i++)
        {
            Vector2 start = worldPositions[i];
            Vector2 end = worldPositions[(i + 1) % worldPositions.Length];
            Handles.DrawAAPolyLine(8, start, end);  // Thicker line for glow
        }

        // Drawing the actual hexagon lines
        for (int i = 0; i < worldPositions.Length; i++)
        {
            Vector2 start = worldPositions[i];
            Vector2 end = worldPositions[(i + 1) % worldPositions.Length];
            Handles.color = GetEdgeColor(tile.Edges[i].Type);
            Handles.DrawAAPolyLine(4, start, end);
        }

        // Adding a small dot inside the hexagon near each edge and displaying the edge index
        for (int i = 0; i < worldPositions.Length; i++)
        {
            // Get the position for the inside of the edge (just inside the line)
            Vector2 start = worldPositions[i];
            Vector2 end = worldPositions[(i + 1) % worldPositions.Length];
            Vector2 edgeCenter = (start + end) / 2;

            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            Vector2 dotPosition = edgeCenter - perpendicular * 8f;

            Handles.color = GetEdgeVisualColor(tile.Edges[i].Type);
            Handles.DrawSolidDisc(dotPosition, Vector3.forward, 5);     // Draw little dot

            // Display edge index next to the dot
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            Vector2 labelPosition = edgeCenter + perpendicular * 8f;
            Handles.Label(labelPosition, i.ToString(), labelStyle);
        }

        return worldPositions;
    }

    private void DrawEdgeTypeSelectionHexagon(Tile tile, Vector2[] worldPositions)
    {
        for (int i = 0; i < tile.Edges.Length; i++)
        {
            Vector2 start = worldPositions[i];
            Vector2 end = worldPositions[(i + 1) % worldPositions.Length];
            Vector2 pos = (start + end) / 2;

            float offset = 200f;

            Color edgeColor = GetEdgeVisualColor(tile.Edges[i].Type);
            if (tile.Edges[i].Type != EdgeType.None)
            {
                GUI.backgroundColor = edgeColor;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            tile.Edges[i].Type = (EdgeType)EditorGUI.EnumPopup(
                new Rect(pos.x - 25 + offset, pos.y - 15, 80f, 20f),
                tile.Edges[i].Type
            );

            // Reset the background color to default after the EnumPopup
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif

