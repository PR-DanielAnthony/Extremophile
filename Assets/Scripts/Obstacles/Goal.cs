using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
            StartCoroutine(LevelClear(player));
    }

    private IEnumerator LevelClear(Player player)
    {
        GetComponent<BoxCollider2D>().enabled = false;
        player.Controller.enabled = false;
        yield return new WaitForSecondsRealtime(1f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Load the next scene
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);

        else
            SceneManager.LoadScene(1);
    }
}