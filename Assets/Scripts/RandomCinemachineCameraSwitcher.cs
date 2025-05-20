using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class RandomCinemachineCameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera[] virtualCameras;
    public float switchInterval = 3f;

    [Header("Loading Screen")]
    public GameObject loadingScreenPanel;
    public Slider loadingSlider;
    public TMP_Text loadingText;

    private CinemachineVirtualCamera currentCamera;

    void Start()
    {
        if (virtualCameras.Length == 0)
        {
            Debug.LogWarning("No virtual cameras assigned!");
            return;
        }

        StartCoroutine(SwitchCameraRoutine());
    }

    IEnumerator SwitchCameraRoutine()
    {
        while (true)
        {
            // Disable all cameras
            foreach (var cam in virtualCameras)
            {
                cam.enabled = false;
            }

            // Choose a random camera and enable it
            int index = Random.Range(0, virtualCameras.Length);
            currentCamera = virtualCameras[index];
            currentCamera.enabled = true;

            yield return new WaitForSeconds(switchInterval);
        }
    }

    // 🔁 Call this to switch scenes with a loading screen
    public void SwitchToMainGameScene()
    {
        StartCoroutine(LoadSceneAsync("MainGame"));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreenPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            loadingText.text = $"{(int)(progress * 100)}%";

            // Wait until scene is nearly loaded (>= 90%)
            if (operation.progress >= 0.9f)
            {
                loadingText.text = "100%";
                loadingSlider.value = 1f;

                yield return new WaitForSeconds(0.5f); // small delay for visual feedback
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
