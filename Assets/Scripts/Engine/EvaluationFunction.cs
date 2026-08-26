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


    // =====================================================
    // MOBILITY
    // =====================================================

    private const int MobilityWeight = 4;


    // =====================================================
    // KING SAFETY
    // =====================================================

    private const int KingAttackWeight = 18;
    private const int PawnShieldWeight = 12;
    private const int CheckPenalty = 120;


    // =====================================================
    // PAWN STRUCTURE
    // =====================================================

    private const int DoubledPawnPenalty = 12;
    private const int IsolatedPawnPenalty = 12;
    private const int PassedPawnBonus = 35;
    private const int ConnectedPawnBonus = 8;


    // =====================================================
    // PIECE-SQUARE TABLES
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
          0,   0,   5,   5,   5,   5,   0,  -5,
          0,   0,   5,   5,   5,   5,   0,  -5,
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

    public static int Evaluate(
        Board board
    )
    {
        int score = 0;


        // -------------------------------------------------
        // MATERIAL + PIECE-SQUARE TABLES
        // -------------------------------------------------

        for (
            int square = 0;
            square < Board.SquareCount;
            square++
        )
        {
            Piece piece =
                board.GetPiece(square);


            if (piece.IsEmpty())
                continue;


            int materialValue =
                GetPieceValue(
                    piece.type
                );


            int positionalValue =
                GetPositionValue(
                    piece,
                    square
                );


            int totalValue =
                materialValue +
                positionalValue;


            if (
                piece.color ==
                PieceColor.White
            )
            {
                score += totalValue;
            }
            else
            {
                score -= totalValue;
            }
        }


        // -------------------------------------------------
        // MOBILITY
        // -------------------------------------------------

        score +=
            EvaluateMobility(
                board
            );


        // -------------------------------------------------
        // KING SAFETY
        // -------------------------------------------------

        score +=
            EvaluateKingSafety(
                board
            );


        // -------------------------------------------------
        // PAWN STRUCTURE
        // -------------------------------------------------

        score +=
            EvaluatePawnStructure(
                board
            );


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


        if (
            color ==
            PieceColor.Black
        )
        {
            rank =
                7 - rank;
        }


        return
            rank * 8 +
            file;
    }


    // =====================================================
    // MOBILITY EVALUATION
    // =====================================================

    private static int EvaluateMobility(
        Board board
    )
    {
        int whiteMoves = 0;
        int blackMoves = 0;


        for (
            int square = 0;
            square < Board.SquareCount;
            square++
        )
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


            if (
                piece.color ==
                PieceColor.White
            )
            {
                whiteMoves +=
                    moves.Count;
            }
            else
            {
                blackMoves +=
                    moves.Count;
            }
        }


        return
            (
                whiteMoves -
                blackMoves
            )
            * MobilityWeight;
    }


    // =====================================================
    // KING SAFETY
    // =====================================================

    private static int EvaluateKingSafety(
        Board board
    )
    {
        int score = 0;


        score +=
            EvaluateKingSafetyForColor(
                board,
                PieceColor.White
            );


        score +=
            EvaluateKingSafetyForColor(
                board,
                PieceColor.Black
            );


        return score;
    }


    private static int EvaluateKingSafetyForColor(
        Board board,
        PieceColor color
    )
    {
        int kingSquare =
            FindKing(
                board,
                color
            );


        if (kingSquare == -1)
            return 0;


        PieceColor enemyColor =
            color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;


        int score = 0;


        // -------------------------------------------------
        // CHECK PENALTY
        // -------------------------------------------------

        if (
            IsSquareAttacked(
                board,
                kingSquare,
                enemyColor
            )
        )
        {
            score -=
                CheckPenalty;
        }


        // -------------------------------------------------
        // ATTACKS AROUND KING
        // -------------------------------------------------

        int kingFile =
            kingSquare % 8;

        int kingRank =
            kingSquare / 8;


        int enemyAttacks = 0;


        for (
            int fileOffset = -1;
            fileOffset <= 1;
            fileOffset++
        )
        {
            for (
                int rankOffset = -1;
                rankOffset <= 1;
                rankOffset++
            )
            {
                if (
                    fileOffset == 0 &&
                    rankOffset == 0
                )
                {
                    continue;
                }


                int file =
                    kingFile +
                    fileOffset;

                int rank =
                    kingRank +
                    rankOffset;


                if (
                    file < 0 ||
                    file >= 8 ||
                    rank < 0 ||
                    rank >= 8
                )
                {
                    continue;
                }


                int square =
                    rank * 8 +
                    file;


                if (
                    IsSquareAttacked(
                        board,
                        square,
                        enemyColor
                    )
                )
                {
                    enemyAttacks++;
                }
            }
        }


        score -=
            enemyAttacks *
            KingAttackWeight;


        // -------------------------------------------------
        // PAWN SHIELD
        // -------------------------------------------------

        int shield =
            CountPawnShield(
                board,
                kingSquare,
                color
            );


        score +=
            shield *
            PawnShieldWeight;


        // Convert Black's safety
        // into White-positive scoring.

        if (
            color ==
            PieceColor.Black
        )
        {
            score = -score;
        }


        return score;
    }


    // =====================================================
    // FIND KING
    // =====================================================

    private static int FindKing(
        Board board,
        PieceColor color
    )
    {
        for (
            int square = 0;
            square < Board.SquareCount;
            square++
        )
        {
            Piece piece =
                board.GetPiece(square);


            if (
                !piece.IsEmpty() &&
                piece.type == PieceType.King &&
                piece.color == color
            )
            {
                return square;
            }
        }


        return -1;
    }


    // =====================================================
    // ATTACK DETECTION
    // =====================================================

    private static bool IsSquareAttacked(
        Board board,
        int square,
        PieceColor attacker
    )
    {
        return
            AttackDetector.IsSquareAttacked(
                board,
                square,
                attacker
            );
    }


    // =====================================================
    // PAWN SHIELD
    // =====================================================

    private static int CountPawnShield(
        Board board,
        int kingSquare,
        PieceColor color
    )
    {
        int kingFile =
            kingSquare % 8;

        int kingRank =
            kingSquare / 8;


        int direction =
            color == PieceColor.White
                ? 1
                : -1;


        int pawnRank =
            kingRank +
            direction;


        if (
            pawnRank < 0 ||
            pawnRank >= 8
        )
        {
            return 0;
        }


        int shield = 0;


        for (
            int fileOffset = -1;
            fileOffset <= 1;
            fileOffset++
        )
        {
            int file =
                kingFile +
                fileOffset;


            if (
                file < 0 ||
                file >= 8
            )
            {
                continue;
            }


            int square =
                pawnRank * 8 +
                file;


            Piece piece =
                board.GetPiece(square);


            if (
                !piece.IsEmpty() &&
                piece.type == PieceType.Pawn &&
                piece.color == color
            )
            {
                shield++;
            }
        }


        return shield;
    }


    // =====================================================
    // PAWN STRUCTURE
    // =====================================================

    private static int EvaluatePawnStructure(
        Board board
    )
    {
        int whiteScore =
            EvaluatePawnStructureForColor(
                board,
                PieceColor.White
            );


        int blackScore =
            EvaluatePawnStructureForColor(
                board,
                PieceColor.Black
            );


        return
            whiteScore -
            blackScore;
    }


    // =====================================================
    // PAWN STRUCTURE FOR ONE COLOR
    // =====================================================

    private static int EvaluatePawnStructureForColor(
        Board board,
        PieceColor color
    )
    {
        int score = 0;


        int[] pawnsPerFile =
            new int[8];


        // -------------------------------------------------
        // COUNT PAWNS ON EACH FILE
        // -------------------------------------------------

        for (
            int square = 0;
            square < Board.SquareCount;
            square++
        )
        {
            Piece piece =
                board.GetPiece(square);


            if (
                !piece.IsEmpty() &&
                piece.type == PieceType.Pawn &&
                piece.color == color
            )
            {
                int file =
                    square % 8;


                pawnsPerFile[file]++;
            }
        }


        // -------------------------------------------------
        // DOUBLED PAWNS
        // -------------------------------------------------

        for (
            int file = 0;
            file < 8;
            file++
        )
        {
            if (
                pawnsPerFile[file] > 1
            )
            {
                int extraPawns =
                    pawnsPerFile[file] - 1;


                score -=
                    extraPawns *
                    DoubledPawnPenalty;
            }
        }


        // -------------------------------------------------
        // INDIVIDUAL PAWNS
        // -------------------------------------------------

        for (
            int square = 0;
            square < Board.SquareCount;
            square++
        )
        {
            Piece piece =
                board.GetPiece(square);


            if (
                piece.IsEmpty() ||
                piece.type != PieceType.Pawn ||
                piece.color != color
            )
            {
                continue;
            }


            int file =
                square % 8;

            int rank =
                square / 8;


            // -------------------------------------------------
            // ISOLATED PAWN
            // -------------------------------------------------

            bool hasAdjacentPawn =
                false;


            if (
                file > 0 &&
                pawnsPerFile[file - 1] > 0
            )
            {
                hasAdjacentPawn = true;
            }


            if (
                file < 7 &&
                pawnsPerFile[file + 1] > 0
            )
            {
                hasAdjacentPawn = true;
            }


            if (!hasAdjacentPawn)
            {
                score -=
                    IsolatedPawnPenalty;
            }


            // -------------------------------------------------
            // PASSED PAWN
            // -------------------------------------------------

            if (
                IsPassedPawn(
                    board,
                    file,
                    rank,
                    color
                )
            )
            {
                score +=
                    PassedPawnBonus;
            }


            // -------------------------------------------------
            // CONNECTED PAWN
            // -------------------------------------------------

            if (
                HasAdjacentPawn(
                    board,
                    file,
                    rank,
                    color
                )
            )
            {
                score +=
                    ConnectedPawnBonus;
            }
        }


        return score;
    }


    // =====================================================
    // PASSED PAWN
    // =====================================================

    private static bool IsPassedPawn(
        Board board,
        int file,
        int rank,
        PieceColor color
    )
    {
        PieceColor enemyColor =
            color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;


        int direction =
            color == PieceColor.White
                ? 1
                : -1;


        int currentRank =
            rank +
            direction;


        while (
            currentRank >= 0 &&
            currentRank < 8
        )
        {
            for (
                int fileOffset = -1;
                fileOffset <= 1;
                fileOffset++
            )
            {
                int checkFile =
                    file +
                    fileOffset;


                if (
                    checkFile < 0 ||
                    checkFile >= 8
                )
                {
                    continue;
                }


                int square =
                    currentRank * 8 +
                    checkFile;


                Piece piece =
                    board.GetPiece(square);


                if (
                    !piece.IsEmpty() &&
                    piece.type == PieceType.Pawn &&
                    piece.color == enemyColor
                )
                {
                    return false;
                }
            }


            currentRank +=
                direction;
        }


        return true;
    }


    // =====================================================
    // CONNECTED PAWN
    // =====================================================

    private static bool HasAdjacentPawn(
        Board board,
        int file,
        int rank,
        PieceColor color
    )
    {
        int[] adjacentFiles =
        {
            file - 1,
            file + 1
        };


        foreach (
            int adjacentFile
            in adjacentFiles
        )
        {
            if (
                adjacentFile < 0 ||
                adjacentFile >= 8
            )
            {
                continue;
            }


            // Same rank
            int square =
                rank * 8 +
                adjacentFile;


            Piece piece =
                board.GetPiece(square);


            if (
                !piece.IsEmpty() &&
                piece.type == PieceType.Pawn &&
                piece.color == color
            )
            {
                return true;
            }


            // One rank forward
            int direction =
                color == PieceColor.White
                    ? 1
                    : -1;


            int forwardRank =
                rank +
                direction;


            if (
                forwardRank >= 0 &&
                forwardRank < 8
            )
            {
                square =
                    forwardRank * 8 +
                    adjacentFile;


                piece =
                    board.GetPiece(square);


                if (
                    !piece.IsEmpty() &&
                    piece.type == PieceType.Pawn &&
                    piece.color == color
                )
                {
                    return true;
                }
            }
        }


        return false;
    }
}