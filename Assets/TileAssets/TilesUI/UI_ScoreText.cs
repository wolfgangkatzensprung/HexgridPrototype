using TMPro;
using UnityEngine;

public class UI_ScoreText : MonoBehaviour
{
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Game.Instance.ScoreGained += OnScoreGained;
    }

    private void Destroy()
    {
        if (Game.Instance == null) return;

        Game.Instance.ScoreGained -= OnScoreGained;
    }

    private void OnScoreGained(int _scoreDelta)
    {
        text.text = Game.Instance.Score.ToString();
    }
}
