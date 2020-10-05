using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;

public class PlayerNameInput : MonoBehaviour
{
    InputField input;
    Button continueButton;

    private void Start()
    {
        input = GetComponentInChildren<InputField>();
        continueButton = GetComponentInChildren<Button>();
        SetName("");
    }
    public void SetName(string inputString)
    {
        continueButton.interactable = !string.IsNullOrEmpty(inputString);
    }
    public void SaveName()
    {
        Game.PlayerPrefs.playerName = input.text;
    }
}
