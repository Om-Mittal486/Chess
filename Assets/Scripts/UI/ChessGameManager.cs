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

    private Dictionary<int, PieceView> pieceViews = new Dictionary<int, PieceView>();

    private void Start()
    {
        board = new Board();
        board.SetStartingPosition();

        SpawnPieces();

        TestKnightMoves(1);
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
        Piece piece = board.GetPiece(square);

        // Don't select empty squares
        if (piece.IsEmpty())
            return;

        selectedSquare = square;

        Debug.Log(
            "Selected: " + GetSquareName(square)
        );
    }

    // Called when the player clicks a destination square
    public void SelectDestination(int square)
    {
        if (selectedSquare == -1)
            return;

        Piece selectedPiece =
            board.GetPiece(selectedSquare);

        // Check whether the requested move is legal
        if (!IsLegalMove(selectedSquare, square))
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

        // Update the internal board
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
        // Nothing selected yet → select this piece
        if (selectedSquare == -1)
        {
            SelectPiece(square);
            return;
        }

        // Something is already selected.
        // Treat this clicked piece as the destination for now.
        SelectDestination(square);
    }

    private void TestKnightMoves(int square)
    {
        List<Move> moves =
            MoveGenerator.GenerateKnightMoves(
                board,
                square
            );

        Debug.Log(
            "Knight moves from " +
            GetSquareName(square)
        );

        foreach (Move move in moves)
        {
            Debug.Log(
                GetSquareName(move.From) +
                " → " +
                GetSquareName(move.To)
            );
        }
    }

    private bool IsLegalMove(int from, int to)
    {
        List<Move> moves =
            MoveGenerator.GenerateMoves(
                board,
                from
            );


        foreach(Move move in moves)
        {
            if(move.To == to)
                return true;
        }


        return false;
    }
}