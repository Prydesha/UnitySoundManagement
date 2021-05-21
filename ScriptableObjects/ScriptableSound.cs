using UnityEngine;
using System.Collections;

/// <summary>
/// Author: Shawn Pryde
/// Generic class used by an AudioManager to represent 
/// the data for a particular sound. Create in inspector.
/// </summary>
[System.Serializable][CreateAssetMenu(fileName = "New Sound", menuName = "Audio/Sound")]
public class ScriptableSound : ScriptableSoundItem
{
    [Tooltip("The AudioClip associated with this sound")]
    [SerializeField] private AudioClip _clip = null;
    [Tooltip("The pitch at which to play this sound")]
    [Range(.1f, 3f)]
    [SerializeField] private float _pitch = 1f;
    [Tooltip("Will this sound loop?")]
    [SerializeField] private bool _loop = false;
    [Tooltip("Will this sound play on awake?")]
    [SerializeField] private bool _playOnAwake = false;
    [Tooltip("Minimum time between each 'play' instance of this sound")]
    [Range(0.01f,1f)]
    [SerializeField] private float _stackDelay = 0.1f;
    [SerializeField] private bool _usesRandomPitches = false;
    [Tooltip("Range of random pitches to play.\n"+
             "X = lower bound\n"+
             "Y = upper bound\n"+
             "Use values between 0.1 and 3.\n" +
             "1.0 is the default pitch")]
    [SerializeField] private Vector2 _randomPitchRange = new Vector2(0.5f, 1.5f);

    #region Getters and Setters
    public AudioClip Clip => _clip;
    public float Pitch => _pitch;
    public bool Loop => _loop;
    public bool PlayOnAwake => _playOnAwake;
    public float StackDelay =>  _stackDelay;
    public bool UsesRandomPitches => _usesRandomPitches;
    public Vector2 RandomPitchRange => _randomPitchRange;

    public override void Accept(ScriptableSoundItemVisitor visitor)
    {
        visitor.VisitSound(this);
    }
    #endregion
}
