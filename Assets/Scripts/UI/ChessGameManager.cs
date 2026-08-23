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

    [Header("Promotion")]
    [SerializeField] private GameObject whitePromotionPanel;

    [SerializeField] private GameObject blackPromotionPanel;

    private ChessEngine engine;

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

    private int enPassantSquare = -1;

    // Number of consecutive half-moves
    // without a pawn move or capture.
    private int halfmoveClock = 0;

    private bool waitingForPromotion = false;

    private int promotionSquare = -1;

    private PieceColor promotionColor;

    private bool gameOver = false;

    private Dictionary<string, int> positionHistory = new Dictionary<string, int>();

    private Stack<MoveState> moveHistory = new Stack<MoveState>();

    private ulong currentPositionHash;

    private void Start()
    {
        engine = new ChessEngine();

        currentTurn = PieceColor.White;
        enPassantSquare = -1;

        whiteKingMoved = false;
        blackKingMoved = false;

        whiteKingsideRookMoved = false;
        whiteQueensideRookMoved = false;

        blackKingsideRookMoved = false;
        blackQueensideRookMoved = false;

        SyncEngineState();

        RecordPosition();
        UpdatePositionHash();
        SpawnPieces();
    }

    private void SyncEngineState()
    {
        if (engine == null)
            return;

        engine.currentTurn = currentTurn;
        engine.enPassantSquare = enPassantSquare;

        engine.whiteKingMoved = whiteKingMoved;
        engine.blackKingMoved = blackKingMoved;

        engine.whiteKingsideRookMoved =
            whiteKingsideRookMoved;
        engine.whiteQueensideRookMoved =
            whiteQueensideRookMoved;

        engine.blackKingsideRookMoved =
            blackKingsideRookMoved;
        engine.blackQueensideRookMoved =
            blackQueensideRookMoved;
    }

    private void UpdatePositionHash()
    {
        currentPositionHash =
            ZobristHash.CalculateHash(
                engine.board,
                currentTurn,
                whiteKingMoved,
                blackKingMoved,
                whiteKingsideRookMoved,
                whiteQueensideRookMoved,
                blackKingsideRookMoved,
                blackQueensideRookMoved,
                enPassantSquare
            );
    }

    private bool IsInsufficientMaterialDraw()
    {
        int whiteBishops = 0;
        int whiteKnights = 0;

        int blackBishops = 0;
        int blackKnights = 0;

        int whiteOtherPieces = 0;
        int blackOtherPieces = 0;


        // Examine the entire board
        for (int square = 0; square < 64; square++)
        {
            Piece piece =
                engine.board.GetPiece(square);

            if (piece.IsEmpty())
                continue;


            switch (piece.type)
            {
                case PieceType.Bishop:

                    if (piece.color == PieceColor.White)
                        whiteBishops++;
                    else
                        blackBishops++;

                    break;


                case PieceType.Knight:

                    if (piece.color == PieceColor.White)
                        whiteKnights++;
                    else
                        blackKnights++;

                    break;


                case PieceType.King:

                    // Ignore kings
                    break;


                default:

                    // Pawn, Rook or Queen
                    if (piece.color == PieceColor.White)
                        whiteOtherPieces++;
                    else
                        blackOtherPieces++;

                    break;
            }
        }


        // Pawn, Rook or Queen means
        // we have potentially sufficient material.
        if (whiteOtherPieces > 0 ||
            blackOtherPieces > 0)
        {
            return false;
        }


        int whiteMinorPieces =
            whiteBishops + whiteKnights;

        int blackMinorPieces =
            blackBishops + blackKnights;


        // -------------------------------------------------
        // King vs King
        // -------------------------------------------------

        if (whiteMinorPieces == 0 &&
            blackMinorPieces == 0)
        {
            return true;
        }


        // -------------------------------------------------
        // King + Bishop/Knight vs King
        // -------------------------------------------------

        if (whiteMinorPieces == 1 &&
            blackMinorPieces == 0)
        {
            return true;
        }

        if (blackMinorPieces == 1 &&
            whiteMinorPieces == 0)
        {
            return true;
        }

        return false;
    }

    private string RecordPosition()
    {
        string key =
            GetPositionKey();

        if (positionHistory.ContainsKey(key))
        {
            positionHistory[key]++;
        }
        else
        {
            positionHistory[key] = 1;
        }

        return key;
    }

    private string GetPositionKey()
    {
        System.Text.StringBuilder key =
            new System.Text.StringBuilder();

        // Board pieces
        for (int square = 0; square < 64; square++)
        {
            Piece piece =
                engine.board.GetPiece(square);

            key.Append(
                (int)piece.type
            );

            key.Append(
                (int)piece.color
            );
        }

        // Side to move
        key.Append(
            (int)currentTurn
        );

        // Castling rights
        key.Append(
            whiteKingMoved ? "1" : "0"
        );

        key.Append(
            blackKingMoved ? "1" : "0"
        );

        key.Append(
            whiteKingsideRookMoved ? "1" : "0"
        );

        key.Append(
            whiteQueensideRookMoved ? "1" : "0"
        );

        key.Append(
            blackKingsideRookMoved ? "1" : "0"
        );

        key.Append(
            blackQueensideRookMoved ? "1" : "0"
        );

        // En Passant state
        key.Append(
            enPassantSquare
        );

        return key.ToString();
    }

    public void PromoteToQueen()
    {
        PromoteTo(PieceType.Queen);
    }

    public void PromoteToRook()
    {
        PromoteTo(PieceType.Rook);
    }

    public void PromoteToBishop()
    {
        PromoteTo(PieceType.Bishop);
    }

    public void PromoteToKnight()
    {
        PromoteTo(PieceType.Knight);
    }

    private void SpawnPieces()
    {
        for (int square = 0; square < 64; square++)
        {
            Piece piece = engine.board.GetPiece(square);

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

    private void SpawnPiece(
        PieceType type,
        PieceColor color,
        int square
    )
    {
        GameObject pieceObject =
            Instantiate(
                piecePrefab,
                boardTransform
            );


        pieceObject.name =
            GetSquareName(square);


        RectTransform rect =
            pieceObject.GetComponent<RectTransform>();


        rect.anchoredPosition =
            GetPosition(square);


        PieceView pieceView =
            pieceObject.GetComponent<PieceView>();


        Piece piece =
            new Piece(
                type,
                color
            );


        pieceView.SetSprite(
            GetSprite(piece)
        );


        pieceView.SetSquare(square);

        pieceView.SetGameManager(this);


        pieceViews.Add(
            square,
            pieceView
        );


        Button button =
            pieceObject.GetComponent<Button>();


        button.onClick.AddListener(
            pieceView.OnPieceClicked
        );
    }

    // Called when the player clicks a chess piece
    public void SelectPiece(int square)
    {
        if (currentGameState == GameState.Checkmate ||
            currentGameState == GameState.Stalemate)
        {
            return;
        }

        Piece piece = engine.board.GetPiece(square);

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

        SyncEngineState();

        Debug.Log(
            "Selected: " +
            GetSquareName(square)
        );

        ShowLegalMoves(square);
    }

    // Called when the player clicks a destination square
    public void SelectDestination(int square)
    {
        if (gameOver)
            return;
            
        if (selectedSquare == -1)
            return;

        Piece movingPiece =
            engine.board.GetPiece(selectedSquare);

        // Make sure the selected piece belongs
        // to the current player
        if (movingPiece.color != currentTurn)
        {
            selectedSquare = -1;
            ClearMoveHints();
            return;
        }

        // Synchronize engine state before validating the move.
        SyncEngineState();

        // Check whether the move is legal
        if (!engine.IsMoveLegal(selectedSquare, square))
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

        // Determine move type
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
        else if (movingPiece.type == PieceType.Pawn &&
                square == enPassantSquare &&
                Mathf.Abs(square - selectedSquare) != 8)
        {
            moveType = MoveType.EnPassant;
        }

        PieceType promotionPiece = PieceType.None;


        if(movingPiece.type == PieceType.Pawn)
        {
            int targetRank = square / 8;


            if(targetRank == 7 ||
                targetRank == 0)
            {
                moveType = MoveType.Promotion;

                waitingForPromotion = true;

                promotionSquare = square;

                promotionColor = movingPiece.color;

                // Open correct promotion panel
                if (promotionColor == PieceColor.White)
                {
                    whitePromotionPanel.SetActive(true);
                }
                else
                {
                    blackPromotionPanel.SetActive(true);
                }
            }
        }


        Move move =
            new Move(
                selectedSquare,
                square,
                moveType,
                promotionPiece
            );

        // Handle capture
        if (move.Type == MoveType.EnPassant)
        {
            HandleEnPassantCapture(square);
        }
        else if (move.Type != MoveType.CastleKingSide &&
                move.Type != MoveType.CastleQueenSide)
        {
            HandleCapture(square);
        }

        bool wasCapture = !engine.board.GetPiece(square).IsEmpty();

        SaveMoveState(move);
        // Update internal board
        engine.board.MakeMove(move);

        UpdateHalfmoveClock(movingPiece, wasCapture, move);

        // If En Passant, remove the captured pawn
        // from the actual board as well
        if (move.Type == MoveType.EnPassant)
        {
            int capturedSquare =
                currentTurn == PieceColor.White
                    ? square - 8
                    : square + 8;

            engine.board.RemovePiece(capturedSquare);
        }

        // Update En Passant availability
        UpdateEnPassant(
            selectedSquare,
            square,
            movingPiece
        );

        // Update king/rook movement rights
        UpdateMovementRights(
            selectedSquare,
            movingPiece
        );

        SyncEngineState();

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
        if(!pieceViews.ContainsKey(selectedSquare))
        {
            Debug.LogError(
                "Missing PieceView at square "
                + selectedSquare
            );

            selectedSquare = -1;
            return;
        }


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

        if(move.Type == MoveType.Promotion)
        {
            PromotePiece(
                square,
                move.PromotionPiece
            );
        }

        // Clear move suggestions
        ClearMoveHints();

        // Clear selection
        selectedSquare = -1;

        // Switch player
        SwitchTurn();

        UpdatePositionHash();

        string key = RecordPosition();

        moveHistory.Peek().positionKeyAfterMove = key;
        // -------------------------------------------------
        // INSUFFICIENT MATERIAL
        // -------------------------------------------------

        if (IsInsufficientMaterialDraw())
        {
            DeclareDraw(
                "Draw by insufficient material."
            );

            return;
        }

        if (IsThreefoldRepetition())
        {
            DeclareDraw(
                "Draw by threefold repetition."
            );

            return;
        }

        if (IsFiftyMoveDraw())
        {
            DeclareDraw(
                "Draw by 50-move rule."
            );
        }
    }
    
    private void SaveMoveState(Move move)
    {
        MoveState state =
            new MoveState(move);


        // Moving piece
        state.movedPiece =
            engine.board.GetPiece(move.From);


        // Default captured square
        state.capturedSquare =
            move.To;


        // Normal capture
        state.capturedPiece =
            engine.board.GetPiece(move.To);


        // En Passant
        if (move.Type == MoveType.EnPassant)
        {
            int direction =
                state.movedPiece.color == PieceColor.White
                    ? -8
                    : 8;


            state.capturedSquare =
                move.To + direction;


            state.capturedPiece =
                engine.board.GetPiece(
                    state.capturedSquare
                );
        }


        // Promotion
        if (move.Type == MoveType.Promotion)
        {
            state.promotedPiece =
                move.PromotionPiece;
        }


        // Castling
        if (move.Type == MoveType.CastleKingSide)
        {
            if (state.movedPiece.color ==
                PieceColor.White)
            {
                state.rookFrom = 7;
                state.rookTo = 5;
            }
            else
            {
                state.rookFrom = 63;
                state.rookTo = 61;
            }


            state.rookPiece =
                engine.board.GetPiece(
                    state.rookFrom
                );
        }


        if (move.Type == MoveType.CastleQueenSide)
        {
            if (state.movedPiece.color ==
                PieceColor.White)
            {
                state.rookFrom = 0;
                state.rookTo = 3;
            }
            else
            {
                state.rookFrom = 56;
                state.rookTo = 59;
            }


            state.rookPiece =
                engine.board.GetPiece(
                    state.rookFrom
                );
        }


        // Previous special-rule state

        state.previousEnPassantSquare =
            enPassantSquare;


        state.previousWhiteKingMoved =
            whiteKingMoved;

        state.previousBlackKingMoved =
            blackKingMoved;


        state.previousWhiteKingsideRookMoved =
            whiteKingsideRookMoved;

        state.previousWhiteQueensideRookMoved =
            whiteQueensideRookMoved;


        state.previousBlackKingsideRookMoved =
            blackKingsideRookMoved;

        state.previousBlackQueensideRookMoved =
            blackQueensideRookMoved;


        // Draw state

        state.previousHalfmoveClock =
            halfmoveClock;

        state.previousTurn =
            currentTurn;

        state.previousGameOver =
            gameOver;


        moveHistory.Push(state);
    }

    private void RestoreVisualAfterUndo(
        MoveState state
    )
    {
        Move move =
            state.move;


        // -------------------------------------------------
        // Remove current piece from destination
        // -------------------------------------------------

        if (pieceViews.TryGetValue(
            move.To,
            out PieceView movedView))
        {
            pieceViews.Remove(
                move.To
            );

            // Promotion:
            // Queen/Rook/Bishop/Knight → Pawn
            if (move.Type == MoveType.Promotion)
            {
                movedView.SetSprite(
                    GetSprite(
                        state.movedPiece
                    )
                );
            }


            movedView.SetSquare(
                move.From
            );


            movedView.MoveTo(
                GetPosition(move.From)
            );


            pieceViews[
                move.From
            ] = movedView;
        }


        // -------------------------------------------------
        // Restore captured piece visually
        // -------------------------------------------------

        if (!state.capturedPiece.IsEmpty())
        {
            SpawnPiece(
                state.capturedPiece.type,
                state.capturedPiece.color,
                state.capturedSquare
            );
        }


        // -------------------------------------------------
        // Restore castling rook visually
        // -------------------------------------------------

        if (move.Type == MoveType.CastleKingSide ||
            move.Type == MoveType.CastleQueenSide)
        {
            if (pieceViews.TryGetValue(
                state.rookTo,
                out PieceView rookView))
            {
                pieceViews.Remove(
                    state.rookTo
                );


                rookView.SetSquare(
                    state.rookFrom
                );


                rookView.MoveTo(
                    GetPosition(
                        state.rookFrom
                    )
                );


                pieceViews[
                    state.rookFrom
                ] = rookView;
            }
        }
    }

    public void UndoMove()
    {
        if (moveHistory.Count == 0)
            return;


        // Remove the position created by this move
        RemoveLastPosition();


        MoveState state =
            moveHistory.Pop();


        Move move =
            state.move;


        // -------------------------------------------------
        // Remove the moved piece from its current square
        // -------------------------------------------------

        engine.board.RemovePiece(move.To);


        // -------------------------------------------------
        // Restore the original moving piece
        // -------------------------------------------------

        engine.board.SetPiece(
            move.From,
            state.movedPiece
        );


        // -------------------------------------------------
        // Restore captured piece
        // -------------------------------------------------

        if (!state.capturedPiece.IsEmpty())
        {
            engine.board.SetPiece(
                state.capturedSquare,
                state.capturedPiece
            );
        }


        // -------------------------------------------------
        // Restore castling rook
        // -------------------------------------------------

        if (move.Type == MoveType.CastleKingSide ||
            move.Type == MoveType.CastleQueenSide)
        {
            engine.board.RemovePiece(
                state.rookTo
            );


            engine.board.SetPiece(
                state.rookFrom,
                state.rookPiece
            );
        }


        // -------------------------------------------------
        // Restore special-rule state
        // -------------------------------------------------

        enPassantSquare =
            state.previousEnPassantSquare;


        whiteKingMoved =
            state.previousWhiteKingMoved;

        blackKingMoved =
            state.previousBlackKingMoved;


        whiteKingsideRookMoved =
            state.previousWhiteKingsideRookMoved;

        whiteQueensideRookMoved =
            state.previousWhiteQueensideRookMoved;


        blackKingsideRookMoved =
            state.previousBlackKingsideRookMoved;

        blackQueensideRookMoved =
            state.previousBlackQueensideRookMoved;


        // -------------------------------------------------
        // Restore draw state
        // -------------------------------------------------

        halfmoveClock =
            state.previousHalfmoveClock;


        currentTurn =
            state.previousTurn;

        SyncEngineState();


        gameOver =
            state.previousGameOver;


        // -------------------------------------------------
        // Restore visual pieces
        // -------------------------------------------------

        RestoreVisualAfterUndo(
            state
        );


        Debug.Log(
            "Move undone successfully."
        );
    }

    private void RemoveLastPosition()
    {
        if(moveHistory.Count == 0)
            return;


        MoveState lastMove =
            moveHistory.Peek();


        string key =
            lastMove.positionKeyAfterMove;


        if(positionHistory.ContainsKey(key))
        {
            positionHistory[key]--;


            if(positionHistory[key] <= 0)
            {
                positionHistory.Remove(key);
            }
        }
    }

    private bool IsThreefoldRepetition()
    {
        string key =
            GetPositionKey();

        if (positionHistory.TryGetValue(
            key,
            out int count))
        {
            return count >= 3;
        }

        return false;
    }

    private void DeclareDraw(string reason)
    {
        gameOver = true;

        ClearMoveHints();

        selectedSquare = -1;

        Debug.Log(
            "DRAW: " + reason
        );
    }

    private void UpdateHalfmoveClock(
        Piece movingPiece,
        bool wasCapture,
        Move move)
    {
        // Pawn move
        if (movingPiece.type == PieceType.Pawn)
        {
            halfmoveClock = 0;
            return;
        }

        // Capture
        if (wasCapture ||
            move.Type == MoveType.EnPassant)
        {
            halfmoveClock = 0;
            return;
        }

        // Otherwise increment
        halfmoveClock++;
    }

    private bool IsFiftyMoveDraw()
    {
        return halfmoveClock >= 100;
    }

    private void PromotePiece(
        int square,
        PieceType newType)
    {
        PieceView oldPiece =
            pieceViews[square];


        Destroy(
            oldPiece.gameObject
        );


        pieceViews.Remove(square);


        SpawnPiece(
            newType,
            engine.board.GetPiece(square).color,
            square
        );
    }

    public void PromoteTo(PieceType type)
    {
        if(!waitingForPromotion)
            return;


        Piece oldPiece =
            engine.board.GetPiece(promotionSquare);


        engine.board.SetPiece(
            promotionSquare,
            new Piece(
                type,
                promotionColor
            )
        );


        PromotePiece(
            promotionSquare,
            type
        );


        waitingForPromotion = false;

        Debug.Log(
            "Promoted to " + type
        );

        whitePromotionPanel.SetActive(false);
        blackPromotionPanel.SetActive(false);
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
            engine.board.GetPiece(square);

        // Nothing selected
        if (selectedSquare == -1)
        {
            SelectPiece(square);
            return;
        }

        Piece selectedPiece =
            engine.board.GetPiece(selectedSquare);

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

    private void HandleEnPassantCapture(
        int destination)
    {
        int capturedSquare;

        if (currentTurn == PieceColor.White)
        {
            capturedSquare = destination - 8;
        }
        else
        {
            capturedSquare = destination + 8;
        }

        if (pieceViews.TryGetValue(
            capturedSquare,
            out PieceView capturedPiece))
        {
            Destroy(capturedPiece.gameObject);

            pieceViews.Remove(capturedSquare);
        }

        Debug.Log(
            "En Passant capture: " +
            GetSquareName(capturedSquare)
        );
    }


    private void UpdateEnPassant(
        int from,
        int to,
        Piece piece)
    {
        // En Passant is available for only one turn.
        enPassantSquare = -1;

        if (piece.type != PieceType.Pawn)
            return;

        // Pawn moved two squares.
        if (Mathf.Abs(to - from) == 16)
        {
            // Square jumped over by the pawn.
            enPassantSquare =
                (from + to) / 2;

            Debug.Log(
                "En Passant available on " +
                GetSquareName(enPassantSquare)
            );
        }
    }

    private void HandleCapture(int square)
    {
        Piece targetPiece =
            engine.board.GetPiece(square);

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
        currentTurn =
            currentTurn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

        SyncEngineState();

        Debug.Log(
            "Turn: " +
            currentTurn
        );

        currentGameState =
            engine.GetGameState();

        Debug.Log(
            "Game State: " +
            currentGameState
        );
    }


    private bool IsMoveLegal(int from, int to)
    {
        SyncEngineState();
        return engine.IsMoveLegal(from, to);
    }

    private List<Move> GetAllLegalMoves(PieceColor color)
    {
        SyncEngineState();
        return engine.GetAllLegalMoves(color);
    }

    private GameState GetGameState()
    {
        SyncEngineState();
        return engine.GetGameState();
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

        SyncEngineState();

        Piece piece =
            engine.board.GetPiece(from);

        if (piece.IsEmpty())
            return;

        List<Move> moves =
            engine.GetLegalMoves(from);

        foreach (Move move in moves)
        {
            if (squareViews.TryGetValue(
                move.To,
                out SquareView squareView))
            {
                squareView.ShowMoveHint();
            }
        }
    }

    private bool IsCastlingLegal(
        int from,
        int to,
        PieceColor color)
    {
        SyncEngineState();

        return engine.IsCastlingLegal(
            from,
            to,
            color
        );
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

        engine.board.MakeMove(
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