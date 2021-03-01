using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour{
    [SerializeField] private AudioSource m_Audio_Src_Ref;
    [SerializeField] private AudioClip m_Audio_Tension_Lock;
    [SerializeField] private AudioClip m_Audio_Pin_Fall;
    [SerializeField] private AudioClip m_Audio_Plug_Turn;

    //Plays the specified sound clip
    public void Play(int chosen_Audio) {
        if (!m_Audio_Src_Ref) {
            return;
        }

        switch (chosen_Audio) {
            case 1:
                if (m_Audio_Tension_Lock) {
                    m_Audio_Src_Ref.clip = m_Audio_Tension_Lock;
                    m_Audio_Src_Ref.Play();
                    //AudioSource.PlayClipAtPoint(m_Audio_Tension_Lock, m_Audio_Src_Ref.transform.position);
                }
                break;
            case 2:
                if (m_Audio_Pin_Fall) {
                    m_Audio_Src_Ref.clip = m_Audio_Pin_Fall;
                    m_Audio_Src_Ref.Play();
                    //AudioSource.PlayClipAtPoint(m_Audio_Pin_Fall, m_Audio_Src_Ref.transform.position);
                }
                break;
            default:
                break;
        }

    }
}
