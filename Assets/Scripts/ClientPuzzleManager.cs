using System.Collections.Generic;
using UnityEngine;

public class ClientPuzzleManager : MonoBehaviour
{
    public delegate void PieceResponseDelegate(PuzzlePiece RequestedPiece);

    private List<PuzzlePiece> Pieces;

    private Dictionary<int, List<PieceResponseDelegate>> QueuedRequests;

    
    private void Start()
    {
        Pieces = new List<PuzzlePiece>();
        QueuedRequests = new Dictionary<int, List<PieceResponseDelegate>>();
        StaticJigsawData.ObjectManager.RegisterObject(gameObject, "ClientPuzzleManager");
    }


    public void SetPiece(PuzzlePiece Piece, int Id)
    {
        Pieces.Insert(Id, Piece);
        if (QueuedRequests.ContainsKey(Id))
        {
            List<PieceResponseDelegate> callbackQueue = QueuedRequests[Id];
            foreach (PieceResponseDelegate callback in callbackQueue)
            {
                callback(Piece);
            }
            QueuedRequests.Remove(Id);
        }
    }


    public PuzzlePiece GetPiece(int Id)
    {
        if (Id < 0 || Id >= Pieces.Count)
        {
            return null;
        }
        return Pieces[Id];
    }


    public void RequestPiece(int Id, PieceResponseDelegate Callback)
    {
        if (Id < Pieces.Count && Pieces[Id] != null)
        {
            Callback(Pieces[Id]);
        }
        else
        {
            if (QueuedRequests.ContainsKey(Id))
            {
                QueuedRequests[Id].Add(Callback);
            }
            else
            {
                List<PieceResponseDelegate> callbackQueue = new List<PieceResponseDelegate>();
                callbackQueue.Add(Callback);
                QueuedRequests.Add(Id, callbackQueue);
            }
        }
    }
}
