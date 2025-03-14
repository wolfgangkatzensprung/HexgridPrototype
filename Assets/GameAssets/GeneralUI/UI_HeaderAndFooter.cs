using TMPro;
using UnityEngine;

public class UI_HeaderAndFooter : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text tilesText;

    private void Start()
    {
        Game.Instance.ScoreGained += OnScoreGained;
        Game.Instance.TileCountChanged += OnTileCountChanged;
    }

    private void OnDestroy()
    {
        if (Game.Instance == null) return;

        Game.Instance.ScoreGained -= OnScoreGained;
    }

    private void OnScoreGained(int _scoreDelta)
    {
        scoreText.text = Game.Instance.Score.ToString();
    }

    private void OnTileCountChanged(int tileAmount)
    {
        tilesText.text = tileAmount.ToString();
    }
}
