using System.Collections.Generic;
using UnityEngine;

namespace MB6
{
    public class BlurbManager : MonoBehaviour
    {
        public static BlurbManager Instance { get; private set; }
        [SerializeField] private List<Blurb> _blurbs;
        
        private Queue<int> _triggers;
        private int _currentShowingBlurb;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            
            _triggers = new Queue<int>();
        }

        private void Update()
        {
            if (_triggers.Count > 0)
            {
                
                if (_blurbs[_currentShowingBlurb].IsAnimating) return;

                
                _currentShowingBlurb = _triggers.Dequeue();
                _blurbs[_currentShowingBlurb].Show();
                if (_blurbs[_currentShowingBlurb].HasSound)
                {
                    SoundManager.Instance.PlayBlurbSound(_blurbs[_currentShowingBlurb].SoundIndex);
                }
            }
        }

        public void ShowBlurb(int blurb)
        {
            _triggers.Enqueue(blurb);
            _blurbs[_currentShowingBlurb].Hide();
        }
    }
}