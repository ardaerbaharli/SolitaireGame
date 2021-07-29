using System.Collections;
using System.Collections.Generic;

namespace Assets.Script
{
    public class Helpers
    {
        public static bool DoesContains(List<List<Move>> possibleMoves, List<Move> move)
        {
            for (int i = 0; i < possibleMoves.Count; i++)
            {
                foreach (var possibleMove in possibleMoves[i])
                {
                    for (int k = 0; k < move.Count; k++)
                    {
                        if (possibleMove.Card.name == move[k].Card.name && possibleMove.Target.name == move[k].Target.name && possibleMove.Origin.name == move[k].Origin.name)
                            return true;
                    }
                }
            }
            return false;
        }
        public static bool IsNullOrEmpty(ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
