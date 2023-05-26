﻿using System;
using System.Collections.Generic;
using MB6.NPCs.States;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MB6
{
    public class BadNPC: MonoBehaviour, INPCEntity, IHealth
    { 
        [SerializeField] private Player _player;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private ParticleSystem _badEnergyParticles;
        [SerializeField] private ItemEnergySource _energyProvider;

        [SerializeField] private int _maxHealth;
        public INPCState CurrentState => _states.Count > 0 ? _states.Peek() : null;
        public EnergyType NPCEnergyType => _energyProvider.EnergyForm;
        
        
        public int Health { get; protected set; }

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                if (value <= 0)
                {
                    _maxHealth = 1;
                }
                else
                {
                    _maxHealth = value;
                }
            }
        }
        public bool IsDead; 
        private bool _startedFalling;

        public event EventHandler<EventArgs> OnDied;

        private Stack<INPCState> _states;
        private Dictionary<string, INPCState> _statesLibrary;
        
        private NPCController _npcController;

        private bool _isBeingDrained;
        private bool _particlesActive;
        


        private void Awake()
        {
            IsDead = false;
            Health = MaxHealth;
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
            _statesLibrary.Add("StandStill", new StationaryNPCState(_npcController, _player, transform));
            _statesLibrary.Add("Attack", new AttackSpirtNPCState(_npcController, _player, transform));
            _statesLibrary.Add("Dead", new DeadNPCState(_npcController));

            _particlesActive = true;
            _startedFalling = false;
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
            _isBeingDrained = false;
        }

        private void HandleBeingDrained(object sender, EventArgs e)
        {
            _isBeingDrained = true;
        }

        public void Update()
        {
            if (IsDead) return;
            
            if (_isBeingDrained && _player.IsManifesting)
            {
                if (_particlesActive)
                {
                    if (NPCEnergyType == _player.PlayerEnergyType || _player.PlayerEnergyType == EnergyType.Either)
                    {
                        PushState(_statesLibrary["StandStill"]);
                    }
                    else if (NPCEnergyType != _player.PlayerEnergyType)
                    {
                        PushState(_statesLibrary["Attack"]);
                    }
                    _badEnergyParticles.Stop();
                    _particlesActive = false;
                }
            }

            if (!_particlesActive && !_player.IsManifesting)
            {
                if (_states.Peek() == _statesLibrary["StandStill"] || _states.Peek() == _statesLibrary["Attack"])
                {
                    PopState();
                }
                _particlesActive = true;
                _badEnergyParticles.Play();
            }

            if (!_isBeingDrained && !_particlesActive)
            {
                if (_states.Peek() == _statesLibrary["StandStill"] || _states.Peek() == _statesLibrary["Attack"])
                {
                    PopState();
                }
                _particlesActive = true;
                _badEnergyParticles.Play();
            }
            
        }

        public void FixedUpdate()
        {
            if (_states.Count > 0)
            {
                _states.Peek().Tick();
                
                if (CurrentState == _statesLibrary["Attack"])
                {
                    if (((AttackSpirtNPCState)CurrentState).IsAttacking)
                    {
                        Debug.Log("Firing the Darkbolt");
                        DarkboltPool.Instance.FireDarkBolt(transform.position, 
                            (_player.transform.position - transform.position).normalized);
                    }
                }
            }
            
            if (IsDead) return;
            if (_npcController.TrackFalling() > 0f)
            {
                _startedFalling = true;
            }
            else
            {
                if (_startedFalling)
                {
                    _startedFalling = false;
                    TakeDamage(MaxHealth);
                }
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

        public void Die()
        {
            int stateCount = _states.Count;
            for(var i = 0; i< stateCount; i++ )
            {
                PopState();
            }
            PushState(_statesLibrary["Dead"]);
            _badEnergyParticles.Stop();
            _energyProvider.DisableEnergyCollider(false);
            _energyProvider.enabled = false;
        }

        #region Health Related Functions...
        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            
            Health -= amount;
            if (Health <= 0)
            {
                OnDied?.Invoke(this, EventArgs.Empty);
                Die();
                IsDead = true;
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            Health += amount;
            if (Health >= MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public float NormalizedHealth => Mathf.Clamp01((float)Health / MaxHealth);
        #endregion
    }
}