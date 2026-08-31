using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private string fallbackSceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        string targetScene = string.IsNullOrEmpty(SceneLoader.TargetScene)
            ? fallbackSceneName
            : SceneLoader.TargetScene;

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            progressBar.value = operation.progress / 0.9f;
            yield return null;
        }

        progressBar.value = 1f;
        operation.allowSceneActivation = true;
    }
}
