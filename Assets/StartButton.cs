
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class StartButton : UdonSharpBehaviour
{
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _desktopStartButton;
    [SerializeField] private Keyboard keyboard;
    void Start()
    {
        bool isDesktopMode = !Networking.LocalPlayer.IsUserInVR();
        _startButton.SetActive(!isDesktopMode);
        _desktopStartButton.SetActive(isDesktopMode);
        keyboard.isDesktopMode = isDesktopMode;
    }
}
