using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    private int score = 0;
    public int Score { get { return score; }
        set
        { 
            if (score != value)
            {
                score = value;
                ScoreGained?.Invoke(value);
            }
        }
    }

    public Action<int> ScoreGained;

    private void Awake()
    {
        Instance = this;
    }
}
