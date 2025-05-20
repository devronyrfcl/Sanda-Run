using UnityEngine;
using System.Collections;

public class StepSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] stepClips;
    public float stepInterval = 0.5f;

    private bool isPlaying = false;
    private Coroutine playCoroutine;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void StartSteps()
    {
        if (stepClips.Length == 0 || audioSource == null) return;
        if (isPlaying) return;

        isPlaying = true;
        playCoroutine = StartCoroutine(PlayStepSounds());
    }

    public void StopSteps()
    {
        if (!isPlaying) return;
        isPlaying = false;
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
    }

    private IEnumerator PlayStepSounds()
    {
        while (isPlaying)
        {
            AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(stepInterval);
        }
    }
}