using System;
using System.Collections.Generic;
using System.Diagnostics;

public class ChessAI
{
    private const int MateScore = 1000000;
    private const int DrawContempt = 35;

    private ChessEngine engine;

    // Maximum search depth.
    public int SearchDepth = 8;

    // Maximum thinking time.
    public int ThinkTimeMilliseconds = 1500;

    private Stopwatch searchTimer;
    private bool searchTimedOut;

    // Positions currently present in the AI's search line.
    private Dictionary<string, int> searchRepetitions =
        new Dictionary<string, int>();

    private const int MaxTranspositionEntries = 200000;

    private Dictionary<string, TTEntry> transpositionTable =
        new Dictionary<string, TTEntry>();

    private enum TTFlag
    {
        Exact,
        LowerBound,
        UpperBound
    }

    private struct TTEntry
    {
        public int depth;
        public int score;
        public TTFlag flag;
        public Move bestMove;

        public TTEntry(int depth, int score, TTFlag flag, Move bestMove)
        {
            this.depth = depth;
            this.score = score;
            this.flag = flag;
            this.bestMove = bestMove;
        }
    }


    public ChessAI(ChessEngine engine)
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
            engine.GetAllLegalMoves(aiColor);

        if (legalMoves.Count == 0)
        {
            return default(Move);
        }

        // Fresh table for each actual AI turn.
        // Entries are reused by iterative deepening.
        transpositionTable.Clear();

        OrderMoves(engine, legalMoves);

        // Always have a legal fallback move.
        Move bestMove = legalMoves[0];

        int bestScore =
            aiColor == PieceColor.White
                ? int.MinValue
                : int.MaxValue;

        searchTimer = Stopwatch.StartNew();
        searchTimedOut = false;

        searchRepetitions.Clear();

        string rootKey =
            GetPositionKey(engine);

        searchRepetitions[rootKey] = 1;


        // =================================================
        // ITERATIVE DEEPENING
        // =================================================

        for (int depth = 1;
             depth <= SearchDepth;
             depth++)
        {
            if (TimeExpired())
            {
                break;
            }

            searchTimedOut = false;

            Move iterationBestMove =
                bestMove;

            int iterationBestScore =
                aiColor == PieceColor.White
                    ? int.MinValue
                    : int.MaxValue;

            // Previous iteration's best move first.
            MoveToFront(
                legalMoves,
                bestMove
            );


            foreach (Move move in legalMoves)
            {
                if (TimeExpired())
                {
                    searchTimedOut = true;
                    break;
                }

                ChessEngine child =
                    CreateTestEngine();

                child.MakeMove(move);

                string childKey =
                    GetPositionKey(child);

                int score;

                if (IsSearchRepetition(childKey))
                {
                    score =
                        GetDrawScore(child);
                }
                else
                {
                    AddSearchRepetition(childKey);

                    score =
                        AlphaBeta(
                            child,
                            depth - 1,
                            int.MinValue + 1,
                            int.MaxValue - 1,
                            aiColor == PieceColor.Black
                        );

                    RemoveSearchRepetition(childKey);
                }


                if (searchTimedOut)
                {
                    break;
                }


                if (aiColor == PieceColor.White)
                {
                    if (score > iterationBestScore)
                    {
                        iterationBestScore = score;
                        iterationBestMove = move;
                    }
                }
                else
                {
                    if (score < iterationBestScore)
                    {
                        iterationBestScore = score;
                        iterationBestMove = move;
                    }
                }
            }


            // Only accept a completely finished iteration.
            if (searchTimedOut)
            {
                break;
            }

            bestMove = iterationBestMove;
            bestScore = iterationBestScore;


            // Stop early when a forced mate is found.
            if (bestScore > MateScore - 1000 ||
                bestScore < -MateScore + 1000)
            {
                break;
            }
        }


        searchTimer.Stop();

        return bestMove;
    }


    // =====================================================
    // ALPHA-BETA
    // =====================================================

    private int AlphaBeta(
        ChessEngine position,
        int depth,
        int alpha,
        int beta,
        bool maximizingPlayer
    )
    {
        if (TimeExpired())
        {
            searchTimedOut = true;
            return 0;
        }

        string positionKey = GetPositionKey(position);
        bool repetitionSensitive = IsRepetitionSensitive(positionKey);
        int originalAlpha = alpha;
        int originalBeta = beta;

        TTEntry cachedEntry;

        if (!repetitionSensitive &&
            transpositionTable.TryGetValue(positionKey, out cachedEntry) &&
            cachedEntry.depth >= depth)
        {
            if (cachedEntry.flag == TTFlag.Exact)
                return cachedEntry.score;

            if (cachedEntry.flag == TTFlag.LowerBound && cachedEntry.score > alpha)
                alpha = cachedEntry.score;

            if (cachedEntry.flag == TTFlag.UpperBound && cachedEntry.score < beta)
                beta = cachedEntry.score;

            if (alpha >= beta)
                return cachedEntry.score;
        }

        PieceColor sideToMove = position.currentTurn;
        List<Move> legalMoves = position.GetAllLegalMoves(sideToMove);

        if (legalMoves.Count == 0)
        {
            if (position.IsKingInCheck(sideToMove))
            {
                if (sideToMove == PieceColor.White)
                    return -MateScore - depth;

                return MateScore + depth;
            }

            return GetDrawScore(position);
        }

        if (depth <= 0)
        {
            return QuiescenceSearch(
                position, alpha, beta, maximizingPlayer, 3
            );
        }

        Move hashMove = default(Move);

        if (!repetitionSensitive &&
            transpositionTable.TryGetValue(positionKey, out cachedEntry))
        {
            hashMove = cachedEntry.bestMove;
        }

        OrderMoves(position, legalMoves);

        if (!IsDefaultMove(hashMove))
            MoveToFront(legalMoves, hashMove);

        Move bestMove = legalMoves[0];

        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;

            foreach (Move move in legalMoves)
            {
                if (TimeExpired())
                {
                    searchTimedOut = true;
                    return 0;
                }

                ChessEngine child = CreateTestEngine(position);
                child.MakeMove(move);

                string childKey = GetPositionKey(child);
                int score;

                if (IsSearchRepetition(childKey))
                {
                    score = GetDrawScore(child);
                }
                else
                {
                    AddSearchRepetition(childKey);
                    score = AlphaBeta(child, depth - 1, alpha, beta, false);
                    RemoveSearchRepetition(childKey);
                }

                if (searchTimedOut)
                    return 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                if (score > alpha)
                    alpha = score;

                if (beta <= alpha)
                    break;
            }

            StoreTransposition(
                positionKey, depth, bestScore,
                GetTTFlag(bestScore, originalAlpha, originalBeta),
                bestMove, repetitionSensitive
            );

            return bestScore;
        }

        int worstScore = int.MaxValue;

        foreach (Move move in legalMoves)
        {
            if (TimeExpired())
            {
                searchTimedOut = true;
                return 0;
            }

            ChessEngine child = CreateTestEngine(position);
            child.MakeMove(move);

            string childKey = GetPositionKey(child);
            int score;

            if (IsSearchRepetition(childKey))
            {
                score = GetDrawScore(child);
            }
            else
            {
                AddSearchRepetition(childKey);
                score = AlphaBeta(child, depth - 1, alpha, beta, true);
                RemoveSearchRepetition(childKey);
            }

            if (searchTimedOut)
                return 0;

            if (score < worstScore)
            {
                worstScore = score;
                bestMove = move;
            }

            if (score < beta)
                beta = score;

            if (beta <= alpha)
                break;
        }

        StoreTransposition(
            positionKey, depth, worstScore,
            GetTTFlag(worstScore, originalAlpha, originalBeta),
            bestMove, repetitionSensitive
        );

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
        if (TimeExpired())
        {
            searchTimedOut = true;
            return 0;
        }


        int standPat =
            EvaluationFunction.Evaluate(
                position.board
            );


        if (remainingDepth <= 0)
        {
            return standPat;
        }


        List<Move> legalMoves =
            position.GetAllLegalMoves(
                position.currentTurn
            );

        List<Move> tacticalMoves =
            GetTacticalMoves(
                position,
                legalMoves
            );


        OrderMoves(
            position,
            tacticalMoves
        );


        // =================================================
        // MAXIMIZING
        // =================================================

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


            foreach (Move move in tacticalMoves)
            {
                if (TimeExpired())
                {
                    searchTimedOut = true;
                    return 0;
                }


                ChessEngine child =
                    CreateTestEngine(position);

                child.MakeMove(move);


                int score =
                    QuiescenceSearch(
                        child,
                        alpha,
                        beta,
                        false,
                        remainingDepth - 1
                    );


                if (searchTimedOut)
                {
                    return 0;
                }


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


        // =================================================
        // MINIMIZING
        // =================================================

        if (standPat <= alpha)
        {
            return alpha;
        }

        if (standPat < beta)
        {
            beta = standPat;
        }


        foreach (Move move in tacticalMoves)
        {
            if (TimeExpired())
            {
                searchTimedOut = true;
                return 0;
            }


            ChessEngine child =
                CreateTestEngine(position);

            child.MakeMove(move);


            int score =
                QuiescenceSearch(
                    child,
                    alpha,
                    beta,
                    true,
                    remainingDepth - 1
                );


            if (searchTimedOut)
            {
                return 0;
            }


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
    // TACTICAL MOVES
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


        return !target.IsEmpty();
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
            delegate (
                Move a,
                Move b
            )
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


    private int GetMoveScore(
        ChessEngine position,
        Move move
    )
    {
        int score = 0;


        // Promotion
        if (
            move.Type ==
            MoveType.Promotion
        )
        {
            score += 10000;


            switch (move.PromotionPiece)
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


        // Castling
        if (
            move.Type ==
            MoveType.CastleKingSide ||
            move.Type ==
            MoveType.CastleQueenSide
        )
        {
            score += 150;
        }


        // Capture
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
    // PIECE VALUES
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
    // DRAW CONTEMPT
    // =====================================================

    private int GetDrawScore(
        ChessEngine position
    )
    {
        int evaluation =
            EvaluationFunction.Evaluate(
                position.board
            );


        // White is better.
        // Therefore a draw is bad for White.
        if (evaluation > DrawContempt)
        {
            return -DrawContempt;
        }


        // Black is better.
        // Therefore a draw is bad for Black.
        if (evaluation < -DrawContempt)
        {
            return DrawContempt;
        }


        // Roughly equal position.
        return 0;
    }


    // =====================================================
    // SEARCH REPETITION HELPERS
    // =====================================================

    private bool IsSearchRepetition(
        string positionKey
    )
    {
        int count;

        if (
            searchRepetitions.TryGetValue(
                positionKey,
                out count
            )
        )
        {
            return count >= 2;
        }


        return false;
    }


    private void AddSearchRepetition(
        string positionKey
    )
    {
        int count;

        if (
            searchRepetitions.TryGetValue(
                positionKey,
                out count
            )
        )
        {
            searchRepetitions[positionKey] =
                count + 1;
        }
        else
        {
            searchRepetitions[positionKey] = 1;
        }
    }


    private void RemoveSearchRepetition(
        string positionKey
    )
    {
        int count;

        if (
            searchRepetitions.TryGetValue(
                positionKey,
                out count
            )
        )
        {
            if (count <= 1)
            {
                searchRepetitions.Remove(
                    positionKey
                );
            }
            else
            {
                searchRepetitions[positionKey] =
                    count - 1;
            }
        }
    }


    // =====================================================
    // TRANSPOSITION TABLE HELPERS
    // =====================================================

    private bool IsRepetitionSensitive(string positionKey)
    {
        int count;
        if (searchRepetitions.TryGetValue(positionKey, out count))
            return count > 1;
        return false;
    }

    private TTFlag GetTTFlag(int score, int originalAlpha, int originalBeta)
    {
        if (score <= originalAlpha)
            return TTFlag.UpperBound;
        if (score >= originalBeta)
            return TTFlag.LowerBound;
        return TTFlag.Exact;
    }

    private void StoreTransposition(
        string positionKey, int depth, int score, TTFlag flag,
        Move bestMove, bool repetitionSensitive
    )
    {
        if (repetitionSensitive || searchTimedOut)
            return;

        if (transpositionTable.Count >= MaxTranspositionEntries)
            transpositionTable.Clear();

        TTEntry existing;
        if (transpositionTable.TryGetValue(positionKey, out existing) &&
            existing.depth > depth)
            return;

        transpositionTable[positionKey] =
            new TTEntry(depth, score, flag, bestMove);
    }


    // =====================================================
    // POSITION KEY
    // =====================================================

    private string GetPositionKey(
        ChessEngine position
    )
    {
        System.Text.StringBuilder key =
            new System.Text.StringBuilder();


        // Board
        for (
            int square = 0;
            square < 64;
            square++
        )
        {
            Piece piece =
                position.board.GetPiece(
                    square
                );


            if (piece.IsEmpty())
            {
                key.Append('.');
            }
            else
            {
                key.Append(
                    (int)piece.type
                );

                key.Append(
                    piece.color ==
                    PieceColor.White
                        ? 'w'
                        : 'b'
                );
            }
        }


        // Side to move
        key.Append(
            position.currentTurn ==
            PieceColor.White
                ? 'W'
                : 'B'
        );


        // Castling rights
        key.Append(
            position.whiteKingMoved
                ? '1'
                : '0'
        );

        key.Append(
            position.blackKingMoved
                ? '1'
                : '0'
        );

        key.Append(
            position.whiteKingsideRookMoved
                ? '1'
                : '0'
        );

        key.Append(
            position.whiteQueensideRookMoved
                ? '1'
                : '0'
        );

        key.Append(
            position.blackKingsideRookMoved
                ? '1'
                : '0'
        );

        key.Append(
            position.blackQueensideRookMoved
                ? '1'
                : '0'
        );


        // En passant
        key.Append('|');
        key.Append(
            position.enPassantSquare
        );


        return key.ToString();
    }


    // =====================================================
    // MOVE TO FRONT
    // =====================================================

    private void MoveToFront(
        List<Move> moves,
        Move preferredMove
    )
    {
        for (
            int i = 0;
            i < moves.Count;
            i++
        )
        {
            if (
                SameMove(
                    moves[i],
                    preferredMove
                )
            )
            {
                if (i == 0)
                {
                    return;
                }


                Move temp =
                    moves[0];

                moves[0] =
                    moves[i];

                moves[i] =
                    temp;

                return;
            }
        }
    }


    // =====================================================
    // MOVE COMPARISON
    // =====================================================

    private bool SameMove(
        Move a,
        Move b
    )
    {
        return
            a.From == b.From &&
            a.To == b.To &&
            a.Type == b.Type &&
            a.PromotionPiece ==
                b.PromotionPiece;
    }


    // =====================================================
    // DEFAULT MOVE CHECK
    // =====================================================

    private bool IsDefaultMove(Move move)
    {
        return
            move.From == 0 &&
            move.To == 0 &&
            move.Type == MoveType.Normal &&
            move.PromotionPiece == PieceType.None;
    }


    // =====================================================
    // TIME CHECK
    // =====================================================

    private bool TimeExpired()
    {
        if (searchTimer == null)
        {
            return false;
        }


        return
            searchTimer.ElapsedMilliseconds >=
            ThinkTimeMilliseconds;
    }


    // =====================================================
    // CREATE TEST ENGINE
    // =====================================================

    private ChessEngine CreateTestEngine()
    {
        return CreateTestEngine(engine);
    }


    private ChessEngine CreateTestEngine(
        ChessEngine source
    )
    {
        ChessEngine test =
            new ChessEngine();


        test.board =
            source.board.Copy();


        test.currentTurn =
            source.currentTurn;


        test.enPassantSquare =
            source.enPassantSquare;


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
