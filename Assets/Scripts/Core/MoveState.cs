public class MoveState
{
    public Move move;

    // Pieces
    public Piece movedPiece;
    public Piece capturedPiece;

    // Square where the captured piece actually came from.
    // Normally this is move.To.
    // For En Passant, it is the pawn's square beside move.To.
    public int capturedSquare;

    // Promotion
    public PieceType promotedPiece;

    // Castling rook movement
    public int rookFrom;
    public int rookTo;
    public Piece rookPiece;

    // Special rules state
    public int previousEnPassantSquare;

    public bool previousWhiteKingMoved;
    public bool previousBlackKingMoved;

    public bool previousWhiteKingsideRookMoved;
    public bool previousWhiteQueensideRookMoved;

    public bool previousBlackKingsideRookMoved;
    public bool previousBlackQueensideRookMoved;

    // Draw state
    public int previousHalfmoveClock;

    public PieceColor previousTurn;

    public string positionKeyAfterMove;

    public bool previousGameOver;


    public MoveState(Move move)
    {
        this.move = move;

        capturedSquare = -1;

        promotedPiece = PieceType.None;

        rookFrom = -1;
        rookTo = -1;

        rookPiece =
            new Piece(
                PieceType.None,
                PieceColor.White
            );
    }
}