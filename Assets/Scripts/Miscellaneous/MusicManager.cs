using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private int trackIndex = 0;
    private AudioSource src;
    private AudioHighPassFilter hpf;
    private AudioLowPassFilter lpf;

    private GameObject mixer;

    private Object[] clips;
    private string currentScene;
    private float whenChangeStarted;
    private List<string> currentEffects;
    private float deltaPitchTime;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        mixer = new GameObject("MusicPlayer", typeof(AudioSource), typeof(AudioHighPassFilter), typeof(AudioLowPassFilter));
        mixer.transform.parent = transform;

        src = mixer.GetComponent<AudioSource>();
        src.pitch = 1F;

        clips = Resources.LoadAll("Audio/Music", typeof(AudioClip));
        currentEffects = new List<string>();

        ApplyAudioFilters(mixer);
        PlayTrack(src, trackIndex);
    }

    private void Update()
    {
        var before = trackIndex;
        int next = before;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name != currentScene)
        {
            currentScene = scene.name;

            switch (currentScene)
            {
                case "Title Screen":
                    next = 0;
                    break;

                default:
                    Debug.Log("Got an unexpected scene name: " + currentScene);
                    src.Stop();
                    break;
            }
        }

        if (next != before)
        {
            trackIndex = next;
            PlayTrack(src, trackIndex);
        }

        if (currentEffects.Count > 0)
            currentEffects = HandleEffects(src, currentEffects);

        ApplyAudioFilters(mixer);
    }

    /** Given a gameobject, assign high and low pass filters to it with bandwith cutoff values.*/
    private void ApplyAudioFilters(GameObject mixer)
    {
        hpf = mixer.GetComponent<AudioHighPassFilter>();
        lpf = mixer.GetComponent<AudioLowPassFilter>();

        hpf.cutoffFrequency = (Mathf.Sin(Time.time / 10) * 3000 + 100);
        lpf.cutoffFrequency = (Mathf.Sin(Time.time / 50) * 5000 + 1000);
    }

    /** Stops current playback of the track, loads a new track, and plays it.*/
    private void PlayTrack(AudioSource src, int index)
    {
        src.Stop();
        src.clip = clips[index] as AudioClip;
        src.Play();
    }

    /** Given a starting time of when to apply the effect,
    pitch the music down until it is stopped.*/
    public void KillTheMusic(float when, float howLong)
    {
        whenChangeStarted = when;
        deltaPitchTime = howLong;
        currentEffects.Add("pitch_down");
    }

    /** Given a starting time of when to apply the effect,
    start the music and pitch it back to normal.*/
    public void LiveTheMusic(float when, float howLong)
    {
        Debug.Log("Now I'm Living");
        whenChangeStarted = when;
        deltaPitchTime = howLong;
        currentEffects.Add("pitch_up");
    }

    /**Given an AudioSource and list of named effects, 
    apply the signal processing methods.*/
    private List<string> HandleEffects(AudioSource src, List<string> effects)
    {
        float progress;

        foreach (string fx in effects)
            switch (fx)
            {
                // jumps happen outside of DeathCycle, so it is safe to 
                // modulate the pitch with mulitplication.
                case "jumping":
                    break;
                // set a new pitch value on each frame while dying
                case "pitch_down":
                    progress = (Time.deltaTime - whenChangeStarted) / deltaPitchTime;
                    src.pitch = 1 - progress;

                    if (progress >= 1)
                    {
                        src.Pause();
                        effects.Remove("pitch_down");
                    }
                    break;
                // set a new pitch value on each frame while spawning
                case "pitch_up":

                    progress = (Time.deltaTime - whenChangeStarted) / deltaPitchTime;
                    Debug.Log("progress " + progress);
                    if (progress == 0)
                        src.UnPause();

                    if (progress >= 1)
                        effects.Remove("pitch_up");

                    src.pitch = progress;
                    break;
                default:
                    break;
            }

        return effects;
    }
}