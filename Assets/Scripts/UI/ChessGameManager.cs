using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChessGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform boardTransform;
    [SerializeField] private GameObject piecePrefab;

    [Header("White Pieces")]
    [SerializeField] private Sprite whitePawn;
    [SerializeField] private Sprite whiteKnight;
    [SerializeField] private Sprite whiteBishop;
    [SerializeField] private Sprite whiteRook;
    [SerializeField] private Sprite whiteQueen;
    [SerializeField] private Sprite whiteKing;

    [Header("Black Pieces")]
    [SerializeField] private Sprite blackPawn;
    [SerializeField] private Sprite blackKnight;
    [SerializeField] private Sprite blackBishop;
    [SerializeField] private Sprite blackRook;
    [SerializeField] private Sprite blackQueen;
    [SerializeField] private Sprite blackKing;

    private Board board;

    // Currently selected piece
    private int selectedSquare = -1;

    private PieceColor currentTurn = PieceColor.White;

    private GameState currentGameState = GameState.Playing;

    private Dictionary<int, PieceView> pieceViews = new Dictionary<int, PieceView>();

    private Dictionary<int, SquareView> squareViews = new Dictionary<int, SquareView>();

    private bool whiteKingMoved = false;
    private bool blackKingMoved = false;

    private bool whiteKingsideRookMoved = false;
    private bool whiteQueensideRookMoved = false;

    private bool blackKingsideRookMoved = false;
    private bool blackQueensideRookMoved = false;

    private void Start()
    {
        board = new Board();
        board.SetStartingPosition();

        SpawnPieces();
    }

    private void SpawnPieces()
    {
        for (int square = 0; square < 64; square++)
        {
            Piece piece = board.GetPiece(square);

            // Don't create anything for empty squares
            if (piece.IsEmpty())
                continue;

            // Create visual piece
            GameObject pieceObject =
                Instantiate(piecePrefab, boardTransform);

            pieceObject.name = GetSquareName(square);

            // Position the piece
            RectTransform rect =
                pieceObject.GetComponent<RectTransform>();

            rect.anchoredPosition = GetPosition(square);

            // Configure PieceView
            PieceView pieceView =
                pieceObject.GetComponent<PieceView>();

            pieceView.SetSprite(GetSprite(piece));
            pieceView.SetSquare(square);
            pieceView.SetGameManager(this);

            pieceViews.Add(square, pieceView);

            // Make the piece clickable
            Button button =
                pieceObject.GetComponent<Button>();

            button.onClick.AddListener(
                pieceView.OnPieceClicked
            );
        }
    }

    // Called when the player clicks a chess piece
    public void SelectPiece(int square)
    {
        if (currentGameState == GameState.Checkmate ||
            currentGameState == GameState.Stalemate)
        {
            return;
        }

        Piece piece = board.GetPiece(square);

        if (piece.IsEmpty())
            return;

        // Only allow the current player to select their pieces
        if (piece.color != currentTurn)
        {
            Debug.Log(
                "It's " +
                currentTurn +
                "'s turn."
            );

            return;
        }

        selectedSquare = square;

        Debug.Log(
            "Selected: " +
            GetSquareName(square)
        );

        ShowLegalMoves(square);
    }

    // Called when the player clicks a destination square
    public void SelectDestination(int square)
    {
        if (selectedSquare == -1)
            return;

        Piece movingPiece =
            board.GetPiece(selectedSquare);

        // Make sure the selected piece belongs
        // to the current player
        if (movingPiece.color != currentTurn)
        {
            selectedSquare = -1;
            ClearMoveHints();
            return;
        }

        // Check whether the move is legal
        if (!IsMoveLegal(selectedSquare, square))
        {
            Debug.Log(
                "Illegal move: " +
                GetSquareName(selectedSquare) +
                " → " +
                GetSquareName(square)
            );

            return;
        }

        Debug.Log(
            "Legal move: " +
            GetSquareName(selectedSquare) +
            " → " +
            GetSquareName(square)
        );

        MoveType moveType = MoveType.Normal;

        if (movingPiece.type == PieceType.King &&
            Mathf.Abs(square - selectedSquare) == 2)
        {
            if (square > selectedSquare)
            {
                moveType = MoveType.CastleKingSide;
            }
            else
            {
                moveType = MoveType.CastleQueenSide;
            }
        }

        Move move =
            new Move(
                selectedSquare,
                square,
                moveType
            );

        // Handle capture
        if (move.Type != MoveType.CastleKingSide &&
            move.Type != MoveType.CastleQueenSide)
        {
            HandleCapture(square);
        }

        // Update internal board
        board.MakeMove(move);

        // Update king/rook movement rights
        UpdateMovementRights(
            selectedSquare,
            movingPiece
        );

        // Handle castling rook movement
        if (move.Type == MoveType.CastleKingSide)
        {
            PerformCastle(
                selectedSquare,
                square,
                true
            );
        }
        else if (move.Type == MoveType.CastleQueenSide)
        {
            PerformCastle(
                selectedSquare,
                square,
                false
            );
        }

        // Move visual piece
        PieceView pieceView =
            pieceViews[selectedSquare];

        pieceView.MoveTo(
            GetPosition(square)
        );

        // Update dictionary
        pieceViews.Remove(selectedSquare);
        pieceViews[square] = pieceView;

        // Update PieceView
        pieceView.SetSquare(square);

        // Clear move suggestions
        ClearMoveHints();

        // Clear selection
        selectedSquare = -1;

        // Switch player
        SwitchTurn();
    }

    // Returns the correct sprite for a piece
    private Sprite GetSprite(Piece piece)
    {
        if (piece.color == PieceColor.White)
        {
            switch (piece.type)
            {
                case PieceType.Pawn:
                    return whitePawn;

                case PieceType.Knight:
                    return whiteKnight;

                case PieceType.Bishop:
                    return whiteBishop;

                case PieceType.Rook:
                    return whiteRook;

                case PieceType.Queen:
                    return whiteQueen;

                case PieceType.King:
                    return whiteKing;
            }
        }
        else
        {
            switch (piece.type)
            {
                case PieceType.Pawn:
                    return blackPawn;

                case PieceType.Knight:
                    return blackKnight;

                case PieceType.Bishop:
                    return blackBishop;

                case PieceType.Rook:
                    return blackRook;

                case PieceType.Queen:
                    return blackQueen;

                case PieceType.King:
                    return blackKing;
            }
        }

        return null;
    }

    // Converts square index into a position on the UI board
    private Vector2 GetPosition(int square)
    {
        int file = square % 8;
        int rank = square / 8;

        float cellSize = 100f;

        float x =
            (file * cellSize)
            + cellSize / 2f
            - 400f;

        float y =
            (rank * cellSize)
            + cellSize / 2f
            - 400f;

        return new Vector2(x, y);
    }

    // Converts:
    // 0  -> a1
    // 4  -> e1
    // 60 -> e8
    // 63 -> h8
    private string GetSquareName(int square)
    {
        int file = square % 8;
        int rank = square / 8;

        char fileLetter =
            (char)('a' + file);

        return $"{fileLetter}{rank + 1}";
    }

    public void SelectDestinationOrSelectPiece(int square)
    {
        Piece clickedPiece =
            board.GetPiece(square);

        // Nothing selected
        if (selectedSquare == -1)
        {
            SelectPiece(square);
            return;
        }

        Piece selectedPiece =
            board.GetPiece(selectedSquare);

        // Clicked another friendly piece
        if (!clickedPiece.IsEmpty() &&
            clickedPiece.color == selectedPiece.color)
        {
            SelectPiece(square);
            return;
        }

        // Otherwise, treat it as a destination
        SelectDestination(square);
    }

    private void HandleCapture(int square)
    {
        Piece targetPiece =
            board.GetPiece(square);

        // Nothing to capture
        if (targetPiece.IsEmpty())
            return;

        // Find the visual piece
        if (pieceViews.TryGetValue(
            square,
            out PieceView capturedPiece))
        {
            Destroy(capturedPiece.gameObject);

            pieceViews.Remove(square);
        }

        Debug.Log(
            "Captured piece on " +
            GetSquareName(square)
        );
    }

    private void SwitchTurn()
    {
        if (currentTurn == PieceColor.White)
        {
            currentTurn = PieceColor.Black;
        }
        else
        {
            currentTurn = PieceColor.White;
        }

        Debug.Log(
            "Turn: " +
            currentTurn
        );

        currentGameState = GetGameState();

        Debug.Log(
            "Game State: " +
            currentGameState
        );
    }

    private bool IsKingInCheck(
        PieceColor color)
    {
        int kingSquare = -1;

        for (int square = 0;
            square < 64;
            square++)
        {
            Piece piece =
                board.GetPiece(square);

            if (piece.type == PieceType.King &&
                piece.color == color)
            {
                kingSquare = square;
                break;
            }
        }

        // Safety check
        if (kingSquare == -1)
            return false;

        PieceColor enemyColor =
            color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

        return AttackDetector.IsSquareAttacked(
            board,
            kingSquare,
            enemyColor);
    }

    private bool IsKingInCheck(
        Board checkBoard,
        PieceColor color)
    {
        int kingSquare = -1;


        for (int square = 0;
            square < 64;
            square++)
        {
            Piece piece =
                checkBoard.GetPiece(square);


            if (piece.type == PieceType.King &&
                piece.color == color)
            {
                kingSquare = square;
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

    private bool IsMoveLegal(int from, int to)
    {
        Piece movingPiece =
            board.GetPiece(from);

        if (movingPiece.IsEmpty())
            return false;

        // -------------------------------------------------
        // CASTLING
        // -------------------------------------------------

        if (movingPiece.type == PieceType.King &&
            Mathf.Abs(to - from) == 2)
        {
            return IsCastlingLegal(
                from,
                to,
                movingPiece.color
            );
        }

        // -------------------------------------------------
        // NORMAL MOVES
        // -------------------------------------------------

        List<Move> moves =
            MoveGenerator.GenerateMoves(
                board,
                from
            );

        bool pseudoLegal = false;

        foreach (Move move in moves)
        {
            if (move.To == to)
            {
                pseudoLegal = true;
                break;
            }
        }

        if (!pseudoLegal)
            return false;

        // Create temporary board
        Board testBoard =
            board.Copy();

        // Apply move
        testBoard.MakeMove(
            new Move(from, to)
        );

        // Make sure our own King isn't left in check
        if (IsKingInCheck(
            testBoard,
            movingPiece.color))
        {
            return false;
        }

        return true;
    }

    private List<Move> GetAllLegalMoves(PieceColor color)
    {
        List<Move> legalMoves = new List<Move>();

        for (int square = 0; square < 64; square++)
        {
            Piece piece = board.GetPiece(square);

            // Ignore empty squares
            if (piece.IsEmpty())
                continue;

            // Ignore opponent pieces
            if (piece.color != color)
                continue;

            List<Move> pseudoLegalMoves =
                MoveGenerator.GenerateMoves(
                    board,
                    square
                );

            foreach (Move move in pseudoLegalMoves)
            {
                if (IsMoveLegal(
                    move.From,
                    move.To))
                {
                    legalMoves.Add(move);
                }
            }
        }

        return legalMoves;
    }

    private GameState GetGameState()
    {
        bool inCheck =
            IsKingInCheck(currentTurn);

        List<Move> legalMoves =
            GetAllLegalMoves(currentTurn);

        if (legalMoves.Count == 0)
        {
            if (inCheck)
            {
                return GameState.Checkmate;
            }

            return GameState.Stalemate;
        }

        if (inCheck)
        {
            return GameState.Check;
        }

        return GameState.Playing;
    }

    public void RegisterSquareView(
        int square,
        SquareView squareView
    )
    {
        squareViews[square] = squareView;
    }

    private void ClearMoveHints()
    {
        foreach (SquareView squareView in squareViews.Values)
        {
            squareView.HideMoveHint();
        }
    }

    private void ShowLegalMoves(int from)
    {
        ClearMoveHints();

        List<Move> moves =
            MoveGenerator.GenerateMoves(
                board,
                from
            );

        foreach (Move move in moves)
        {
            if (IsMoveLegal(
                move.From,
                move.To))
            {
                if (squareViews.TryGetValue(
                    move.To,
                    out SquareView squareView))
                {
                    squareView.ShowMoveHint();
                }
            }
        }
    }

    private bool IsCastlingLegal(
        int from,
        int to,
        PieceColor color)
    {
        bool kingside = to > from;

        bool kingMoved =
            color == PieceColor.White
                ? whiteKingMoved
                : blackKingMoved;

        bool rookMoved;

        if (color == PieceColor.White)
        {
            rookMoved = kingside
                ? whiteKingsideRookMoved
                : whiteQueensideRookMoved;
        }
        else
        {
            rookMoved = kingside
                ? blackKingsideRookMoved
                : blackQueensideRookMoved;
        }

        // King or required rook already moved
        if (kingMoved || rookMoved)
            return false;

        // King must currently be on its starting square
        int expectedKingSquare =
            color == PieceColor.White ? 4 : 60;

        if (from != expectedKingSquare)
            return false;

        // King must currently exist there
        Piece king =
            board.GetPiece(from);

        if (king.type != PieceType.King ||
            king.color != color)
        {
            return false;
        }

        // King cannot castle while in check
        if (IsKingInCheck(color))
            return false;

        // Determine squares
        int direction =
            kingside ? 1 : -1;

        int middleSquare =
            from + direction;

        // King cannot pass through an attacked square
        if (AttackDetector.IsSquareAttacked(
            board,
            middleSquare,
            GetOpponentColor(color)))
        {
            return false;
        }

        // King cannot land on an attacked square
        if (AttackDetector.IsSquareAttacked(
            board,
            to,
            GetOpponentColor(color)))
        {
            return false;
        }

        // Make sure the correct rook exists
        int rookSquare =
            kingside
                ? from + 3
                : from - 4;

        Piece rook =
            board.GetPiece(rookSquare);

        if (rook.type != PieceType.Rook ||
            rook.color != color)
        {
            return false;
        }

        // Make sure the squares between King and Rook are empty
        if (kingside)
        {
            if (!board.GetPiece(from + 1).IsEmpty() ||
                !board.GetPiece(from + 2).IsEmpty())
            {
                return false;
            }
        }
        else
        {
            if (!board.GetPiece(from - 1).IsEmpty() ||
                !board.GetPiece(from - 2).IsEmpty() ||
                !board.GetPiece(from - 3).IsEmpty())
            {
                return false;
            }
        }

        return true;
    }

    private PieceColor GetOpponentColor(
        PieceColor color)
    {
        return color == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;
    }

    private void PerformCastle(
        int kingFrom,
        int kingTo,
        bool kingside)
    {
        int rookFrom;
        int rookTo;

        if (kingTo > kingFrom)
        {
            // Kingside
            rookFrom = kingFrom + 3;
            rookTo = kingFrom + 1;
        }
        else
        {
            // Queenside
            rookFrom = kingFrom - 4;
            rookTo = kingFrom - 1;
        }

        PieceView rookView =
            pieceViews[rookFrom];

        rookView.MoveTo(
            GetPosition(rookTo)
        );

        pieceViews.Remove(rookFrom);
        pieceViews[rookTo] = rookView;

        rookView.SetSquare(rookTo);

        board.MakeMove(
            new Move(
                rookFrom,
                rookTo
            )
        );
    }

    private void UpdateMovementRights(
        int from,
        Piece piece)
    {
        if (piece.type == PieceType.King)
        {
            if (piece.color == PieceColor.White)
                whiteKingMoved = true;
            else
                blackKingMoved = true;
        }

        if (piece.type == PieceType.Rook)
        {
            if (piece.color == PieceColor.White)
            {
                if (from == 0)
                    whiteQueensideRookMoved = true;

                if (from == 7)
                    whiteKingsideRookMoved = true;
            }
            else
            {
                if (from == 56)
                    blackQueensideRookMoved = true;

                if (from == 63)
                    blackKingsideRookMoved = true;
            }
        }
    }
}