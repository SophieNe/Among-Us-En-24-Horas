using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class PlayerLobby : NetworkBehaviour
{
    [SyncVar(hook = nameof(ChangeReadyState))]
    public bool isReady;
    [SyncVar(hook = nameof(ChangeName))]
    public string playerName;
    public Image square;
    public Text playerText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        ChangeReadyColor(isReady);
    }

    [Command(ignoreAuthority = true)]
    public void SetReady(bool ready)
    {
        isReady = ready;
    }
    [Command(ignoreAuthority = true)]
    public void SetName(string playName)
    {
        playerName = playName;
    }
    public void ChangeReadyState(bool _, bool newValue)
    {
        isReady = newValue;
        ChangeReadyColor(newValue);
    }
    private void ChangeReadyColor(bool ready)
    {
        switch (ready)
        {
            case true:
                square.color = new Color32(0, 255, 0, 255);
                break;
            case false:
                if (playerText.text.Equals(DefaultName()))
                {
                    square.color = new Color32(166, 166, 166, 255);
                }
                else
                {
                    square.color = new Color32(255, 255, 255, 255);
                }
                break;
        }
    }
    public void ChangeName(string _, string newName)
    {
        playerName = newName;
        ChangeNameState(newName);
    }
    void ChangeNameState(string namee)
    {
        playerText.text = namee;
        if (namee.Equals(DefaultName()))
        {
            isReady = false;
            square.color = new Color32(166, 166, 166, 255);
        }
        else
        {
            square.color = new Color32(255, 255, 255, 255);
        }
    }
    public string DefaultName()
    {
        return "Esperando...";
    }
}
