
using System;
using UnityEngine;

namespace MB6
{
    public class ItemEnergySource : MonoBehaviour, IProvideEnergy
    {
        [SerializeField] private EnergyType _energyType;
        [SerializeField] private float _activeDistance;
        [SerializeField] private float _maxStrength;
        [SerializeField] private float _minStrength;
        
        public bool BeingDrained { get; private set; }
        
        public event EventHandler<EventArgs> OnBeingDrained;
        public event EventHandler<EventArgs> OnBeingDrainedEnded;
        
        private float _activeDistanceSqr;
        private bool _wasDrainedThisFrame;

        private void Awake()
        {
            _activeDistanceSqr = GetActiveDistanceSqr();
            _wasDrainedThisFrame = false;
        }

        private void LateUpdate()
        {
            if (BeingDrained == false && _wasDrainedThisFrame)
            {
                BeingDrained = true;
                OnBeingDrained?.Invoke(this, EventArgs.Empty);
                Debug.Log("Being Drained");
            }

            if (_wasDrainedThisFrame == false && BeingDrained)
            {
                BeingDrained = false;
                OnBeingDrainedEnded?.Invoke(this, EventArgs.Empty);
                Debug.Log("Drain Ended");
            }

            _wasDrainedThisFrame = false;
        }

        public float GetActiveDistanceSqr() => _activeDistance * _activeDistance;

        [ContextMenu("UpdateActictiveDistanceSqr")]
        public void UpdateActiveDistanceSqr()
        {
            _activeDistanceSqr = GetActiveDistanceSqr();
        }

        public EnergyType EnergyForm => _energyType;

        public float GetEnergy(Vector3 receiversPosition)
        {
            float power = 0f;
            var calculatedVector = transform.position - receiversPosition;
            if (calculatedVector.sqrMagnitude <= _activeDistanceSqr)
            {
                var normalizedPower = Mathf.Clamp01(_activeDistanceSqr / calculatedVector.sqrMagnitude);
                power = Mathf.Lerp(_minStrength, _maxStrength, normalizedPower);
                _wasDrainedThisFrame = true;
            }

            return power * Time.deltaTime;
        }
    }
}