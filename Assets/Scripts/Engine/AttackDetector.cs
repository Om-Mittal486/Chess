using System;

public static class AttackDetector
{
    public static bool IsSquareAttacked(
        Board board,
        int square,
        PieceColor attackingColor)
    {
        int file = square % 8;
        int rank = square / 8;

        // -------------------------------------------------
        // PAWN ATTACKS
        // -------------------------------------------------

        int pawnDirection =
            attackingColor == PieceColor.White ? -1 : 1;

        int pawnRank = rank + pawnDirection;

        if (pawnRank >= 0 && pawnRank < 8)
        {
            // Left pawn attack
            int leftFile = file - 1;

            if (leftFile >= 0)
            {
                int pawnSquare =
                    pawnRank * 8 + leftFile;

                Piece piece =
                    board.GetPiece(pawnSquare);

                if (piece.type == PieceType.Pawn &&
                    piece.color == attackingColor)
                {
                    return true;
                }
            }

            // Right pawn attack
            int rightFile = file + 1;

            if (rightFile < 8)
            {
                int pawnSquare =
                    pawnRank * 8 + rightFile;

                Piece piece =
                    board.GetPiece(pawnSquare);

                if (piece.type == PieceType.Pawn &&
                    piece.color == attackingColor)
                {
                    return true;
                }
            }
        }


        // -------------------------------------------------
        // KNIGHT ATTACKS
        // -------------------------------------------------

        int[,] knightDirections =
        {
            { 1, 2 },
            { 2, 1 },
            { 2, -1 },
            { 1, -2 },
            { -1, -2 },
            { -2, -1 },
            { -2, 1 },
            { -1, 2 }
        };

        for (int i = 0; i < 8; i++)
        {
            int targetFile =
                file + knightDirections[i, 0];

            int targetRank =
                rank + knightDirections[i, 1];

            if (!IsInsideBoard(targetFile, targetRank))
                continue;

            int targetSquare =
                targetRank * 8 + targetFile;

            Piece piece =
                board.GetPiece(targetSquare);

            if (piece.type == PieceType.Knight &&
                piece.color == attackingColor)
            {
                return true;
            }
        }


        // -------------------------------------------------
        // KING ATTACKS
        // -------------------------------------------------

        int[,] kingDirections =
        {
            { 1, 0 },
            { -1, 0 },
            { 0, 1 },
            { 0, -1 },

            { 1, 1 },
            { 1, -1 },
            { -1, 1 },
            { -1, -1 }
        };

        for (int i = 0; i < 8; i++)
        {
            int targetFile =
                file + kingDirections[i, 0];

            int targetRank =
                rank + kingDirections[i, 1];

            if (!IsInsideBoard(targetFile, targetRank))
                continue;

            int targetSquare =
                targetRank * 8 + targetFile;

            Piece piece =
                board.GetPiece(targetSquare);

            if (piece.type == PieceType.King &&
                piece.color == attackingColor)
            {
                return true;
            }
        }


        // -------------------------------------------------
        // BISHOP / QUEEN DIAGONALS
        // -------------------------------------------------

        int[,] diagonalDirections =
        {
            { 1, 1 },
            { 1, -1 },
            { -1, 1 },
            { -1, -1 }
        };

        if (IsSlidingPieceAttacking(
            board,
            square,
            attackingColor,
            diagonalDirections,
            PieceType.Bishop,
            PieceType.Queen))
        {
            return true;
        }


        // -------------------------------------------------
        // ROOK / QUEEN STRAIGHT LINES
        // -------------------------------------------------

        int[,] straightDirections =
        {
            { 1, 0 },
            { -1, 0 },
            { 0, 1 },
            { 0, -1 }
        };

        if (IsSlidingPieceAttacking(
            board,
            square,
            attackingColor,
            straightDirections,
            PieceType.Rook,
            PieceType.Queen))
        {
            return true;
        }


        return false;
    }


    private static bool IsSlidingPieceAttacking(
        Board board,
        int square,
        PieceColor attackingColor,
        int[,] directions,
        PieceType primaryPiece,
        PieceType secondaryPiece)
    {
        int file = square % 8;
        int rank = square / 8;

        for (int i = 0;
             i < directions.GetLength(0);
             i++)
        {
            int currentFile = file;
            int currentRank = rank;

            while (true)
            {
                currentFile += directions[i, 0];
                currentRank += directions[i, 1];

                if (!IsInsideBoard(
                    currentFile,
                    currentRank))
                {
                    break;
                }

                int targetSquare =
                    currentRank * 8 + currentFile;

                Piece piece =
                    board.GetPiece(targetSquare);

                // Empty square → continue looking
                if (piece.IsEmpty())
                    continue;

                // We hit a piece.
                // Check if it is the attacking piece.
                if (piece.color == attackingColor &&
                    (piece.type == primaryPiece ||
                     piece.type == secondaryPiece))
                {
                    return true;
                }

                // Any piece blocks the line.
                break;
            }
        }

        return false;
    }


    private static bool IsInsideBoard(
        int file,
        int rank)
    {
        return file >= 0 &&
               file < 8 &&
               rank >= 0 &&
               rank < 8;
    }
}