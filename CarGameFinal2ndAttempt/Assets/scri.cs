using UnityEngine;
    using UnityEngine.SceneManagement; // Important: Add this namespace

    public class scri : MonoBehaviour
    {
        public void LoadSpecificScene(string sceneName)
        {
            SceneManager.LoadScene("Level1");
        }
    }