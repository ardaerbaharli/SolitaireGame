using System.Collections.Generic;
using UnityEngine;

public class Move
{   


    public Transform Origin { get; set; }
    public Transform Target { get; set; }
    public GameObject Card { get; set; }

    public Move(GameObject card=null, Transform origin=null, Transform target=null)
    {
        this.Card = card;
        this.Origin = origin;
        this.Target = target;
    }
    public List<Move> ToList()
    {
        List<Move> moves = new List<Move>();
        Move m = new Move();
        m.Origin = Origin;
        m.Card = Card;
        m.Target = Target;

        moves.Add(m);
        return moves;
    }
}