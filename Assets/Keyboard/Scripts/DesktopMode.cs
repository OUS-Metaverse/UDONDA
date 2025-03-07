
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

    public void AttachPlayer()
    {
        Networking.LocalPlayer.UseAttachedStation();
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
