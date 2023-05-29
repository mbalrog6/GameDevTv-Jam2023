using System;
using UnityEngine;

namespace MB6
{
    public class PlayerSounds : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private float _hurtTimerCoolDown;
        private bool _isMinorPowerOn;
        private float _hurtTimeStamp;

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        private void Start()
        {
            _player.OnMinorPower += Handle_MinorPower;
            _player.OnManifestPower += Handle_ManifestPower;
            _player.OnHeal += Handle_Heal;
            _player.OnDied += Handle_OnDied;
            _player.OnTakeDamage += Handle_TakingDamage;
            _player.OnManifestPowerEnded += HandleManifestPowerEnded;
        }

        private void Update()
        {
           
            SoundManager.Instance.PlayWalkingSound(_player.IsMoving);
            
        }

        private void HandleManifestPowerEnded(object sender, EventArgs e)
        {
            
        }

        private void Handle_TakingDamage(object sender, EventArgs e)
        {
            if (Time.time >= _hurtTimeStamp)
            {
                _hurtTimeStamp = Time.time + _hurtTimerCoolDown;
                SoundManager.Instance.PlayHurtSounds();
            }
        }

        private void Handle_OnDied(object sender, EventArgs e)
        {
            SoundManager.Instance.PlayDyingScream();
        }

        private void Handle_Heal(object sender, EventArgs e)
        {
            
        }

        private void Handle_ManifestPower(object sender, EventArgs e)
        {
            
        }

        private void Handle_MinorPower(object sender, MinorPowerEventArg e)
        {
            if (_player._isMinorPowerOn && !_isMinorPowerOn)
            {
                if (_player.PlayerEnergyType == EnergyType.Light)
                {
                    SoundManager.Instance.PlayLightMinorPower();
                }
                else
                {
                    SoundManager.Instance.PlayDarkMinorPower();
                }
            }

            _isMinorPowerOn = !_isMinorPowerOn;
        }
    }
}