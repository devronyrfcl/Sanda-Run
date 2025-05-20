using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class AnimatedLoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI percentageText;
    public RectTransform imageRectTransform;
    public GameObject loadingBarGameObject;
    public Slider loadingSlider;
    public AudioSource animationSound;

    private float fakeLoadTime = 3f;
    public string sceneToLoad = "Lobby";

    void Start()
    {
        Application.targetFrameRate = 60;

        // Start size is 0x0
        imageRectTransform.sizeDelta = Vector2.zero;

        // Hide loading bar and slider at start
        if (loadingBarGameObject != null)
            loadingBarGameObject.SetActive(false);

        if (loadingSlider != null)
        {
            loadingSlider.value = 0;
            loadingSlider.gameObject.SetActive(false);
        }

        StartCoroutine(AnimationThenLoading());
    }

    System.Collections.IEnumerator AnimationThenLoading()
    {
        // Play sound and animate size to 310x310
        if (animationSound != null)
            animationSound.Play();

        yield return imageRectTransform.DOSizeDelta(new Vector2(225f, 225f), 1.2f)
            .SetEase(Ease.OutElastic)
            .WaitForCompletion();

        // Activate UI
        if (loadingBarGameObject != null)
            loadingBarGameObject.SetActive(true);

        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(true);

        // Begin loading
        yield return StartCoroutine(CombinedLoadingSequence());
    }

    System.Collections.IEnumerator CombinedLoadingSequence()
    {
        float elapsed = 0f;

        // ---------- Fake loading (0–80%) ----------
        while (elapsed < fakeLoadTime)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Lerp(0, 80, elapsed / fakeLoadTime);
            UpdateLoadingUI(percent);
            yield return null;
        }

        UpdateLoadingUI(80);

        // ---------- Real loading (80–100%) ----------
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float percent = 80 + targetProgress * 20;
            UpdateLoadingUI(percent);

            if (asyncLoad.progress >= 0.9f)
            {
                UpdateLoadingUI(100);
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    void UpdateLoadingUI(float percent)
    {
        int displayPercent = Mathf.RoundToInt(percent);
        percentageText.text = displayPercent + "%";

        if (loadingSlider != null)
            loadingSlider.value = percent / 100f;
    }
}
