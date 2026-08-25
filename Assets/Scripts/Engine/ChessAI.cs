using System.Collections.Generic;

public class ChessAI
{
    private const int MateScore = 1000000;

    // -----------------------------------------------------
    // SEARCH SETTINGS
    // -----------------------------------------------------

    // Start with 4.
    //
    // Depth 4 with Alpha-Beta should be considerably
    // stronger than the previous depth 3 Minimax.
    //
    // Later, after optimization:
    // 5 -> 6 -> 7+
    public int SearchDepth = 4;


    private ChessEngine engine;


    public ChessAI(
        ChessEngine engine
    )
    {
        this.engine = engine;
    }


    // =====================================================
    // FIND BEST MOVE
    // =====================================================

    public Move FindBestMove()
    {
        PieceColor aiColor =
            engine.currentTurn;


        List<Move> legalMoves =
            engine.GetAllLegalMoves(
                aiColor
            );


        if (legalMoves.Count == 0)
        {
            return default(Move);
        }


        // -------------------------------------------------
        // ORDER MOVES
        // -------------------------------------------------

        OrderMoves(
            engine,
            legalMoves
        );


        Move bestMove =
            legalMoves[0];


        int bestScore =
            aiColor == PieceColor.White
                ? int.MinValue
                : int.MaxValue;


        foreach (Move move in legalMoves)
        {
            ChessEngine child =
                CreateTestEngine();


            child.MakeMove(move);


            int score;


            if (aiColor == PieceColor.White)
            {
                score =
                    AlphaBeta(
                        child,
                        SearchDepth - 1,
                        int.MinValue + 1,
                        int.MaxValue - 1,
                        false
                    );


                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            else
            {
                score =
                    AlphaBeta(
                        child,
                        SearchDepth - 1,
                        int.MinValue + 1,
                        int.MaxValue - 1,
                        true
                    );


                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }


        return bestMove;
    }


    // =====================================================
    // ALPHA-BETA SEARCH
    // =====================================================

    private int AlphaBeta(
        ChessEngine position,
        int depth,
        int alpha,
        int beta,
        bool maximizingPlayer
    )
    {
        // -------------------------------------------------
        // GET LEGAL MOVES
        // -------------------------------------------------

        PieceColor sideToMove =
            position.currentTurn;


        List<Move> legalMoves =
            position.GetAllLegalMoves(
                sideToMove
            );


        // -------------------------------------------------
        // CHECKMATE / STALEMATE
        // -------------------------------------------------

        if (legalMoves.Count == 0)
        {
            if (
                position.IsKingInCheck(
                    sideToMove
                )
            )
            {
                if (
                    sideToMove ==
                    PieceColor.White
                )
                {
                    return
                        -MateScore -
                        depth;
                }
                else
                {
                    return
                        MateScore +
                        depth;
                }
            }


            // Stalemate
            return 0;
        }


        // -------------------------------------------------
        // QUIESCENCE AT LEAF
        // -------------------------------------------------

        if (depth <= 0)
        {
            return QuiescenceSearch(
                position,
                alpha,
                beta,
                maximizingPlayer,
                3
            );
        }


        // -------------------------------------------------
        // MOVE ORDERING
        // -------------------------------------------------

        OrderMoves(
            position,
            legalMoves
        );


        // -------------------------------------------------
        // MAXIMIZING PLAYER
        // -------------------------------------------------

        if (maximizingPlayer)
        {
            int bestScore =
                int.MinValue;


            foreach (Move move in legalMoves)
            {
                ChessEngine child =
                    CreateTestEngine(
                        position
                    );


                child.MakeMove(move);


                int score =
                    AlphaBeta(
                        child,
                        depth - 1,
                        alpha,
                        beta,
                        false
                    );


                if (score > bestScore)
                {
                    bestScore = score;
                }


                if (score > alpha)
                {
                    alpha = score;
                }


                // -------------------------------------------------
                // BETA CUTOFF
                // -------------------------------------------------

                if (beta <= alpha)
                {
                    break;
                }
            }


            return bestScore;
        }


        // -------------------------------------------------
        // MINIMIZING PLAYER
        // -------------------------------------------------

        int worstScore =
            int.MaxValue;


        foreach (Move move in legalMoves)
        {
            ChessEngine child =
                CreateTestEngine(
                    position
                );


            child.MakeMove(move);


            int score =
                AlphaBeta(
                    child,
                    depth - 1,
                    alpha,
                    beta,
                    true
                );


            if (score < worstScore)
            {
                worstScore = score;
            }


            if (score < beta)
            {
                beta = score;
            }


            // -------------------------------------------------
            // ALPHA CUTOFF
            // -------------------------------------------------

            if (beta <= alpha)
            {
                break;
            }
        }


        return worstScore;
    }


    // =====================================================
    // QUIESCENCE SEARCH
    // =====================================================

    private int QuiescenceSearch(
        ChessEngine position,
        int alpha,
        int beta,
        bool maximizingPlayer,
        int remainingDepth
    )
    {
        int standPat =
            EvaluationFunction.Evaluate(
                position.board
            );


        // -------------------------------------------------
        // STOP QUIESCENCE
        // -------------------------------------------------

        if (remainingDepth <= 0)
        {
            return standPat;
        }


        // -------------------------------------------------
        // MAXIMIZING
        // -------------------------------------------------

        if (maximizingPlayer)
        {
            if (standPat >= beta)
            {
                return beta;
            }


            if (standPat > alpha)
            {
                alpha = standPat;
            }


            List<Move> moves =
                position.GetAllLegalMoves(
                    position.currentTurn
                );


            List<Move> tacticalMoves =
                GetTacticalMoves(
                    position,
                    moves
                );


            OrderMoves(
                position,
                tacticalMoves
            );


            foreach (Move move in tacticalMoves)
            {
                ChessEngine child =
                    CreateTestEngine(
                        position
                    );


                child.MakeMove(move);


                int score =
                    QuiescenceSearch(
                        child,
                        alpha,
                        beta,
                        false,
                        remainingDepth - 1
                    );


                if (score >= beta)
                {
                    return beta;
                }


                if (score > alpha)
                {
                    alpha = score;
                }
            }


            return alpha;
        }


        // -------------------------------------------------
        // MINIMIZING
        // -----------------------------------------------------

        if (standPat <= alpha)
        {
            return alpha;
        }


        if (standPat < beta)
        {
            beta = standPat;
        }


        List<Move> blackMoves =
            position.GetAllLegalMoves(
                position.currentTurn
            );


        List<Move> blackTacticalMoves =
            GetTacticalMoves(
                position,
                blackMoves
            );


        OrderMoves(
            position,
            blackTacticalMoves
        );


        foreach (Move move in blackTacticalMoves)
        {
            ChessEngine child =
                CreateTestEngine(
                    position
                );


            child.MakeMove(move);


            int score =
                QuiescenceSearch(
                    child,
                    alpha,
                    beta,
                    true,
                    remainingDepth - 1
                );


            if (score <= alpha)
            {
                return alpha;
            }


            if (score < beta)
            {
                beta = score;
            }
        }


        return beta;
    }


    // =====================================================
    // GET TACTICAL MOVES
    // =====================================================

    private List<Move> GetTacticalMoves(
        ChessEngine position,
        List<Move> legalMoves
    )
    {
        List<Move> tacticalMoves =
            new List<Move>();


        foreach (Move move in legalMoves)
        {
            if (
                IsCapture(
                    position,
                    move
                )
            )
            {
                tacticalMoves.Add(move);
                continue;
            }


            // Every promotion is tactical.
            if (
                move.Type ==
                MoveType.Promotion
            )
            {
                tacticalMoves.Add(move);
            }
        }


        return tacticalMoves;
    }


    // =====================================================
    // CAPTURE DETECTION
    // =====================================================

    private bool IsCapture(
        ChessEngine position,
        Move move
    )
    {
        if (
            move.Type ==
            MoveType.Capture
        )
        {
            return true;
        }


        if (
            move.Type ==
            MoveType.EnPassant
        )
        {
            return true;
        }


        Piece target =
            position.board.GetPiece(
                move.To
            );


        return
            !target.IsEmpty();
    }


    // =====================================================
    // MOVE ORDERING
    // =====================================================

    private void OrderMoves(
        ChessEngine position,
        List<Move> moves
    )
    {
        moves.Sort(
            delegate (Move a, Move b)
            {
                return
                    GetMoveScore(
                        position,
                        b
                    )
                    -
                    GetMoveScore(
                        position,
                        a
                    );
            }
        );
    }


    // =====================================================
    // MOVE SCORE
    // =====================================================

    private int GetMoveScore(
        ChessEngine position,
        Move move
    )
    {
        int score = 0;


        // -------------------------------------------------
        // PROMOTION
        // -------------------------------------------------

        if (
            move.Type ==
            MoveType.Promotion
        )
        {
            score += 10000;


            switch (
                move.PromotionPiece
            )
            {
                case PieceType.Queen:
                    score += 900;
                    break;

                case PieceType.Rook:
                    score += 500;
                    break;

                case PieceType.Bishop:
                    score += 330;
                    break;

                case PieceType.Knight:
                    score += 320;
                    break;
            }
        }


        // -------------------------------------------------
        // CASTLING
        // -------------------------------------------------

        if (
            move.Type ==
            MoveType.CastleKingSide ||
            move.Type ==
            MoveType.CastleQueenSide
        )
        {
            score += 150;
        }


        // -------------------------------------------------
        // CAPTURE
        // -------------------------------------------------

        if (
            IsCapture(
                position,
                move
            )
        )
        {
            Piece victim =
                position.board.GetPiece(
                    move.To
                );


            if (!victim.IsEmpty())
            {
                score +=
                    GetPieceValue(
                        victim.type
                    ) * 10;
            }


            Piece attacker =
                position.board.GetPiece(
                    move.From
                );


            if (!attacker.IsEmpty())
            {
                score -=
                    GetPieceValue(
                        attacker.type
                    );
            }


            // En passant gets a useful tactical bonus.
            if (
                move.Type ==
                MoveType.EnPassant
            )
            {
                score += 100;
            }
        }


        return score;
    }


    // =====================================================
    // PIECE VALUES FOR MOVE ORDERING
    // =====================================================

    private int GetPieceValue(
        PieceType type
    )
    {
        switch (type)
        {
            case PieceType.Pawn:
                return 100;

            case PieceType.Knight:
                return 320;

            case PieceType.Bishop:
                return 330;

            case PieceType.Rook:
                return 500;

            case PieceType.Queen:
                return 900;

            case PieceType.King:
                return 20000;

            default:
                return 0;
        }
    }


    // =====================================================
    // CREATE TEST ENGINE
    // =====================================================

    private ChessEngine CreateTestEngine()
    {
        return CreateTestEngine(
            engine
        );
    }


    private ChessEngine CreateTestEngine(
        ChessEngine source
    )
    {
        ChessEngine test =
            new ChessEngine();


        // -------------------------------------------------
        // BOARD
        // -------------------------------------------------

        test.board =
            source.board.Copy();


        // -------------------------------------------------
        // TURN
        // -------------------------------------------------

        test.currentTurn =
            source.currentTurn;


        // -------------------------------------------------
        // EN PASSANT
        // -------------------------------------------------

        test.enPassantSquare =
            source.enPassantSquare;


        // -------------------------------------------------
        // CASTLING RIGHTS
        // -------------------------------------------------

        test.whiteKingMoved =
            source.whiteKingMoved;

        test.blackKingMoved =
            source.blackKingMoved;


        test.whiteKingsideRookMoved =
            source.whiteKingsideRookMoved;

        test.whiteQueensideRookMoved =
            source.whiteQueensideRookMoved;


        test.blackKingsideRookMoved =
            source.blackKingsideRookMoved;

        test.blackQueensideRookMoved =
            source.blackQueensideRookMoved;


        return test;
    }
}