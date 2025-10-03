using System;
using UnityEngine;

public class InteractibleStructure : MonoBehaviour
{

    public string interactStructName;
    public Vector2 buildingWorldPos;

    [Header("Sound")]
    [SerializeField] private AudioSource interactAudio;
    [SerializeField] private float interactAudioPlayTime;

    private void SetWorldPos()
    {
        buildingWorldPos = transform.position;
    }
    private void Awake()
    {
        SetWorldPos();
    }

    public virtual void Interact(Villager villager)
    {
        InteractSong();
        print(interactStructName);
    }

    public virtual void DestroyStructure()
    {
        print($"Destroy {interactStructName}");
        EventBus.Publish(EventType.DestroyInteractibleStruct, this);
        Destroy(gameObject);
    }

    public virtual void InteractSong()
    {
        if (interactAudio == null) return;
        interactAudio.Play();
        TimerManager.StartTimer(interactAudioPlayTime, new Action(() => interactAudio.Stop()));
    }
}