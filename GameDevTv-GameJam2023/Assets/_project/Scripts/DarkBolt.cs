using UnityEngine;

namespace MB6
{
    public class DarkBolt : MonoBehaviour
    {
        public bool IsActive { get; set; }
        public Vector3 Direction { get; private set; }
        public float Speed { get; private set; }
        private float _lifeTime;
        private float _lifeTimer;
        [SerializeField] private ParticleSystem _visuals; 

        private void Awake()
        {
            IsActive = false;
            Direction = Vector3.left;
            _lifeTime = 10f;
            _lifeTimer = 0f;
        }

        public void FixedUpdate()
        {
            if (!IsActive) return;

            _lifeTimer += Time.fixedDeltaTime;
            if (_lifeTimer >= _lifeTime)
            {
                IsActive = false;
            }
        }

        public void Fire(Vector3 origin, Vector3 direction, float speed)
        {
            transform.position = origin;
            Direction = direction;
            Speed = speed;
            IsActive = true;
            _lifeTimer = 0f;
        }
        public void TranslateBolt(Vector3 position)
        {
            transform.Translate(position);
        }
    }
}