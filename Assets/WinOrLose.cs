using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Game;

public class WinOrLose : MonoBehaviour
{
    public Text ImpOrCrew;
    void Start()
    {
        switch (PlayerStaticVars.winner)
        {
            case PlayerType.crewmate:
                SetText("TRIPULANTES", new Color32(0, 255, 255, 255));
                break;
            case PlayerType.impostor:
                SetText("IMPOSTORES", new Color32(255, 0, 0, 255));
                break;
        }
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(5f);
        NetworkClient.Disconnect();
    }
    void SetText(string text, Color32 color)
    {
        ImpOrCrew.text = text;
        ImpOrCrew.color = color;
    }
}
