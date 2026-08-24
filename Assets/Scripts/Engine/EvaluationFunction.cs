using System;
using System.Collections.Generic;

public static class EvaluationFunction
{
    // =====================================================
    // MATERIAL VALUES
    // =====================================================

    public const int PawnValue = 100;
    public const int KnightValue = 320;
    public const int BishopValue = 330;
    public const int RookValue = 500;
    public const int QueenValue = 900;
    public const int KingValue = 0;


    // =====================================================score += EvaluateMobility(board);
    // PIECE-SQUARE TABLES
    //
    // Values are written from White's perspective,
    // starting from White's back rank.
    //
    // Positive = good square
    // Negative = bad square
    // =====================================================

    private static readonly int[] PawnTable =
    {
         0,   0,   0,   0,   0,   0,   0,   0,
         5,  10,  10, -20, -20,  10,  10,   5,
         5,  -5, -10,   0,   0, -10,  -5,   5,
         0,   0,   0,  20,  20,   0,   0,   0,
         5,   5,  10,  25,  25,  10,   5,   5,
        10,  10,  20,  30,  30,  20,  10,  10,
        50,  50,  50,  50,  50,  50,  50,  50,
         0,   0,   0,   0,   0,   0,   0,   0
    };


    private static readonly int[] KnightTable =
    {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -30,   5,  10,  15,  15,  10,   5, -30,
        -30,   0,  15,  20,  20,  15,   0, -30,
        -30,   5,  15,  20,  20,  15,   5, -30,
        -30,   0,  10,  15,  15,  10,   0, -30,
        -40, -20,   0,   0,   0,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };


    private static readonly int[] BishopTable =
    {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10,   5,   0,   0,   0,   0,   5, -10,
        -10,  10,  10,  10,  10,  10,  10, -10,
        -10,   0,  10,  10,  10,  10,   0, -10,
        -10,   5,   5,  10,  10,   5,   5, -10,
        -10,   0,   5,  10,  10,   5,   0, -10,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    };


    private static readonly int[] RookTable =
    {
         0,   0,   0,   5,   5,   0,   0,   0,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
         5,  10,  10,  10,  10,  10,  10,   5,
         0,   0,   0,   0,   0,   0,   0,   0
    };


    private static readonly int[] QueenTable =
    {
        -20, -10, -10,   0,   0, -10, -10, -20,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,   5,   5,   5,   5,   0, -10,
          0,   0,   5,   5,   5,   5,   0,   -5,
          0,   0,   5,   5,   5,   5,   0,   -5,
        -10,   5,   5,   5,   5,   5,   0, -10,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -20, -10, -10,   0,   0, -10, -10, -20
    };


    private static readonly int[] KingTable =
    {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
         20,  20,   0,   0,   0,   0,  20,  20,
         20,  30,  10,   0,   0,  10,  30,  20
    };


    // =====================================================
    // MAIN EVALUATION
    // =====================================================

    public static int Evaluate(Board board)
    {
        int score = 0;


        for (int square = 0;
             square < Board.SquareCount;
             square++)
        {
            Piece piece =
                board.GetPiece(square);


            if (piece.IsEmpty())
                continue;


            int materialValue =
                GetPieceValue(piece.type);


            int positionalValue =
                GetPositionValue(
                    piece,
                    square
                );


            int totalValue =
                materialValue +
                positionalValue;


            if (piece.color ==
                PieceColor.White)
            {
                score += totalValue;
            }
            else
            {
                score -= totalValue;
            }
        }

        score += EvaluateMobility(board);
        return score;
    }


    // =====================================================
    // MATERIAL VALUE
    // =====================================================

    private static int GetPieceValue(
        PieceType type
    )
    {
        switch (type)
        {
            case PieceType.Pawn:
                return PawnValue;

            case PieceType.Knight:
                return KnightValue;

            case PieceType.Bishop:
                return BishopValue;

            case PieceType.Rook:
                return RookValue;

            case PieceType.Queen:
                return QueenValue;

            case PieceType.King:
                return KingValue;

            default:
                return 0;
        }
    }


    // =====================================================
    // POSITIONAL VALUE
    // =====================================================

    private static int GetPositionValue(
        Piece piece,
        int square
    )
    {
        int tableSquare =
            GetTableSquare(
                piece.color,
                square
            );


        switch (piece.type)
        {
            case PieceType.Pawn:
                return PawnTable[tableSquare];

            case PieceType.Knight:
                return KnightTable[tableSquare];

            case PieceType.Bishop:
                return BishopTable[tableSquare];

            case PieceType.Rook:
                return RookTable[tableSquare];

            case PieceType.Queen:
                return QueenTable[tableSquare];

            case PieceType.King:
                return KingTable[tableSquare];

            default:
                return 0;
        }
    }


    // =====================================================
    // BOARD ORIENTATION
    // =====================================================

    private static int GetTableSquare(
        PieceColor color,
        int square
    )
    {
        int file =
            square % 8;

        int rank =
            square / 8;


        // Flip the board for Black so
        // both sides use the same table
        // from their own perspective.

        if (color == PieceColor.Black)
        {
            rank = 7 - rank;
        }


        return rank * 8 + file;
    }

    // =====================================================
    // MOBILITY
    // =====================================================

    private const int MobilityWeight = 5;

    private static int EvaluateMobility(
        Board board
    )
    {
        int whiteMoves = 0;
        int blackMoves = 0;

        for (int square = 0;
            square < Board.SquareCount;
            square++)
        {
            Piece piece =
                board.GetPiece(square);

            if (piece.IsEmpty())
                continue;

            List<Move> moves =
                MoveGenerator.GenerateMoves(
                    board,
                    square,
                    -1
                );

            if (piece.color == PieceColor.White)
            {
                whiteMoves += moves.Count;
            }
            else
            {
                blackMoves += moves.Count;
            }
        }

        return
            (whiteMoves - blackMoves)
            * MobilityWeight;
    }
}