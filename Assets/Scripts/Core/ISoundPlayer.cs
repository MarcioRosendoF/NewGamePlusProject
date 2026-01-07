namespace Core
{
    public interface ISoundPlayer
    {
        void PlaySound(SoundEffectSO sound);
        void StopLoopingSound(SoundEffectSO sound);
        void StopAllLoopingSounds();
    }
}
