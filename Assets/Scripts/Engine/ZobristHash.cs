using System;

public static class ZobristHash
{
    private static ulong[,,] pieceKeys =
        new ulong[2, 6, 64];

    private static ulong sideToMoveKey;

    private static ulong[] castlingKeys =
        new ulong[4];

    private static ulong[] enPassantKeys =
        new ulong[8];


    static ZobristHash()
    {
        InitializeKeys();
    }


    private static void InitializeKeys()
    {
        Random random =
            new Random(123456789);


        // Piece-square keys
        for (int color = 0; color < 2; color++)
        {
            for (int piece = 0; piece < 6; piece++)
            {
                for (int square = 0; square < 64; square++)
                {
                    pieceKeys[color, piece, square] =
                        RandomUInt64(random);
                }
            }
        }


        // Side to move
        sideToMoveKey =
            RandomUInt64(random);


        // Castling rights
        for (int i = 0; i < 4; i++)
        {
            castlingKeys[i] =
                RandomUInt64(random);
        }


        // En Passant files
        for (int i = 0; i < 8; i++)
        {
            enPassantKeys[i] =
                RandomUInt64(random);
        }
    }


    private static ulong RandomUInt64(
        Random random)
    {
        byte[] bytes =
            new byte[8];

        random.NextBytes(bytes);

        return BitConverter.ToUInt64(
            bytes,
            0
        );
    }


    public static ulong CalculateHash(
        Board board,
        PieceColor sideToMove,
        bool whiteKingMoved,
        bool blackKingMoved,
        bool whiteKingsideRookMoved,
        bool whiteQueensideRookMoved,
        bool blackKingsideRookMoved,
        bool blackQueensideRookMoved,
        int enPassantSquare
    )
    {
        ulong hash = 0;


        // -------------------------------------------------
        // Pieces
        // -------------------------------------------------

        for (int square = 0; square < 64; square++)
        {
            Piece piece =
                board.GetPiece(square);


            if (piece.IsEmpty())
                continue;


            int colorIndex =
                piece.color == PieceColor.White
                    ? 0
                    : 1;


            int pieceIndex =
                GetPieceIndex(piece.type);


            hash ^=
                pieceKeys[
                    colorIndex,
                    pieceIndex,
                    square
                ];
        }


        // -------------------------------------------------
        // Side to move
        // -------------------------------------------------

        if (sideToMove == PieceColor.Black)
        {
            hash ^=
                sideToMoveKey;
        }


        // -------------------------------------------------
        // Castling rights
        // -------------------------------------------------

        if (!whiteKingMoved &&
            !whiteKingsideRookMoved)
        {
            hash ^=
                castlingKeys[0];
        }


        if (!whiteKingMoved &&
            !whiteQueensideRookMoved)
        {
            hash ^=
                castlingKeys[1];
        }


        if (!blackKingMoved &&
            !blackKingsideRookMoved)
        {
            hash ^=
                castlingKeys[2];
        }


        if (!blackKingMoved &&
            !blackQueensideRookMoved)
        {
            hash ^=
                castlingKeys[3];
        }


        // -------------------------------------------------
        // En Passant
        // -------------------------------------------------

        if (enPassantSquare >= 0 &&
            enPassantSquare < 64)
        {
            int file =
                enPassantSquare % 8;

            hash ^=
                enPassantKeys[file];
        }


        return hash;
    }


    private static int GetPieceIndex(
        PieceType type
    )
    {
        switch (type)
        {
            case PieceType.Pawn:
                return 0;

            case PieceType.Knight:
                return 1;

            case PieceType.Bishop:
                return 2;

            case PieceType.Rook:
                return 3;

            case PieceType.Queen:
                return 4;

            case PieceType.King:
                return 5;

            default:
                return 0;
        }
    }
}