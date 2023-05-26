using UnityEngine;

namespace MB6.NPCs.States
{
    public class AttackSpirtNPCState : INPCState
    {
        private Player _player;
        private NPCController _npcController;
        private Transform _npcTransform;
        private float distanceToSpirit;
        public bool IsAttacking { get; private set; }
        private bool _hasAttacked;
        private float _attackTimer;
        private float _attackCooldown;
        private float _distanceToAttackFrom;

        public AttackSpirtNPCState(NPCController npcController, Player player, Transform npcTransform)
        {
            _player = player;
            _npcController = npcController;
            _npcTransform = npcTransform;
            _hasAttacked = false;
        }
        public void OnEnter()
        {
            _npcController.MaxSpeed = 150f;
            FacePlayer();
            _attackTimer = 0f;
            _attackCooldown = 2f;
            _distanceToAttackFrom = 2f;
        }

        public void Tick()
        {
            FacePlayer();
            if (IsAttacking)
            {
                IsAttacking = false;
            }

            if (_hasAttacked)
            {
                _attackTimer += Time.fixedDeltaTime;
            }

            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer = 0f;
                _hasAttacked = false;
            }

            if (distanceToSpirit <= _distanceToAttackFrom && !_hasAttacked)
            {
                IsAttacking = true;
                _hasAttacked = true;
            }

            if (distanceToSpirit > _distanceToAttackFrom)
            {
                _npcController.MaxSpeed = 150;
            }
            else
            {
                _npcController.MaxSpeed = 0f;
            }

            _npcController.Move();
        }

        public void OnExit()
        {
           
        }
        
        private void FacePlayer()
        {
            var dir = _player.transform.position - _npcTransform.position;
            dir.y = 0f;
            dir.z = 0f;
            distanceToSpirit = dir.magnitude;
            _npcController.MoveDirection = dir.normalized;
            _npcController.CalculateFacing();
        }
    }
}