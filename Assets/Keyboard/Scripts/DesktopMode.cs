
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DesktopMode : UdonSharpBehaviour
{
    [SerializeField] private Keyboard keyboard;
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
        Networking.LocalPlayer.UseAttachedStation();
        isAttached = true;
    }

    void Update()
    {
        if (!ButtonActive)
        {
            ButtonActive = true;
        }
        if (!keyboard.isDesktopMode) return;
        if (!isAttached) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Networking.LocalPlayer.TeleportTo(transform.position, transform.rotation);
            ButtonActive = false;
            isAttached = false;
        }
    }
}
