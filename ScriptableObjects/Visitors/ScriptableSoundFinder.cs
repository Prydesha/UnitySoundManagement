using UnityEngine;

/// <summary>
/// Visitor used to find a specific sound object
/// </summary>
public class ScriptableSoundFinder : ScriptableSoundItemVisitor
{
    // the SoundItem this visitor is currently
    // visiting
    private ScriptableSound _sound;
    public ScriptableSound Sound { get { return _sound; } }
    // the name of the SoundItem we are searching for
    private string _name;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of the desired sound</param>
    public ScriptableSoundFinder(string name)
    {
        _name = name;
    }

    /// <summary>
    /// "Visits" a Sound, setting the current visiting 
    /// sound to the parameter sound
    /// </summary>
    /// <param name="sound"></param>
    public override void VisitSound(ScriptableSound sound)
    {
        _sound = sound;
    }

    public override void VisitSoundCollection(ScriptableSoundCollection coll)
    {
        foreach(ScriptableSoundItem ssi in coll.sounds)
        {
            ssi.Accept(this);
            if (HasDesiredSound())
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns true if the current sound acquired by this Finder
    /// is the one which it has set out to find
    /// </summary>
    /// <returns></returns>
    public bool HasDesiredSound()
    {
        if (_sound == null) { return false; }
        return _sound.Name == _name;
    }

}
