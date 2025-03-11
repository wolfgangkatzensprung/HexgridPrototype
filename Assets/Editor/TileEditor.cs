using Sirenix.OdinInspector.Editor;
using System.Linq;
using UnityEditor;
using UnityEngine;

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

        if (GUILayout.Button("Set Edge Materials From Data"))
        {
            SetEdgeMaterialsFromData();
        }

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

    private void SetEdgeMaterialsFromData()
    {
        var tile = serializedObject.targetObject as Tile;
        var renderer = tile.gameObject.GetComponentInChildren<Renderer>();
        var materials = new Material[7];
        var builderData = tile.builderData;
        var edges = tile.Edges;
        
        for (int i = 0; i < 6; i++)
        {
            materials[i] = (builderData.materialsLookup[edges[i].Type]);
            Debug.Log($"{i} = {materials[i]}");
        }

        materials[6] = (builderData.materialsLookup[EdgeType.None]);  // add the center ground material
        renderer.SetMaterials(materials.ToList());
    }
}

