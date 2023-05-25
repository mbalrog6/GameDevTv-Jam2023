namespace MB6
{
    public class Energy
    {
        public EnergyType GetEnergyType => _energyType;
        public float GetEnergy => _energy;
        
        private float _energyMax;
        private EnergyType _energyType;
        private float _energy;

        private float _energyLockThreshold;
        private bool _isLocked;

        public Energy(float energyMax, EnergyType energyType, float energy, float energyLockThreshold)
        {
            _energyMax = energyMax;
            _energyType = energyType;
            _energy = energy;
            _energyLockThreshold = energyLockThreshold;

            if (_energy >= _energyLockThreshold && _energyType != EnergyType.Either)
            {
                _isLocked = true;
            }
            else
            {
                _isLocked = false;
            }
            
        }

        public Energy(float energyMax, float energyLockThreshold) : this(energyMax, EnergyType.Either, 0f,
            energyLockThreshold)
        {
            
        }

        public void AddEnergy(EnergyType energyType, float amount)
        {
            if (_energy <= 0 || _energyType == EnergyType.Either)
            {
                if (!_isLocked)
                {
                    _energyType = energyType;
                }
            }

            if (_energyType == energyType)
            {
                if (amount >= 0)
                {
                    _energy += amount;

                    if (_energyType == EnergyType.Either && _energy >= _energyLockThreshold)
                    {
                        _energy = _energyLockThreshold - 1f;
                    }
                    if (_energy >= _energyMax)
                    {
                        _energy = _energyMax;
                    }

                    if (!_isLocked && _energy >= _energyLockThreshold)
                    {
                        if (_energyType != EnergyType.Either)
                        {
                            _isLocked = true;
                        }
                    }
                }
            }
        }

        public void RemoveEnergy(float amount)
        {
            if (_energy >= 0f)
            {
                _energy -= amount;
                if (_energy <= 0)
                {
                    _energy = 0;
                    if (!_isLocked)
                    {
                        _energyType = EnergyType.Either;
                    }
                }
            }
        }
        
        
    }
}