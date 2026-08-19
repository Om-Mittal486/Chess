using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private ChessGameManager gameManager;

    private const int BoardSize = 8;

    private void Start()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        for (int rank = BoardSize - 1; rank >= 0; rank--)
        {
            for (int file = 0; file < BoardSize; file++)
            {
                GameObject square = Instantiate(
                    squarePrefab,
                    transform
                );

                SquareView squareView =
                    square.GetComponent<SquareView>();

                squareView.Initialize(
                    rank * 8 + file,
                    gameManager
                );

                Image image =
                    square.GetComponent<Image>();

                if ((file + rank) % 2 == 0)
                {
                    image.color =
                        new Color32(240, 217, 181, 255);
                }
                else
                {
                    image.color =
                        new Color32(181, 136, 99, 255);
                }

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