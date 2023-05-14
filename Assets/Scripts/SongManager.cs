using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    #region SINGLETON
    public static SongManager Instance;
    void Awake() => Instance = this;
    #endregion

    private static AudioSource source;

    [SerializeField] private AudioClip[] songs;
    private int songIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (source == null)
            source = GetComponent<AudioSource>();

        if (!source.isPlaying)
            source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // BUG: bug where next song played if user alt tabs out
        if (!source.isPlaying)
        {
            PlayNextSong();
        }

        if (Input.GetKeyDown(KeyCode.F9))
            PlayNextSong();
    }

    public void PlayNextSong()
    {
        source.clip = songs[songIndex];
        source.Play();

        if (++songIndex >= songs.Length)
            songIndex = 0;
    }
}
