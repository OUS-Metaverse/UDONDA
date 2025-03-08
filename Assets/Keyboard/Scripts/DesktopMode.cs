
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DesktopMode : UdonSharpBehaviour
{
    [SerializeField] private Keyboard keyboard;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject StartButton;
    private bool _buttonActive = true;
    private bool ButtonActive {
        get => _buttonActive;
        set {
            _buttonActive = value;
            StartButton.SetActive(_buttonActive);
        }
    }
    private bool isAttached = false;

    void Start()
    {
        gameObject.SetActive(!Networking.LocalPlayer.IsUserInVR());
    }

    public void AttachPlayer()
    {
        if (gameManager.GameStarted) return;
        Networking.LocalPlayer.UseAttachedStation();
        isAttached = true;
    }

    public void DetachPlayer()
    {
        if (!isAttached) return;
        Networking.LocalPlayer.TeleportTo(transform.position, transform.rotation);
        ButtonActive = false;
        isAttached = false;
    }

    void Update()
    {
        if (!ButtonActive)
        {
            ButtonActive = true;
        }
    }
}
