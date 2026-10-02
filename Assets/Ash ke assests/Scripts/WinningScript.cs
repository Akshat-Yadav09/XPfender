using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningScript : MonoBehaviour
{
    public void last_Scene()
    {
        SceneManager.LoadScene("Last Scene Wining");
    }

    public void first_Scene()
    {
        SceneManager.LoadScene("Main Fight Area");
    }
}
