using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{

    [SerializeField] private GameManager m_Game_Manager_Ref;

    // Update is called once per frame
    private void Update(){
        ProcessInput();
    }

    //Chack for user input. Note: For user input regarding the lockpick minigame see PickController.cs
    private void ProcessInput() {
        if (!m_Game_Manager_Ref) {
            Debug.Log("[Error] Game Manager reference missing! Aborting operation...");
            return;
        }
        if (m_Game_Manager_Ref.GetGameState() != GameState.Roaming) {
            return;
        }

        //Activates the lockpicking mini game
        if (Input.GetKeyUp(KeyCode.E)) {
            m_Game_Manager_Ref.SetMiniGameWindow(true);
        }

    }


}
