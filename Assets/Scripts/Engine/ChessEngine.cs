using System.Collections.Generic;

public class ChessEngine
{
    public Board board;

    public PieceColor currentTurn;

    public int enPassantSquare = -1;

    public bool whiteKingMoved;
    public bool blackKingMoved;

    public bool whiteKingsideRookMoved;
    public bool whiteQueensideRookMoved;

    public bool blackKingsideRookMoved;
    public bool blackQueensideRookMoved;


    public ChessEngine()
    {
        board = new Board();

        board.SetStartingPosition();

        currentTurn = PieceColor.White;

        enPassantSquare = -1;

        whiteKingMoved = false;
        blackKingMoved = false;

        whiteKingsideRookMoved = false;
        whiteQueensideRookMoved = false;

        blackKingsideRookMoved = false;
        blackQueensideRookMoved = false;
    }


    // =====================================================
    // LEGAL MOVES FOR ONE PIECE
    // =====================================================

    public List<Move> GetLegalMoves(int square)
    {
        Piece piece =
            board.GetPiece(square);

        if (piece.IsEmpty())
            return new List<Move>();

        List<Move> pseudoLegalMoves =
            MoveGenerator.GenerateMoves(
                board,
                square,
                enPassantSquare
            );


        List<Move> legalMoves =
            new List<Move>();

        if (piece.type == PieceType.King)
        {
            if (IsCastlingLegal(
                square,
                square + 2,
                piece.color))
            {
                legalMoves.Add(
                    new Move(
                        square,
                        square + 2,
                        MoveType.CastleKingSide
                    )
                );
            }

            if (IsCastlingLegal(
                square,
                square - 2,
                piece.color))
            {
                legalMoves.Add(
                    new Move(
                        square,
                        square - 2,
                        MoveType.CastleQueenSide
                    )
                );
            }
        }


        foreach (Move move in pseudoLegalMoves)
        {
            Board testBoard =
                board.Copy();


            testBoard.MakeMove(move);


            // En Passant removes the captured pawn
            if (move.Type == MoveType.EnPassant)
            {
                int capturedSquare =
                    piece.color == PieceColor.White
                        ? move.To - 8
                        : move.To + 8;

                testBoard.RemovePiece(
                    capturedSquare
                );
            }


            if (!IsKingInCheck(
                testBoard,
                piece.color
            ))
            {
                legalMoves.Add(move);
            }
        }


        return legalMoves;
    }


    // =====================================================
    // ALL LEGAL MOVES FOR A COLOR
    // =====================================================

    public List<Move> GetAllLegalMoves(
        PieceColor color
    )
    {
        List<Move> legalMoves =
            new List<Move>();

        for (int square = 0; square < 64; square++)
        {
            Piece piece =
                board.GetPiece(square);

            if (piece.IsEmpty() ||
                piece.color != color)
                continue;

            legalMoves.AddRange(
                GetLegalMoves(square)
            );
        }

        return legalMoves;
    }


    // =====================================================
    // IS MOVE LEGAL?
    // =====================================================

    public bool IsMoveLegal(
        int from,
        int to
    )
    {
        Piece piece =
            board.GetPiece(from);

        if (piece.IsEmpty())
            return false;

        if (piece.color != currentTurn)
            return false;

        List<Move> moves =
            GetLegalMoves(from);

        foreach (Move move in moves)
        {
            if (move.To == to)
                return true;
        }

        return false;
    }


    // =====================================================
    // CURRENT BOARD CHECK
    // =====================================================

    public bool IsKingInCheck(
        PieceColor color
    )
    {
        return IsKingInCheck(
            board,
            color
        );
    }


    // =====================================================
    // CHECK ON SPECIFIC BOARD
    // =====================================================

    private bool IsKingInCheck(
        Board checkBoard,
        PieceColor color
    )
    {
        int kingSquare = -1;


        for (int i = 0; i < 64; i++)
        {
            Piece piece =
                checkBoard.GetPiece(i);


            if (!piece.IsEmpty() &&
                piece.type == PieceType.King &&
                piece.color == color)
            {
                kingSquare = i;
                break;
            }
        }


        if (kingSquare == -1)
            return false;


        PieceColor enemyColor =
            color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;


        return AttackDetector.IsSquareAttacked(
            checkBoard,
            kingSquare,
            enemyColor
        );
    }


    // =====================================================
    // CHECKMATE
    // =====================================================

    public bool IsCheckmate(
        PieceColor color
    )
    {
        if (!IsKingInCheck(color))
            return false;


        List<Move> legalMoves =
            GetAllLegalMoves(color);


        return legalMoves.Count == 0;
    }


    // =====================================================
    // STALEMATE
    // =====================================================

    public bool IsStalemate(
        PieceColor color
    )
    {
        if (IsKingInCheck(color))
            return false;


        List<Move> legalMoves =
            GetAllLegalMoves(color);


        return legalMoves.Count == 0;
    }


    // =====================================================
    // GAME STATE
    // =====================================================

    public GameState GetGameState()
    {
        PieceColor color =
            currentTurn;


        if (IsCheckmate(color))
        {
            return GameState.Checkmate;
        }


        if (IsStalemate(color))
        {
            return GameState.Stalemate;
        }


        if (IsKingInCheck(color))
        {
            return GameState.Check;
        }


        return GameState.Playing;
    }


    // =====================================================
    // MAKE MOVE
    // =====================================================

    public void MakeMove(
        Move move
    )
    {
        Piece movingPiece =
            board.GetPiece(move.From);

        board.MakeMove(move);

        if (move.Type == MoveType.EnPassant)
        {
            int capturedSquare =
                movingPiece.color == PieceColor.White
                    ? move.To - 8
                    : move.To + 8;

            board.RemovePiece(capturedSquare);
        }

        if (move.Type == MoveType.CastleKingSide)
        {
            int rookFrom =
                movingPiece.color == PieceColor.White ? 7 : 63;

            int rookTo =
                movingPiece.color == PieceColor.White ? 5 : 61;

            board.MakeMove(
                new Move(rookFrom, rookTo)
            );
        }
        else if (move.Type == MoveType.CastleQueenSide)
        {
            int rookFrom =
                movingPiece.color == PieceColor.White ? 0 : 56;

            int rookTo =
                movingPiece.color == PieceColor.White ? 3 : 59;

            board.MakeMove(
                new Move(rookFrom, rookTo)
            );
        }

        UpdateSpecialState(move);
        UpdateEnPassantSquare(move);
        SwitchTurn();
    }


    // =====================================================
    // SPECIAL STATE
    // =====================================================

    private void UpdateSpecialState(
        Move move
    )
    {
        Piece piece =
            board.GetPiece(move.To);


        if (piece.type == PieceType.King)
        {
            if (piece.color ==
                PieceColor.White)
            {
                whiteKingMoved = true;
            }
            else
            {
                blackKingMoved = true;
            }
        }


        if (move.From == 0)
            whiteQueensideRookMoved = true;

        if (move.From == 7)
            whiteKingsideRookMoved = true;

        if (move.From == 56)
            blackQueensideRookMoved = true;

        if (move.From == 63)
            blackKingsideRookMoved = true;

        // A rook captured on its original square loses
        // the corresponding castling right.
        if (move.To == 0)
            whiteQueensideRookMoved = true;

        if (move.To == 7)
            whiteKingsideRookMoved = true;

        if (move.To == 56)
            blackQueensideRookMoved = true;

        if (move.To == 63)
            blackKingsideRookMoved = true;

        // Castling rook movement
        if (move.Type ==
            MoveType.CastleKingSide)
        {
            if (piece.color ==
                PieceColor.White)
            {
                whiteKingMoved = true;
                whiteKingsideRookMoved = true;
            }
            else
            {
                blackKingMoved = true;
                blackKingsideRookMoved = true;
            }
        }


        if (move.Type ==
            MoveType.CastleQueenSide)
        {
            if (piece.color ==
                PieceColor.White)
            {
                whiteKingMoved = true;
                whiteQueensideRookMoved = true;
            }
            else
            {
                blackKingMoved = true;
                blackQueensideRookMoved = true;
            }
        }
    }


    // =====================================================
    // EN PASSANT STATE
    // =====================================================

    private void UpdateEnPassantSquare(
        Move move
    )
    {
        enPassantSquare = -1;


        Piece piece =
            board.GetPiece(move.To);


        if (piece.type != PieceType.Pawn)
            return;


        int difference =
            move.To - move.From;


        if (difference == 16 ||
            difference == -16)
        {
            enPassantSquare =
                (move.From + move.To) / 2;
        }
    }


    // =====================================================
    // TURN
    // =====================================================

    private void SwitchTurn()
    {
        currentTurn =
            currentTurn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;
    }

    public bool IsCastlingLegal(
        int from,
        int to,
        PieceColor color
    )
    {
        // Must be a king
        Piece king = board.GetPiece(from);

        if (king.IsEmpty() ||
            king.type != PieceType.King ||
            king.color != color)
        {
            return false;
        }

        // -------------------------------------------------
        // WHITE
        // -------------------------------------------------

        if (color == PieceColor.White)
        {
            // King side: e1 -> g1
            if (from == 4 && to == 6)
            {
                if (whiteKingMoved ||
                    whiteKingsideRookMoved)
                    return false;

                // Rook must exist
                Piece rook = board.GetPiece(7);

                if (rook.IsEmpty() ||
                    rook.type != PieceType.Rook ||
                    rook.color != PieceColor.White)
                    return false;

                // f1 and g1 must be empty
                if (!board.GetPiece(5).IsEmpty() ||
                    !board.GetPiece(6).IsEmpty())
                    return false;

                // King cannot castle out of, through,
                // or into check
                if (IsKingInCheck(PieceColor.White))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        5,
                        PieceColor.Black))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        6,
                        PieceColor.Black))
                    return false;

                return true;
            }


            // Queen side: e1 -> c1
            if (from == 4 && to == 2)
            {
                if (whiteKingMoved ||
                    whiteQueensideRookMoved)
                    return false;

                // Rook must exist
                Piece rook = board.GetPiece(0);

                if (rook.IsEmpty() ||
                    rook.type != PieceType.Rook ||
                    rook.color != PieceColor.White)
                    return false;

                // b1, c1 and d1 must be empty
                if (!board.GetPiece(1).IsEmpty() ||
                    !board.GetPiece(2).IsEmpty() ||
                    !board.GetPiece(3).IsEmpty())
                    return false;

                // King cannot castle out of,
                // through, or into check
                if (IsKingInCheck(PieceColor.White))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        3,
                        PieceColor.Black))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        2,
                        PieceColor.Black))
                    return false;

                return true;
            }
        }


        // -------------------------------------------------
        // BLACK
        // -------------------------------------------------

        if (color == PieceColor.Black)
        {
            // King side: e8 -> g8
            if (from == 60 && to == 62)
            {
                if (blackKingMoved ||
                    blackKingsideRookMoved)
                    return false;

                Piece rook = board.GetPiece(63);

                if (rook.IsEmpty() ||
                    rook.type != PieceType.Rook ||
                    rook.color != PieceColor.Black)
                    return false;

                // f8 and g8 must be empty
                if (!board.GetPiece(61).IsEmpty() ||
                    !board.GetPiece(62).IsEmpty())
                    return false;

                if (IsKingInCheck(PieceColor.Black))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        61,
                        PieceColor.White))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        62,
                        PieceColor.White))
                    return false;

                return true;
            }


            // Queen side: e8 -> c8
            if (from == 60 && to == 58)
            {
                if (blackKingMoved ||
                    blackQueensideRookMoved)
                    return false;

                Piece rook = board.GetPiece(56);

                if (rook.IsEmpty() ||
                    rook.type != PieceType.Rook ||
                    rook.color != PieceColor.Black)
                    return false;

                // b8, c8 and d8 must be empty
                if (!board.GetPiece(57).IsEmpty() ||
                    !board.GetPiece(58).IsEmpty() ||
                    !board.GetPiece(59).IsEmpty())
                    return false;

                if (IsKingInCheck(PieceColor.Black))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        59,
                        PieceColor.White))
                    return false;

                if (AttackDetector.IsSquareAttacked(
                        board,
                        58,
                        PieceColor.White))
                    return false;

                return true;
            }
        }


        return false;
    }

    public int EvaluatePosition()
    {
        return EvaluationFunction.Evaluate(board);
    }
}