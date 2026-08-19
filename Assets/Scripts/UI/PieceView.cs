using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
    private Image image;

    private int squareIndex;
    private ChessGameManager gameManager;

    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetSquare(int square)
    {
        squareIndex = square;
    }

    public int GetSquare()
    {
        return squareIndex;
    }

    public void SetGameManager(ChessGameManager manager)
    {
        gameManager = manager;
    }

    public void OnPieceClicked()
    {
        gameManager.SelectDestinationOrSelectPiece(squareIndex);
    }

    public void MoveTo(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}