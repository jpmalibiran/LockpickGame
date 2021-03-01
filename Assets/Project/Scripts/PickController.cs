using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls the diamond tipped lockpick. A lot of collision that used Unity physics was substituted by my own created collision checks. This is because Unity physics did not behave properly- collisions clipped through.
public class PickController : MonoBehaviour{

    //Note: I had to implement my own collision detection because Unity's physics is unreliable.

    [Header("References")]
    [SerializeField] private GameManager m_Game_Manager_Ref;
    [SerializeField] private PinManager m_Pin_Manager_Ref;
    [SerializeField] private Transform m_Pick_Ref;          //The lockpick itself. This is used to convey left or right movement on mouse horizontal axis.
    [SerializeField] private Transform m_Pick_Axis_Ref;     //The part of the lockpick that represents the axis of rotation. This is controlled by mouse vertical axis.
    [SerializeField] private Transform m_Pick_Tip_Ref;

    [Header("Settings")]
    [SerializeField] private float m_Pick_Movement_Upper_Bound = 3;
    [SerializeField] private float m_Pick_Movement_Default_Lower_Bound = 0;
    [SerializeField] private float m_Pick_Movement_Speed = 20;

    [SerializeField] private float m_Pick_Rotation_Upper_Bound = 7;
    [SerializeField] private float m_Pick_Rotation_Lower_Bound = 340;
    [SerializeField] private float m_Pick_Rotation_Speed = 200;

    [Header("Debug")]
    [SerializeField] private float m_Pick_Movement_Lower_Bound = 0;
    [SerializeField] private float m_Pick_Movement;
    [SerializeField] private float m_Pick_Rotation;
    [SerializeField] private float m_Tip_X = 0;
    [SerializeField] private float m_Tip_Y = 0;
    [SerializeField] private int m_Pick_Lane = 0;
    [SerializeField] private bool m_Tension_Applied = false;

    private void Start() {
        SetPickStartPosition();
    }

    private void Update() {
        InputListener();
    }

    private void FixedUpdate() {
        TrackTip();                     //Tracks the position of the tip of the lockpick. This is used in custom physics implementation.
        DetermineLowerBound();          //Enforces custom physics upon the lock pick should a key pin be obstructing the way. This prevents the lock pick from clipping through pins.
        DetermineUpperBound();          //This doesn't work: so unfortunately reverse raking is possible and breaks the game and makes it not fun and makes it harder to apply a player skill level mechanic.
        EnforceUpwardPinCollision();    //Enforces custom physics between key pin and lockpick such that the lockpick can raise the key pin from the bottom without clipping through.
    }

    //Resets the lockpick at its starting position.
    private void SetPickStartPosition() {
        if (!m_Pick_Ref) {
            Debug.LogError("[Error] Reference to lockpick object missing! Aborting operation...");
            return;
        }

        m_Pick_Movement_Lower_Bound = m_Pick_Movement_Default_Lower_Bound;
        m_Pick_Ref.position = new Vector3(3, -0.2f, 0);
        m_Pick_Ref.localEulerAngles = Vector3.zero;
    }

    //Enforces custom physics upon the lock pick should a key pin be obstructing the way west bound. This prevents the lock pick from clipping through pins.
    private void DetermineLowerBound() {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] Reference to PinManager.cs missing! Aborting operation...");
            return;
        }
        if (!m_Game_Manager_Ref) {
            Debug.LogError("[Error] Reference to GameManager.cs missing! Aborting operation...");
            return;
        }
        if (m_Game_Manager_Ref.GetGameState() != GameState.Lockpicking) {
            return;
        }
        if (!m_Pin_Manager_Ref.ArePinsReady()) {
            Debug.Log("[Notice] Key Pins are not yet ready. Aborting operation...");
            return;
        }

        m_Pick_Movement_Lower_Bound = m_Pin_Manager_Ref.DetermineNewPickLowerBound(m_Pick_Tip_Ref.position.y, m_Pick_Lane);
    }

    //Dysfunctional. Enforces custom physics upon the lock pick should a key pin be obstructing the way east bound. This was suppose to prevent the lock pick from clipping through pins.
    private void DetermineUpperBound() {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] Reference to PinManager.cs missing! Aborting operation...");
            return;
        }
        if (!m_Game_Manager_Ref) {
            Debug.LogError("[Error] Reference to GameManager.cs missing! Aborting operation...");
            return;
        }
        if (m_Game_Manager_Ref.GetGameState() != GameState.Lockpicking) {
            return;
        }
        if (!m_Pin_Manager_Ref.ArePinsReady()) {
            Debug.Log("[Notice] Key Pins are not yet ready. Aborting operation...");
            return;
        }

        m_Pick_Movement_Upper_Bound = m_Pin_Manager_Ref.DetermineNewPickUpperBound(m_Pick_Tip_Ref.position.y, m_Pick_Lane);
    }

    //Enforces custom physics between key pin and lockpick such that the lockpick can raise the key pin from the bottom without clipping through.
    private void EnforceUpwardPinCollision() {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] Reference to PinManager.cs missing! Aborting operation...");
            return;
        }
        if (!m_Game_Manager_Ref) {
            Debug.LogError("[Error] Reference to GameManager.cs missing! Aborting operation...");
            return;
        }
        if (m_Game_Manager_Ref.GetGameState() != GameState.Lockpicking) {
            return;
        }


        if (!m_Pin_Manager_Ref.ArePinsReady()) {
            Debug.Log("[Notice] Key Pins are not yet ready. Aborting operation...");
            return;
        }

        m_Pin_Manager_Ref.CheckPinCollision(m_Pick_Tip_Ref.position.y, m_Pick_Lane);
    }

    //Handles user input. User controls the lockpick with the mouse, user can apply or release tension by holding the spacebar.
    private void InputListener() {

        if (!m_Game_Manager_Ref) {
            Debug.LogError("[Error] Reference to GameManager.cs missing! Aborting operation...");
            return;
        }
        if (m_Game_Manager_Ref.GetGameState() != GameState.Lockpicking) {
            return;
        }

        float initial_Angle;

        m_Pick_Movement = Input.GetAxis("Mouse X");
        m_Pick_Rotation = Input.GetAxis("Mouse Y");

        if (Input.GetKeyDown(KeyCode.Space)) {
            m_Tension_Applied = true;
            ApplyTension(true);
        }
        else if (Input.GetKeyUp(KeyCode.Space)) {
            m_Tension_Applied = false;
            ApplyTension(false);
        }

        //====Calculate horizontal movement of lockpick====

        if ((m_Pick_Movement > 0 && m_Pick_Ref.transform.localPosition.x < m_Pick_Movement_Upper_Bound) ||
            (m_Pick_Movement < 0 && m_Pick_Ref.transform.localPosition.x > m_Pick_Movement_Lower_Bound)) {
            m_Pick_Ref.Translate(new Vector3(m_Pick_Movement, 0, 0) * m_Pick_Movement_Speed * Time.deltaTime);
        }

        if (m_Pick_Ref.transform.localPosition.x > m_Pick_Movement_Upper_Bound) {
            m_Pick_Ref.transform.localPosition = new Vector3(m_Pick_Movement_Upper_Bound, -0.24f, 0);
        }
        else if (m_Pick_Ref.transform.localPosition.x < m_Pick_Movement_Lower_Bound) {
            m_Pick_Ref.transform.localPosition = new Vector3(m_Pick_Movement_Lower_Bound, -0.24f, 0);
        }

        //====Calculate rotational movement of lockpick====
        //Note: Negative number representation in Unity euler angles are actually positive numbers subtracted from 360 when it comes to calculations.

        if (m_Pick_Axis_Ref.transform.localEulerAngles.z < 0) {
            if (Mathf.Abs(m_Pick_Axis_Ref.transform.localEulerAngles.z) > 360) {
                initial_Angle = 360 - (Mathf.Abs(m_Pick_Axis_Ref.transform.localEulerAngles.z) % 360);
            }
            else {
                initial_Angle = 360 + m_Pick_Axis_Ref.transform.localEulerAngles.z;
            }
        }
        else {
            if (Mathf.Abs(m_Pick_Axis_Ref.transform.localEulerAngles.z) > 360) {
                initial_Angle = m_Pick_Axis_Ref.transform.localEulerAngles.z % 360;
            }
            else {
                initial_Angle = m_Pick_Axis_Ref.transform.localEulerAngles.z;
            }
        }

        if (initial_Angle >= 0 && initial_Angle <= m_Pick_Rotation_Upper_Bound) {
            m_Pick_Axis_Ref.localEulerAngles = new Vector3(0, 0, m_Pick_Axis_Ref.localEulerAngles.z + (m_Pick_Rotation * m_Pick_Rotation_Speed * Time.deltaTime) );
        }
        else if (initial_Angle < 360 && initial_Angle >= m_Pick_Rotation_Lower_Bound) { 
            m_Pick_Axis_Ref.localEulerAngles = new Vector3(0, 0, m_Pick_Axis_Ref.localEulerAngles.z + (m_Pick_Rotation * m_Pick_Rotation_Speed * Time.deltaTime) );
        }
        else if (initial_Angle > m_Pick_Rotation_Upper_Bound && initial_Angle < 180) {
            m_Pick_Axis_Ref.localEulerAngles = new Vector3(0, 0, m_Pick_Rotation_Upper_Bound - 1);
        }
        else if (initial_Angle < m_Pick_Rotation_Lower_Bound && initial_Angle > 180) {
            m_Pick_Axis_Ref.localEulerAngles = new Vector3(0, 0, m_Pick_Rotation_Lower_Bound + 1);
        }
    }

    //Tracks the position of the lockpick diamond tip. This is used in custom physics collision checks.
    private void TrackTip() {
        if (!m_Pick_Tip_Ref) {
            Debug.LogError("[Error] Reference to lockpick tip missing! Aborting operation...");
            return;
        }
        m_Tip_X = m_Pick_Tip_Ref.position.x;
        m_Tip_Y = m_Pick_Tip_Ref.position.y;

        if (m_Tip_X > 0.75f) {
            m_Pick_Lane = 0;
        }
        if (m_Tip_X <= 0.75f && m_Tip_X > 0.45f) {
            m_Pick_Lane = 1;
        }
        else if (m_Tip_X <= 0.45f && m_Tip_X > 0.15f) {
            m_Pick_Lane = 2;
        }
        else if (m_Tip_X <= 0.15f && m_Tip_X > -0.15f) {
            m_Pick_Lane = 3;
        }
        else if (m_Tip_X <= -0.15f && m_Tip_X > -0.45f) {
            m_Pick_Lane = 4;
        }
        else if (m_Tip_X <= -0.45f && m_Tip_X > -0.75f) {
            m_Pick_Lane = 5;
        }
        else if (m_Tip_X <= -0.75f) {
            m_Pick_Lane = 6;
        }
    }

    //This conveys the tension tools ability to apply tension to the key pins.
    private void ApplyTension(bool set) {
        if (!m_Pin_Manager_Ref) {
            Debug.LogError("[Error] Reference to PinManager.cs missing! Aborting operation...");
            return;
        }

        if (!m_Pin_Manager_Ref.ArePinsReady()) {
            Debug.Log("[Notice] Key Pins are not yet ready. Aborting operation...");
            return;
        }

        m_Pin_Manager_Ref.SetPinTension(set);
    }

    //Returns what key pin lane the lockpick tip is currently within.
    public int GetCurrentLane() {
        return m_Pick_Lane;
    }
}
