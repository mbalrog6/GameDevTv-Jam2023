using UnityEngine;

namespace MB6
{
    public class EnergyDoor : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private int _passiveDamage;
        [SerializeField] private float _damageThresholdTime;
        [SerializeField] private EnergyType DoorEnergyType;
        [SerializeField] private float _radius;
        [SerializeField] private Transform _center;

        private Collider _barrierCollider;
        private float _damageTimer;
        private float _radiusSquared;

        private void Awake()
        {
            _barrierCollider = GetComponent<Collider>();
            _radiusSquared = _radius * _radius;
        }

        private void FixedUpdate()
        {
            _radiusSquared = _radius * _radius;
            if (CheckPlayerDistance())
            {
                RadiusWasEntered();
            }
        }

        private bool CheckPlayerDistance() =>
            (_player.transform.position - _center.position).sqrMagnitude < _radiusSquared;

        private void RadiusWasEntered()
        {
            if (_player.PlayerEnergyType == EnergyType.Either)
            {
                _barrierCollider.enabled = true;
                return;
            }

            if (_player.PlayerEnergyType == DoorEnergyType)
            {
                _barrierCollider.enabled = false;
                return;
            }

            _damageTimer += Time.fixedDeltaTime;
            if (_damageTimer > _damageThresholdTime)
            {
                _damageTimer = 0f;
                _player.TakeDamage(_passiveDamage);
            }

            _barrierCollider.enabled = true;
        }
        
    }
}