using System;
using System.Collections.Generic;
using MB6.NPCs.States;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace MB6
{
    public class GoodNPC :MonoBehaviour, INPCEntity
    {
        [SerializeField] private Player _player;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private ItemEnergySource _energyProvider;

        [SerializeField] private float Speed;
        public INPCState CurrentState => _states.Count > 0 ? _states.Peek() : null;

        private Stack<INPCState> _states;
        private NPCController _npcController;

        private Dictionary<string, INPCState> _statesLibrary;

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

            _statesLibrary = new Dictionary<string, INPCState>();
            
            _statesLibrary.Add("Pace", new PaceNPCState(_npcController));
            _statesLibrary.Add("StandStill", new StationaryNPCState(_npcController));
        }

        public NPCController GetNPCController() => _npcController;

        private void Start()
        {
            PushState(_statesLibrary["Pace"]);

            _energyProvider.OnBeingDrained += HandleBeingDrained;
            _energyProvider.OnBeingDrainedEnded += HandleBeingDrainedEnded;
        }

        private void HandleBeingDrainedEnded(object sender, EventArgs e)
        {
            PopState();
        }

        private void HandleBeingDrained(object sender, EventArgs e)
        {
            PushState(_statesLibrary["StandStill"]);
        }

        public void FixedUpdate()
        {
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