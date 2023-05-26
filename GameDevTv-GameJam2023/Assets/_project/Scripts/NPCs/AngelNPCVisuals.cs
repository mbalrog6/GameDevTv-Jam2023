using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MB6
{
    public class AngelNPCVisuals : MonoBehaviour
    {
        private NPCController _npc;
        [SerializeField] private GoodNPC _goodNPC;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        private float blinkTimer;
        private float blinkTimerTarget;
        
        private void Start()
        {
            _npc = _goodNPC.GetNPCController();
            _npc.OnChangedFacing += HandleChangeFacing;
            _animator.SetFloat("Speed", .2f);
            _goodNPC.OnDied += HandleOnDied;
        }

        private void HandleOnDied(object sender, EventArgs e)
        {
            _animator.SetTrigger("Dead");
        }

        private void Update()
        {
            if (_goodNPC.IsDead) return;
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