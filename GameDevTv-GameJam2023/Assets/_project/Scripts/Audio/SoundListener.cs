using System;
using UnityEngine;

namespace MB6
{
    public class SoundListener : MonoBehaviour
    {
        [SerializeField] private MainMenuUI _mainMenu;

        private void Start()
        {
            if (_mainMenu != null)
            {
                _mainMenu.OnNewItemSelected += Handle_NewItemSelected;
                _mainMenu.OnButtonClicked += Handle_ButtonClicked;
            }
        }

        private void Handle_ButtonClicked(object sender, ButtonClickedEventArg e)
        {
            SoundManager.Instance.PlayButtonClicked();
        }

        private void Handle_NewItemSelected(object sender, EventArgs e)
        {
            SoundManager.Instance.PlayButtonSelected();
        }
    }
}