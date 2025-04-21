using UnityEngine;
using System.Collections.Generic;
using System.Linq; //permet d'utiliser des fonctionnalitées déjà pretes des listes


public class PieceController : MonoBehaviour
{
    public bool isPlayerWhite; // Détermine si la pièce appartient au joueur blanc
    public bool isSelected = false;
    private Color Couleur; //couleur des pieces


    //changements pour ameliorer la complexité de isValidMove a partir d'ici:

    public virtual List<Vector3> GetAvailableMoves()
    {
        return new List<Vector3>(); // Par défaut aucune case, a surcharger dans les autres classes
    }

    protected bool IsOccupiedByAlly(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.3f);
        foreach (Collider col in colliders)
        {
            PieceController pc = col.GetComponent<PieceController>();
            if (pc != null && pc.isPlayerWhite == this.isPlayerWhite) return true;
        }
        return false;
    }

    protected (int x, int z) GetBoardIndex()
    {
        int x = Mathf.RoundToInt(transform.position.x + 4);
        int z = Mathf.RoundToInt(transform.position.z + 4);
        return (x, z);
    }

    protected Vector3 BoardToWorldPosition(int x, int z)
    {
        return new Vector3(x - 4, 0, z - 4);
    }


    //jusqu'a ici




    protected virtual void Awake()
    {   //prend la couleur de la piece au tout début donc blanc ou noir
        Couleur = GetComponent<Renderer>().material.color;
    }



    public virtual void OnCaptured()
    {
        Destroy(gameObject);
    }

    // Méthode pour désélectionner la piece cliquée
    public virtual void DeselectPiece()
    {
        isSelected = false;
        UpdateSelectionVisual(); //rechange a la couleur originelle du roi
        //Debug.Log(gameObject.name + " a été désélectionné.");
    }

    //change la couleur quand une piece est selectionnée
    public virtual void UpdateSelectionVisual()
    {
        GetComponent<Renderer>().material.color = isSelected ? Color.green : Couleur;
        //pour ajouter un prefab d'effet de halo si le temps:
        /*if (selectionEffect != null)
        {
            selectionEffect.SetActive(isSelected);
        }*/
    }

    // Méthode appelée lors du clic sur la piece
    public void OnPieceClicked()
    {   
        if (GameManager.IsPromotionActive)
        {
            Debug.Log("Promotion en cours, impossible de cliquer sur les pieces et de jouer");
            return;
        }
        // Vérifie si c'est le tour du joueur auquel appartient ce pion
        if (isPlayerWhite != GameManager.IsWhiteTurn)
        {
            Debug.Log("Ce n'est pas le tour de ce joueur !");
            return; // Ne rien faire si ce n'est pas le tour du joueur
        }
        //deselectionne l'ancienne piece selectionnée
        GameManager.SelectPiece(this);
        
        isSelected = !isSelected; // Alterner l'état sélectionné
        if (isSelected)
        {
            HighlightMoves(); 
        }
        else
        {
            ResetColors();  
        }
        UpdateSelectionVisual();
        Debug.Log(isSelected ? gameObject.name + " a été sélectionné." : gameObject.name + " a été désélectionné.");
    }
    
    // Surligne les cases accessibles en vert
public virtual void HighlightMoves()
{
    GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Tiles");
    List<Vector3> availableMoves = GetAvailableMoves();

    foreach (GameObject tile in allTiles)
    {
        Vector3 tilePos = tile.transform.position;

        foreach (Vector3 move in availableMoves)
        {
            if (Vector3.Distance(tilePos, move) < 0.1f)
            {
                Renderer rend = tile.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = Color.green;

                break; // On arrête dès qu'on trouve un match
            }
        }
    }
}


//Réinitialise les couleurs des cases
public virtual void ResetColors()
{
    GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Tiles");

    foreach (GameObject tile in allTiles)
    {
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            bool isWhite = (Mathf.RoundToInt(tile.transform.position.x) + Mathf.RoundToInt(tile.transform.position.z)) % 2 == 0;
            rend.material.color = isWhite ? Color.white : Color.black;
        }
    }
}

// Désélectionne la pièce + réinitialise les couleurs
public virtual void DeselectCase()
{
    isSelected = false;
    UpdateSelectionVisual(); 
    ResetColors();           
}


}
