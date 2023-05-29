using UnityEngine;
using UnityEngine.SceneManagement;

namespace MB6
{
    public class SceneSingleton : MonoBehaviour
    {
        public static SceneSingleton Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void Load(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }
    }
}