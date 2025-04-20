using UnityEngine;
using System.Collections.Generic;


public class PieceController : MonoBehaviour
{
    public bool isPlayerWhite; // Détermine si la pièce appartient au joueur blanc
    public bool isSelected = false;
    private Color Couleur; //couleur des pieces


    protected virtual void Awake()
    {   //prend la couleur de la piece au tout début donc blanc ou noir
        Couleur = GetComponent<Renderer>().material.color;
    }

    public virtual bool IsValidMove(Vector3 targetPosition)
    {
        return false; // Par défaut, une pièce ne peut pas bouger (surcharge dans les classes enfants)
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
        Debug.Log(gameObject.name + " a été désélectionné.");
    }

    //change la couleur quand une piece est selectionnée
    public virtual void UpdateSelectionVisual()
    {
        GetComponent<Renderer>().material.color = isSelected ? Color.yellow : Couleur;
        //pour ajouter un prefab d'effet de halo si le temps:
        /*if (selectionEffect != null)
        {
            selectionEffect.SetActive(isSelected);
        }*/
    }

    // Méthode appelée lors du clic sur la piece
    public void OnPieceClicked()
    {
        // Vérifie si c'est le tour du joueur auquel appartient ce pion
        if (isPlayerWhite != GameManager.IsWhiteTurn)
        {
            Debug.Log("Ce n'est pas le tour de ce joueur !");
            return; // Ne rien faire si ce n'est pas le tour du joueur
        }
        //deselectionne l'ancienne piece selectionnée
        GameManager.SelectPiece(this);

        isSelected = !isSelected; // Alterner l'état sélectionné
        UpdateSelectionVisual();
        Debug.Log(isSelected ? gameObject.name + " a été sélectionné." : gameObject.name + " a été désélectionné.");
    }
    //public GameObject moveHighlightPrefab; // À utiliser ici

}
