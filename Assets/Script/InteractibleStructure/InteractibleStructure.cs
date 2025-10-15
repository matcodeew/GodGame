using System;
using UnityEngine;

public class InteractibleStructure : MonoBehaviour
{
    public string interactStructName;
    public Vector2 buildingWorldPos;
    [SerializeField] public RessourceType type;

    [Header("Sound")]
    [SerializeField] private AudioSource interactAudio;
    [SerializeField] private float interactAudioPlayTime;

    private void SetWorldPos()
    {
        buildingWorldPos = new Vector2(transform.position.x, transform.position.z);
    }
    private void Awake()
    {
        SetWorldPos();

    }

    private void Start()
    {
        RessourceLocator.Bind(this, type);
    }

    public virtual void Interact(Villager villager)
    {
        InteractSong();
    }

    public virtual void DestroyStructure()
    {
        RessourceLocator.UnBind(this, type);

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