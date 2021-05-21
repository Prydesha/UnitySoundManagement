
/// <summary>
/// Abstract visitor class which is used to visit SoundItem scriptable objects
/// </summary>
public abstract class ScriptableSoundItemVisitor
{
    public virtual void VisitSoundItem(ScriptableSoundItem item) { }
    public virtual void VisitSound(ScriptableSound sound) { }
    public virtual void VisitSoundCollection(ScriptableSoundCollection coll) { }
}
