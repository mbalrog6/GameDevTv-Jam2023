using KinematicCharacterController;
using UnityEngine;

namespace MB6
{
    public class PlayerController : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor _motor;
        [SerializeField] private InputManager _inputManager;
        
        [Header("Stable Movement")]
        [SerializeField] private float MaxStableMoveSpeed = 10f;
        [SerializeField] private float StableMovementSharpness = 15;

        [Header("Air Movement")]
        [SerializeField] private float MaxAirMoveSpeed = 10f;
        [SerializeField] private float AirAccelerationSpeed = 5f;
        [SerializeField] private float Drag = 0.1f;
        [SerializeField] private float LevitatingForce;
        [SerializeField] private float SuperFallSpeed;
        [SerializeField] private float FloatSharpness;
        
        [Header("Misc")]
        [SerializeField] private Vector3 Gravity = new Vector3(0, -30f, 0);

        public bool IsGrounded => _motor.GroundingStatus.IsStableOnGround;

        #region Internal Values for Movement...
        private Vector3 _moveInputVector;
        private Vector3 _internalVelocityAdd;

        private bool isLevitating;
        private bool isSuperFalling;
        private bool isStableFloating;

        private bool _floatSharpnessTimerStarted;
        private float _floatSharpnessTimer;
        public float _normalizedFloatSharpness;
        
        
        #endregion
        
        private void Awake()
        {
            _motor.CharacterController = this;
            
            _internalVelocityAdd = Vector3.zero;
        }

        public void SetInputs(ref SpiritInputs inputs)
        {
            _moveInputVector = inputs.MoveHorizontal;
            _moveInputVector.Normalize();

            isLevitating = inputs.MoveVertical.y > 0.01f;
            isSuperFalling = inputs.MoveVertical.y < -0.01f;
            isStableFloating = inputs.IsStableFloating;
        }
        
        public void AddVelocity(Vector3 velocity)
        {
            _internalVelocityAdd += velocity;
        }

        public void OnLanded()
        {
            
        }

        public void OnLeaveStableGround()
        {
            
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {

        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
             Vector3 targetMovementVelocity = Vector3.zero;

             if (_motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, _motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, _motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(_motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            else
            {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                    // Prevent climbing on un-stable slopes with air movement
                    if (_motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
            
            // Handle StableFloating.
            if (isStableFloating && !_motor.GroundingStatus.IsStableOnGround)
            {
                if (_floatSharpnessTimer <= FloatSharpness)
                {
                    _floatSharpnessTimer += deltaTime;
                    _normalizedFloatSharpness = Mathf.Clamp01(_floatSharpnessTimer / FloatSharpness);
                }
                targetMovementVelocity = currentVelocity - Vector3.Project(currentVelocity, _motor.CharacterUp);
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, _normalizedFloatSharpness);
            }

            if (!isStableFloating || _motor.GroundingStatus.IsStableOnGround)
            {
                _floatSharpnessTimerStarted = false;
                _normalizedFloatSharpness = 0f;
            }

            if (isLevitating)
            {
                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                _motor.ForceUnground(0.1f);

                // Add to the return velocity and reset jump state
                currentVelocity += (_motor.CharacterUp * LevitatingForce) -
                                   Vector3.Project(currentVelocity, _motor.CharacterUp);
            }

            // Handle SuperFalling
            if (!_motor.GroundingStatus.IsStableOnGround && isSuperFalling)
            {
                currentVelocity -= _motor.CharacterUp * SuperFallSpeed;
            }

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            if (!_motor.GroundingStatus.IsStableOnGround && isStableFloating && !_floatSharpnessTimerStarted)
            {
                _floatSharpnessTimerStarted = true;
                _floatSharpnessTimer = 0f;
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (_motor.GroundingStatus.IsStableOnGround && !_motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLanded();
            }
            else if (!_motor.GroundingStatus.IsStableOnGround && _motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLeaveStableGround();
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
           
        }
    }
}
