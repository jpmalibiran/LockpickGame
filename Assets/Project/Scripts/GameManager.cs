using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Manages the core gameplay
public class GameManager : MonoBehaviour{

    [Header("References")]
    [SerializeField] private PinManager m_Pin_Manager_Ref;
    [SerializeField] private MeshRenderer m_Plug_Visual_Ref;
    [SerializeField] private MeshRenderer m_Shell_Visual_Ref;
    [SerializeField] private Material m_Opaque_Grey;
    [SerializeField] private Material m_Opaque_Yellow;
    [SerializeField] private Material m_Translucent_Grey;
    [SerializeField] private Material m_Translucent_Yellow;
    [SerializeField] private Transform m_Plug_Contianer_Ref;
    [SerializeField] private Transform m_Minigame_Container_Ref;
    [SerializeField] private Text m_Difficulty_Dropdown_Result;
    [SerializeField] private Text m_UI_Timer;

    [Header("Properties")]
    [SerializeField] private float m_Time_Limit = 30;
    [SerializeField] private float m_Time_Left = 30;
    //[SerializeField] private float m_Time_Elapsed = 0;

    [Header("Settings")]
    [SerializeField] private GameState m_State = GameState.Roaming;
    [SerializeField] private LockDifficulty m_Lockpick_Game_Difficulty = LockDifficulty.Easy;

    private bool b_Timer_Started = false;

    private void Update() {
        if (b_Timer_Started && m_State == GameState.Lockpicking) {
            CountdownTimer();
            CheckVictory();
        }
    }

    //Subtracts the timer time left by delta time to convey the gameplay of time limit
    private void CountdownTimer() {
        m_Time_Left = m_Time_Left - Time.deltaTime;

        if (m_Time_Left < 0) {
            m_Time_Left = 0;
            ConveyDefeat();
        }
        else {
            m_UI_Timer.text = "Time Remaining: " + m_Time_Left.ToString();
        }
    }

    //Checks whether all key pins are locked by tension at the shear line. 
    //Note: Key pins are the bronze cylinders that prevent the key plug from turning. 
    //The Shear line is the line that separates the key plug from the rest of the lock (shell). Severing the Key Pin link between the key plug and key shell allows the lock to turn.
    private void CheckVictory() {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] PinManager.cs reference missing! Aborting operation...");
            return;
        }

        if (m_Pin_Manager_Ref.AreAllPinsLocked()) {
            VictorySequence();
        }
    }

    //Code that must be executed when Victory condition is met
    private void VictorySequence() {
        b_Timer_Started = false;
        Debug.Log("[Notice] Victory! Player has picked the lock!");
        m_UI_Timer.text = "Time Remaining: " + m_Time_Left.ToString() + "\nSuccess! You have picked the lock!";
        m_State = GameState.Victory;
    }

    //Code that must be executed when Defeat condition is met
    private void ConveyDefeat() {
        b_Timer_Started = false;
        Debug.Log("[Notice] Defeat! Player has run out of time!");
        m_UI_Timer.text = "Time Remaining: 0\nGame Over! You ran out of time!";
        m_State = GameState.Defeat;
    }

    //Sets the visibility of the lockpicking minigame window
    public void SetMiniGameWindow(bool set) {
        if (m_Minigame_Container_Ref) {
            m_Minigame_Container_Ref.gameObject.SetActive(set);
        }
    }

    //Starts the countdown timer of player time limit to complete the lock
    public void StartTimer(bool set) {
        b_Timer_Started = set;
    }

    //Initiates a new game, generates random pins of varying sizes, resets the timer
    public void NewGame() {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] ...");
            return;
        }

        m_Pin_Manager_Ref.GenerateRandomPins();
        SetDifficulty(m_Lockpick_Game_Difficulty);
        m_State = GameState.Lockpicking;
        m_Time_Left = m_Time_Limit;
        StartTimer(true);
    }

    //Returns the initial state of the game
    public GameState GetGameState() {
        return m_State;
    }

    //Checks selected dificulty from the difficulty drop down UI and applies the corresponding settings.
    public void ReadDifficulty() {

        if (!m_Shell_Visual_Ref || !m_Plug_Visual_Ref) {
            Debug.LogError("[Error] Lock Visual references incomplete! Could not set game difficulty.");
            return;
        }
        if (!m_Opaque_Grey || !m_Opaque_Yellow || !m_Translucent_Grey || !m_Translucent_Yellow) {
            Debug.LogError("[Error] Material references incomplete! Could not set game difficulty.");
            return;
        }
        if (!m_Difficulty_Dropdown_Result) {
            Debug.LogError("{Error] ...");
            return;
        }

        switch (m_Difficulty_Dropdown_Result.text) {
            case "Easy":
                m_Lockpick_Game_Difficulty = LockDifficulty.Easy;
                m_Plug_Visual_Ref.material = m_Translucent_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
            case "Medium":
                m_Lockpick_Game_Difficulty = LockDifficulty.Medium;
                m_Plug_Visual_Ref.material = m_Opaque_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
            case "Hard":
                m_Lockpick_Game_Difficulty = LockDifficulty.Hard;
                m_Plug_Visual_Ref.material = m_Opaque_Yellow;
                m_Shell_Visual_Ref.material = m_Opaque_Grey;
                break;
            default:
                m_Plug_Visual_Ref.material = m_Translucent_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
        }
    }

    //Applies selected difficulty setting
    public void SetDifficulty(LockDifficulty insert_Difficulty) {

        if (!m_Shell_Visual_Ref || !m_Plug_Visual_Ref) {
            Debug.LogError("[Error] Lock Visual references incomplete! Could not set game difficulty.");
            return;
        }
        if (!m_Opaque_Grey || !m_Opaque_Yellow || !m_Translucent_Grey || !m_Translucent_Yellow) {
            Debug.LogError("[Error] Material references incomplete! Could not set game difficulty.");
            return;
        }

        switch (insert_Difficulty) {
            case LockDifficulty.Default:
                m_Lockpick_Game_Difficulty = LockDifficulty.Easy;
                m_Plug_Visual_Ref.material = m_Translucent_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
            case LockDifficulty.Easy:
                m_Lockpick_Game_Difficulty = LockDifficulty.Easy;
                m_Plug_Visual_Ref.material = m_Translucent_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
            case LockDifficulty.Medium:
                m_Lockpick_Game_Difficulty = LockDifficulty.Medium;
                m_Plug_Visual_Ref.material = m_Opaque_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
            case LockDifficulty.Hard:
                m_Lockpick_Game_Difficulty = LockDifficulty.Hard;
                m_Plug_Visual_Ref.material = m_Opaque_Yellow;
                m_Shell_Visual_Ref.material = m_Opaque_Grey;
                break;
            default:
                m_Lockpick_Game_Difficulty = LockDifficulty.Easy;
                m_Plug_Visual_Ref.material = m_Translucent_Yellow;
                m_Shell_Visual_Ref.material = m_Translucent_Grey;
                break;
        }
    }

}
