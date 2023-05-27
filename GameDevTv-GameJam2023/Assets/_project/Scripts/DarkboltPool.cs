using System.Collections.Generic;
using UnityEngine;

namespace MB6
{
    public class DarkboltPool : MonoBehaviour
    {
        public static DarkboltPool Instance { get; private set; }
        [SerializeField] private Transform _darkboltPrefab;
        [SerializeField] private LayerMask _layermask;
        private List<DarkBolt> _activeBolts;
        private Stack<DarkBolt> _inactiveBolts;
        private Stack<DarkBolt> _workingStack;
        private int _activeCount;
        [SerializeField] private int _damageAmount;
        [SerializeField] private float _speed;

        private void Awake()
        {
            Instance = this;
            
            _activeBolts = new List<DarkBolt>(20);
            _inactiveBolts = new Stack<DarkBolt>(20);
            _workingStack = new Stack<DarkBolt>(20);

            for (var i = 0; i < 20; i++)
            {
                var darkBolt = Instantiate(_darkboltPrefab, transform).GetComponent<DarkBolt>();
                darkBolt.gameObject.SetActive(false);
                _inactiveBolts.Push(darkBolt);
            }
        }

        public void FireDarkBolt(Vector3 origin, Vector3 direction)
        {
            if (_inactiveBolts.Count > 0)
            {
                var darkbolt = _inactiveBolts.Pop();
                _activeBolts.Add(darkbolt);
                darkbolt.gameObject.SetActive(true);
                darkbolt.Fire(origin, direction, _speed);
            }
        }

        private void FixedUpdate()
        {
            _activeCount = _activeBolts.Count;
            if (_activeCount <= 0) return;
            
            DarkBolt darkbolt;
            int hitCount = 0;
            RaycastHit[] results = new RaycastHit[10];
            
            var distance = _activeBolts[0].Speed * Time.fixedDeltaTime;

            for (var i = 0; i < _activeCount; i++)
            {
                darkbolt = _activeBolts[i];
                if (!darkbolt.IsActive)
                {
                    _workingStack.Push(darkbolt);
                    continue;
                }
                
                // Check for Collisions
                hitCount = Physics.SphereCastNonAlloc(darkbolt.transform.position, .4f, darkbolt.Direction, results, distance,
                    _layermask);
                
                if (hitCount > 0)
                {
                    for(var k = 0; k < hitCount; k++)
                    {
                        var hitObject = results[k].collider.gameObject.GetComponent<IHealth>();
                        if (hitObject != null)
                        {
                            hitObject.TakeDamage(_damageAmount);
                        }
                        darkbolt.IsActive = false;
                        _workingStack.Push(darkbolt);
                    }
                }
                
                // Update positions
                darkbolt.TranslateBolt(darkbolt.Direction * distance);
            }

            // Remove the InActive Bolts and Repool them.
            while (_workingStack.Count > 0)
            {
                darkbolt = _workingStack.Pop();
                darkbolt.gameObject.SetActive(false);
                _inactiveBolts.Push(darkbolt);
                _activeBolts.Remove(darkbolt);
            }
        }

        [ContextMenu("Test Fire Bolt")]
        public void TestFIreBolt()
        {
            FireDarkBolt(new Vector3(14.15f, 18.87f, 0f), Vector3.left);
        }
    }
}