using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    public static Define.Scene targetScene;
    public Slider progressBar;
    public Transform whaleTransform;

    void Start()
    {
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        yield return new WaitForSeconds(0.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene.ToString());
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            progressBar.value = op.progress;
            whaleTransform.position = new Vector3(op.progress * 10f, whaleTransform.position.y, whaleTransform.position.z);
            yield return null;
        }

        // 고래 끝 위치
        progressBar.value = 1f;
        whaleTransform.position = new Vector3(10f, whaleTransform.position.y, whaleTransform.position.z);

        yield return new WaitForSeconds(0.5f);

        op.allowSceneActivation = true;
    }
}
