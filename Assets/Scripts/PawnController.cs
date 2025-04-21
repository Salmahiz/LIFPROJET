using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class PawnController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    //private bool isSelected = false; // État de sélection
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si le pion est en train de se déplacer
    private bool isFirstMove = true; // Premier déplacement du pion
    private PieceController pieceToCapture = null; // Stocke la pièce ennemie à capturer
    private bool hasPromoted = false;



    private void Update()
    {
        // Si le pion est sélectionné, on permet de le déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si le pion est en mouvement, on le déplace
        if (isMoving)
        {
            MovePawn();
        }

    }

    // Gère le clic souris pour déplacer le pion
    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Si clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer un raycast depuis la position de la souris
            if (Physics.Raycast(ray, out hit))
            {
                // Vérifie si on clique sur une case ("Tiles")
                if (hit.collider.CompareTag("Tiles"))
                {
                    Vector3 hitPosition = hit.collider.transform.position;

                    List<Vector3> possibleMoves = GetAvailableMoves();
                    Vector3 matchedMove = possibleMoves.FirstOrDefault(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    bool moveFound = possibleMoves.Any(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    // Vérifie si la case est une case valide (1 case ou 2 cases au premier mouvement)
                    if (moveFound)
                    {
                        targetPosition = matchedMove; // Met à jour la position cible
                        isMoving = true; // Active le mouvement

                        // Désactive le premier mouvement après le déplacement
                        if (isFirstMove)
                            isFirstMove = false;
                    }
                    else
                    {
                        Debug.Log("Déplacement invalide.");
                    }
                }
            }
        }
    }



    public override List<Vector3> GetAvailableMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        (int x, int z) = GetBoardIndex();
        ChessBoardGenerator board = GameObject.FindObjectOfType<ChessBoardGenerator>();

        int direction = isPlayerWhite ? 1 : -1;

        // Une case devant
        int oneStepZ = z + direction;
        if (board.IsInBoard(x, oneStepZ))
        {
            Vector3 pos = BoardToWorldPosition(x, oneStepZ);
            if (!IsOccupied(pos))
                moves.Add(pos);
        }

        // Deux cases si premier coup
        int twoStepZ = z + 2 * direction;
        if (isFirstMove && board.IsInBoard(x, twoStepZ))
        {
            Vector3 middle = BoardToWorldPosition(x, z + direction);
            Vector3 target = BoardToWorldPosition(x, twoStepZ);
            if (!IsOccupied(middle) && !IsOccupied(target))
                moves.Add(target);
        }

        // Capture diagonale gauche
        int diagLeftX = x - 1;
        if (board.IsInBoard(diagLeftX, oneStepZ))
        {
            Vector3 diagPos = BoardToWorldPosition(diagLeftX, oneStepZ);
            if (IsOccupiedByEnemy(diagPos))
                moves.Add(diagPos);
        }

        // Capture diagonale droite
        int diagRightX = x + 1;
        if (board.IsInBoard(diagRightX, oneStepZ))
        {
            Vector3 diagPos = BoardToWorldPosition(diagRightX, oneStepZ);
            if (IsOccupiedByEnemy(diagPos))
                moves.Add(diagPos);
        }

        return moves;
    }

    bool IsOccupied(Vector3 position)
    {
        return Physics.OverlapSphere(position, 0.3f).Any(c => c.GetComponent<PieceController>() != null);
    }

    bool IsOccupiedByEnemy(Vector3 position)
    {
        return Physics.OverlapSphere(position, 0.3f).Any(c =>
            {
                PieceController pc = c.GetComponent<PieceController>();
                return pc != null && pc.isPlayerWhite != this.isPlayerWhite;
            });
    }


    // Déplace le pion vers la position cible
    void MovePawn()
    {
        Vector3 oldPosition = transform.position; // Sauvegarde la position actuelle

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);

            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    pieceToCapture = piece;
                    Debug.Log("Pièce ennemie capturée : " + piece.gameObject.name);
                    Destroy(piece.gameObject);
                    break;
                }
            }

            //ClearOldPosition(oldPosition); // Nettoie l'ancienne position après capture
            /*if (!hasPromoted && IsAtPromotionRow())
            {
                if (uiObj != null){
                GameObject uiObj = GameObject.Find("PromotionUI");
                Debug.Log("Pion arrivé à la rangée de promotion !");
                PromotePawn();
                return;
                }
            }
*/

            DeselectCase();
            GameManager.SwitchTurn();
        }
                

        }
    private bool IsAtPromotionRow()
    {
        (int x, int z) = GetBoardIndex();
        return (isPlayerWhite && z == 7) || (!isPlayerWhite && z == 0);
    }



    void PromotePawn()
    {
        hasPromoted = true;
        Debug.Log("Promotion déclenchée pour " + gameObject.name);
        //GameManager.ShowPromotionUI(this);
    }

    
    public void Promote(string pieceType)
    {
        string path = "BrokenVector/LowpolyChessPack/Prefabs/";
        string prefabName = "";

        if (pieceType == "Queen")
            prefabName = isPlayerWhite ? "White_Queen" : "Black_Queen";
        else if (pieceType == "Knight")
            prefabName = isPlayerWhite ? "White_Knight" : "Black_Knight";

        GameObject prefab = Resources.Load<GameObject>(path + prefabName);

        if (prefab != null)
        {
            GameObject newPiece = Instantiate(prefab, transform.position, Quaternion.identity);
            PieceController pc = newPiece.GetComponent<PieceController>();
            pc.isPlayerWhite = isPlayerWhite;

            Destroy(this.gameObject);
        }
        else
        {
            Debug.LogError("Prefab introuvable pour : " + prefabName);
        }
    }



}


