using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 6;

    public int BoardSizeY = 5;

    public int MatchesMin = 3;

    public float TimeAttackSeconds = 60f;
}
