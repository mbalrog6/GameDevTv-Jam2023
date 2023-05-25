
using UnityEngine;

namespace MB6
{
    public class ItemEnergySource : MonoBehaviour, IProvideEnergy
    {
        [SerializeField] private EnergyType _energyType;
        [SerializeField] private float _activeDistance;
        [SerializeField] private float _maxStrength;
        [SerializeField] private float _minStrength;
        private float _activeDistanceSqr;

        private void Awake()
        {
            _activeDistanceSqr = GetActiveDistanceSqr();
        }

        [ContextMenu("UpdateActictiveDistanceSqr")]
        public float GetActiveDistanceSqr() => _activeDistance * _activeDistance;

        public EnergyType EnergyForm => _energyType;

        public float GetEnergy(Vector3 receiversPosition)
        {
            float power = 0f;
            var calculatedVector = transform.position - receiversPosition;
            if (calculatedVector.sqrMagnitude <= _activeDistanceSqr)
            {
                var normalizedPower = Mathf.Clamp01(_activeDistanceSqr / calculatedVector.sqrMagnitude);
                power = Mathf.Lerp(_minStrength, _maxStrength, normalizedPower);
            }

            return power * Time.deltaTime;
        }
    }
}