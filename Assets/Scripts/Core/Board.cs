using System;

public class Board
{
    public const int BoardSize = 8;
    public const int SquareCount = 64;

    private Piece[] squares = new Piece[SquareCount];

    public Piece GetPiece(int square)
    {
        return squares[square];
    }

    public void SetPiece(int square, Piece piece)
    {
        squares[square] = piece;
    }

    public void RemovePiece(int square)
    {
        squares[square] =
            new Piece(
                PieceType.None,
                PieceColor.White
            );
    }

    public void ClearBoard()
    {
        for (int i = 0; i < SquareCount; i++)
        {
            squares[i] = new Piece(PieceType.None, PieceColor.White);
        }
    }

    public void SetStartingPosition()
    {
        ClearBoard();

        // White back rank
        squares[0] = new Piece(PieceType.Rook, PieceColor.White);
        squares[1] = new Piece(PieceType.Knight, PieceColor.White);
        squares[2] = new Piece(PieceType.Bishop, PieceColor.White);
        squares[3] = new Piece(PieceType.Queen, PieceColor.White);
        squares[4] = new Piece(PieceType.King, PieceColor.White);
        squares[5] = new Piece(PieceType.Bishop, PieceColor.White);
        squares[6] = new Piece(PieceType.Knight, PieceColor.White);
        squares[7] = new Piece(PieceType.Rook, PieceColor.White);

        // White pawns
        for (int file = 0; file < 8; file++)
        {
            squares[8 + file] = new Piece(
                PieceType.Pawn,
                PieceColor.White
            );
        }

        // Black back rank
        squares[56] = new Piece(PieceType.Rook, PieceColor.Black);
        squares[57] = new Piece(PieceType.Knight, PieceColor.Black);
        squares[58] = new Piece(PieceType.Bishop, PieceColor.Black);
        squares[59] = new Piece(PieceType.Queen, PieceColor.Black);
        squares[60] = new Piece(PieceType.King, PieceColor.Black);
        squares[61] = new Piece(PieceType.Bishop, PieceColor.Black);
        squares[62] = new Piece(PieceType.Knight, PieceColor.Black);
        squares[63] = new Piece(PieceType.Rook, PieceColor.Black);

        // Black pawns
        for (int file = 0; file < 8; file++)
        {
            squares[48 + file] = new Piece(
                PieceType.Pawn,
                PieceColor.Black
            );
        }
    }

    public void MakeMove(Move move)
    {
        squares[move.To] = squares[move.From];

        squares[move.From] =
            new Piece(PieceType.None, PieceColor.White);
    }

    public Board Copy()
    {
        Board copy = new Board();

        for (int i = 0; i < SquareCount; i++)
        {
            copy.squares[i] = this.squares[i];
        }

        return copy;
    }
}
