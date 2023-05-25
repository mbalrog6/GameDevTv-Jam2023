using UnityEngine;

namespace MB6
{
    public class StationaryNPCState : INPCState
    {
        private Player _player;
        private NPCController _npcController;
        private Transform _npcTransform;

        public StationaryNPCState(NPCController npcController, Player player, Transform npcTransform)
        {
            _npcController = npcController;
            _player = player;
            _npcTransform = npcTransform;

        }
        public void OnEnter()
        {
            _npcController.MaxSpeed = 0;
        }

        public void Tick()
        {
            FacePlayer();
        }

        public void OnExit()
        {
            
        }
        
        private void FacePlayer()
        {
            var dir = _player.transform.position - _npcTransform.position;
            dir.y = 0f;
            dir.z = 0f;
            _npcController.MoveDirection = dir.normalized;
            _npcController.CalculateFacing();
        }
    }
}