using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PrimeTween;

public class TileScoreDisplay : MonoBehaviour
{
    public GameObject scoreTextPrefab;
    private List<TMP_Text> activeScoreTexts = new List<TMP_Text>();

    public void ShowScores(List<(Vector3 position, int score)> edgeScores)
    {
        float delay = 0f;
        float delayIncrement = 0.1f;

        foreach (var edge in edgeScores)
        {
            Tween.Delay(delay, () =>
            {
                GameObject textObj = Instantiate(scoreTextPrefab, transform);
                textObj.transform.position = edge.position;
                TMP_Text textComponent = textObj.GetComponent<TMP_Text>();
                textComponent.text = edge.score.ToString();
                activeScoreTexts.Add(textComponent);
            });

            delay += delayIncrement;
            delayIncrement *= 1.1f;
        }
    }

    private void Update()
    {
        foreach (var text in activeScoreTexts)
        {
            text.transform.LookAt(Camera.main.transform);
            text.transform.rotation = Quaternion.LookRotation(text.transform.position - Camera.main.transform.position);
        }
    }
}