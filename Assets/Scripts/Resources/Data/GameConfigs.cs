using System;

[Serializable]
public class GameConfigs
{
    public int maxRounds;
    public int timePerPlayerRound;
    public GameSettings gameSettings;
}

[Serializable]
public class GameSettings
{
    public int boardSizeX;
    public int boardSizeY;
    public int turnTimeLimit;
    public int maxMovesPerGame;
}
