using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class InitialGameInfo : MonoBehaviour
    {
        public PlayerList playerList;
        public int secondsToWait;
        public Text impOrCr;
        public GameObject shhhScreen, infoScreen;
        void Start()
        {
            StartCoroutine(WaitForDelay());
        }

        IEnumerator WaitForDelay()
        {
            yield return new WaitForSeconds(secondsToWait);
            SetUpInfo();
            yield return new WaitForSeconds((secondsToWait / 2));
            gameObject.SetActive(false);
        }
        void HideShhAndShowInfo()
        {
            shhhScreen.SetActive(false);
            infoScreen.SetActive(true);
        }
        void SetUpInfo()
        {
            HideShhAndShowInfo();
            switch (playerList.myPlayer.playerType)
            {
                case PlayerType.crewmate:
                    SetTitleType("Tripulante", new Color(0, 255, 255, 255));
                    break;
                case PlayerType.impostor:
                    SetTitleType("Impostor", new Color(255, 0, 0, 255));
                    break;
            }
        }
        void SetTitleType(string titleText, Color32 titleColor)
        {
            impOrCr.text = titleText;
            impOrCr.color = titleColor;
        }
    }
}
