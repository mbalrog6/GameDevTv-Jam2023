using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MB6
{
    public class WraitAnimations : MonoBehaviour
    {
        [SerializeField] private Renderer _energyBubbleRenderer;
        [SerializeField] private Renderer _darkMinorPower;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private Image _healthBar;

        private SpriteRenderer _sprite;
        private Animator _animator;
        private Player _player;
        private bool _moving;
        private float blinkTimer;
        private float blinkTimerTarget;

        private bool _isMinorPower;

        private bool _darkPowerAnimating;
        private float _minorTimer;
        [SerializeField] private float _timeToFullDarkMinorPower;
        private MaterialPropertyBlock _darkPropBlock;

        private EnergyBubbleVisual _energyBubbleVisual;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _player = GetComponent<Player>();

            _energyBubbleVisual = new EnergyBubbleVisual(_energyBubbleRenderer, 100f, _gradient);
            _darkPropBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            _player.OnMinorPower += AttackAnimation;
            _player.OnManifestPower += CastAnimation;
            _player.OnTakeDamage += HandleTakeDamage;
            _player.OnHeal += HandleHeal;
            _player.OnMinorPowerTrackedObjectsChanged += HandleOnTrackedObjectsChanged;
            _healthBar.fillAmount = _player.NormalizedHealth;
        }

        private void HandleOnTrackedObjectsChanged(object sender, MinorPowerEventArg e)
        {
            if (_player.PlayerEnergyType == EnergyType.Dark)
            {
                _minorTimer = _timeToFullDarkMinorPower * .9f;
            }
        }

        private void HandleHeal(object sender, EventArgs e)
        {
            _healthBar.fillAmount = _player.NormalizedHealth;
        }

        private void HandleTakeDamage(object sender, EventArgs e)
        {
            _healthBar.fillAmount = _player.NormalizedHealth;
        }

        private void CastAnimation(object sender, EventArgs e)
        {
            _animator.SetTrigger("Cast");
        }

        private void AttackAnimation(object sender, EventArgs e)
        {
            _isMinorPower = !_isMinorPower;
            _animator.SetTrigger("Attack");
            
            if (_player.PlayerEnergyType == EnergyType.Dark && _isMinorPower)
            {
                _darkPowerAnimating = true;
            }
            
        }

        private void Update()
        {
            Blink();
            
            _sprite.flipX = !_player.IsLookingRight;

            if (_player.IsMoving && _moving == false)
            {
                _moving = true;
                _animator.SetBool("Moving", true);
            }

            if (!_player.IsMoving && _moving == true)
            {
                _moving = false;
                _animator.SetBool("Moving", false);
            }
            
            _energyBubbleVisual.SetEnergyVisual(_player.PlayerEnergyType, _player.PlayerEnergyLevel);
            
            if (_darkPowerAnimating)
            {
                _darkMinorPower.enabled = true;
                HandleDarkMinorPowerEffect();
            }
        }

        private void Blink()
        {
            if (_moving) return;
            
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkTimerTarget)
            {
                blinkTimer = 0f;
                blinkTimerTarget = Random.Range(1f, 2f);
                _animator.SetTrigger("Blink");
            }
        }

        private void HandleDarkMinorPowerEffect()
        {
            if (_isMinorPower)
            {
                _minorTimer += Time.deltaTime;
            }
            else
            {
                _minorTimer -= Time.deltaTime;
            }
            
            if (_minorTimer >= _timeToFullDarkMinorPower)
            {
                _minorTimer = _timeToFullDarkMinorPower;
                return;
            }

            if (_minorTimer <= 0f)
            {
                _darkPowerAnimating = false;
                _darkMinorPower.enabled = false;
                _minorTimer = 0f;
                return;
            }

            var size = _minorTimer / _timeToFullDarkMinorPower;
            _darkMinorPower.GetPropertyBlock(_darkPropBlock);
            
            _darkPropBlock.SetFloat("_Size", size);
            _darkMinorPower.SetPropertyBlock(_darkPropBlock);
        }
    }
}
