using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private ChessGameManager gameManager;

    [Header("Colors")]
    [SerializeField] private Color lightColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color darkColor = new Color(0.3f, 0.3f, 0.3f);

    private const int BoardSize = 8;

    private void Start()
    {
        Debug.Log("BoardView is running!");
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        for (int rank = 0; rank < BoardSize; rank++)
        {
            for (int file = 0; file < BoardSize; file++)
            {
                GameObject square = Instantiate(squarePrefab, transform);
                SquareView squareView = square.GetComponent<SquareView>();

                squareView.Initialize(
                    rank * 8 + file,
                    gameManager
                );

                Image image = square.GetComponent<Image>();

                if ((file + rank) % 2 == 0)
                    image.color = lightColor;
                else
                    image.color = darkColor;

                square.name = GetSquareName(file, rank);
            }
        }
    }

    private string GetSquareName(int file, int rank)
    {
        char fileLetter = (char)('a' + file);
        int rankNumber = rank + 1;

        return $"{fileLetter}{rankNumber}";
    }
}