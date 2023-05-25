using System.Linq;
using UnityEngine;

namespace MB6
{
    public class RaycastController
    {
        public bool DebugOn;
        public LayerMask InteractionLayers;
        public Ray Ray;
        public float DetectionDistance;
        
        private int _hits;  
        private RaycastHit[] _resultingHits;

        public RaycastHit[] DebugResults => _debugResults;
        private RaycastHit[] _debugResults;

        public RaycastController()
        {
            _resultingHits = new RaycastHit[10];
            _debugResults = new RaycastHit[10];
        }
        
        public bool CheckForCollisions()
        {
            _hits = Physics.RaycastNonAlloc(Ray, _resultingHits, DetectionDistance, InteractionLayers);

            if (_hits > 0)
            {
                if (DebugOn)
                {
                    _debugResults = _resultingHits.Take(_hits).ToArray();
                }
                return true;
            }

            return false;
        }
    }
}