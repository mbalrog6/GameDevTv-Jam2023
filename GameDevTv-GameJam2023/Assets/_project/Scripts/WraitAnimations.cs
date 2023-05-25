using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MB6
{
    public class WraitAnimations : MonoBehaviour
    {
        [SerializeField] private Renderer _energyBubbleRenderer;
        [SerializeField] private Gradient _gradient;

        
        private SpriteRenderer _sprite;
        private Animator _animator;
        private Player _player;
        private bool _moving;
        private float blinkTimer;
        private float blinkTimerTarget;

        private bool _isMinorPower;

        private EnergyBubbleVisual _energyBubbleVisual;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _player = GetComponent<Player>();

            _energyBubbleVisual = new EnergyBubbleVisual(_energyBubbleRenderer, 100f, _gradient);
        }

        private void Start()
        {
            _player.OnMinorPower += AttackAnimation;
            _player.OnManifestPower += CastAnimation;
        }

        private void CastAnimation(object sender, EventArgs e)
        {
            _animator.SetTrigger("Cast");
        }

        private void AttackAnimation(object sender, EventArgs e)
        {
            _isMinorPower = !_isMinorPower;
            _animator.SetTrigger("Attack");
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
    }
}
