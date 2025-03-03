using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Workaround for serializing dictionary in Tile Custom Inspector. This object contains data relevant to building Tiles in the Editor.
/// </summary>
[CreateAssetMenu(fileName = "TileBuilderData", menuName = "Scriptable Objects/TileBuilderData")]
public class TileBuilderData : SerializedScriptableObject
{
    public Dictionary<EdgeType, Material> materialsLookup = new();
}
