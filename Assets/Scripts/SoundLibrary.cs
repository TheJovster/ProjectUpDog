using UnityEngine;

/// Container for every clip in the prototype (GDD §26). Named fields rather
/// than a keyed lookup: no string typos, no runtime misses, visible in one place.
/// Create via Assets > Create > UPDOG > Sound Library.
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "UPDOG/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Header("Player")]
    public AudioClip collision;
    public AudioClip pop;
    public AudioClip death;
    public AudioClip reinflate;
    public AudioClip ascent;

    [Header("NPC")]
    public AudioClip talkSqueak;

    [Header("Environment")]
    public AudioClip fan;
    public AudioClip environmentCollision;
    public AudioClip humanRising;
    public AudioClip humanFalling;

    [Header("Music")]
    public AudioClip soundtrack;
    public AudioClip ambience;
}