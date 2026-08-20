public enum MoveType
{
    Normal,
    Capture,
    CastleKingSide,
    CastleQueenSide,
    EnPassant,
    Promotion
}

public struct Move
{
    public int From;
    public int To;
    public MoveType Type;

    public Move(
        int from,
        int to,
        MoveType type = MoveType.Normal)
    {
        From = from;
        To = to;
        Type = type;
    }
}