using PrimeTween;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class Game : Singleton<Game>
{
    [SerializeField] HexGrid grid;

    public ParticleSystem testParticles;

    const int TILES_AMOUNT = 10;

    [ShowInInspector]
    private int tileReserveCount = TILES_AMOUNT;
    public int TileCount
    {
        get
        {
            return tileReserveCount;
        }
        set
        {
            if (value != tileReserveCount)
            {
                tileReserveCount = value;
                TileCountChanged?.Invoke(value);
            }
        }
    }

    public bool HasTiles => tileReserveCount > 0;

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

    public Action<int> TileCountChanged;

    public Action<int> ScoreGained;

    private void Start()
    {
        PrimeTweenConfig.warnZeroDuration = false;
    }

    public void Restart()
    {
        Debug.Log($"Player lost with a score of {score}! Restarting ...");
        score = 0;
        TileCount = TILES_AMOUNT;
        grid.ResetAll();
    }

    internal void ReceiveTileReward()
    {
        tileReserveCount += 5;
        testParticles.Play();
    }
}
