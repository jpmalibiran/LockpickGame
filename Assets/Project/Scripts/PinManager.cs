using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages the pins in the lock
public class PinManager : MonoBehaviour{

    [Header("Prefabs")]
    [SerializeField] private GameObject m_Long_Key_Pin_Prefab;
    [SerializeField] private GameObject m_Medium_Key_Pin_Prefab;
    [SerializeField] private GameObject m_Short_Key_Pin_Prefab;

    [Header("References")]
    [SerializeField] private KeyPinScr[] m_Key_Pin_Array = new KeyPinScr[5];
    [SerializeField] private Transform[] m_Key_Pin_Container_Array = new Transform[5];
    [SerializeField] private Transform[] m_Driver_Pin_Array = new Transform[5];
    [SerializeField] private AudioManager m_Audio_Manager_Ref;
    [SerializeField] private float m_Pick_Root_To_Tip_Distance_Offset = 0.9f;

    private bool b_Pins_Ready = false;

    //Returns the Y position value of a key pin bottom at specified key pin lane (x position)
    private float GetPinHeightAtLane(int insert_Lane) {

        if (m_Key_Pin_Array.Length < 0) {
            Debug.LogError("[Error] No pin objects assigned to array! Returning 0...");
            return 0;
        }

        if (insert_Lane <= 0 || insert_Lane > 5) {
            Debug.LogError("[Error] Invalid lane, " + insert_Lane + "; could not find pin height. Returning 0...");
            return 0;
        }

        if (m_Key_Pin_Array[insert_Lane - 1]) {
            return m_Key_Pin_Array[insert_Lane - 1].GetTipHeight();
        }
        else {
            Debug.LogError("[Error] Object missing from array reference! Returning 0...");
            return 0;
        }
    }

    //Returns a new movement limit upon the x axis to the lockpick tool IF there are pins obstructing the way. Prevents the lockpick from clipping through pins when coming from the front (west bound).
    public float DetermineNewPickLowerBound(float insert_Lockpick_Y_Position, int insert_Lockpick_Lane) {

        if (insert_Lockpick_Lane < 0 || insert_Lockpick_Lane > 6) {
            Debug.LogError("[Error] Invalid Lockpick lane! Returning 0...");
            return 0;
        }

        if (insert_Lockpick_Lane == 6 || insert_Lockpick_Lane == 5) {
            return 0;
        }

        //Apply lower bound blockade at first pin lane obstruction
        for (int chkLane = (insert_Lockpick_Lane + 1); chkLane <= 5; chkLane++) {

            //Debug.Log("insert_Lockpick_Y_Position: " + insert_Lockpick_Y_Position + " vs GetPinHeightAtLane(chkLane): " + GetPinHeightAtLane(chkLane));

            if (insert_Lockpick_Y_Position > GetPinHeightAtLane(chkLane) ) {
                //return 0.9f - (chkLane * 0.15f) + m_Pick_Root_To_Tip_Distance_Offset;
                switch (chkLane) {
                    case 1:
                        return 0.75f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 2:
                        return 0.45f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 3:
                        return 0.15f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 4:
                        return -0.15f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 5:
                        return -0.45f + m_Pick_Root_To_Tip_Distance_Offset;
                    default:
                        Debug.LogError("[Error] Unexpected code reached. Returning 0...");
                        return 0;
                }
            }
        }

        //Debug.LogError("[Error] Unexpected code reached. Returning 0...");
        //No obstruction applied
        return 0;
    }

    //Does not work due to Unity Physics being unwieldy. It was suppose to function the same way as DetermineNewPickLowerBound(), but east-bound on the x axis.
    public float DetermineNewPickUpperBound(float insert_Lockpick_Y_Position, int insert_Lockpick_Lane) {
        if (insert_Lockpick_Lane < 0 || insert_Lockpick_Lane > 6) {
            Debug.LogError("[Error] Invalid Lockpick lane! Returning 3...");
            return 3;
        }

        if (insert_Lockpick_Lane == 0 || insert_Lockpick_Lane == 1) {
            return 3;
        }

        for (int chkLane = (insert_Lockpick_Lane - 1); chkLane > 0; chkLane--) {

            if (insert_Lockpick_Y_Position > GetPinHeightAtLane(chkLane)) {
                switch (chkLane) {
                    case 1:
                        return 0.45f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 2:
                        return 0.15f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 3:
                        return -0.15f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 4:
                        return -0.45f + m_Pick_Root_To_Tip_Distance_Offset;
                    case 5:
                        return -0.75f + m_Pick_Root_To_Tip_Distance_Offset;
                    default:
                        Debug.LogError("[Error] Unexpected code reached. Returning 3...");
                        return 3;
                }
            }
        }

        //No obstruction applied
        return 3;
    }

    //Custom physics used because Unity physics was unwieldy. It allows the lockpick to precisely move pins upwards without clipping through them.
    public void CheckPinCollision(float insert_Lockpick_Y_Position, int insert_Lockpick_Lane) {
        if (insert_Lockpick_Lane < 0 || insert_Lockpick_Lane > 6) {
            Debug.LogError("[Error] Invalid Lockpick lane! Aborting Operation...");
            return;
        }

        //There are no pins in these lanes
        if (insert_Lockpick_Lane == 0 || insert_Lockpick_Lane == 6) {
            return;
        }

        float y_Elevation_Difference = 0;
        float pin_Height = GetPinHeightAtLane(insert_Lockpick_Lane);

        if (insert_Lockpick_Y_Position > pin_Height) {
            y_Elevation_Difference = insert_Lockpick_Y_Position - pin_Height; 

            if (m_Key_Pin_Array[insert_Lockpick_Lane - 1].transform.localPosition.y + y_Elevation_Difference >= m_Key_Pin_Array[insert_Lockpick_Lane - 1].GetTensionLockHeight()) {
                m_Key_Pin_Array[insert_Lockpick_Lane - 1].transform.localPosition = new Vector3(0, m_Key_Pin_Array[insert_Lockpick_Lane - 1].GetTensionLockHeight(), 0);
            }
            else {
                m_Key_Pin_Array[insert_Lockpick_Lane - 1].transform.localPosition = new Vector3(0, m_Key_Pin_Array[insert_Lockpick_Lane - 1].transform.localPosition.y + y_Elevation_Difference, 0);
            }
            
        }
    }

    //Can apply or release tension to the key pins which would allow them to lock (get stuck) along the shear line.
    public void SetPinTension(bool set) {
        for (int pin_Index = 0; pin_Index < m_Key_Pin_Array.Length; pin_Index++) {
            m_Key_Pin_Array[pin_Index].ApplyTension(set);
        }
    }

    //Generates new pins of varying sizes and deletes old pins if there are any.
    public void GenerateRandomPins() {
        GameObject temp_Pin_Holder;
        b_Pins_Ready = false;

        if (!m_Long_Key_Pin_Prefab || !m_Medium_Key_Pin_Prefab || !m_Short_Key_Pin_Prefab) {
            Debug.LogError("[Error] Incomplete key pin prefab collection. Aborting operation...");
            return;
        }

        for (int containerIndex = 0; containerIndex < m_Key_Pin_Container_Array.Length; containerIndex++) {

            if (!m_Key_Pin_Container_Array[containerIndex]) {
                Debug.LogError("[Error] ...");
                return;
            }
            if (!m_Driver_Pin_Array[containerIndex]) {
                Debug.LogError("[Error] ...");
                return;
            }

            switch (Random.Range(1, 4)) {
                case 1:
                    //Destroy existing pin if it exists; Only one pin per lane allowed.
                    if (m_Key_Pin_Array[containerIndex]) {
                        Destroy(m_Key_Pin_Array[containerIndex].gameObject);
                    }
                    temp_Pin_Holder = Instantiate(m_Long_Key_Pin_Prefab);
                    temp_Pin_Holder.transform.SetParent(m_Key_Pin_Container_Array[containerIndex]);
                    temp_Pin_Holder.transform.localPosition = Vector3.zero;
                    m_Key_Pin_Array[containerIndex] = temp_Pin_Holder.GetComponent<KeyPinScr>();
                    m_Key_Pin_Array[containerIndex].AssignDriverPin(m_Driver_Pin_Array[containerIndex]);
                    m_Key_Pin_Array[containerIndex].AssignAudioManager(m_Audio_Manager_Ref);
                    //temp_Pin_Holder.GetComponent<KeyPinScr>().AssignKeyLane();
                    break;
                case 2:
                    if (m_Key_Pin_Array[containerIndex]) {
                        Destroy(m_Key_Pin_Array[containerIndex].gameObject);
                    }
                    temp_Pin_Holder = Instantiate(m_Medium_Key_Pin_Prefab);
                    temp_Pin_Holder.transform.SetParent(m_Key_Pin_Container_Array[containerIndex]);
                    temp_Pin_Holder.transform.localPosition = Vector3.zero;
                    m_Key_Pin_Array[containerIndex] = temp_Pin_Holder.GetComponent<KeyPinScr>();
                    m_Key_Pin_Array[containerIndex].AssignDriverPin(m_Driver_Pin_Array[containerIndex]);
                    m_Key_Pin_Array[containerIndex].AssignAudioManager(m_Audio_Manager_Ref);
                    break;
                case 3:
                    if (m_Key_Pin_Array[containerIndex]) {
                        Destroy(m_Key_Pin_Array[containerIndex].gameObject);
                    }
                    temp_Pin_Holder = Instantiate(m_Short_Key_Pin_Prefab);
                    temp_Pin_Holder.transform.SetParent(m_Key_Pin_Container_Array[containerIndex]);
                    temp_Pin_Holder.transform.localPosition = Vector3.zero;
                    m_Key_Pin_Array[containerIndex] = temp_Pin_Holder.GetComponent<KeyPinScr>();
                    m_Key_Pin_Array[containerIndex].AssignDriverPin(m_Driver_Pin_Array[containerIndex]);
                    m_Key_Pin_Array[containerIndex].AssignAudioManager(m_Audio_Manager_Ref);
                    break;
                default:
                    Debug.LogError("[Error] ...");
                    return;
            }
        }

        b_Pins_Ready = true;
    }

    //Returns whether the key pins are ready to be played with.
    public bool ArePinsReady() {
        return b_Pins_Ready;
    }

    //Checks if all of the key pins are locked at the shear line. This would mean the player successfully picked the locks.
    public bool AreAllPinsLocked() {
        for (int pinIndex = 0; pinIndex < m_Key_Pin_Array.Length; pinIndex++) {
            if (m_Key_Pin_Array[pinIndex]) {
                if (!m_Key_Pin_Array[pinIndex].IsTensionLocked()) {
                    return false;
                }
            }
        }

        return true;
    }
}
