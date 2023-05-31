using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MB6
{
    public struct SpiritInputs
    {
        public Vector3 MoveHorizontal;
        public Vector3 MoveVertical;
        public bool IsStableFloating;
        public bool MinorPowerActive;
        public bool Manifesting;
    }
    public class InputManager : MonoBehaviour, MainInput.ISpiritPlayerActionsActions
    {
        private MainInput _mainInput;

        public SpiritInputs _spiritInputs;

        private void Awake()
        {
            _mainInput = new MainInput();
            _spiritInputs = new SpiritInputs();
        }

        private void Start()
        {
            _mainInput.SpiritPlayerActions.Escape.performed += OnEscape;
            _mainInput.SpiritPlayerActions.StableFloat.performed += OnStableFloat;
            _mainInput.SpiritPlayerActions.MinorPower.performed += OnMinorPower;

            _mainInput.SpiritPlayerActions.Manifest.performed += OnManifest;
            _mainInput.SpiritPlayerActions.Manifest.canceled += OnManifest;
        }

        private void Update()
        {
            var direction = _mainInput.SpiritPlayerActions.Move.ReadValue<Vector2>();
            
            #if UNITY_WEBGL
            var x = _mainInput.SpiritPlayerActions.MoveRightLeft.ReadValue<float>();
            var y = _mainInput.SpiritPlayerActions.MoveUpDown.ReadValue<float>();
            
            _spiritInputs.MoveHorizontal = new Vector3(x, 0f, 0f);
            _spiritInputs.MoveVertical = new Vector3(0f, y, 0f);
            return;
            #endif

            _spiritInputs.MoveHorizontal = new Vector3(direction.x, 0f, 0f);
            _spiritInputs.MoveVertical = new Vector3(0f, direction.y, 0f);
        }

        private void OnEnable()
        {
            _mainInput.Enable();
        }

        private void OnDisable()
        {
            _mainInput.Disable();
        }

        public void SetStableFloating(bool value)
        {
            _spiritInputs.IsStableFloating = value;
        }

        public void SetMinorPower(bool value) => _spiritInputs.MinorPowerActive = value;

        public void OnEscape(InputAction.CallbackContext context)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif

            #if UNITY_WEBGL
            SceneManager.LoadScene("Main Menu");
            #endif
            
            Application.Quit();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            
        }

        public void OnStableFloat(InputAction.CallbackContext context)
        {
            _spiritInputs.IsStableFloating = !_spiritInputs.IsStableFloating;
        }

        public void OnMinorPower(InputAction.CallbackContext context)
        {
            _spiritInputs.MinorPowerActive = !_spiritInputs.MinorPowerActive;
        }

        public void OnManifest(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _spiritInputs.Manifesting = true;
            }

            if (context.canceled)
            {
                _spiritInputs.Manifesting = false;
            }
        }

        public void OnMoveUpDown(InputAction.CallbackContext context)
        {
            
        }

        public void OnMoveRightLeft(InputAction.CallbackContext context)
        {
            
        }

        public SpiritInputs GetSpiritInputs()
        {
            return _spiritInputs;
        }
    }
}
