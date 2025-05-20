using UnityEngine;

public class DialogueAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    public AudioClip[] Dialogue_1;
    public AudioClip[] Dialogue_2;
    public AudioClip[] Dialogue_3;
    public AudioClip[] Dialogue_4;

    // Public Dialogue Play Functions
    public void PlayRandomDialogue1()
    {
        TryPlayFromArray(Dialogue_1);
    }

    public void PlayRandomDialogue2()
    {
        TryPlayFromArray(Dialogue_2);
    }

    public void PlayRandomDialogue3()
    {
        TryPlayFromArray(Dialogue_3);
    }

    public void PlayRandomDialogue4()
    {
        TryPlayFromArray(Dialogue_4);
    }

    // Internal Method: Plays audio only if nothing is currently playing
    private void TryPlayFromArray(AudioClip[] clipArray)
    {
        if (audioSource == null || clipArray.Length == 0)
        {
            Debug.LogWarning("Missing audio source or clips.");
            return;
        }

        if (audioSource.isPlaying)
        {
            // Skip if audio is currently playing
            return;
        }

        int index = Random.Range(0, clipArray.Length);
        audioSource.clip = clipArray[index];
        audioSource.Play();
    }
}
