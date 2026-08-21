using System.Collections.Generic;
using System;

public static class MoveGenerator
{
    public static List<Move> GenerateKnightMoves(
        Board board,
        int square
    )
    {
        List<Move> moves = new List<Move>();

        Piece piece = board.GetPiece(square);

        if (piece.IsEmpty() ||
            piece.type != PieceType.Knight)
        {
            return moves;
        }

        int file = square % 8;
        int rank = square / 8;

        int[,] directions =
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
            int targetFile = file + directions[i, 0];
            int targetRank = rank + directions[i, 1];

            if (!IsInsideBoard(targetFile, targetRank))
                continue;

            int targetSquare =
                targetRank * 8 + targetFile;

            Piece target =
                board.GetPiece(targetSquare);

            if (target.IsEmpty() ||
                target.color != piece.color)
            {
                moves.Add(
                    new Move(square, targetSquare)
                );
            }
        }

        return moves;
    }


    public static List<Move> GenerateSlidingMoves(
        Board board,
        int square,
        int[,] directions
    )
    {
        List<Move> moves = new List<Move>();

        Piece piece = board.GetPiece(square);

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

                Piece target =
                    board.GetPiece(targetSquare);

                // Empty square
                if (target.IsEmpty())
                {
                    moves.Add(
                        new Move(square, targetSquare)
                    );
                }

                // Piece encountered
                else
                {
                    // Enemy piece can be captured
                    if (target.color != piece.color)
                    {
                        moves.Add(
                            new Move(square, targetSquare)
                        );
                    }

                    // Stop after hitting any piece
                    break;
                }
            }
        }

        return moves;
    }


    private static bool IsInsideBoard(
        int file,
        int rank
    )
    {
        return file >= 0 &&
               file < 8 &&
               rank >= 0 &&
               rank < 8;
    }


    public static List<Move> GenerateBishopMoves(
        Board board,
        int square
    )
    {
        int[,] directions =
        {
            { 1, 1 },
            { 1, -1 },
            { -1, 1 },
            { -1, -1 }
        };

        return GenerateSlidingMoves(
            board,
            square,
            directions
        );
    }


    public static List<Move> GenerateRookMoves(
        Board board,
        int square
    )
    {
        int[,] directions =
        {
            { 1, 0 },
            { -1, 0 },
            { 0, 1 },
            { 0, -1 }
        };

        return GenerateSlidingMoves(
            board,
            square,
            directions
        );
    }


    public static List<Move> GenerateQueenMoves(
        Board board,
        int square
    )
    {
        int[,] directions =
        {
            { 1, 1 },
            { 1, -1 },
            { -1, 1 },
            { -1, -1 },

            { 1, 0 },
            { -1, 0 },
            { 0, 1 },
            { 0, -1 }
        };

        return GenerateSlidingMoves(
            board,
            square,
            directions
        );
    }


    public static List<Move> GeneratePawnMoves(
        Board board,
        int square,
        int enPassantSquare
    )
    {
        List<Move> moves = new List<Move>();

        Piece pawn =
            board.GetPiece(square);

        if (pawn.IsEmpty() ||
            pawn.type != PieceType.Pawn)
        {
            return moves;
        }

        int file = square % 8;
        int rank = square / 8;

        int direction;

        // White moves upward
        if (pawn.color == PieceColor.White)
            direction = 1;

        // Black moves downward
        else
            direction = -1;


        // -------------------------------------------------
        // FORWARD MOVEMENT
        // -------------------------------------------------

        int nextRank =
            rank + direction;

        if (nextRank >= 0 &&
            nextRank < 8)
        {
            int forwardSquare =
                nextRank * 8 + file;

            // One square forward
            if (board.GetPiece(forwardSquare).IsEmpty())
            {
                moves.Add(
                    new Move(
                        square,
                        forwardSquare
                    )
                );


                // Two squares from starting position
                bool startingRank =
                    (pawn.color == PieceColor.White &&
                     rank == 1)
                    ||
                    (pawn.color == PieceColor.Black &&
                     rank == 6);


                if (startingRank)
                {
                    int doubleSquare =
                        (rank + direction * 2) * 8
                        + file;

                    if (board.GetPiece(doubleSquare).IsEmpty())
                    {
                        moves.Add(
                            new Move(
                                square,
                                doubleSquare
                            )
                        );
                    }
                }
            }
        }


        // -------------------------------------------------
        // NORMAL CAPTURES
        // -------------------------------------------------

        int[] captureFiles =
        {
            file - 1,
            file + 1
        };


        foreach (int targetFile in captureFiles)
        {
            if (targetFile < 0 ||
                targetFile >= 8)
            {
                continue;
            }

            int targetRank =
                rank + direction;

            if (targetRank < 0 ||
                targetRank >= 8)
            {
                continue;
            }

            int targetSquare =
                targetRank * 8 + targetFile;

            Piece target =
                board.GetPiece(targetSquare);


            // Normal capture
            if (!target.IsEmpty() &&
                target.color != pawn.color)
            {
                moves.Add(
                    new Move(
                        square,
                        targetSquare
                    )
                );
            }
        }


        // -------------------------------------------------
        // EN PASSANT
        // -------------------------------------------------

        if (enPassantSquare != -1)
        {
            int epFile =
                enPassantSquare % 8;

            int epRank =
                enPassantSquare / 8;


            // En passant destination must be
            // one rank forward from the pawn
            if (epRank == rank + direction)
            {
                // Destination must be exactly
                // one file away
                if (Math.Abs(epFile - file) == 1)
                {
                    moves.Add(
                        new Move(
                            square,
                            enPassantSquare,
                            MoveType.EnPassant
                        )
                    );
                }
            }
        }


        return moves;
    }


    public static List<Move> GenerateKingMoves(
        Board board,
        int square
    )
    {
        List<Move> moves = new List<Move>();

        Piece king =
            board.GetPiece(square);

        if (king.IsEmpty() ||
            king.type != PieceType.King)
        {
            return moves;
        }

        int file = square % 8;
        int rank = square / 8;

        int[,] directions =
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


        for (int i = 0;
             i < directions.GetLength(0);
             i++)
        {
            int targetFile =
                file + directions[i, 0];

            int targetRank =
                rank + directions[i, 1];

            if (!IsInsideBoard(
                targetFile,
                targetRank))
            {
                continue;
            }

            int targetSquare =
                targetRank * 8 + targetFile;

            Piece target =
                board.GetPiece(targetSquare);


            // Empty or enemy piece
            if (target.IsEmpty() ||
                target.color != king.color)
            {
                moves.Add(
                    new Move(
                        square,
                        targetSquare
                    )
                );
            }
        }

        return moves;
    }


    public static List<Move> GenerateMoves(
        Board board,
        int square,
        int enPassantSquare
    )
    {
        List<Move> moves =
            new List<Move>();

        Piece piece =
            board.GetPiece(square);

        if (piece.IsEmpty())
        {
            return moves;
        }


        switch (piece.type)
        {
            case PieceType.Pawn:

                moves.AddRange(
                    GeneratePawnMoves(
                        board,
                        square,
                        enPassantSquare
                    )
                );

                break;


            case PieceType.Knight:

                moves.AddRange(
                    GenerateKnightMoves(
                        board,
                        square
                    )
                );

                break;


            case PieceType.Bishop:

                moves.AddRange(
                    GenerateBishopMoves(
                        board,
                        square
                    )
                );

                break;


            case PieceType.Rook:

                moves.AddRange(
                    GenerateRookMoves(
                        board,
                        square
                    )
                );

                break;


            case PieceType.Queen:

                moves.AddRange(
                    GenerateQueenMoves(
                        board,
                        square
                    )
                );

                break;


            case PieceType.King:

                moves.AddRange(
                    GenerateKingMoves(
                        board,
                        square
                    )
                );

                break;
        }

        return moves;
    }


    public static List<Move> GenerateCastlingMoves(
        Board board,
        PieceColor color,
        bool kingMoved,
        bool kingsideRookMoved,
        bool queensideRookMoved
    )
    {
        List<Move> moves =
            new List<Move>();

        if (kingMoved)
            return moves;


        int kingSquare;
        int kingsideRookSquare;
        int queensideRookSquare;


        if (color == PieceColor.White)
        {
            kingSquare = 4;              // e1
            kingsideRookSquare = 7;      // h1
            queensideRookSquare = 0;     // a1
        }
        else
        {
            kingSquare = 60;             // e8
            kingsideRookSquare = 63;     // h8
            queensideRookSquare = 56;    // a8
        }


        // Make sure king exists
        Piece king =
            board.GetPiece(kingSquare);

        if (king.type != PieceType.King ||
            king.color != color)
        {
            return moves;
        }


        // -------------------------------------------------
        // KING SIDE CASTLING
        // -------------------------------------------------

        if (!kingsideRookMoved)
        {
            Piece rook =
                board.GetPiece(
                    kingsideRookSquare
                );

            if (rook.type == PieceType.Rook &&
                rook.color == color)
            {
                int f =
                    kingSquare + 1;

                int g =
                    kingSquare + 2;


                if (board.GetPiece(f).IsEmpty() &&
                    board.GetPiece(g).IsEmpty())
                {
                    moves.Add(
                        new Move(
                            kingSquare,
                            g,
                            MoveType.CastleKingSide
                        )
                    );
                }
            }
        }


        // -------------------------------------------------
        // QUEEN SIDE CASTLING
        // -------------------------------------------------

        if (!queensideRookMoved)
        {
            Piece rook =
                board.GetPiece(
                    queensideRookSquare
                );

            if (rook.type == PieceType.Rook &&
                rook.color == color)
            {
                int b =
                    kingSquare - 3;

                int c =
                    kingSquare - 2;

                int d =
                    kingSquare - 1;


                if (board.GetPiece(b).IsEmpty() &&
                    board.GetPiece(c).IsEmpty() &&
                    board.GetPiece(d).IsEmpty())
                {
                    moves.Add(
                        new Move(
                            kingSquare,
                            c,
                            MoveType.CastleQueenSide
                        )
                    );
                }
            }
        }


        return moves;
    }
}