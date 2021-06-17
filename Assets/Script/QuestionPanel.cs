using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
    public GameObject background;
    public GameObject noButton;

    private float fadeTime = 0.3f;

    private void Start()
    {
        noButton.GetComponent<Button>().onClick.AddListener(delegate { NoButtonClick(); });
        StartCoroutine(FadeInBackground());
    }

    public void NoButtonClick()
    {
        StartCoroutine(FadeOutBackground());
        Destroy(gameObject, fadeTime - 0.1f);
    }

    private IEnumerator FadeInBackground()
    {
        float seconds = fadeTime;
        float t = 0f;
        Color32 targetColor = new Color32(36, 36, 36, 150);
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            background.GetComponent<Image>().color = Color32.Lerp(background.GetComponent<Image>().color, targetColor, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
    private IEnumerator FadeOutBackground()
    {
        float seconds = fadeTime;
        float t = 0f;
        Color32 targetColor = new Color32(36, 36, 36, 0);
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            background.GetComponent<Image>().color = Color32.Lerp(background.GetComponent<Image>().color, targetColor, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}