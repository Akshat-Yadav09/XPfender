using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VidPlayandScene : MonoBehaviour
{
    [SerializeField] private GameObject VidCanvas;
    [SerializeField] private GameObject TeleCanvas1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VidPlay()
    {
        StartCoroutine(playvidWithDelay());
    }

    private IEnumerator playvidWithDelay()
    {
        VidCanvas.SetActive(true);
        TeleCanvas1.SetActive(false);
        yield return new WaitForSeconds(2f);
        VidCanvas.SetActive(false);
        SceneManager.LoadScene("Main Fight Area");
    }
}
