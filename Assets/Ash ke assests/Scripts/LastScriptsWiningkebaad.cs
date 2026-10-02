using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastScriptsWiningkebaad : MonoBehaviour
{
    [SerializeField] private GameObject Start;
    [SerializeField] private GameObject signout_option;
    
    public void StartGame()
    {
        Start.SetActive(true);
    }

    public void Signout()
    {
        signout_option.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
