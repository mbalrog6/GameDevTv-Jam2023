using UnityEngine;

namespace MB6
{
    [CreateAssetMenu()]
    public class SoundLibrarySO : ScriptableObject
    {
        public AudioClip Music;
        public AudioClip[] ButtonClicked;
        public AudioClip[] ButtonSelected;
        public AudioClip[] LightMinorPower;
        public AudioClip[] DarkMinorPower;
        public AudioClip[] HurtSounds;
        public AudioClip[] WraithDying;
        public AudioClip[] Moans;
        public AudioClip[] GoodSideMusic;
        public AudioClip[] BadSideMusic;
        public AudioClip[] ShortMusic;
    }
}