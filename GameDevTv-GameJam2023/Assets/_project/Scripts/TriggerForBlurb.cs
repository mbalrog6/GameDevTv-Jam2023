using UnityEngine;

namespace MB6
{
    [RequireComponent(typeof(Rigidbody))]
    public class TriggerForBlurb : MonoBehaviour
    {
        public int BlurbIndex;

        private void OnTriggerEnter(Collider other)
        {
            var obj = other.GetComponent<Player>();
            if (obj != null)
            {
                BlurbManager.Instance.ShowBlurb(BlurbIndex);
                gameObject.SetActive(false);
            }
        }
    }
}