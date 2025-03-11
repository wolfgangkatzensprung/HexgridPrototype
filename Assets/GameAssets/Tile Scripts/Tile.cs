using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Tile : MonoBehaviour
{
    HexGrid hexGrid;
    TMP_Text text;

#if UNITY_EDITOR
    public TileBuilderData builderData;
#endif

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

    [Header("Debug Colors")]
    public Color qColor = new Color(1f, 0.5f, 0.5f); // Default red
    public Color rColor = new Color(0.5f, 0.62f, 1f); // Default blue
    public Color sColor = new Color(0.5f, 1f, 0.5f); // Default green

    private void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();

        startMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            startMaterials[i] = renderers[i].materials;
        }
    }


    /// <summary>
    /// Try Setup and place the Tile
    /// </summary>
    /// <param name="hex">Hex Coordinates</param>
    /// <param name="worldPosition">Transform Coordinates</param>
    /// <param name="parent">Tiles Holder</param>
    /// <param name="defaultMaterial">Material to switch to, when placed</param>
    public bool TryPlace(Hex hex, Transform parent, HexGrid hexGrid)
    {
        Debug.Log($"Try place Tile {hex}");

        text = GetComponentInChildren<TMP_Text>();
        this.hexGrid = hexGrid;

        if (hexGrid.IsPositionOccupied(hex)) return false;
        Debug.Log($"Tile pos is not occupied");
        if (!hexGrid.HasNeighbours(hex)) return false;

        hexGrid.AddTile(hex, this);

        this.hexPosition = hex;

        Vector3 worldPosition = hexGrid.HexToWorld(hex);
        transform.position = worldPosition;
        transform.parent = parent;

        ResetMaterials();

        for (int i = 0; i < Edges.Length; i++)
        {
            Debug.Log($"Edge{i} is {Edges[i].ToString()}");
        }

        SetupCoordinatesText(hex);
        if (TrySetupScoreTexts(hex, 100, out int neighbourCount))
        {
            Game.Instance.Score += 100 * neighbourCount;
        }

        return true;
    }

    public void ResetMaterials()
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
