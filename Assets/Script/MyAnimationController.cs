using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class MyAnimationController : MonoBehaviour
    {
        public static MyAnimationController instance;
        private void Awake()
        {
            instance = this;
        }
        public static IEnumerator SlideAndParentToGround(GameObject card, float time, Vector3 pos, bool animation = true)
        {
            card.GetComponent<CardController>().isSliding = true;
            Transform parent = GameControl.instance.groundObj.transform;
            if (animation)
            {
                float seconds = time;
                float t = 0f;
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    card.transform.position = Vector3.Lerp(card.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
            }

            card.transform.SetParent(parent);

            if (parent.childCount > 3)
            {
                for (int k = 0; k < parent.childCount - 3; k++)
                {
                    parent.GetChild(k).gameObject.SetActive(false);
                }
            }
            card.GetComponent<CardController>().isDummy = false;
            card.GetComponent<CardController>().isSliding = false;
        }

        public static IEnumerator SlideAndParent(GameObject card, Transform targetParent, float time)
        {
            var parent = card.transform.parent;
            int siblingIndex = card.transform.GetSiblingIndex();
            int childCount = card.transform.parent.childCount;

            var positions = GetDummyPositions(card, targetParent);
            var cards = new List<GameObject>();
            int index = 0;

            for (int i = siblingIndex; i < childCount; i++)
            {
                cards.Add(parent.GetChild(i).gameObject);
            }


            foreach (var c in cards)
            {
                instance.StartCoroutine(Slide(c, targetParent.transform, positions[index], time));
                index++;
            }

            do
            {
                yield return null;

            } while (parent.childCount != childCount - index);

            DestroyDummies();

            while (parent.name == card.transform.parent.name)
                yield return null;

            if (targetParent.name.Contains("Panel"))
                targetParent.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(targetParent);

        }

        public static IEnumerator SlideToDeck(GameObject card, float time)
        {
            Transform targetParent = GameControl.instance.deckObj.transform;

            var parent = card.transform.parent;
            int childCount = card.transform.parent.childCount;

            var pos = GetDummyPosition(card, targetParent);

            instance.StartCoroutine(Slide(card, targetParent.transform, pos, time));

            do
            {
                yield return null;

            } while (parent.childCount != childCount - 1);

            DestroyDummies();

            while (parent.name == card.transform.parent.name)
                yield return null;

            if (targetParent.name.Contains("Panel"))
                targetParent.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(targetParent);

        }

        private static void DestroyDummies()
        {
            var dummies = GameObject.FindGameObjectsWithTag("dummy");

            foreach (var dummy in dummies)
            {
                DestroyImmediate(dummy);
            }
        }

        private static void SetParent(GameObject c, Transform targetParent)
        {
            var parent = c.transform.parent;
            c.transform.SetParent(targetParent);

            if (parent.name.Contains("Panel"))
                parent.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(parent); // set the spacing for the panel layout

            c.GetComponent<Canvas>().overrideSorting = false;
            c.GetComponent<CardController>().isDummy = false;
            c.GetComponent<CardController>().isSliding = false;

        }

        private static IEnumerator Slide(GameObject card, Transform targetParent, Vector3 pos, float time)
        {
            card.GetComponent<CardController>().isSliding = true;
            card.GetComponent<CardController>().isDummy = true;

            card.GetComponent<Canvas>().overrideSorting = true;
            card.GetComponent<Canvas>().sortingOrder = 2;

            if (targetParent.name.Contains("Deck"))
                instance.StartCoroutine(RotateToHideCard(card.transform));

            float seconds = time;
            float t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                card.transform.position = Vector3.Lerp(card.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            SetParent(card, targetParent);

        }
        public static Vector3 GetDummyPosition(GameObject card, Transform targetParent)
        {
            card.GetComponent<CardController>().isSliding = true;

            var positionDummy = Instantiate(GameControl.instance.cardPrefab, targetParent) as GameObject;
            positionDummy.GetComponent<CardController>().isDummy = true;
            positionDummy.tag = "dummy";
            positionDummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            if (targetParent.parent.name.Contains("Panel"))
                targetParent.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(targetParent, 1);

            LayoutRebuilder.ForceRebuildLayoutImmediate(targetParent.GetComponent<RectTransform>());

            var pos = positionDummy.transform.position;
            return pos;
        }
        public static List<Vector3> GetDummyPositions(GameObject card, Transform targetParent)
        {
            card.GetComponent<CardController>().isSliding = true;

            var parent = card.transform.parent;
            int siblingIndex = card.transform.GetSiblingIndex();
            int childCount = card.transform.parent.childCount;

            var cardPositions = new List<Vector3>();
            var posDummies = new List<GameObject>();
            for (int i = siblingIndex; i < childCount; i++)
            {
                var positionDummy = Instantiate(GameControl.instance.cardPrefab, targetParent) as GameObject;
                positionDummy.GetComponent<CardController>().isDummy = true;
                positionDummy.tag = "dummy";
                positionDummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                cardPositions.Add(parent.GetChild(i).transform.position);
                posDummies.Add(positionDummy);
            }

            if (targetParent.parent.name.Contains("Panel"))
                targetParent.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(targetParent, 1);

            LayoutRebuilder.ForceRebuildLayoutImmediate(targetParent.GetComponent<RectTransform>());

            var positions = new List<Vector3>();
            foreach (var positionDummy in posDummies)
            {
                var pos = positionDummy.transform.position;
                positions.Add(pos);
            }
            return positions;
        }

        public static IEnumerator RotateToRevealCard(Transform card, float time = 0.2f, bool animation = true)
        {
            if (animation)
            {
                if (!card.GetComponent<CardController>().isFacingUp)
                {

                    card.GetComponent<CardController>().isSliding = true;

                    float seconds = time;
                    float t = 0f;
                    var v = new Vector3(0, 90f, 0);
                    var q = Quaternion.Euler(v);
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                        yield return null;
                    }

                    card.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.name);
                    card.GetComponent<CardController>().isFacingUp = true;

                    seconds = time;
                    t = 0f;
                    v = new Vector3(0, 0f, 0);
                    q = Quaternion.Euler(v);
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                        yield return null;
                    }
                    card.GetComponent<CardController>().isSliding = false;
                }

            }
            else
            {
                card.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.name);
                card.GetComponent<CardController>().isFacingUp = true;
            }
        }
        public static IEnumerator RotateToHideCard(Transform card, float time = 0.2f)
        {
            float seconds = time;
            float t = 0f;
            var v = new Vector3(0, 90f, 0);
            var q = Quaternion.Euler(v);
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            card.GetComponent<Image>().sprite = Resources.Load<Sprite>(GameControl.BACK_OF_A_CARD_SPRITE_NAME);
            card.GetComponent<CardController>().isFacingUp = false;
            seconds = time;
            t = 0f;
            v = new Vector3(0, 0f, 0);
            q = Quaternion.Euler(v);
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        public static IEnumerator Shake(GameObject obj, float s = 0.05f)
        {
            float shakeRadius = obj.GetComponent<RectTransform>().rect.width / 32f;
            if (obj.transform.parent.name.Contains("Panel"))
            {
                var parent = obj.transform.parent;
                var sibIndex = obj.transform.GetSiblingIndex();
                var childCount = parent.transform.childCount;

                float seconds;
                float t;
                var leftPositions = new List<Vector3>();
                var rightPositions = new List<Vector3>();
                var startPositions = new List<Vector3>();

                for (int k = sibIndex; k < childCount; k++)
                {
                    var startPos = parent.GetChild(k).GetComponent<RectTransform>().position;
                    startPositions.Add(startPos);
                    leftPositions.Add(new Vector3(startPos.x + shakeRadius, startPos.y, startPos.z));
                    rightPositions.Add(new Vector3(startPos.x - shakeRadius, startPos.y, startPos.z));
                }

                for (int i = 0; i < 5; i++)
                {
                    seconds = s;
                    t = 0f;
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        int index = 0;
                        for (int k = sibIndex; k < childCount; k++)
                        {
                            parent.GetChild(k).GetComponent<RectTransform>().position = Vector3.Lerp(parent.GetChild(k).GetComponent<RectTransform>().position, leftPositions[index], Mathf.SmoothStep(0f, 1f, t));
                            index++;
                        }
                        yield return null;
                    }

                    seconds = s;
                    t = 0f;
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        int index = 0;

                        for (int k = sibIndex; k < childCount; k++)
                        {
                            parent.GetChild(k).GetComponent<RectTransform>().position = Vector3.Lerp(parent.GetChild(k).GetComponent<RectTransform>().position, rightPositions[index], Mathf.SmoothStep(0f, 1f, t));
                            index++;
                        }
                        yield return null;
                    }
                }

                seconds = s;
                t = 0f;
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    int index = 0;
                    for (int k = sibIndex; k < childCount; k++)
                    {
                        parent.GetChild(k).GetComponent<RectTransform>().position = Vector3.Lerp(parent.GetChild(k).GetComponent<RectTransform>().position, startPositions[index], Mathf.SmoothStep(0f, 1f, t));
                        index++;
                    }
                    yield return null;
                }
            }
            else
            {
                float seconds;
                float t;
                var startPos = obj.GetComponent<RectTransform>().position;
                Vector3 left = new Vector3(startPos.x + shakeRadius, startPos.y, startPos.z);
                Vector3 right = new Vector3(startPos.x - shakeRadius, startPos.y, startPos.z);
                for (int i = 0; i < 5; i++)
                {
                    seconds = s;
                    t = 0f;
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        obj.GetComponent<RectTransform>().position = Vector3.Lerp(obj.GetComponent<RectTransform>().position, left, Mathf.SmoothStep(0f, 1f, t));
                        yield return null;
                    }
                    seconds = s;
                    t = 0f;
                    while (t <= 1.0)
                    {
                        t += Time.deltaTime / seconds;
                        obj.GetComponent<RectTransform>().position = Vector3.Lerp(obj.GetComponent<RectTransform>().position, right, Mathf.SmoothStep(0f, 1f, t));
                        yield return null;
                    }
                }
                seconds = s;
                t = 0f;
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    obj.GetComponent<RectTransform>().position = Vector3.Lerp(obj.GetComponent<RectTransform>().position, startPos, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
            }
        }
        public static IEnumerator SlideHelpCard(List<Move> helpMoves, bool willDisappear)
        {
            var target = helpMoves.First().Target;
            var cardPositions = new List<Vector3>();
            var posDummies = new List<GameObject>();
            foreach (var move in helpMoves)
            {
                var positionDummy = Instantiate(GameControl.instance.cardPrefab, move.Target) as GameObject;
                positionDummy.GetComponent<CardController>().isDummy = true;
                positionDummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                cardPositions.Add(move.Card.transform.position);
                posDummies.Add(positionDummy);
            }

            if (target.name.Contains("Panel"))
                target.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(target); // set the spacing for the panel layout


            LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout

            var positions = new List<Vector3>();
            foreach (var positionDummy in posDummies)
            {
                var pos = positionDummy.transform.position;
                positions.Add(pos);
            }

            var movingDummies = new List<GameObject>();

            for (int i = 0; i < positions.Count; i++)
            {
                var movingDummy = Instantiate(GameControl.instance.cardPrefab, GameControl.instance.canvas.transform) as GameObject;
                movingDummy.GetComponent<CardController>().isDummy = true;
                movingDummy.transform.position = cardPositions[i];
                movingDummy.GetComponent<Canvas>().overrideSorting = true;
                movingDummy.GetComponent<Image>().sprite = helpMoves[i].Card.GetComponent<Image>().sprite;
                movingDummies.Add(movingDummy);
            }

            for (int i = 0; i < movingDummies.Count; i++)
            {
                instance.StartCoroutine(SlideAndDisappear(movingDummies[i], positions[i], willDisappear));
            }

            foreach (var positionDummy in posDummies)
            {
                DestroyImmediate(positionDummy);
            }

            if (target.name.Contains("Panel"))
                target.transform.GetComponent<VerticalLayoutGroup>().spacing = GameControl.CalculateSpacing(target); // set the spacing for the panel layout


            LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout
            yield return null;
        }
        public static IEnumerator SlideAndDisappear(GameObject movingDummy, Vector3 pos, bool willDisappear, float time = 1f)
        {
            float seconds = time;
            float t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                movingDummy.transform.position = Vector3.Lerp(movingDummy.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (willDisappear)
            {
                seconds = 0.1f;
                t = 0f;
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    movingDummy.GetComponent<RectTransform>().localScale = Vector3.Lerp(movingDummy.GetComponent<RectTransform>().localScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
                Destroy(movingDummy);
            }
        }
    }
}
