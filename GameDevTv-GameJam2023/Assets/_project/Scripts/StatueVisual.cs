using System;
using UnityEngine;

namespace MB6
{
    public class StatueVisual : MonoBehaviour
    {
        [SerializeField] private Renderer _bubbleRenderer;
        [SerializeField] private float _timeForBubbleToReachFullSize; 
        [SerializeField] private ItemEnergyPolorizer _polorizer; 
        
        private bool _bubbleAnimating;
        private bool _isDraining;
        private float _drainingTimer;
        private MaterialPropertyBlock _bubblePropBlock;

        private void Awake()
        {
            _polorizer.OnBeingDrained += Handle_BeingDrained;
            _polorizer.OnBeingDrainedEnded += Handle_StoppedBeingDrained;

            _bubblePropBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_bubbleAnimating)
            {
                _bubbleRenderer.enabled = true;
                HandleBubbleEffect();
            }
        }


        private void Handle_StoppedBeingDrained(object sender, EventArgs e)
        {
            _isDraining = false;
            
        }

        private void Handle_BeingDrained(object sender, EventArgs e)
        {
            _bubbleAnimating = true;
            _isDraining = true;
            SoundManager.Instance.PlayBubbleOpening();
        }


        private void HandleBubbleEffect()
        {
            if (_isDraining)
            {
                _drainingTimer += Time.deltaTime;
            }
            else
            {
                _drainingTimer -= Time.deltaTime;
            }
            
            if (_drainingTimer >= _timeForBubbleToReachFullSize)
            {
                _drainingTimer = _timeForBubbleToReachFullSize;
                return;
            }

            if (_drainingTimer <= 0f)
            {
                _bubbleAnimating = false;
                _bubbleRenderer.enabled = false;
                _drainingTimer = 0f;
                return;
            }
            
            var size = Mathf.Clamp(_drainingTimer / _timeForBubbleToReachFullSize, .1f, .8f);
            _bubbleRenderer.GetPropertyBlock(_bubblePropBlock);
            _bubblePropBlock.SetFloat("_Size", size);
            _bubbleRenderer.SetPropertyBlock(_bubblePropBlock);
        }
    }
}