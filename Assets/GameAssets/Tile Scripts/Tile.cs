using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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

    public Hex hex { get; private set; }
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
    /// Setup and place the Tile if it follows all tile placement rules
    /// </summary>
    /// <param name="hex">Hex Coordinates</param>
    /// <param name="worldPosition">Transform Coordinates</param>
    /// <param name="parent">Tiles Holder</param>
    /// <param name="defaultMaterial">Material to switch to, when placed</param>
    public bool TryPlace(Hex hex, HexGrid hexGrid)
    {
        if (!hexGrid.CanBePlaced(hex, this)) return false;

        Setup(hex, hexGrid);

        Place(hex);

        return true;
    }

    private void Setup(Hex hex, HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
        this.text = GetComponentInChildren<TMP_Text>();
        this.hex = hex;
    }
    public void ForceSetup(Hex hex, HexGrid grid) => Setup(hex, grid);

    public void Place(Hex hex)
    {
        hexGrid.AddTile(hex, this);

        Vector3 worldPosition = hexGrid.HexToWorld(hex);
        transform.position = worldPosition;

        ResetMaterials();

        //SetupCoordinatesText(hex);
        if (TrySetupScoreTexts(hex, 100, out int neighbourCount))
        {
            Game.Instance.Score += 100 * neighbourCount;
        }

        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        var neighborsHex = hexGrid.GetOccupiedNeighbours(hex);

        foreach (var nHex in neighborsHex)
        {
            int sharedEdge = HexUtils.GetSharedEdgeIndex(hex, nHex);
            int rotatedEdge = (sharedEdge + CurrentRotation) % 6;
            var edgeType = Edges[rotatedEdge].Type;

            Debug.Log($"{nHex} is neighbour of {hex}. Shared edge index is {sharedEdge}. So due to current rotation {CurrentRotation}, edge {rotatedEdge} is matching {edgeType}");

            if (Level.Instance.IsAreaCompleted(hex, edgeType))
            {
                Debug.Log($"Area completed! - {edgeType}");
                Game.Instance.ReceiveTileReward();
            }
        }
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

    public void AddRotation(float angle)
    {
        transform.Rotate(0, angle, 0);
        CurrentRotation -= (int)Mathf.Sign(angle);
    }

    public void ResetRotation()
    {
        _currentRotation = 0;
    }

    private string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public bool Matches(Tile other, int sharedEdgeIndex)
    {
        int edgeIndex = (sharedEdgeIndex + CurrentRotation) % 6;
        Edge thisEdge = this.Edges[edgeIndex];

        int otherEdgeIndex = (sharedEdgeIndex + 3 + other.CurrentRotation) % 6; // opposite edge
        Edge otherEdge = other.Edges[otherEdgeIndex];
        return thisEdge.Type == otherEdge.Type;
    }

    /// <summary>
    /// Returns true when all edges of this type are connected to other tiles
    /// </summary>
    internal bool IsClosed(EdgeType type)
    {
        var tile = hexGrid.Board[hex];

        for (int edgeIndex = 0; edgeIndex < tile.Edges.Length; edgeIndex++)
        {
            if (tile.Edges[edgeIndex].Type == type)
            {
                int rotatedEdge = (edgeIndex + tile.CurrentRotation) % 6;
                var neighborHex = HexUtils.GetNeighbour(hex, rotatedEdge);

                if (!hexGrid.HasTile(hex)) return false;
            }
        }

        return true;
    }
}
