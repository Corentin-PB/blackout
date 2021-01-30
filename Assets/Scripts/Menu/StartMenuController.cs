using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Menu
{
    /// <summary>Class <c>MainMenuController</c> control the button of the main menu</summary>
    ///
    public class StartMenuController : MonoBehaviour
    {
        /// <summary>Function <c>PlayGame</c> used to launch the game scene</summary>
        ///
        public void PlayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        /// <summary>Function <c>QuitGame</c> used to quit the game</summary>
        ///
        public void QuitGame()
        {
            Debug.Log("Quit game");
            Application.Quit();
        }
    }
}