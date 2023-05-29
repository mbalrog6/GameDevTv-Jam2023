using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MB6
{
    public class HoverButton : Button, IPointerEnterHandler
    {
        private TextMeshProUGUI _text;
        private Image _image;

        protected override void Awake()
        {
            base.Awake();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            var icons = GetComponentsInChildren<Image>();
            if (icons.Length > 1)
            {
                _image = icons[1];
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
            base.OnPointerEnter(eventData);
        }

        public void SetText(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
        }

        public void SetImage(Sprite icon)
        {
            if (_image == null) return;
            
            _image.sprite = icon;
        }
        
    }
}