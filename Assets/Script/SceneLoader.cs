using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    void Start()
    {
        instance = this;
    }
    private void Update()
    {
        if (instance == null)
            instance = this;
    }
    public void LoadGameScreen()
    {
        StartCoroutine(LoadAsyncScene("Game"));
    }

    public void LoadMainScreen()
    {
        StartCoroutine(LoadAsyncScene("Main"));
    }

    public IEnumerator LoadAsyncScene(string sceneName)
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
