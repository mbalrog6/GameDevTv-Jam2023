using UnityEngine;

namespace MB6
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private InputManager _inputManager;

        private SpiritInputs _spiritInputs;
        private float _energy;

        private void Awake()
        {
            _energy = 100f;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            _spiritInputs = _inputManager.GetSpiritInputs();
            _playerController.SetInputs(ref _spiritInputs);
        }
    }
}
