using System.Collections.Generic;
using UnityEngine;

public class AIControllerLevel3 : MonoBehaviour
{
    public static AIControllerLevel3 Instance; // Singleton
    private int maxDepth = 2; // Profondeur de Minimax
    private Vector3 originalPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("AIController existe déjà ! Destruction de l'instance en double.");
            Destroy(gameObject);
        }
    }

   public void PlayAITurn()
    {
        Debug.Log("IA Level 3: PlayAITurn est lancé");

        // L'IA joue seulement avec les noirs
        if (GameManager.IsWhiteTurn) return;
        // Désélectionner toutes les pièces avant de jouer
        DeselectAllPieces();
        // Trouver le meilleur coup
        Move bestMove = FindBestMove();
        if (bestMove != null)
        {
            // Exécute un seul mouvement
            ExecuteMove(bestMove);
        }
        else
        {
            Debug.Log("L'IA ne peut pas bouger !");
        }
    }


    private Move FindBestMove()
    {
        List<PieceController> aiPieces = GetAIPieces();
        List<Move> bestMoves = new List<Move>();
        float bestValue = float.NegativeInfinity;

        foreach (PieceController piece in aiPieces)
        {
            List<Vector3> validMoves = piece.GetAvailableMoves();

            foreach (Vector3 move in validMoves)
            {
                PieceController capturedPiece = SimulateMove(piece, move);
                float moveValue = Minimax(maxDepth, float.NegativeInfinity, float.PositiveInfinity, false);
                UndoMove(piece, move, capturedPiece);

                if (Mathf.Approximately(moveValue, bestValue))
                {
                    bestMoves.Add(new Move(piece, move)); // plusieurs coups aussi bons
                }
                else if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMoves.Clear();
                    bestMoves.Add(new Move(piece, move));
                }
            }
        }

        if (bestMoves.Count > 0)
        {
            return bestMoves[Random.Range(0, bestMoves.Count)]; //choisir un coup au hasard parmi les meilleurs
        }

        return null;
    }



    private float Minimax(int depth, float alpha, float beta, bool isMaximizingPlayer){
        if (depth == 0 || IsGameOver()) return EvaluateBoard();

        if (isMaximizingPlayer){
            float maxEval = float.NegativeInfinity;
            List<PieceController> aiPieces = GetAIPieces();

            foreach (PieceController piece in aiPieces){
                List<Vector3> validMoves = piece.GetAvailableMoves();
                foreach (Vector3 move in validMoves){
                    PieceController capturedPiece = SimulateMove(piece, move);
                    float eval = Minimax(depth - 1, alpha, beta, false);
                    UndoMove(piece, move, capturedPiece);

                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);

                    if (beta <= alpha) break; // Élagage alpha-bêta
                }
            }
            return maxEval;
        }
        else{
            float minEval = float.PositiveInfinity;
            List<PieceController> opponentPieces = GetOpponentPieces();

            foreach (PieceController piece in opponentPieces){
                List<Vector3> validMoves = piece.GetAvailableMoves();
                foreach (Vector3 move in validMoves){
                    PieceController capturedPiece = SimulateMove(piece, move);
                    float eval = Minimax(depth - 1, alpha, beta, true);
                    UndoMove(piece, move, capturedPiece);

                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);

                    if (beta <= alpha) break; // Élagage alpha-bêta
                }
            }
            return minEval;
        }
    }



    private float EvaluateBoard()
    {
        float score = 0;
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            if (piece.isPlayerWhite)
            {
                score -= EvaluatePiece(piece); // Pièces blanches négatives
            }
            else
            {
                score += EvaluatePiece(piece); // Pièces noires positives
            }
        }

        return score;
    }

    private float EvaluatePiece(PieceController piece)
    {
        float value = piece switch
        {
            KingController => 10000,
            QueenController => 9,
            RookController => 5,
            BishopController => 3.5f,
            KnightController => 3,
            PawnController => 1,
            _ => 0
        };

        value += EvaluatePosition(piece.transform.position);
        return value;
    }


    private float EvaluatePosition(Vector3 position)
    {
        float centerX = Mathf.Abs(position.x - 3.5f);
        float centerZ = Mathf.Abs(position.z - 3.5f);
        return 1 - (centerX + centerZ) / 7f;
    }

    private List<PieceController> GetAIPieces()
    {
        List<PieceController> pieces = new List<PieceController>();
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            if (!piece.isPlayerWhite) // Sélectionne les Noirs
            {
                pieces.Add(piece);
            }
        }

        return pieces;
    }

    private List<PieceController> GetOpponentPieces()
    {
        List<PieceController> opponentPieces = new List<PieceController>();
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            if (piece.isPlayerWhite) // Sélectionne les Blancs
            {
                opponentPieces.Add(piece);
            }
        }

        return opponentPieces;
    }

    private void ExecuteMove(Move move)
    {
        // Capture s’il y a une pièce ennemie
        Collider[] colliders = Physics.OverlapSphere(move.TargetPosition, 0.3f);
        foreach (Collider col in colliders)
        {
            PieceController enemy = col.GetComponent<PieceController>();
            if (enemy != null && enemy.isPlayerWhite)
            {
                enemy.OnCaptured(); // méthode clean
                break;
            }
        }

        // Déplacement immédiat (ou remplacer par animation si tu veux)
        move.Piece.transform.position = move.TargetPosition;

        // Réinitialise les couleurs et la sélection
        move.Piece.DeselectCase(); //reset couleurs et isSelected

        // Passe le tour
        GameManager.SwitchTurn(); 
        Debug.Log($"l'IA a déplacé {move.Piece.name} vers {move.TargetPosition}");
    }


    private bool IsGameOver()
    {
        return false; // Ajoute une logique pour l'échec et mat
    }

    private PieceController SimulateMove(PieceController piece, Vector3 targetPosition)
    {
        originalPosition = piece.transform.position;

        PieceController capturedPiece = null;

        Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
        foreach (Collider collider in colliders)
        {
            PieceController otherPiece = collider.GetComponent<PieceController>();
            if (otherPiece != null && otherPiece != piece && otherPiece.isPlayerWhite != piece.isPlayerWhite)
            {
                capturedPiece = otherPiece;
                otherPiece.gameObject.SetActive(false); // Cache temporairement
                break;
            }
        }

        // TEMPORAIRE : on ne touche plus à transform.position
        piece.gameObject.SetActive(false); // on "simule" en l'enlevant

        return capturedPiece;
    }


    private void UndoMove(PieceController piece, Vector3 targetPosition, PieceController capturedPiece)
    {
        piece.gameObject.SetActive(true); // On "réactive" la pièce

        if (capturedPiece != null)
        {
            capturedPiece.gameObject.SetActive(true);
        }
    }



    private class Move
    {
        public PieceController Piece { get; }
        public Vector3 TargetPosition { get; }

        public Move(PieceController piece, Vector3 targetPosition)
        {
            Piece = piece;
            TargetPosition = targetPosition;
        }
    }

    private void DeselectAllPieces()
    {
        foreach (PieceController piece in FindObjectsOfType<PieceController>())
        {
            if (piece.GetComponent<Renderer>() != null) //On vérifie qu'il y a un Renderer
            {
                piece.DeselectCase();
            }
        }
    }


}
