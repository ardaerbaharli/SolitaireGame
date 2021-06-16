using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Transform Origin { get; set; }
    public Transform Target { get; set; }
    public GameObject Card { get; set; }

    internal List<Move> ToList()
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