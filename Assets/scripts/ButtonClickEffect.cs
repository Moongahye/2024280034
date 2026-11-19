using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ButtonClickEffect : MonoBehaviour
{
    public AudioClip clickSound;
    public GameObject sparkleUI;  // Optional: a star image under the Canvas
    public string nextSceneName;   // Leave blank for buttons that don't change scenes
    public float effectDuration = 0.5f;

    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // UI sound (2D)
        originalScale = transform.localScale;

        if (sparkleUI != null)
            sparkleUI.SetActive(false);
    }

    public void Click()
    {
        if (!isPlaying)
            StartCoroutine(PlayClickEffect());
    }

    private IEnumerator PlayClickEffect()
    {
        isPlaying = true;

        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (sparkleUI != null)
            sparkleUI.SetActive(true);

        float duration = Mathf.Max(0.01f, effectDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float bump = 1f + 0.10f * Mathf.Sin(progress * Mathf.PI);
            transform.localScale = originalScale * bump;
            yield return null;
        }

        transform.localScale = originalScale;

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            if (sparkleUI != null)
                sparkleUI.SetActive(false);
            isPlaying = false;
        }
    }
}
