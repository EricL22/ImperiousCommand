using UnityEngine;
using UnityEngine.UI;

public class NButtonSwitcher : MonoBehaviour
{
    public Button[] buttons;  // Array to hold all buttons
    public GameObject[] gameObjects;  // Array to hold all game objects

    // Handle button click
    public void HandleButtonClick()
    {
        // Find the index of the clicked button
        int clickedIndex = System.Array.IndexOf(buttons, UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>());

        if (clickedIndex == -1)
        {
            Debug.LogError("Button not found in the buttons array.");
            return;
        }

        // Disable the clicked button and activate the corresponding game object
        buttons[clickedIndex].interactable = false;
        gameObjects[clickedIndex].SetActive(true);

        // Enable the other buttons and deactivate the corresponding game objects
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i != clickedIndex)
            {
                buttons[i].interactable = true;
                gameObjects[i].SetActive(false);
            }
        }
    }
}
