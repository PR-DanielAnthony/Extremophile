using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private string startScene;

    //private MusicManager music;

    public void StartGame()
    {
        SceneManager.LoadScene(startScene);
        //music = FindFirstObjectByType<MusicManager>();
        //music.self.LiveTheMusic(Time.deltaTime, 2F);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}