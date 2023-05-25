using UnityEngine;

namespace MB6
{
    public class RotateTransform : MonoBehaviour
    {
        [SerializeField] private Transform[] _transforms;
        [SerializeField] private float[] _rotateSpeeds;

        public bool IsRotating { get; set; }
        private void Update()
        {
            if (IsRotating)
            {
                for (var i = 0; i < _transforms.Length; i++)
                {
                    _transforms[i].Rotate(0f, 0f, _rotateSpeeds[i] * Time.deltaTime);
                }
            }
        }

        [ContextMenu("BeginRotating")]
        public void BeginRotating()
        {
            IsRotating = true;
        }
    }
}