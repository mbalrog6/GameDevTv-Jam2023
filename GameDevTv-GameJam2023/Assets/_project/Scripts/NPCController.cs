using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MB6
{
    public class NPCController
    { 
        public float MaxSpeed { get; set; }

        public Vector3 MoveDirection { get; set; }
        public float NormalizedSpeed => ProvideSpeedParameter();
        public bool ShouldCheckForGround { get; set; }
        
        private float _currentSpeed;
        
        private Transform _npcTransform;
        private Vector3[] _footOffsets;
        private Rigidbody _rb;
        private bool _isFacingRight;
        private bool _lastFacing;
        private LayerMask _layerMask;

        private NPCStatus _status;

        public event EventHandler<EventArgs> OnStopped;
        public event EventHandler<EventArgs> OnChangedFacing;

        private RaycastController _raycastController;
        private Ray[] _groundCheckRays;

        public bool IsFalling; 
        private float _fallDamageVelocity;
        private float _lastYVelocity;
        
        
        public NPCController(INPCEntity npc, Vector3[] footOffsets, bool initialFacing, LayerMask layerMask)
        {
            _npcTransform = npc.GetNPCGameObject().transform;
            _footOffsets = footOffsets;

            _rb = npc.GetNPCGameObject().GetComponent<Rigidbody>();

            _status = new NPCStatus();
            _isFacingRight = initialFacing;
            _lastFacing = initialFacing;

            _raycastController = new RaycastController();
            _raycastController.InteractionLayers = layerMask;
            _raycastController.DetectionDistance = 3f;
            _raycastController.DebugOn = true;

            _groundCheckRays = new Ray[3];
            _groundCheckRays[0] = new Ray(Vector3.zero, -Vector3.up);
            _groundCheckRays[1] = new Ray(Vector3.zero, -Vector3.up);

            _fallDamageVelocity = -16f;
        }

        private void GenerateRays()
        {
            var displacementVector = Vector3.zero;
            
            if (_isFacingRight)
            {
                _groundCheckRays[0].origin =
                    _npcTransform.position + _footOffsets[2] + displacementVector;
            }
            else
            {
                _groundCheckRays[0].origin = 
                    _npcTransform.position + _footOffsets[0] + displacementVector;
            }
            
            _groundCheckRays[1].origin = 
                _npcTransform.position + _footOffsets[1] + displacementVector;
        }

        public bool CheckForGroundAhead()
        { 
            GenerateRays();

            _raycastController.DetectionDistance = 3f;
            _raycastController.Ray = _groundCheckRays[0];
            if (!_raycastController.CheckForCollisions())
            {
                _raycastController.Ray = _groundCheckRays[1];
                if (!_raycastController.CheckForCollisions())
                {
                    return false;
                }
            }

            return true;
        }

        public bool CheckInfrontOfNPC()
        {
            _raycastController.DetectionDistance = 1f;
            _raycastController.Ray = new Ray(_npcTransform.position + Vector3.up * -.3f, MoveDirection);
            if (_raycastController.CheckForCollisions())
            {
                return true;
            }

            return false;
        }

        public void Move()
        {
            CalculateFacing();
            IsFalling = false;

            if (ShouldCheckForGround)
            {
                if (!CheckForGroundAhead())
                {
                    if (_rb.velocity.y < -0.1f)
                    {
                        IsFalling = true;
                    }
                    _rb.velocity = Vector3.zero + Vector3.Project(_rb.velocity, Vector3.up);
                    OnStopped?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            if (CheckInfrontOfNPC())
            {
                _rb.velocity = Vector3.zero + Vector3.Project(_rb.velocity, Vector3.up);
                OnStopped?.Invoke(this, EventArgs.Empty);
                return;
            }

            _rb.velocity = (MoveDirection * (MaxSpeed * Time.fixedDeltaTime)) + 
                           Vector3.Project(_rb.velocity, Vector3.up);
        }

        public void CalculateFacing()
        {
            if (MoveDirection.x > 0)
            {
                _isFacingRight = true;
                if (_isFacingRight != _lastFacing)
                {
                    OnChangedFacing?.Invoke(this, EventArgs.Empty);
                }

                _lastFacing = _isFacingRight;
                return;
            }

            if (MoveDirection.x < 0)
            {
                _isFacingRight = false;
                if (_isFacingRight != _lastFacing)
                {
                    OnChangedFacing?.Invoke(this, EventArgs.Empty);
                }
                _lastFacing = _isFacingRight;
            }
        }
        
        private float ProvideSpeedParameter()
        {
            if (MaxSpeed < 1f)
            {
                return 0f;
            }

            if (MaxSpeed < 150)
            {
                return .2f;
            }

            return 1f;
        }

        public float TrackFalling()
        {
            if (_rb.velocity.y != _lastYVelocity)
            {
                _lastYVelocity = _rb.velocity.y;
                if (_rb.velocity.y <= _fallDamageVelocity)
                {
                    return _fallDamageVelocity - _rb.velocity.y;
                }
            }
            
            return 0f;
        }

        public void GizmoDrawRays()
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_raycastController.Ray.origin, _raycastController.Ray.direction * _raycastController.DetectionDistance);
            Handles.Label(_raycastController.Ray.origin, "Simple Origin");

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_npcTransform.position, MoveDirection * 5f);

            foreach (var result in _raycastController.DebugResults)
            {
                Handles.Label(result.point, result.transform.name);
            }
            #endif
        }
    }
    
}