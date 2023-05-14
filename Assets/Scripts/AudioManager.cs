using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region SINGLETON
    public static AudioManager Instance;
    void Awake() => Instance = this;
    #endregion

    public static int PLACE_STRUCTURE = 0;
    public static int PLACE_BRIDGE = 1;
    public static int REMOVE_STRUCTURE = 2;
    public static int MORTAR_SOUND = 3;
    public static int BULLET_SOUND = 4;
    public static int SHOTGUN_SOUND = 5;

    private AudioSource source;
    [SerializeField] private List<AudioClip> sounds;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlaySound(int index)
    {
        // grab the audioclip from the list of clips
        var sound = sounds[index];

        // play the clip through our source
        source.PlayOneShot(sound);
    }
}
