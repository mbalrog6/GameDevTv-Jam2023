using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MB6
{
    public class TestRaycast : MonoBehaviour, INPCEntity
    {
        [SerializeField] private bool isCastingRay;
        [SerializeField] private bool isCastingFromNPC;
        [SerializeField] private bool NPCControllerTest;
        public int _hits;
        public LayerMask _layerMask;
        private RaycastHit[] _resultingHits;
        private RaycastHit[] _debugResults;
        private Ray _ray;

        private RaycastController _raycastController;
        private NPCController _npc;
        private Vector3[] _offsets;
        
        private void Awake()
        {
            _resultingHits = new RaycastHit[10];
            _debugResults = new RaycastHit[10];

            _raycastController = new RaycastController();
            _raycastController.InteractionLayers = _layerMask;
            _raycastController.DebugOn = true;
            _raycastController.DetectionDistance = Mathf.Infinity;

            _offsets = new Vector3[3];
            _offsets[0] = new Vector3(-.4f, 0f, 0f);
            _offsets[1] = new Vector3(0f, 0f, 0f);
            _offsets[2] = new Vector3(.4f, 0f, 0f);

            _npc = new NPCController(this, _offsets, true, _layerMask);
        }

        private void Update()
        {
            if (isCastingRay)
            {
                _ray = new Ray(transform.position, -Vector3.up);

                _hits = Physics.RaycastNonAlloc(_ray, _resultingHits, Mathf.Infinity, _layerMask);

                if (_hits > 0)
                {
                    _debugResults = _resultingHits.Take(_hits).ToArray();
                }
            }

            if (NPCControllerTest)
            {
                _npc.CheckForGroundAhead();
            }
            
            if (!isCastingFromNPC) return;
            _raycastController.Ray = new Ray(transform.position, -Vector3.up);
            _raycastController.CheckForCollisions();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (isCastingFromNPC)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_raycastController.Ray.origin, _raycastController.Ray.direction * 5f);
                Handles.Label(_raycastController.Ray.origin, "Simple Origin");

                foreach (var result in _raycastController.DebugResults)
                {
                    Handles.Label(result.point, result.transform.name);
                }
            }

            if (NPCControllerTest)
            {
                _npc.GizmoDrawRays();
            }
            
            if (!isCastingRay) return;
            
            if (EditorApplication.isPlaying)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(_ray.origin, _ray.direction * 5f);
                
                Handles.Label(_ray.origin, "Origin");
                
                foreach (var result in _debugResults)
                {
                    UnityEditor.Handles.Label(result.point, result.transform.name);
                }
            }
        }
        #endif

        public GameObject GetNPCGameObject()
        {
            return gameObject;
        }
    }
}