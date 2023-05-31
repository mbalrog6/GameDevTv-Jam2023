using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MB6
{
    public class Blurb : MonoBehaviour
    {
        public int SoundIndex;
        public bool HasSound;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _timeToShow;
        [SerializeField] private float _timeToDisplay;
        private float _timer;
        private float _durationTimer;
        private bool _isActive;
        private bool _showingDuration;

        public bool IsAnimating;

        private void Update()
        {
            if (IsAnimating)
            {
                if(_isActive)
                {
                    _timer += Time.deltaTime;
                    Fade();
                    if (_timer >= _timeToShow)
                    {
                        _timer = _timeToShow;
                        _showingDuration = true;
                    }

                    if (_showingDuration)
                    {
                        _durationTimer += Time.deltaTime;
                        if (_durationTimer >= _timeToDisplay)
                        {
                            _isActive = false;
                        }
                    }
                }
                else
                {
                    _timer -= Time.deltaTime;
                    Fade();
                    if (_timer <= 0)
                    {
                        IsAnimating = false;
                        _timer = 0f;
                        _durationTimer = 0f;
                        _image.gameObject.SetActive(false);
                        _text.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Fade()
        {
            var alpha = Mathf.Lerp(0f, .9f, _timer / _timeToShow);
            _group.alpha = alpha;
        }

        public void Show()
        {
            _image.gameObject.SetActive(true);
            _text.gameObject.SetActive(true);
            IsAnimating = true;
            _isActive = true;
        }

        public void Hide()
        {
            _isActive = false;
            _durationTimer = 0f;
        }
        
    }
}