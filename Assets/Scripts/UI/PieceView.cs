using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
    private Image image;

    private int squareIndex;

    private ChessGameManager gameManager;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetSquare(int square)
    {
        squareIndex = square;
    }

    public void SetGameManager(ChessGameManager manager)
    {
        gameManager = manager;
    }

    public void OnPieceClicked()
    {
        gameManager.SelectPiece(squareIndex);
    }
}