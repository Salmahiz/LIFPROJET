using UnityEngine;

public class ChessPieceGenerator : MonoBehaviour
{
    public GameObject whitePawnPrefab, blackPawnPrefab;
    public GameObject whiteRookPrefab, blackRookPrefab;
    public GameObject whiteKnightPrefab, blackKnightPrefab;
    public GameObject whiteBishopPrefab, blackBishopPrefab;
    public GameObject whiteQueenPrefab, blackQueenPrefab;
    public GameObject whiteKingPrefab, blackKingPrefab;

    public float tileSize = 1.0f; // Taille des cases pour aligner les pièces

    void Start()
    {
        PlacePawns();
        PlaceOtherPieces();
    }

    void PlacePawns()
    {   
        for (int i = 0; i < 8; i++)
        {
            // Place les pions blancs
            Instantiate(whitePawnPrefab, new Vector3(-4 + i * tileSize, 0.03f, -3 * tileSize), Quaternion.identity);

            // Place les pions noirs
            Instantiate(blackPawnPrefab, new Vector3(-4 + i * tileSize, 0.03f, 2 * tileSize), Quaternion.identity);
        }
    }

    void PlaceOtherPieces()
    {   
        // Placement des tours
        Instantiate(whiteRookPrefab, new Vector3(-4, 0.19f, -4), Quaternion.identity);
        Instantiate(whiteRookPrefab, new Vector3(3 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(blackRookPrefab, new Vector3(-4, 0.19f, 3 * tileSize), Quaternion.identity);
        Instantiate(blackRookPrefab, new Vector3(3 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);

        // Placement des cavaliers
        Instantiate(whiteKnightPrefab, new Vector3(-3 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(whiteKnightPrefab, new Vector3(2 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(blackKnightPrefab, new Vector3(-3 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);
        Instantiate(blackKnightPrefab, new Vector3(2 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);

        // Placement des fous
        Instantiate(whiteBishopPrefab, new Vector3(-2 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(whiteBishopPrefab, new Vector3(1 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(blackBishopPrefab, new Vector3(-2 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);
        Instantiate(blackBishopPrefab, new Vector3(1 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);

        // Placement des reines
        Instantiate(whiteQueenPrefab, new Vector3(-1 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(blackQueenPrefab, new Vector3(-1 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);


        // Placement des rois
        Instantiate(whiteKingPrefab, new Vector3(0 * tileSize, 0.19f, -4), Quaternion.identity);
        Instantiate(blackKingPrefab, new Vector3(0 * tileSize, 0.19f, 3 * tileSize), Quaternion.identity);
    }
}
