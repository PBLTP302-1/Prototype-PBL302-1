using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public TMP_Text panelName;
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ExitApp()
    {
        Application.Quit();
    }
    public void SetPanelName(string name)
    {
        panelName.text = name;
    }

 
}
