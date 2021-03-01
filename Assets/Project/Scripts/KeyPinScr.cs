using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles the individual key pins
public class KeyPinScr : MonoBehaviour{
    
    [SerializeField] private Transform m_Tip_Ref;
    [SerializeField] private Transform m_Assigned_Driver_Pin_Ref;
    [SerializeField] private Rigidbody m_Rigidbody_Ref;

    //[SerializeField] private KeyPinType m_Type = KeyPinType.Default;
    [SerializeField] private Vector2 m_Key_Lane = Vector2.zero; //The two values represent the min and max range of the lane along the x axis.
    [SerializeField] private float m_Tension_Lock_Height = 0;
    [SerializeField] private bool b_Tension_Locked = false;
    [SerializeField] private bool b_Tension_Applied = false;

    private AudioManager m_Audio_Manager_Ref;
    private float m_Driver_Pin_Release_Elevation = 0;

    private void FixedUpdate() {
        CheckElevation();
    }

    //Checks should this key pin be locked factoring in the height of the shear line and whether tension is being applied.
    private void CheckElevation() {
        if (m_Assigned_Driver_Pin_Ref) {
            if (m_Assigned_Driver_Pin_Ref.position.y < this.transform.position.y) {
                m_Assigned_Driver_Pin_Ref.localPosition = new Vector3(m_Assigned_Driver_Pin_Ref.position.x, 0, 0);
            }
        }
        else {
            Debug.LogWarning("[Warning] Driver Pin reference missing!");
        }

        if (b_Tension_Applied) {
            if (this.transform.localPosition.y > m_Tension_Lock_Height) {
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, m_Tension_Lock_Height, this.transform.localPosition.z);
            }
            if (this.transform.localPosition.y >= m_Tension_Lock_Height) {
                LockInPlace();
            }
        }
    }

    //Locks this pin in place. Usually as a result of meeting the shearline while tension is applied.
    private void LockInPlace() {
        //Pin is already Locked
        if (b_Tension_Locked) {
            return;
        }

        if (!m_Rigidbody_Ref) {
            if (this.GetComponent<Rigidbody>()) {
                m_Rigidbody_Ref = this.GetComponent<Rigidbody>();
            }
            else {
                Debug.LogError("[Error] Rigidbody reference missing! Aborting operation.");
                return;
            }
        }

        if (m_Audio_Manager_Ref) {
            m_Audio_Manager_Ref.Play(1);
        }

        b_Tension_Locked = true;
        m_Rigidbody_Ref.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        if (m_Assigned_Driver_Pin_Ref) {
            m_Assigned_Driver_Pin_Ref.localPosition = new Vector3(m_Assigned_Driver_Pin_Ref.position.x, 0, 0);
        }
        else {
            Debug.LogWarning("[Warning] Driver Pin reference missing!");
        }

    }

    //Releases the tension lock of this pin. Can happen if user releases tension.
    private void ReleaseLock() {
        if (!m_Rigidbody_Ref) {
            if (this.GetComponent<Rigidbody>()) {
                m_Rigidbody_Ref = this.GetComponent<Rigidbody>();
            }
            else {
                Debug.LogError("[Error] Rigidbody reference missing! Aborting operation.");
                return;
            }
        }

        b_Tension_Locked = false;
        m_Rigidbody_Ref.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;;
    }

    //Sets whether tension lock should be applied or release.
    public void SetTensionLock(bool set) {
        if (set) {
            LockInPlace();
        }
        else {
            ReleaseLock();
        }
    }

    //Sets whether tension should be applied or not.
    public void ApplyTension(bool set) {
        b_Tension_Applied = set;
    }

    //Returns the position of the bottom of a key pin.
    public float GetTipHeight() {
        if (!m_Tip_Ref) {
            Debug.LogError("[Error] Could not find tip reference! Returning this.transform.position.y instead...");
            return this.transform.position.y;
        }

        return m_Tip_Ref.position.y;
    }

    //Returns the height value of the shear line
    public float GetTensionLockHeight() {
        return m_Tension_Lock_Height;
    }

    //Checks whether given x value is in the same key lane as this key pin
    public bool IsWithinLane(float insert_X_Value) {
        if (insert_X_Value > m_Key_Lane.x && insert_X_Value < m_Key_Lane.y) {
            return true;
        }
        return false;
    }

    //Returns whether this pin is tension locked at the shear line. This is used to check victory status.
    public bool IsTensionLocked() {
        return b_Tension_Locked;
    }

    //Provides a reference to a driver pin. Driver Pins are what goes directly above the key pins.
    public void AssignDriverPin(Transform insert_Driver_Pin) {
        m_Assigned_Driver_Pin_Ref = insert_Driver_Pin;
    }

    //Describes the perimeter of the area or lane that this key pin resides in.
    public void AssignKeyLane(float insert_X_Min, float insert_X_Max) {
        m_Key_Lane.x = insert_X_Min;
        m_Key_Lane.y = insert_X_Max;
    }

    //Provides a reference to the audio manager so the key pins can make noises upon collision.
    public void AssignAudioManager(AudioManager insert_Manager) {
        m_Audio_Manager_Ref = insert_Manager;
    }

    //Plays a sound when this key pinc ollides with a wall.
    void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "Wall") { //Note: Compaing tags using == is inefficient
            if (m_Audio_Manager_Ref) {
                m_Audio_Manager_Ref.Play(2);
            }
        }
    }

}
