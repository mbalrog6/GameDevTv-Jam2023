using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MB6
{
    public class BadNPCVisuals : MonoBehaviour
    {
        private NPCController _npc;
        [SerializeField] private BadNPC _badNPC;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        private float blinkTimer;
        private float blinkTimerTarget;
        
        private void Start()
        {
            _npc = _badNPC.GetNPCController();
            _npc.OnChangedFacing += HandleChangeFacing;
            _animator.SetFloat("Speed", .2f);
            _badNPC.OnAttack += HandleAttack;
            _badNPC.OnDied += HandleOnDied;
        }

        private void HandleAttack(object sender, EventArgs e)
        {
            _animator.SetTrigger("Throw");
        }

        private void HandleOnDied(object sender, EventArgs e)
        {
            _animator.SetTrigger("Dead");
        }

        private void Update()
        {
            if (_badNPC.IsDead) return;
            _animator.SetFloat("Speed", _npc.NormalizedSpeed);
            Blink();
        }

        private void HandleChangeFacing(object sender, EventArgs e)
        {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }

        private void OnDisable()
        {
            _npc.OnChangedFacing -= HandleChangeFacing;
        }
        
        private void Blink()
        {
            if (_npc.NormalizedSpeed != 0f) return;
            
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkTimerTarget)
            {
                blinkTimer = 0f;
                blinkTimerTarget = Random.Range(1f, 4f);
                _animator.SetTrigger("Blink");
            }
        }
        
    }
}