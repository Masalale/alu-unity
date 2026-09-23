using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Material trapMat;
    public Material goalMat;
    public Toggle colorblindMode;

    public void PlayMaze()
    {
        if (colorblindMode != null && colorblindMode.isOn)
        {
            if (trapMat != null)
            {
                trapMat.color = new Color32(255, 112, 0, 1);
            }
            if (goalMat != null)
            {
                goalMat.color = Color.blue;
            }
        }
        SceneManager.LoadScene("maze");
    }

    public void QuitMaze()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
