using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MB6
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private InputManager _inputManager;
        
        [SerializeField] private float FloatingEnergyDrain;
        [SerializeField] private float LevitatingEnergyDrain;
        
        [SerializeField] private float MaxEnergyCapacity;
        [SerializeField] private float IdolEnergyGeneration;

        [SerializeField] private float AttractionPower;
        [SerializeField] private float MinorPowerCost;

        [SerializeField] private LayerMask _energyLayer;
        [SerializeField] private LayerMask _MinorPower;
        public bool IsLookingRight { get; private set; }
        public event EventHandler<MinorPowerEventArg> OnMinorPower;
        public event EventHandler<MinorPowerEventArg> OnMinorPowerTrackedObjectsChanged;
        public event EventHandler<EventArgs> OnManifestPower;
        public event EventHandler<EventArgs> OnManifestPowerEnded;

        public bool IsMoving =>
            _spiritInputs.MoveVertical.sqrMagnitude > 0 || _spiritInputs.MoveHorizontal.sqrMagnitude > 0;

        public EnergyType PlayerEnergyType => _energy.GetEnergyType;
        public float PlayerEnergyLevel => _energy.GetEnergy;
        
        private SpiritInputs _spiritInputs;
        private Energy _energy;

        public float energy;
        public EnergyType _allowedEnergy;
        public bool IsGrounded;

        private Collider[] _castColliders;
        private float _energyDetectionRange;

        private SpriteMask[] _manifestMask;
        private float _currentAlphaCutOff;
        private float _targetAlphaCutOff;
        private RotateTransform _rotateTransform;
        
        //used to see if Manifest Power just started
        private bool _isManifesting;
        [SerializeField] private float _timeToOpenPortal;
        private float _portalTimer;

        private bool _isMinorPowerOn;
        private int _lastNumberOfTrackedObjects;

        private List<Transform> _trackedObjectsForMinorPower;
        private List<IAttractable> _attractables;

        private CapsuleCollider _capsuleCollider;

        private void Awake()
        {
            _trackedObjectsForMinorPower = new List<Transform>(5);
            _attractables = new List<IAttractable>(5);
            
            _castColliders = new Collider[10];
            _energyDetectionRange = 10f;
            _energy = new Energy(100f, 110f);

            _manifestMask = GetComponentsInChildren<SpriteMask>();
            _rotateTransform = GetComponentInChildren<RotateTransform>();
            _targetAlphaCutOff = 0.01f;
            _currentAlphaCutOff = 1f;

            IsLookingRight = true;
            _capsuleCollider = GetComponent<CapsuleCollider>();
            
        }

        private void Update()
        {
            _spiritInputs = _inputManager.GetSpiritInputs();
            
            HandleInput();
            LookingRight();
            HandleEnergyGeneration();
            HandleMinorPower();
            HandleManifesting();
            
            _allowedEnergy = _energy.GetEnergyType;
            energy = _energy.GetEnergy;
        }

        private void HandleManifesting()
        {
            if (_isManifesting == false && _spiritInputs.Manifesting == true)
            {
                OnManifestPower?.Invoke(this, EventArgs.Empty);
                _rotateTransform.IsRotating = true;
                _portalTimer = 0f;
            }

            if (_isManifesting == true && _spiritInputs.Manifesting == false)
            {
                OnManifestPowerEnded?.Invoke(this, EventArgs.Empty);
                _portalTimer = (1 - Mathf.Clamp01(_currentAlphaCutOff / _targetAlphaCutOff)) * _timeToOpenPortal;
            }

            if (_spiritInputs.Manifesting && _currentAlphaCutOff > _targetAlphaCutOff)
            {
                _isManifesting = true;
                _portalTimer += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(_portalTimer / _timeToOpenPortal);
                _currentAlphaCutOff = Mathf.Lerp(_currentAlphaCutOff, _targetAlphaCutOff, 1f - Mathf.Pow(1f - normalizedTime, 5f));

                for (int i = 0; i < _manifestMask.Length; i++)
                {
                    _manifestMask[i].alphaCutoff = _currentAlphaCutOff;
                }

                return;
            }

            if (!_spiritInputs.Manifesting && _currentAlphaCutOff < 1f)
            {
                _isManifesting = false;
                
                _portalTimer += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(_portalTimer / _timeToOpenPortal);
                
                _currentAlphaCutOff = Mathf.Lerp(_currentAlphaCutOff, 1f,  1f - Mathf.Pow(1f - normalizedTime, 4f));

                for (int i = 0; i < _manifestMask.Length; i++)
                {
                    _manifestMask[i].alphaCutoff = _currentAlphaCutOff;
                }

                if (_currentAlphaCutOff >= .95f)
                {
                    _rotateTransform.IsRotating = false;
                }
                
            }
        }

        private void HandleMinorPower()
        {
            // if this is button click to turn off the minor power.
            if (_isMinorPowerOn == true && !_spiritInputs.MinorPowerActive)
            {
                OnMinorPower?.Invoke(this, new MinorPowerEventArg(_trackedObjectsForMinorPower));
                _trackedObjectsForMinorPower.Clear(); 
                _isMinorPowerOn = false;
                
                foreach (var obj in _attractables)
                {
                    obj.BeenReleased();
                }
                
            }
            
            if (!_spiritInputs.MinorPowerActive) return;

            if (_energy.GetEnergy < MinorPowerCost)
            {
                _inputManager.SetMinorPower(false);
                return;
            }

            _energy.RemoveEnergy(MinorPowerCost * Time.deltaTime);
            
            int results = Physics.OverlapSphereNonAlloc(transform.position, _energyDetectionRange, _castColliders, _MinorPower);
            
            // store if this is the activation click for the minor power
            var isActivationClick = _isMinorPowerOn == false && _spiritInputs.MinorPowerActive;

            foreach (var obj in _attractables)
            {
                obj.BeenReleased();
            }
            
            _trackedObjectsForMinorPower.Clear();
            _attractables.Clear();

            for (int i = 0; i < results; i++)
            {
                IAttractable attractableObject = _castColliders[i].gameObject.GetComponent<IAttractable>();
                if (attractableObject != null)
                {
                    attractableObject.AttractTowards(transform.position, AttractionPower);
                    _trackedObjectsForMinorPower.Add(_castColliders[i].transform);
                    _attractables.Add(attractableObject);
                }
            }

            if (_trackedObjectsForMinorPower.Count != _lastNumberOfTrackedObjects)
            {
                _lastNumberOfTrackedObjects = _trackedObjectsForMinorPower.Count;
                // send event with new list of objects.
                if (!isActivationClick)
                {
                    OnMinorPowerTrackedObjectsChanged?.Invoke(this, new MinorPowerEventArg(_trackedObjectsForMinorPower));
                }
            }
            
            if (isActivationClick)
            {
                OnMinorPower?.Invoke(this, new MinorPowerEventArg(_trackedObjectsForMinorPower));
                _isMinorPowerOn = true;
            }
        }

        private void HandleEnergyGeneration()
        {
            if (IsGrounded)
            {
                _energy.AddEnergy( _energy.GetEnergyType, IdolEnergyGeneration * Time.deltaTime);
            }
            
            AddNearByEnergySources();
        }

        private void AddNearByEnergySources()
        {
            int results = Physics.OverlapSphereNonAlloc(transform.position, _energyDetectionRange, _castColliders, _energyLayer);
            
            for (int i = 0; i < results; i++)
            {
                IProvideEnergy energySource = _castColliders[i].gameObject.GetComponent<IProvideEnergy>();
                if (energySource != null)
                {
                    _energy.AddEnergy(energySource.EnergyForm, energySource.GetEnergy(transform.position));
                }
            }
        }

        private void HandleInput()
        {
            IsGrounded = _playerController.IsGrounded;
            if (_spiritInputs.IsStableFloating && !_playerController.IsGrounded)
            {
                if (_energy.GetEnergy > 0f)
                {
                    _energy.RemoveEnergy(FloatingEnergyDrain * Time.deltaTime);
                }
                else
                {
                    _inputManager.SetStableFloating(false);
                    _spiritInputs.IsStableFloating = false;
                }
            }

            if (_spiritInputs.MoveVertical.sqrMagnitude > 0)
            {
                if (_energy.GetEnergy > 0f)
                {
                    _energy.RemoveEnergy( LevitatingEnergyDrain * Time.deltaTime);
                }
                else
                {
                    _spiritInputs.MoveVertical = Vector3.zero;
                }
            }

            if (!_playerController.IsGrounded && _spiritInputs.IsStableFloating &&
                _spiritInputs.MoveHorizontal.sqrMagnitude > 0)
            {
                _energy.RemoveEnergy(LevitatingEnergyDrain * Time.deltaTime);
            }
            
            _playerController.SetInputs(ref _spiritInputs);
        }

        private void LookingRight()
        {
            if (_spiritInputs.MoveHorizontal.x > 0)
            {
                IsLookingRight = true;
            }
            else if(_spiritInputs.MoveHorizontal.x < 0)
            {
                IsLookingRight = false;
            }
        }
    }
    
}
