
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class DesktopMode : UdonSharpBehaviour
{
    [SerializeField] private Keyboard keyboard;

    void Start()
    {
        gameObject.SetActive(!Networking.LocalPlayer.IsUserInVR());
    }

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            keyboard.isDesktopMode = true;
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            keyboard.isDesktopMode = false;
        }
    }

    void Update()
    {
        if (!keyboard.isDesktopMode) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Networking.LocalPlayer.TeleportTo(transform.position, transform.rotation);
        }
    }
}
