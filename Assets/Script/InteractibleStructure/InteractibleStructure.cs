using System;
using UnityEngine;

public class InteractibleStructure : MonoBehaviour
{

    public string interactStructName;
    public Vector2 buildingWorldPos;

    [Header("Sound")]
    [SerializeField] private AudioSource interactAudio;
    [SerializeField] private float interactAudioPlayTime;


    public virtual void Interact(Villager villager)
    {
        InteractSong();
        print(interactStructName);
    }

    public virtual void DestroyStructure()
    {
        print($"Destroy {interactStructName}");
        Destroy(gameObject);
    }

    public virtual void InteractSong()
    {
        interactAudio.Play();
        TimerManager.StartTimer(interactAudioPlayTime, new Action(() => interactAudio.Stop()));
    }
}