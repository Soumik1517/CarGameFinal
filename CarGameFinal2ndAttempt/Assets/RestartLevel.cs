using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class RestartLevel : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1"; // Scene to load
    [SerializeField] private TextMeshProUGUI myText;
    [SerializeField] private TextMeshProUGUI myText2;

    void Start()
    {
        myText.gameObject.SetActive(false);
        myText2.gameObject.SetActive(false);
    }
    private void Reset()
    {
        // Automatically set the collider as trigger if not already
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Update()
    {
         if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape pressed — quitting game.");
            Application.Quit();
        }
            // Quit the application
            
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is in the "Car" layer
        if (other.gameObject.layer == LayerMask.NameToLayer("car"))
        {

            Debug.Log("Car entered trigger, loading scene: " + sceneName);
            myText.gameObject.SetActive(true);
            StartCoroutine(FreezeForSeconds());   
        }
        else if ((other.gameObject.layer == LayerMask.NameToLayer("BOT")))
        {
                myText2.gameObject.SetActive(true);
                StartCoroutine(FreezeForSeconds());
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FreezeForSeconds()
    {
        // Temporarily disable player input or other actions
        
        Time.timeScale=0;
        // Wait for the specified duration (3 seconds)
        yield return new WaitForSecondsRealtime(3);
        Time.timeScale=1;

        // Re-enable player input and other actions
        LoadScene();
    }


}