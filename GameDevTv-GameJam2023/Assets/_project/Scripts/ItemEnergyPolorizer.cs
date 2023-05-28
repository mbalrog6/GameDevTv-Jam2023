using System;
using UnityEngine;

namespace MB6
{
    public class ItemEnergyPolorizer : MonoBehaviour, IProvideEnergy
    {
        [SerializeField] private EnergyType _energyType;
        [SerializeField] private float _activeDistance;
        [SerializeField] private float _maxStrength;
        [SerializeField] private float _minStrength;
        [SerializeField] private bool _shouldChangeEnergy;
        [SerializeField] private bool _shouldDrainEnergy;

        public bool BeingDrained { get; private set; }
        
        public event EventHandler<EventArgs> OnBeingDrained;
        public event EventHandler<EventArgs> OnBeingDrainedEnded;
        
        private float _activeDistanceSqr;
        private bool _wasDrainedThisFrame;
        private Collider _energyCollider;

        public bool ShouldChangeEnergy
        {
            get
            {
                return _shouldChangeEnergy;
            }
            private set
            {
                _shouldChangeEnergy = value;
            }
        }
        public bool ShouldDrainEnergy
        {
            get
            {
                return _shouldDrainEnergy;
            }
            private set
            {
                _shouldChangeEnergy = value;
            }
        }
        public EnergyType EnergyForm => _energyType;
        
        private void Awake()
        {
            _energyCollider = GetComponent<Collider>();
            _activeDistanceSqr = GetActiveDistanceSqr();
            _wasDrainedThisFrame = false;
        }

        private void LateUpdate()
        {
            if (BeingDrained == false && _wasDrainedThisFrame)
            {
                BeingDrained = true;
                OnBeingDrained?.Invoke(this, EventArgs.Empty);
            }

            if (_wasDrainedThisFrame == false && BeingDrained)
            {
                BeingDrained = false;
                OnBeingDrainedEnded?.Invoke(this, EventArgs.Empty);
            }

            _wasDrainedThisFrame = false;
        }

        public float GetActiveDistanceSqr() => _activeDistance * _activeDistance;

        [ContextMenu("UpdateActictiveDistanceSqr")]
        public void UpdateActiveDistanceSqr()
        {
            _activeDistanceSqr = GetActiveDistanceSqr();
        }
        public float GetEnergy(Vector3 receiversPosition)
        {
            float power = 0f;
            var calculatedVector = transform.position - receiversPosition;
            if (calculatedVector.sqrMagnitude <= _activeDistanceSqr)
            {
                power = 1f; 
                _wasDrainedThisFrame = true;
            }

            return power;
        }

        public void DisableEnergyCollider(bool value)
        {
            if (value)
            {
                _energyCollider.enabled = true;
            }
            else
            {
                _energyCollider.enabled = false;
            }
        }
    }
}