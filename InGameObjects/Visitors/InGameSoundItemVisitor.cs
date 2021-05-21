
/// <summary>
/// Abstract visitor class which is used to visit in game SoundItem objects
/// </summary>
public abstract class InGameSoundItemVisitor
{
    public virtual void VisitSoundItem(InGameSoundItem item) { }
    public virtual void VisitSound(InGameSound sound) { }
    public virtual void VisitSoundCollection(InGameSoundCollection coll) { }
}
