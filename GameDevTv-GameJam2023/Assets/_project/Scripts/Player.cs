using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MB6
{
    public class Player : MonoBehaviour, IHealth
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
        [SerializeField] private LayerMask _MinorPowerL;
        [SerializeField] private LayerMask _MinorPowerD;

        [SerializeField] private int _goodAuraDamage;
        public bool IsLookingRight { get; private set; }
        public bool IsDead { get; private set; }
        
        public int Health { get; private set; }
        [SerializeField] private int _maxHealth;
        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }

            set
            {
                _maxHealth = value;
            }
        }

        public float NormalizedHealth => Mathf.Clamp01((float)Health / MaxHealth);

        private float _auraDrainTickCount;
        [SerializeField] private float _auraDrainThreshold;

        public event EventHandler<EventArgs> OnTakeDamage;
        public event EventHandler<EventArgs> OnHeal;
        public event EventHandler<EventArgs> OnDied;
        public event EventHandler<MinorPowerEventArg> OnMinorPower;
        public event EventHandler<MinorPowerEventArg> OnMinorPowerTrackedObjectsChanged;
        public event EventHandler<EventArgs> OnManifestPower;
        public event EventHandler<EventArgs> OnManifestPowerEnded;

        public bool IsMoving =>
            _spiritInputs.MoveVertical.sqrMagnitude > 0 || _spiritInputs.MoveHorizontal.sqrMagnitude > 0;

        public EnergyType PlayerEnergyType => _energy.GetEnergyType;
        public float PlayerEnergyLevel => _energy.GetEnergy;

        public bool IsManifesting { get; private set; }
        
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

        public bool _isMinorPowerOn;
        private bool _lastMinorPowerOn;
        private int _lastNumberOfTrackedObjects;

        private List<Transform> _trackedObjectsForMinorPower;
        private List<IAttractable> _attractables;

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

            Health = 50;
            _auraDrainTickCount = 0f;
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
                IsManifesting = true;
            }

            if (_isManifesting == true && _spiritInputs.Manifesting == false)
            {
                OnManifestPowerEnded?.Invoke(this, EventArgs.Empty);
                _portalTimer = (1 - Mathf.Clamp01(_currentAlphaCutOff / _targetAlphaCutOff)) * _timeToOpenPortal;
                IsManifesting = false;
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
            #region Handles if MinorPower was Activated and Has the right amount of energy...

            // Make sure the button press is not captured if EnergyType is Either.
            if (_energy.GetEnergyType == EnergyType.Either && _spiritInputs.MinorPowerActive)
            {
                _spiritInputs.MinorPowerActive = false;
                _inputManager.SetMinorPower(false);
            }
            
            if (_spiritInputs.MinorPowerActive && _energy.GetEnergy >= MinorPowerCost)
            {
                _isMinorPowerOn = true;
            }
            else
            {
                _isMinorPowerOn = false;
            }
            
            if (_energy.GetEnergy < MinorPowerCost || _energy.GetEnergyType == EnergyType.Either)
            {
                _isMinorPowerOn = false;
                _spiritInputs.MinorPowerActive = false;
                _inputManager.SetMinorPower(false);
            }
            
            var buttonPressedThisFrame = _lastMinorPowerOn != _isMinorPowerOn;

            if (buttonPressedThisFrame)
            {
                Debug.Log("Button Pressed this Frame");
            }
            
            _lastMinorPowerOn = _isMinorPowerOn;
            #endregion

            // the power turned off in this frame. 
            if (buttonPressedThisFrame && _isMinorPowerOn == false)
            {
                OnMinorPower?.Invoke(this, new MinorPowerEventArg(_trackedObjectsForMinorPower));
                _attractables.Clear();
                _trackedObjectsForMinorPower.Clear();
            }
            
            #region Determine the direction of the Attraction Force...
            float attractionPower;
            switch (_energy.GetEnergyType)
            {
                case EnergyType.Dark:
                    attractionPower = -AttractionPower;
                    break;
                case EnergyType.Light:
                    attractionPower = AttractionPower;
                    break;
                default:
                    attractionPower = 0f;
                    break;
            }
            #endregion

            if (!_isMinorPowerOn) return;
            
            // Remove the cost of activating the Minor Power
            _energy.RemoveEnergy(MinorPowerCost * Time.deltaTime);
            
            // Detect the objects in range for MinorPower.
            int results = Physics.OverlapSphereNonAlloc(transform.position, _energyDetectionRange, _castColliders, IsManifesting ? _MinorPowerD : _MinorPowerL);
            
            // Clear previous frames list of attractables. 
            foreach (var obj in _attractables)
            {
                obj.BeenReleased();
            }
            
            _trackedObjectsForMinorPower.Clear();
            _attractables.Clear();
            
            // Run through the list of object detected by Overlapping Sphere.
            for (int i = 0; i < results; i++)
            {
                IAttractable attractableObject = _castColliders[i].gameObject.transform.parent.GetComponent<IAttractable>();
                if (attractableObject != null)
                {
                    attractableObject.AttractTowards(transform.position, attractionPower);
                    _trackedObjectsForMinorPower.Add(_castColliders[i].transform);
                    _attractables.Add(attractableObject);
                }
            }
            
            // number of tracked objects changed fire the Objects changed Event.
            if (_trackedObjectsForMinorPower.Count != _lastNumberOfTrackedObjects)
            {
                _lastNumberOfTrackedObjects = _trackedObjectsForMinorPower.Count;
                if (!buttonPressedThisFrame)
                {
                    OnMinorPowerTrackedObjectsChanged?.Invoke(this, new MinorPowerEventArg(_trackedObjectsForMinorPower));
                }
            }
            
            // button was pressed to activate the Minor Power this frame. 
            if (buttonPressedThisFrame)
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
            float power = 0f;
            
            for (int i = 0; i < results; i++)
            {
                IProvideEnergy energySource = _castColliders[i].gameObject.GetComponent<IProvideEnergy>();
                if (energySource != null)
                {
                    if (energySource.ShouldChangeEnergy)
                    {
                        // GetEnergy for objects that ShouldChangeEnergy return 1 if in range and 0 if out of range
                        if (energySource.GetEnergy(transform.position) > 0f)
                        {
                            if (energySource.EnergyForm == EnergyType.Either && _isMinorPowerOn)
                            {
                                foreach (var obj in _attractables)
                                {
                                    obj.BeenReleased();
                                }

                                if (PlayerEnergyType == EnergyType.Light)
                                {
                                    OnMinorPower?.Invoke(this, new MinorPowerEventArg(new List<Transform>()));
                                }
                                
                                _isMinorPowerOn = false;
                                _spiritInputs.MinorPowerActive = false;
                                _inputManager.SetMinorPower(false);
                            }
                            
                            _energy.ChangeEnergy(energySource.EnergyForm);
                            return;
                        }
                    }

                    power = energySource.GetEnergy(transform.position);
                    
                    if (energySource.ShouldDrainEnergy)
                    {
                        _energy.RemoveEnergy(power);
                        return;
                    }
                    
                    if (power > 0)
                    {
                        if (_energy.GetEnergyType == EnergyType.Dark && energySource.EnergyForm == EnergyType.Light)
                        {
                            _auraDrainTickCount += Time.deltaTime;
                            if (_auraDrainTickCount >= _auraDrainThreshold)
                            {
                                TakeDamage(_goodAuraDamage);
                                _auraDrainTickCount = 0f;
                            }
                        }

                        if (_energy.GetEnergyType == EnergyType.Light && energySource.EnergyForm == EnergyType.Light)
                        {
                            _auraDrainTickCount += Time.deltaTime;
                            if (_auraDrainTickCount >= _auraDrainThreshold)
                            {
                                Heal(_goodAuraDamage);
                                _auraDrainTickCount = 0f;
                            }
                        }
                    }
                    
                    _energy.AddEnergy(energySource.EnergyForm, power);
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

        #region Health Related Functions...
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;
            
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsDead = true;
                OnDied?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnTakeDamage?.Invoke(this, EventArgs.Empty);
            }
            
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            Health += amount;
            if (Health >= MaxHealth)
            {
                Health = MaxHealth;
            }

            OnHeal?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
    
}
