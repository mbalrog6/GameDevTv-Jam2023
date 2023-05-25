using System;
using Unity.VisualScripting;
using UnityEngine;

namespace MB6
{
    [RequireComponent(typeof(Rigidbody))]
    public class ObjectEntity : MonoBehaviour, IAttractable, IChangeDimension
    {
        [SerializeField] private float _attactionLimitDistance;
        [SerializeField] private Player _player;
        [SerializeField] private AnimationCurve AttractionCurve;
        
        private Rigidbody _rb;
        private bool _isBeingAttracted;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _player.OnManifestPower += HandleManifestOn;
            _player.OnManifestPowerEnded += HandleManifestOff;
        }

        private void OnDisable()
        {
            _player.OnManifestPower -= HandleManifestOn;
            _player.OnManifestPowerEnded -= HandleManifestOff;
        }

        private void HandleManifestOff(object sender, EventArgs e)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.constraints = RigidbodyConstraints.FreezePositionZ | 
                              RigidbodyConstraints.FreezeRotationX |
                              RigidbodyConstraints.FreezeRotationY;
        }

        private void HandleManifestOn(object sender, EventArgs e)
        {
            if (!_isBeingAttracted)
            {
                _rb.constraints |= RigidbodyConstraints.FreezePosition;
            }
        }

        public void AttractTowards(Vector3 position, float pullStrength)
        {
            _isBeingAttracted = true;
            Vector3 direction = position - transform.position;
            var pull = pullStrength * (AttractionCurve.Evaluate(1 - direction.magnitude / 10f));
            if (direction.sqrMagnitude < _attactionLimitDistance)
            {
                _rb.velocity = Vector3.zero;
            }
            else
            {
                _rb.velocity = direction.normalized * pull;
            }
        }

        public void BeenReleased()
        {
            _isBeingAttracted = false;
        }

        public void SwapDimension()
        {
            
        }
    }
}