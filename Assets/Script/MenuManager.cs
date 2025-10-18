using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Play(string gameScene)
    {
        StartCoroutine(FadeOutAndChangeScene(gameScene));
    }

    public void Setting()
    {
        Debug.Log("Comming Soon !!");
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnEnable()
    {
        Time.timeScale = 0.0f;
    }
    public void OnDisable()
    {
        Time.timeScale = 1.0f;
    }


    private IEnumerator FadeOutAndChangeScene(string nextScene)
    {
        Time.timeScale = 1.0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
