using UnityEngine;

namespace MB6
{
    public interface IProvideEnergy
    {
        public EnergyType EnergyForm { get; }
        public float GetEnergy(Vector3 receiversPosition);

        public bool ShouldChangeEnergy { get; }
        public bool ShouldDrainEnergy { get; }
    }
}
