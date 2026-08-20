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

        Move move =
            new Move(selectedSquare, square);

        // Handle capture
        HandleCapture(square);

        // Update internal board
        board.MakeMove(move);

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
        Piece clickedPiece = board.GetPiece(square);

        if (selectedSquare == -1)
        {
            SelectPiece(square);
            return;
        }

        Piece selectedPiece = board.GetPiece(selectedSquare);

        if (!clickedPiece.IsEmpty() &&
            clickedPiece.color == selectedPiece.color)
        {
            SelectPiece(square);
            return;
        }

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


        // Generate the piece's movement possibilities
        List<Move> moves =
            MoveGenerator.GenerateMoves(
                board,
                from
            );


        // Check whether destination is a possible move
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
        Board testBoard = board.Copy();


        // Apply the move to the temporary board
        testBoard.MakeMove(
            new Move(from, to)
        );


        // Check whether our King is now attacked
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
}