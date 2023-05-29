using UnityEngine;
using UnityEngine.SceneManagement;

namespace MB6
{
    public class Goal : MonoBehaviour
    {
        [SerializeField] private string _levelToLoad;

        private void OnTriggerEnter(Collider other)
        {
            var obj = other.gameObject.GetComponent<Player>();
                if (obj != null)
                {
                    SceneManager.LoadScene(_levelToLoad);
                }
        }
    }
}