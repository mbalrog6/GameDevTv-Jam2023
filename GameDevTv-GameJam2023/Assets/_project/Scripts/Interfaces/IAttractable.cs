using UnityEngine;

namespace MB6
{
    public interface IAttractable
    {
        public void AttractTowards(Vector3 position, float pullStrength);
        public void BeenReleased();
    }
}