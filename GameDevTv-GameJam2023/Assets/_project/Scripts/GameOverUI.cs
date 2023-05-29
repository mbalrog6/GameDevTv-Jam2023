using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MB6
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _quitButton;
        
        private GameObject _currentSelectedItem;
        private ButtonClickedEventArg _buttonClickedEventArg;

        public event EventHandler<EventArgs> OnNewItemSelected;
        public event EventHandler<ButtonClickedEventArg> OnButtonClicked;

        private bool _isActive;
        private bool _isInitialized;
        
        private void Awake()
        {
            
            _mainMenuButton.onClick.AddListener(Handle_MainMenuButtonClicked);
            _quitButton.onClick.AddListener(Handle_QuitButtonClicked);
            
            _buttonClickedEventArg = new ButtonClickedEventArg();
            _isActive = true;
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

        private void Handle_QuitButtonClicked()
        {
            _buttonClickedEventArg.PressedButton = _quitButton;
            OnButtonClicked?.Invoke(this, _buttonClickedEventArg);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }

        private void Handle_MainMenuButtonClicked()
        {
            SceneManager.LoadScene("Main Menu");
        }

        private void Start()
        {
            _currentSelectedItem = _mainMenuButton.gameObject;
            _mainMenuButton.Select();
        }
    }
}