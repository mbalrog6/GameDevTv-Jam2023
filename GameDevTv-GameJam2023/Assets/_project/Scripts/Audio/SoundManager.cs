using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MB6
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        [SerializeField] private SoundLibrarySO _soundLibrary;

        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource _soundFX;
        [SerializeField] private AudioSource _walkingSoundFX;

        [SerializeField] private int _level;
        private float _timer;
        [SerializeField] private float _pauseBetweenMusic;
        
        private const string SIRITGUIDE_PREFS = "SpiritGuidePrefs";
        private const string MASTER_PARAM = "MasterVolume";
        private const string MUSIC_PARAM = "MusicVolume";
        private const string SFX_PARAM = "SFXVolume";
        private const string VOLUME_STEP = "VolumeStep";
        
        [SerializeField] private AudioMixer _mixer;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            _music.clip = _soundLibrary.Music;
            _walkingSoundFX.volume = .4f;
            
        }

        private void Start()
        {
            if (_level > 0)
            {
                UpdateMixerVolumes();
            }
        }
        

        private void Update()
        {
            if (_music.isPlaying) return;

            _timer += Time.deltaTime;
            if (_timer >= _pauseBetweenMusic)
            {
                _timer = 0;

                switch (_level)
                {
                    case 0:
                        _music.clip = GetSound(_soundLibrary.ShortMusic);
                        break;
                    case 1:
                        _music.clip = GetSound(_soundLibrary.GoodSideMusic);
                        break;
                    case 2:
                        _music.clip = GetSound(_soundLibrary.BadSideMusic);
                        break;
                    case 3: 
                        _music.clip = GetSound(_soundLibrary.ShortMusic);
                        break;
                }
                
                _music.Play();
            }
        }
        
        public void UpdateMixerVolumes()
        {
            if (PlayerPrefs.HasKey(SIRITGUIDE_PREFS))
            {
                var step = PlayerPrefs.GetInt(VOLUME_STEP);
                var masterVol = PlayerPrefs.GetInt(MASTER_PARAM);
                var musicVol = PlayerPrefs.GetInt(MUSIC_PARAM);
                var sfxVol = PlayerPrefs.GetInt(SFX_PARAM);
                
                
                var caculatedVolumeStep = 1f / step;
                
                var volume = masterVol * caculatedVolumeStep;
                SetVolume(MASTER_PARAM, volume);
            
                volume = musicVol * caculatedVolumeStep;
                SetVolume(MUSIC_PARAM, volume);
            
                volume = sfxVol * caculatedVolumeStep;
                SetVolume(SFX_PARAM, volume);
            }
            else
            {
                SetVolume(MASTER_PARAM, 1f);
                SetVolume(MUSIC_PARAM, .5f);
                SetVolume(SFX_PARAM, .5f);
            }
        }
        
        private void SetVolume(string MixerParam, float volumeLevel)
        {
            var volume = Mathf.Clamp(volumeLevel, 0.0001f, 1f);
            _mixer.SetFloat(MixerParam, Mathf.Log10(volume) * 20f);
        }

        private AudioClip GetSound(AudioClip[] audioShelf) => audioShelf[Random.Range(0, audioShelf.Length)];

        
        public void PlayWalkingSound(bool isMoving)
        {

            if (!_walkingSoundFX.isPlaying && isMoving)
            {
                if (Random.Range(0, 500) > 498)
                {
                    _walkingSoundFX.clip = GetSound(_soundLibrary.Moans);
                    _walkingSoundFX.Play();
                }
                
            }
        }

        public void PlayPickUpObjectSound()
        {
            // _soundFX.PlayOneShot(GetSound(_soundLibrary.ObjectPickup));
        }
        
        public void PlayButtonClicked()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.ButtonClicked));
        }
        
        public void PlayButtonSelected()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.ButtonSelected));
        }

        public void PlayHurtSounds()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.HurtSounds));
        }

        public void PlayDyingScream()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.WraithDying));
        }
        
        public void PlayLightMinorPower()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.LightMinorPower));
        }

        public void PlayDarkMinorPower()
        {
            _soundFX.PlayOneShot(GetSound(_soundLibrary.DarkMinorPower));
        }

        public void PlayContiniousLightPower()
        {
            
        }
    }
}