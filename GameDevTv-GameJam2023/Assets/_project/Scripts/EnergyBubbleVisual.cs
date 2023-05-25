using UnityEngine;

namespace MB6
{
    public class EnergyBubbleVisual
    {
        private Renderer _energyBubbleRenderer;
        private MaterialPropertyBlock _materialPropertyBlock;

        private float _energyCapacity;
        private float _currentColor;

        private Gradient _gradient;


        public EnergyBubbleVisual(Renderer energyBubbleRenderer, float energyCapacity, Gradient gradient)
        {
            _energyBubbleRenderer = energyBubbleRenderer;
            _materialPropertyBlock = new MaterialPropertyBlock();

            _energyCapacity = energyCapacity;
            _gradient = gradient;
        }

        public void SetEnergyVisual(EnergyType energyType, float energy)
        {
            var normalizedCapacity = Mathf.Clamp01(energy / _energyCapacity);
            float targetColor = 0f;
            
            _energyBubbleRenderer.GetPropertyBlock(_materialPropertyBlock);
            
            _materialPropertyBlock.SetFloat("_Thickness", Mathf.Lerp(0.05f, 0.25f, normalizedCapacity * normalizedCapacity));
            _materialPropertyBlock.SetFloat("_Size", Mathf.Lerp(0f, 0.55f, normalizedCapacity));
            
            switch (energyType)
            {
                case EnergyType.Light:
                    targetColor = 0f;
                    break;
                case EnergyType.Either:
                    targetColor = .5f;
                    break;
                case EnergyType.Dark:
                    targetColor = 1f;
                    break;
            }

            _currentColor = Mathf.Lerp(_currentColor, targetColor, Time.deltaTime);
            _materialPropertyBlock.SetColor("_Color", _gradient.Evaluate(_currentColor));
            
            _energyBubbleRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}