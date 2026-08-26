using System;
using System.Collections.Generic;

public class OpeningBook
{
    private class BookCandidate
    {
        public Move move;
        public int weight;

        public BookCandidate(
            Move move,
            int weight
        )
        {
            this.move = move;
            this.weight = weight;
        }
    }


    private Dictionary<string, List<BookCandidate>> book =
        new Dictionary<string, List<BookCandidate>>();


    // Standard opening book depth.
    // The supplied lines contain roughly the first
    // 8-10 plies of several common openings.
    private const int MaxBookPly = 10;


    public OpeningBook()
    {
        BuildBook();
    }


    // =====================================================
    // PUBLIC LOOKUP
    // =====================================================

    public bool TryGetMove(
        ChessEngine position,
        List<Move> legalMoves,
        out Move bookMove
    )
    {
        bookMove = default(Move);

        string key =
            GetPositionKey(position);

        List<BookCandidate> candidates;

        if (
            !book.TryGetValue(
                key,
                out candidates
            )
        )
        {
            return false;
        }


        // Keep the book legal even if the engine's move
        // representation changes.
        List<BookCandidate> legalCandidates =
            new List<BookCandidate>();


        foreach (
            BookCandidate candidate
            in candidates
        )
        {
            if (
                ContainsMove(
                    legalMoves,
                    candidate.move
                )
            )
            {
                legalCandidates.Add(
                    candidate
                );
            }
        }


        if (legalCandidates.Count == 0)
        {
            return false;
        }


        // Deterministic weighted selection.
        // This gives variety while keeping games reproducible
        // for benchmarking.
        int totalWeight = 0;

        foreach (
            BookCandidate candidate
            in legalCandidates
        )
        {
            totalWeight +=
                Math.Max(
                    1,
                    candidate.weight
                );
        }


        int selector =
            Math.Abs(
                key.GetHashCode()
            ) % totalWeight;


        foreach (
            BookCandidate candidate
            in legalCandidates
        )
        {
            int weight =
                Math.Max(
                    1,
                    candidate.weight
                );

            if (selector < weight)
            {
                // Return the exact legal move object, not the
                // book's coordinate-only copy.
                foreach (
                    Move legalMove
                    in legalMoves
                )
                {
                    if (
                        SameMoveForBook(
                            legalMove,
                            candidate.move
                        )
                    )
                    {
                        bookMove =
                            legalMove;

                        return true;
                    }
                }
            }

            selector -= weight;
        }


        Move fallback =
            legalCandidates[
                legalCandidates.Count - 1
            ].move;

        foreach (
            Move legalMove
            in legalMoves
        )
        {
            if (
                SameMoveForBook(
                    legalMove,
                    fallback
                )
            )
            {
                bookMove =
                    legalMove;

                return true;
            }
        }

        return false;
    }


    // =====================================================
    // BUILD BOOK
    // =====================================================

    private void BuildBook()
    {
        // Ruy Lopez
        AddLine(
            30,
            "e2e4",
            "e7e5",
            "g1f3",
            "b8c6",
            "f1b5",
            "a7a6",
            "b5a4",
            "g8f6",
            "e1g1",
            "f8e7"
        );


        // Italian Game
        AddLine(
            25,
            "e2e4",
            "e7e5",
            "g1f3",
            "b8c6",
            "f1c4",
            "f8c5",
            "e1g1",
            "g8f6",
            "d2d3",
            "e8g8"
        );


        // Scotch Game
        AddLine(
            20,
            "e2e4",
            "e7e5",
            "g1f3",
            "b8c6",
            "d2d4",
            "e5d4",
            "f3d4",
            "g8f6",
            "b1c3",
            "f8b4"
        );


        // Sicilian Defense
        AddLine(
            30,
            "e2e4",
            "c7c5",
            "g1f3",
            "d7d6",
            "d2d4",
            "c5d4",
            "f3d4",
            "g8f6",
            "b1c3",
            "a7a6"
        );


        // Caro-Kann
        AddLine(
            20,
            "e2e4",
            "c7c6",
            "d2d4",
            "d7d5",
            "e4d5",
            "c6d5",
            "b1c3",
            "g8f6",
            "c1f4",
            "e7e6"
        );


        // French Defense
        AddLine(
            20,
            "e2e4",
            "e7e6",
            "d2d4",
            "d7d5",
            "b1c3",
            "g8f6",
            "e4e5",
            "f6d7",
            "f1d3",
            "c7c5"
        );


        // Queen's Gambit
        AddLine(
            30,
            "d2d4",
            "d7d5",
            "c2c4",
            "e7e6",
            "b1c3",
            "g8f6",
            "c1g5",
            "f8e7",
            "e2e3",
            "e8g8"
        );


        // Queen's Gambit Declined alternative
        AddLine(
            15,
            "d2d4",
            "d7d5",
            "c2c4",
            "e7e6",
            "g1f3",
            "g8f6",
            "g2g3",
            "f8e7",
            "f1g2",
            "e8g8"
        );


        // King's Indian Defense
        AddLine(
            25,
            "d2d4",
            "g8f6",
            "c2c4",
            "g7g6",
            "b1c3",
            "f8g7",
            "e2e4",
            "d7d6",
            "g1f3",
            "e8g8"
        );


        // English Opening
        AddLine(
            20,
            "c2c4",
            "e7e5",
            "b1c3",
            "g8f6",
            "g2g3",
            "f8b4",
            "f1g2",
            "e8g8",
            "e2e3",
            "d7d6"
        );


        // London System
        AddLine(
            20,
            "d2d4",
            "d7d5",
            "g1f3",
            "g8f6",
            "c1f4",
            "e7e6",
            "e2e3",
            "f8d6",
            "f1d3",
            "e8g8"
        );
    }


    // =====================================================
    // ADD LINE
    // =====================================================

    private void AddLine(
        int weight,
        params string[] moves
    )
    {
        if (
            moves == null ||
            moves.Length == 0
        )
        {
            return;
        }


        ChessEngine position =
            new ChessEngine();


        int maxPlies =
            Math.Min(
                moves.Length,
                MaxBookPly
            );


        for (
            int ply = 0;
            ply < maxPlies;
            ply++
        )
        {
            string key =
                GetPositionKey(position);


            Move requestedMove =
                ParseMove(
                    moves[ply]
                );

            // Resolve the coordinate move against the
            // engine's actual legal move list. This preserves
            // Capture, EnPassant, Promotion and Castling types.
            List<Move> legalMoves =
                position.GetAllLegalMoves(
                    position.currentTurn
                );

            Move move;

            if (
                !TryResolveLegalMove(
                    legalMoves,
                    requestedMove,
                    out move
                )
            )
            {
                // If a book line ever becomes incompatible
                // with the engine's move generator, stop that
                // line instead of corrupting the temporary board.
                break;
            }


            AddCandidate(
                key,
                move,
                weight
            );


            position.MakeMove(move);
        }
    }


    private void AddCandidate(
        string key,
        Move move,
        int weight
    )
    {
        List<BookCandidate> candidates;

        if (
            !book.TryGetValue(
                key,
                out candidates
            )
        )
        {
            candidates =
                new List<BookCandidate>();

            book[key] =
                candidates;
        }


        // Avoid duplicate entries.
        for (
            int i = 0;
            i < candidates.Count;
            i++
        )
        {
            if (
                SameMove(
                    candidates[i].move,
                    move
                )
            )
            {
                candidates[i].weight += weight;
                return;
            }
        }


        candidates.Add(
            new BookCandidate(
                move,
                weight
            )
        );
    }


    // =====================================================
    // MOVE PARSER
    // =====================================================

    private Move ParseMove(
        string coordinateMove
    )
    {
        int from =
            SquareFromName(
                coordinateMove.Substring(
                    0,
                    2
                )
            );

        int to =
            SquareFromName(
                coordinateMove.Substring(
                    2,
                    2
                )
            );


        // Castling
        if (
            (from == SquareFromName("e1") &&
             to == SquareFromName("g1")) ||
            (from == SquareFromName("e8") &&
             to == SquareFromName("g8"))
        )
        {
            return new Move(
                from,
                to,
                MoveType.CastleKingSide
            );
        }


        if (
            (from == SquareFromName("e1") &&
             to == SquareFromName("c1")) ||
            (from == SquareFromName("e8") &&
             to == SquareFromName("c8"))
        )
        {
            return new Move(
                from,
                to,
                MoveType.CastleQueenSide
            );
        }


        // Normal/capture type is determined by the engine's
        // MakeMove implementation. For book matching, the
        // coordinates are the important part.
        return new Move(
            from,
            to,
            MoveType.Normal
        );
    }


    private int SquareFromName(
        string square
    )
    {
        int file =
            square[0] - 'a';

        int rank =
            square[1] - '1';


        return
            rank * 8 +
            file;
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


        key.Append(
            position.currentTurn ==
            PieceColor.White
                ? 'W'
                : 'B'
        );


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


        key.Append('|');

        key.Append(
            position.enPassantSquare
        );


        return key.ToString();
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private bool TryResolveLegalMove(
        List<Move> legalMoves,
        Move requestedMove,
        out Move resolvedMove
    )
    {
        foreach (
            Move legalMove
            in legalMoves
        )
        {
            if (
                SameMoveForBook(
                    legalMove,
                    requestedMove
                )
            )
            {
                resolvedMove =
                    legalMove;

                return true;
            }
        }


        resolvedMove =
            default(Move);

        return false;
    }


    private bool ContainsMove(
        List<Move> legalMoves,
        Move target
    )
    {
        foreach (
            Move move
            in legalMoves
        )
        {
            if (
                SameMoveForBook(
                    move,
                    target
                )
            )
            {
                return true;
            }
        }


        return false;
    }


    private bool SameMoveForBook(
        Move a,
        Move b
    )
    {
        return
            a.From == b.From &&
            a.To == b.To;
    }


    private bool SameMove(
        Move a,
        Move b
    )
    {
        return
            a.From == b.From &&
            a.To == b.To;
    }
}
