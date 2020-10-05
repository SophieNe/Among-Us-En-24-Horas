using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public InputField serverCode;
    public Button connectButton, hostButton;
    public NetworkRoomManagerExt network;
    public void SetCode(string code)
    {
        connectButton.interactable = !string.IsNullOrEmpty(code);
    }
    public void JoinButton()
    {
        network.networkAddress = serverCode.text;
        network.StartClient();
    }
    public void HostButton()
    {
        network.StartHost();
    }
}
