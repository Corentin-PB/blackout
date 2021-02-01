using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Menu
{
    /// <summary>Class <c>MainMenuController</c> control the button of the main menu</summary>
    ///
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pauseMenu;
        public GameObject patternsUI;
        public VHS.FirstPersonController playerController;
        public VHS.CameraController cameraController;
        public AudioSource playerSource;
        public PlayerSounds soundController;
        
        /// <summary>Function <c>ReturnToMenu</c> used to go back to the main menu</summary>
        ///
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        /// <summary>Function <c>QuitGame</c> used to quit the game</summary>
        ///
        public void QuitGame()
        {
            Debug.Log("Quit game");
            Application.Quit();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                pauseMenu.SetActive(!pauseMenu.activeSelf);
                patternsUI.SetActive(!patternsUI.activeSelf);
                cameraController.enabled = !cameraController.enabled;
                playerController.enabled = !playerController.enabled;
                soundController.enabled = !soundController.enabled;
                playerSource.Stop();

                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                } else {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                Cursor.visible = !Cursor.visible;
            }

            if (!pauseMenu.activeSelf)
            {
                bool keyDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); 
                if( keyDown && ! patternsUI.activeSelf )
                {
                    patternsUI.SetActive( true );
                }
                else if ( !keyDown && patternsUI.activeSelf )
                {
                    patternsUI.SetActive( false );
                }
            }
        }
    }
}