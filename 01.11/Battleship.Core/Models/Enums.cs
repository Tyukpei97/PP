namespace Battleship.Core.Models;

public enum CellState
{
    Unknown = 0,
    Empty = 1,
    Ship = 2,
    Hit = 3,
    Miss = 4,
    Sunk = 5
}

public enum ShotOutcome
{
    Invalid,
    Repeat,
    Miss,
    Hit,
    Sunk
}

public enum GamePhase
{
    Lobby,
    Placement,
    Battle,
    GameOver
}

public enum PlayerRole
{
    Server,
    Client
}

public enum Orientation
{
    Horizontal,
    Vertical
}
