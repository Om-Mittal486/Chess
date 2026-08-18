using UnityEngine;
using UnityEngine.UI;

public class SquareView : MonoBehaviour
{
    private int squareIndex;
    private ChessGameManager gameManager;

    public void Initialize(int square, ChessGameManager manager)
    {
        squareIndex = square;
        gameManager = manager;

        Button button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        gameManager.SelectDestination(squareIndex);
    }
}