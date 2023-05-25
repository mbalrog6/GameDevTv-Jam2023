using System;
using UnityEngine;

namespace MB6.NPCs.States
{
    public class PaceNPCState : INPCState
    {
        private NPCController _npcController;
        private bool _isMovingRight;

        public PaceNPCState(NPCController npcController)
        {
            _npcController = npcController;
        }
        public void OnEnter()
        {
            _npcController.ShouldCheckForGround = true;
            _npcController.OnStopped += HandleOnStopped;
            _npcController.MaxSpeed = 100f;
        }

        private void HandleOnStopped(object sender, EventArgs e)
        {
            _isMovingRight = !_isMovingRight;
        }

        public void Tick()
        {
            if (_isMovingRight)
            {
                _npcController.MoveDirection = -Vector3.right;
            }
            else
            {
                _npcController.MoveDirection = Vector3.right;
            }
            
            _npcController.Move();
        }

        public void OnExit()
        {
            _npcController.OnStopped -= HandleOnStopped;
        }
    }
}