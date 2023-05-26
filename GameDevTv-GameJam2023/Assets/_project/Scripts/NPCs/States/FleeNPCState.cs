using UnityEngine;

namespace MB6.NPCs.States
{
    public class FleeNPCState : INPCState   
    {
        public bool DoneFleeing { get; private set; }
        
        private Player _player;
        private NPCController _npcController;
        private Transform _npcTransform;
        private float _timer;
        private float _timerCheck;

        public FleeNPCState(NPCController npcController, Player player, Transform npcTransform)
        {
            _player = player;
            _npcController = npcController;
            _npcTransform = npcTransform;
        }
        public void OnEnter()
        {
            _npcController.MaxSpeed =300f;
            _npcController.ShouldCheckForGround = false;
            
            _timerCheck = 1f;
            _timer = 0f;
            DoneFleeing = false;
        }

        public void Tick()
        {
            FaceAwayFromPlayer();
            _npcController.Move();
            _timer += Time.fixedDeltaTime;
            if (_timer >= _timerCheck)
            {
                DoneFleeing = true;
            }
        }

        public void OnExit()
        {
            
        }
        
        private void FaceAwayFromPlayer()
        {
            var dir = _npcTransform.position - _player.transform.position ;
            dir.y = 0f;
            dir.z = 0f;
            _npcController.MoveDirection = dir.normalized;
        }
    }
}