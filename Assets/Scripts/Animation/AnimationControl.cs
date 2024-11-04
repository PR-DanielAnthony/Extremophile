using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimationControl : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private AudioClip sparkle;
    [SerializeField] private Text promptText;
    [SerializeField] private string promptString;

    private void Update()
    {
        if (!string.IsNullOrEmpty(promptText.text) && Input.GetMouseButtonDown(0))
            SceneManager.LoadScene(nextScene);
    }

    private void ClickToContinue()
    {
        promptText.text = promptString;
    }

    private void PlayClip()
    {
        GetComponent<AudioSource>().Play();
        GetComponent<AudioSource>().PlayOneShot(sparkle);
    }
}