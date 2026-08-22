public class MoveState
{
    public Move move;


    // Pieces
    public Piece movedPiece;
    public Piece capturedPiece;


    // Special rules state
    public int previousEnPassantSquare;


    public bool previousWhiteKingMoved;
    public bool previousBlackKingMoved;

    public bool previousWhiteKingsideRookMoved;
    public bool previousWhiteQueensideRookMoved;

    public bool previousBlackKingsideRookMoved;
    public bool previousBlackQueensideRookMoved;


    public int previousHalfmoveClock;

    public PieceColor previousTurn;

    public string positionKeyAfterMove;

    public bool previousGameOver;

    public MoveState(
        Move move
    )
    {
        this.move = move;
    }
}