using System;
using UnityEngine;

namespace MB6
{
    [RequireComponent(typeof(Rigidbody))]
    public class ObjectEntity : MonoBehaviour, IAttractable, IChangeDimension
    {
        [SerializeField] private float _attactionLimitDistance;
        [SerializeField] private Player _player;
        [SerializeField] private AnimationCurve AttractionCurve;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private Rigidbody _rb;
        private bool _isBeingAttracted;
        private Collider[] _colliders;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
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
            float pull;
            if (_player.PlayerEnergyType == EnergyType.Light)
            {
                pull = pullStrength * (AttractionCurve.Evaluate(1 - direction.magnitude / 10f));
            }
            else
            {
               pull =  pullStrength * (AttractionCurve.Evaluate(direction.magnitude / 10f));
            }
            if (direction.sqrMagnitude < _attactionLimitDistance)
            {
                if (_player.PlayerEnergyType == EnergyType.Light)
                {
                    _rb.velocity = Vector3.zero;
                }
            }
            else
            {
                _rb.velocity = direction.normalized * pull;
            }
        }

        public void BeenReleased()
        {
            _isBeingAttracted = false;

            if (_player.PlayerEnergyType == EnergyType.Light)
            {
                if (_player.IsManifesting)
                {
                    SetDimension(false);
                }
                else
                {
                    SetDimension(true);
                }
            }
        }

        public void SwapDimension()
        {
            
        }

        public void SetDimension(bool isOriginalWorld)
        {
            if (isOriginalWorld)
            {
                _colliders[0].enabled = true;
                _colliders[1].enabled = false;
                _spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
            else
            {
                _colliders[0].enabled = false;
                _colliders[1].enabled = true;
                _spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
    }
}