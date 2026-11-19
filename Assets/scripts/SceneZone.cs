using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneZone : MonoBehaviour
{
    public GameObject prompt;   // PRESS E 글자
    public string sceneName;    // 넘어갈 씬 이름
    private bool playerNear;

    private void Start()
    { GetComponent<MeshRenderer>().enabled = false; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        prompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        prompt.SetActive(false);
    }

    private void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
            SceneManager.LoadScene(sceneName);
    }
}
