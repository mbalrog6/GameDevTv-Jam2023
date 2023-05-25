using System.Collections.Generic;
using MB6.NPCs.States;
using UnityEditor;
using UnityEngine;


namespace MB6
{
    public class GoodNPC :MonoBehaviour, INPCEntity
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float Speed;
        public INPCState CurrentState => _states.Count > 0 ? _states.Peek() : null;

        private Stack<INPCState> _states;
        private NPCController _npcController;

        private void Awake()
        {
            _states = new Stack<INPCState>();
            
            var rayCastPoints = new Vector3[]
                { 
                    new Vector3(-.4f, 0, 0), 
                    new Vector3(0f, 0, 0f), 
                    new Vector3(.4f, 0, 0f) 
                };
            
            _npcController = new NPCController(this, rayCastPoints, true, _layerMask);
        }

        public NPCController GetNPCController() => _npcController;

        private void Start()
        {
            PushState(new PaceNPCState(_npcController));
        }

        public void FixedUpdate()
        {
            _npcController.MaxSpeed = Speed;
            if (_states.Count > 0)
            {
                _states.Peek().Tick();
            }
        }

        private void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying)
            {
                _npcController.GizmoDrawRays();
            }
        }

        public GameObject GetNPCGameObject()
        {
            return gameObject;
        }

        #region State Management...
        public void ChangeState(INPCState state)
        {
            _states.Pop().OnExit();
            _states.Push(state);
            state.OnEnter();
        }

        public void PushState(INPCState state)
        {
            if (_states.Count > 0)
            { 
                _states.Peek().OnExit(); 
            }
            _states.Push(state);
            state.OnEnter();
        }

        public void PopState()
        {
            _states.Pop().OnExit();
            if (_states.Count > 0)
            {
                _states.Peek().OnEnter();
            }
        }
        #endregion
    }
}