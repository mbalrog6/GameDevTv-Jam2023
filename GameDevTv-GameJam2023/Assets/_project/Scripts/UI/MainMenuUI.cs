using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MB6
{
    public class MainMenuUI : MonoBehaviour
    {
        private const string SIRITGUIDE_PREFS = "SpiritGuidePrefs";
        private const string MASTER_PARAM = "MasterVolume";
        private const string MUSIC_PARAM = "MusicVolume";
        private const string SFX_PARAM = "SFXVolume";
        private const string VOLUME_STEP = "VolumeStep";
        
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        [Header("Audio Dependencies")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private HoverButton _masterIncrease;
        [SerializeField] private HoverButton _masterDecrease;
        [SerializeField] private HoverButton _musicIncrease;
        [SerializeField] private HoverButton _musicDecrease;
        [SerializeField] private HoverButton _sfxIncrease;
        [SerializeField] private HoverButton _sfxDecrease;
        
        [SerializeField] private Slider _masterBar;
        [SerializeField] private Slider _musicBar;
        [SerializeField] private Slider _sfxBar;
        
        [Header("Parameters")]
        [SerializeField] private int _volumeSteps;
        
        public bool IsDirty { get; private set; }
        
        private float _caculatedVolumeStep;
        private Dictionary<string, int> _channelVolumes;
        
        private GameObject _currentSelectedItem;
        private ButtonClickedEventArg _buttonClickedEventArg;

        public event EventHandler<EventArgs> OnNewItemSelected;
        public event EventHandler<ButtonClickedEventArg> OnButtonClicked;

        private bool _isActive;
        private bool _isInitialized;

        private void Awake()
        {
            _caculatedVolumeStep = 1f / _volumeSteps;
            var volume = _volumeSteps / 2;
            _channelVolumes = new Dictionary<string, int>
            {
                { MASTER_PARAM, volume },
                { MUSIC_PARAM, volume },
                { SFX_PARAM, volume },
            };
            
            _playButton.onClick.AddListener(Handle_PlayButtonClicked);
            _quitButton.onClick.AddListener(Handle_QuitButtonClicked);
            
            // Sound Setting Listeners
            _masterIncrease.onClick.AddListener(Handle_MasterVolumeIncrease);
            _masterDecrease.onClick.AddListener(Handle_MasterVolumeDecrease);
            _musicIncrease.onClick.AddListener(Handle_MusicVolumeIncrease);
            _musicDecrease.onClick.AddListener(Handle_MusicVolumeDecrease);
            _sfxIncrease.onClick.AddListener(Handle_SFXVolumeIncrease);
            _sfxDecrease.onClick.AddListener(Handle_SFXVolumeDecrease);

            _buttonClickedEventArg = new ButtonClickedEventArg();
            _isActive = true;
        }

        private void Start()
        {
            _currentSelectedItem = _playButton.gameObject;
            _playButton.Select();
            
            LoadOptions();
            
            UpdateVolumesAndSliders();

            _sfxBar.onValueChanged.AddListener((e) => Handle_SFXSlider());
            _musicBar.onValueChanged.AddListener((e) =>  Handle_MusicSlider());
            _masterBar.onValueChanged.AddListener((e) =>  Handle_MasterSlider());
        }

        private void UpdateVolumesAndSliders()
        {
            SetVolume(MASTER_PARAM, _channelVolumes[MASTER_PARAM] *_caculatedVolumeStep);
            SetBarFill(MASTER_PARAM, _channelVolumes[MASTER_PARAM] *_caculatedVolumeStep);
            SetVolume(MUSIC_PARAM, _channelVolumes[MUSIC_PARAM] *_caculatedVolumeStep);
            SetBarFill(MUSIC_PARAM, _channelVolumes[MUSIC_PARAM] *_caculatedVolumeStep);
            SetVolume(SFX_PARAM, _channelVolumes[SFX_PARAM] *_caculatedVolumeStep);
            SetBarFill(SFX_PARAM, _channelVolumes[SFX_PARAM] *_caculatedVolumeStep);
        }

        private void Handle_MasterSlider()
        {
            _channelVolumes[MASTER_PARAM] = (int)SliderUpdateAfterValueChange(_masterBar);
            var volume = _channelVolumes[MASTER_PARAM] * _caculatedVolumeStep;
            SetVolume(MASTER_PARAM, volume);
            SetBarFill(MASTER_PARAM, volume);
        }

        private void Handle_MusicSlider()
        {
            _channelVolumes[MUSIC_PARAM] = (int)SliderUpdateAfterValueChange(_musicBar);
            var volume = _channelVolumes[MUSIC_PARAM] * _caculatedVolumeStep;
            SetVolume(MUSIC_PARAM, volume);
            SetBarFill(MUSIC_PARAM, volume);
        }

        private void Handle_SFXSlider()
        {
            _channelVolumes[SFX_PARAM] = (int)SliderUpdateAfterValueChange(_sfxBar);
            var volume = _channelVolumes[SFX_PARAM] * _caculatedVolumeStep;
            SetVolume(SFX_PARAM, volume);
            SetBarFill(SFX_PARAM, volume);
        }

        public float SliderUpdateAfterValueChange(Slider slider)
        {
            float floorValue = Mathf.Floor(10f * slider.value);
            return floorValue;
        }

        private void Update()
        {
            if (!_isActive) return;
            
            if (_currentSelectedItem != _eventSystem.currentSelectedGameObject)
            {
                if (_eventSystem.currentSelectedGameObject != null)
                {
                    _currentSelectedItem = _eventSystem.currentSelectedGameObject;
                    OnNewItemSelected?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _currentSelectedItem.GetComponent<Selectable>().Select();
                }
            }
        }

        private void Handle_PlayButtonClicked()
        {
            _buttonClickedEventArg.PressedButton = _playButton;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
            SaveSoundSettings();
            SceneManager.LoadScene("Level_01");
        }

        private void Handle_QuitButtonClicked()
        {
            _buttonClickedEventArg.PressedButton = _quitButton;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
            SaveSoundSettings();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }

        public void LoadOptions()
        {
            if (PlayerPrefs.HasKey(SIRITGUIDE_PREFS))
            {
                var step = PlayerPrefs.GetInt(VOLUME_STEP);
                var masterVol = PlayerPrefs.GetInt(MASTER_PARAM);
                var musicVol = PlayerPrefs.GetInt(MUSIC_PARAM);
                var sfxVol = PlayerPrefs.GetInt(SFX_PARAM);

                _channelVolumes[MASTER_PARAM] = masterVol;
                _channelVolumes[MUSIC_PARAM] = musicVol; 
                _channelVolumes[SFX_PARAM] = sfxVol;
                _volumeSteps = step;
                
                _caculatedVolumeStep = 1f / _volumeSteps;
            }
        }
        
        #region Sound Handlers
        private void Handle_SFXVolumeDecrease()
        {
            if (_channelVolumes[SFX_PARAM] <= 0) return;
            
            _channelVolumes[SFX_PARAM]--;
            var volume = _channelVolumes[SFX_PARAM] * _caculatedVolumeStep;
            SetVolume(SFX_PARAM, volume);
            SetBarFill(SFX_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _sfxDecrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void Handle_SFXVolumeIncrease()
        {
            if (_channelVolumes[SFX_PARAM] >= _volumeSteps) return;
            
            _channelVolumes[SFX_PARAM]++;
            var volume = _channelVolumes[SFX_PARAM] * _caculatedVolumeStep;
            SetVolume(SFX_PARAM, volume);
            SetBarFill(SFX_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _sfxIncrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void Handle_MusicVolumeDecrease()
        {
            if (_channelVolumes[MUSIC_PARAM] <= 0) return;
            
            _channelVolumes[MUSIC_PARAM]--;
            var volume = _channelVolumes[MUSIC_PARAM] * _caculatedVolumeStep;
            SetVolume(MUSIC_PARAM, volume);
            SetBarFill(MUSIC_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _musicDecrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void Handle_MusicVolumeIncrease()
        {
            if (_channelVolumes[MUSIC_PARAM] >= _volumeSteps) return;
            
            _channelVolumes[MUSIC_PARAM]++;
            var volume = _channelVolumes[MUSIC_PARAM] * _caculatedVolumeStep;
            SetVolume(MUSIC_PARAM, volume);
            SetBarFill(MUSIC_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _musicIncrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void Handle_MasterVolumeDecrease()
        {
            if (_channelVolumes[MASTER_PARAM] <= 0) return;
            
            _channelVolumes[MASTER_PARAM]--;
            var volume = _channelVolumes[MASTER_PARAM] * _caculatedVolumeStep;
            SetVolume(MASTER_PARAM, volume);
            SetBarFill(MASTER_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _masterDecrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void Handle_MasterVolumeIncrease()
        {
            if (_channelVolumes[MASTER_PARAM] >= _volumeSteps) return;
            
            _channelVolumes[MASTER_PARAM]++;
            var volume = _channelVolumes[MASTER_PARAM] * _caculatedVolumeStep;
            SetVolume(MASTER_PARAM, volume);
            SetBarFill(MASTER_PARAM, volume);
            
            _buttonClickedEventArg.PressedButton = _masterIncrease;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
        }

        private void SetVolume(string MixerParam, float volumeLevel)
        {
            var volume = Mathf.Clamp(volumeLevel, 0.0001f, 1f);
            _mixer.SetFloat(MixerParam, Mathf.Log10(volume) * 20f);
        }

        public void UpdateVolumeBars()
        {
            SetBarFill(SFX_PARAM, _channelVolumes[SFX_PARAM] * _caculatedVolumeStep );
            SetBarFill(MUSIC_PARAM, _channelVolumes[MUSIC_PARAM] * _caculatedVolumeStep);
            SetBarFill(MASTER_PARAM, _channelVolumes[MASTER_PARAM] * _caculatedVolumeStep);
        }

        public void UpdateMixerVolumes()
        {
            var volume = _channelVolumes[MASTER_PARAM] * _caculatedVolumeStep;
            SetVolume(MASTER_PARAM, volume);
            
            volume = _channelVolumes[MUSIC_PARAM] * _caculatedVolumeStep;
            SetVolume(MUSIC_PARAM, volume);
            
            volume = _channelVolumes[SFX_PARAM] * _caculatedVolumeStep;
            SetVolume(SFX_PARAM, volume);
        }
        
        private void SetBarFill(string MixerParam, float volume)
        {
            switch (MixerParam)
            {
                case SFX_PARAM:
                    _sfxBar.value = volume;
                    break;
                case MUSIC_PARAM:
                    _musicBar.value = volume;
                    break;
                case MASTER_PARAM:
                    _masterBar.value = volume;
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void SaveSoundSettings()
        {
            PlayerPrefs.SetString(SIRITGUIDE_PREFS, "V1");
            PlayerPrefs.SetInt(MASTER_PARAM, _channelVolumes[MASTER_PARAM]);
            PlayerPrefs.SetInt(MUSIC_PARAM, _channelVolumes[MUSIC_PARAM]);
            PlayerPrefs.SetInt(SFX_PARAM, _channelVolumes[SFX_PARAM]);
            PlayerPrefs.SetInt(VOLUME_STEP, _volumeSteps);
            
            PlayerPrefs.Save();
        }
    }
}