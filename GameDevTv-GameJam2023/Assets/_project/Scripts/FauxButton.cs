using UnityEngine;
using UnityEngine.SceneManagement;

namespace MB6
{
    public class FauxButton : MonoBehaviour
    {
        [SerializeField] private bool _isMainMenu;

        private void OnTriggerEnter(Collider other)
        {
            var obj = other.gameObject.GetComponent<Player>();
            if (obj != null)
            {
                if (_isMainMenu)
                {
                    SceneManager.LoadScene("Main Menu");
                }
                else
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #endif
                    Application.Quit();
                }
            }
            
        }
    }
}