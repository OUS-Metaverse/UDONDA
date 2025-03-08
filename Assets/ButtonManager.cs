using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ButtonManager : UdonSharpBehaviour
{
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _desktopStartButton;
    [SerializeField] private GameObject _leaveButton;
    [SerializeField] private Keyboard keyboard;
    void Start()
    {
        bool isDesktopMode = !Networking.LocalPlayer.IsUserInVR();
        _startButton.SetActive(!isDesktopMode);
        _desktopStartButton.SetActive(isDesktopMode);
        keyboard.isDesktopMode = isDesktopMode;
    }

    public void LeaveButtonSetActive(bool value)
    {
        _leaveButton.SetActive(value);
    }
}
