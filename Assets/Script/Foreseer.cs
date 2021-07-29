using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class Foreseer : MonoBehaviour
    {
        public static List<Move> GetNextMove()
        {
            var moveList = FromPlayground();
            if (!Helpers.IsNullOrEmpty(moveList))
                return moveList;

            var moves = FromGround();
            if (!Helpers.IsNullOrEmpty(moves))
                return moves;

            return null;
        }
        public static bool IsThereAMove()
        {
            int deckCardCount = GameControl.instance.deckObj.transform.childCount;
            for (int i = 0; i < deckCardCount; i++)
            {
                var card = GameControl.instance.deckObj.transform.GetChild(i);

                if (CanItGoToAnyAcePanel(card))
                    return true;
                if (CanItGoToAnyPlaygroundPanel(card))
                    return true;
            }

            int groundCardCount = GameControl.instance.groundObj.transform.childCount;
            for (int i = 0; i < groundCardCount; i++)
            {
                var card = GameControl.instance.groundObj.transform.GetChild(i);

                if (CanItGoToAnyAcePanel(card))
                    return true;
                if (CanItGoToAnyPlaygroundPanel(card))
                    return true;
            }

            return false;
        }
        public static bool CanItGoToAnyAcePanel(Transform card)
        {
            if (!card.GetComponent<CardController>().isDummy)
            {
                var cardSuit = card.name[0];
                int cardValue = int.Parse(card.name.Substring(1));
                var parent = card.parent;
                bool isCardA = (cardValue == 1);

                int acePanelCount = GameControl.instance.acePanel.transform.childCount;
                for (int i = 0; i < acePanelCount; i++)
                {
                    var panel = GameControl.instance.acePanel.transform.GetChild(i);
                    if (panel.childCount > 0)
                    {
                        var lastCard = panel.GetChild(panel.childCount - 1);
                        var lastCardSuit = lastCard.name[0];
                        int lastCardValue = int.Parse(lastCard.name.Substring(1));
                        if (lastCardSuit == cardSuit && cardValue == lastCardValue + 1)
                        {
                            var m = new Move();
                            m.Card = card.gameObject;
                            m.Origin = parent;
                            m.Target = panel;
                            if (!IsItDumbMove(m.ToList()))
                                return true;
                        }
                    }
                    else if (panel.childCount == 0 && isCardA)
                    {
                        var m = new Move();
                        m.Card = card.gameObject;
                        m.Origin = parent;
                        m.Target = panel;
                        if (!IsItDumbMove(m.ToList()))
                            return true;
                    }
                }
            }
            return false;
        }
        public static bool CanItGoToAnyPlaygroundPanel(Transform card)
        {
            if (!card.GetComponent<CardController>().isDummy)
            {
                var cardSuit = card.name[0];
                int cardValue = int.Parse(card.name.Substring(1));
                var parent = card.parent;

                bool isCardK = (cardValue == 13);

                int playgroundPanelCount = GameControl.instance.playcardsObj.transform.childCount;
                for (int i = 0; i < playgroundPanelCount; i++)
                {
                    var panel = GameControl.instance.playcardsObj.transform.GetChild(i);
                    if (panel.childCount > 0)
                    {
                        var lastCard = panel.GetChild(panel.childCount - 1);
                        if (!lastCard.GetComponent<CardController>().isDummy)
                        {
                            var lastCardSuit = lastCard.name[0];
                            int lastCardValue = int.Parse(lastCard.name.Substring(1));

                            if (cardValue == lastCardValue - 1 && IsPlaygroudCardSuitTrue(lastCardSuit, cardSuit))
                            {
                                var m = new Move();
                                m.Card = card.gameObject;
                                m.Origin = parent;
                                m.Target = panel;
                                if (!IsItDumbMove(m.ToList()))
                                    return true;
                            }
                        }
                    }
                    else if (panel.childCount == 0 && isCardK)
                    {
                        var m = new Move();
                        m.Card = card.gameObject;
                        m.Origin = parent;
                        m.Target = panel;
                        if (!IsItDumbMove(m.ToList()))
                            return true;
                    }
                }
            }
            return false;
        }
        public static List<Move> FromGround()
        {
            if (GameControl.instance.groundObj.transform.childCount > 0)
            {
                var card = GameControl.instance.groundObj.transform.GetChild(GameControl.instance.groundObj.transform.childCount - 1);

                if (CanItGoToAnyAcePanel(card))
                {
                    var move = GetAcePanelMove(card);

                    if (!IsItDumbMove(move.ToList()) && !Helpers.DoesContains(GameControl.instance.possibleMoves, move.ToList()))//!DoesExistInHelpHistory(move.ToList()) &&
                        return move.ToList();
                }
                if (CanItGoToAnyPlaygroundPanel(card))
                {
                    var moves = GetPlaygroundMoves(card);
                    if (!IsItDumbMove(moves) && !Helpers.DoesContains(GameControl.instance.possibleMoves, moves))//!DoesExistInHelpHistory(moves) && 
                        return moves;
                }
            }
            return null;
        }
        public static List<Move> FromPlayground()
        {
            int playgroundPanelCount = GameControl.instance.playcardsObj.transform.childCount;

            // to ace panel
            for (int i = 0; i < playgroundPanelCount; i++)
            {
                var panel = GameControl.instance.playcardsObj.transform.GetChild(i);
                if (panel.childCount > 0)
                {
                    var lastCard = panel.transform.GetChild(panel.childCount - 1);
                    if (CanItGoToAnyAcePanel(lastCard))
                    {
                        var move = GetAcePanelMove(lastCard);

                        if (!IsItDumbMove(move.ToList()) && !Helpers.DoesContains(GameControl.instance.possibleMoves, move.ToList()))//!DoesExistInHelpHistory(move.ToList()) &&
                            return move.ToList();
                    }
                }
            }

            // to playground
            for (int i = 0; i < playgroundPanelCount; i++)
            {
                var playgroundPanel = GameControl.instance.playcardsObj.transform.GetChild(i);
                int playgroundPanelCardcount = playgroundPanel.childCount;
                if (playgroundPanelCardcount > 0)
                {
                    for (int k = 0; k < playgroundPanelCardcount; k++)
                    {
                        var card = playgroundPanel.GetChild(k);
                        if (card.GetComponent<CardController>().isFacingUp)
                            if (CanItGoToAnyPlaygroundPanel(card))
                            {
                                var moves = GetPlaygroundMoves(card);
                                if (!IsItDumbMove(moves) && !Helpers.DoesContains(GameControl.instance.possibleMoves, moves))//!DoesExistInHelpHistory(moves) &&
                                    return moves;
                            }
                    }
                }
            }
            return null;
        }
        public static Move GetAcePanelMove(Transform card)
        {
            var cardSuit = card.name[0];
            int cardValue = int.Parse(card.name.Substring(1));

            bool isCardA = (cardValue == 1);

            int acePanelCount = GameControl.instance.acePanel.transform.childCount;
            for (int i = 0; i < acePanelCount; i++)
            {
                var panel = GameControl.instance.acePanel.transform.GetChild(i);
                if (panel.childCount > 0)
                {
                    var lastCard = panel.GetChild(panel.childCount - 1);
                    var lastCardSuit = lastCard.name[0];
                    int lastCardValue = int.Parse(lastCard.name.Substring(1));
                    if (lastCardSuit == cardSuit && cardValue == lastCardValue + 1)
                    {
                        var move = new Move(card.gameObject, card.transform.parent, panel);
                        return move;
                    }

                }
                else if (panel.childCount == 0 && isCardA)
                {
                    var move = new Move(card.gameObject, card.transform.parent, panel);
                    return move;
                }
            }
            return null;
        }
        public static List<Move> GetPlaygroundMoves(Transform card)
        {
            var cardSuit = card.name[0];
            int cardValue = int.Parse(card.name.Substring(1));
            int cardIndex = card.GetSiblingIndex();
            int parentChildCount = card.transform.parent.childCount;

            bool isCardK = (cardValue == 13);
            bool isLastCard = (parentChildCount - 1 == cardIndex);


            int playgroundPanelCount = GameControl.instance.playcardsObj.transform.childCount;
            for (int i = 0; i < playgroundPanelCount; i++)
            {
                var panel = GameControl.instance.playcardsObj.transform.GetChild(i);
                if (panel.childCount > 0)
                {
                    var lastCard = panel.GetChild(panel.childCount - 1);
                    var lastCardSuit = lastCard.name[0];
                    int lastCardValue = int.Parse(lastCard.name.Substring(1));

                    if (cardValue == lastCardValue - 1 && IsPlaygroudCardSuitTrue(lastCardSuit, cardSuit))
                    {
                        if (isLastCard)
                        {
                            var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                            return move.ToList();
                        }
                        else
                        {
                            var moves = new List<Move>();
                            for (int k = cardIndex; k < parentChildCount; k++)
                            {
                                var move = new Move(card.transform.parent.GetChild(k).gameObject, card.transform.parent, panel.transform);
                                moves.Add(move);
                            }
                            return moves;
                        }
                    }
                }
                else if (panel.childCount == 0 && isCardK)
                {
                    if (isLastCard)
                    {
                        var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                        return move.ToList();
                    }
                    else
                    {
                        var moves = new List<Move>();
                        for (int k = cardIndex; k < parentChildCount; k++)
                        {
                            var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                            moves.Add(move);
                        }
                        return moves;
                    }
                }
            }
            return null;
        }
        public static List<List<Move>> GetAllPossibleMoves()
        {
            GameControl.instance.possibleMoves = new List<List<Move>>();
            var moves = GetNextMove();
            while (!Helpers.IsNullOrEmpty(moves))
            {
                GameControl.instance.possibleMoves.Add(moves);
                moves = GetNextMove();
            }

            return GameControl.instance.possibleMoves;
        }
        public static bool IsPlaygroudCardSuitTrue(char targetCardSuit, char cardSuit)
        {
            if (targetCardSuit == 'H' && (cardSuit == 'S' || cardSuit == 'C'))
                return true;
            else if (targetCardSuit == 'D' && (cardSuit == 'S' || cardSuit == 'C'))
                return true;
            else if (targetCardSuit == 'S' && (cardSuit == 'H' || cardSuit == 'D'))
                return true;
            else if (targetCardSuit == 'C' && (cardSuit == 'H' || cardSuit == 'D'))
                return true;
            else
                return false;
        }
        public static bool IsItDumbMove(List<Move> result)
        {
            // return true => it is dumb move
            var origin = result.First().Origin;
            var target = result.First().Target;
            var card = result.First().Card;
            int cardValue = int.Parse(card.name.Substring(1));
            bool isCardK = (cardValue == 13);
            bool isCardA = (cardValue == 1);

            if (card.transform.GetSiblingIndex() == 0) // for the card K => if it is already in the empty panel, it doesnt need to change position
            {
                if (origin.name.Contains("Ground"))
                    return false;
                if (target.childCount == 0 && isCardK)
                    return true;
                else
                    return false;
            }
            else if (origin.childCount >= 2 && target.childCount >= 1) // if its already in the same color and value, its donest need to move
            {
                if (origin.name.Contains("Panel"))
                {
                    var bigBrotherOfOriginCardIndex = result.First().Card.transform.GetSiblingIndex() - 1;
                    var bigBrotherOfOriginCard = origin.GetChild(bigBrotherOfOriginCardIndex);

                    if (bigBrotherOfOriginCard.GetComponent<CardController>().isFacingUp)
                    {
                        var bigBrotherOfOriginCardValue = int.Parse(bigBrotherOfOriginCard.name.Substring(1));

                        var targetCard = target.GetChild(target.childCount - 1);
                        int targetCardValue = int.Parse(targetCard.name.Substring(1));

                        if (bigBrotherOfOriginCardValue == targetCardValue)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

    }
}
