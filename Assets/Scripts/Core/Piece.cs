[System.Serializable]
public struct Piece
{
    public PieceType type;
    public PieceColor color;

    public Piece(PieceType type, PieceColor color)
    {
        this.type = type;
        this.color = color;
    }

    public bool IsEmpty()
    {
        return type == PieceType.None;
    }
}