using UnityEngine;

public class ChessBoardGenerator : MonoBehaviour
{
public int rows = 8; // Nombre de lignes
    public int columns = 8; // Nombre de colonnes
    public float tileSize = 1.0f; // Taille d'une case
    public GameObject whiteTilePrefab; // Préfabriqué pour une case blanche
    public GameObject blackTilePrefab; // Préfabriqué pour une case noire


    void Start()
    {
        GenerateChessBoard();
    }

    void GenerateChessBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calcule la position de chaque case
                Vector3 position = new Vector3(-4 + col * tileSize, 0, -4 + row * tileSize);

                // Alternance des couleurs (cases noires et blanches)
                bool isWhite = (row + col) % 2 == 0;

                // Instancier la case appropriée
                GameObject tilePrefab = isWhite ? whiteTilePrefab : blackTilePrefab;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.tag = "Tiles"; 

                // Ajuste l'échelle et le parent de l'objet
                tile.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
                tile.transform.parent = this.transform;
            }
        }
    }

    public bool IsInBoard(int x, int z)
    {
        return x >= 0 && x < columns && z >= 0 && z < rows;
    }

}
