using System.Collections.Generic;

public class ChessEngine
{
    public Board board;

    public PieceColor currentTurn;

    public int enPassantSquare = -1;


    public ChessEngine()
    {
        board = new Board();

        board.SetStartingPosition();

        currentTurn = PieceColor.White;
    }


    public List<Move> GetLegalMoves(
        int square
    )
    {
        return MoveGenerator.GenerateMoves(
            board,
            square,
            enPassantSquare
        );
    }


    public void MakeMove(
        Move move
    )
    {
        board.MakeMove(move);

        SwitchTurn();
    }


    public void UndoMove()
    {
        // Undo is still handled
        // by ChessGameManager for now.
    }


    private void SwitchTurn()
    {
        currentTurn =
            currentTurn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;
    }

    public bool IsMoveLegal(
        int from,
        int to
    )
    {
        List<Move> moves =
            MoveGenerator.GenerateMoves(
                board,
                from,
                enPassantSquare
            );


        foreach(Move move in moves)
        {
            if(move.To != to)
                continue;


            Board testBoard =
                board.Copy();


            testBoard.MakeMove(move);


            if(!IsKingInCheck(
                board.GetPiece(from).color
            ))
            {
                return true;
            }
        }


        return false;
    }

    public bool IsKingInCheck(
        PieceColor color
    )
    {
        int kingSquare = -1;


        // Find king
        for(int i = 0; i < 64; i++)
        {
            Piece piece =
                board.GetPiece(i);


            if(!piece.IsEmpty() &&
            piece.type == PieceType.King &&
            piece.color == color)
            {
                kingSquare = i;
                break;
            }
        }


        if(kingSquare == -1)
            return false;


        PieceColor enemyColor =
            color == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;


        return AttackDetector.IsSquareAttacked(
            board,
            kingSquare,
            enemyColor
        );
    }
}