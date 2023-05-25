using System;
using UnityEngine;

namespace MB6
{
    public class GroundObject : MonoBehaviour, IChangeDimension
    {
        [SerializeField] private Player _player;
        private Collider[] _colliders;
        private bool _isOrginalDimension;
        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>(true);
            _isOrginalDimension = true;
        }

        private void OnEnable()
        {
            _player.OnManifestPower += HandleSwapDimension;
            _player.OnManifestPowerEnded += HandleSwapDimension;
        }

        private void OnDisable()
        {
            _player.OnManifestPower -= HandleSwapDimension;
            _player.OnManifestPowerEnded -= HandleSwapDimension;
        }

        private void HandleSwapDimension(object sender, EventArgs e)
        {
            SwapDimension();
        }

        public void SwapDimension()
        {
            if (_isOrginalDimension)
            {
                _isOrginalDimension = !_isOrginalDimension;
                _colliders[1].enabled = true;
            }
            else
            {
                _isOrginalDimension = !_isOrginalDimension;
                _colliders[1].enabled = false;
            }
        }
        
        
    }
}