using UnityEngine;
using UnityEngine.UI;

public class SquareView : MonoBehaviour
{
    private int squareIndex;
    private ChessGameManager gameManager;

    [SerializeField] private GameObject moveHint;

    private void Awake()
    {
        Button button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    public void Initialize(
        int square,
        ChessGameManager manager
    )
    {
        squareIndex = square;
        gameManager = manager;

        HideMoveHint();
    }

    private void OnClicked()
    {
        gameManager.SelectDestinationOrSelectPiece(squareIndex);
    }

    public void ShowMoveHint()
    {
        if (moveHint != null)
            moveHint.SetActive(true);
    }

    public void HideMoveHint()
    {
        if (moveHint != null)
            moveHint.SetActive(false);
    }
}