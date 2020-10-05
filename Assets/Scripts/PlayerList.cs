using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

namespace Game
{
    public class PlayerList : NetworkBehaviour
    {
        [HideInInspector] public PlayerScript[] players;
        [HideInInspector] public List<Transform> playersTransforms = new List<Transform>(), playersSpriteTranforms = new List<Transform>();
        [HideInInspector] public PlayerScript myPlayer;
        public override void OnStartClient()
        {
            StartCoroutine(ClientStart());
        }
        IEnumerator ClientStart()
        {
            yield return new WaitForSeconds(6.1f);
            PopulateLists();
            myPlayer = GetMyPlayer();
        }
        public override void OnStartServer()
        {
            StartCoroutine(ServerStart());
        }
        IEnumerator ServerStart()
        {
            yield return new WaitForSecondsRealtime(7f);
            PopulateLists();
            SetUpPlayers();
        }
        void SetUpPlayers()
        {
            List<Color32> availableColors = new List<Color32>() { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255), new Color32(255, 0, 255, 255),
            new Color32(255, 255, 255, 255), new Color32(0, 255, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 146, 0, 255), new Color32(135, 135, 135, 255), new Color32(64, 64, 64, 255)};

            int impostors = CheckImpostorQuantity();

            for (int i = 0; i < players.Length; i++)
            {
                PlayerScript player = players[i];
                int randomIndex = Random.Range(0, availableColors.Count);
                player.color = availableColors[randomIndex];
                availableColors.RemoveAt(randomIndex);

                PlayerType thisPlayType = (impostors > 0) ? ((Random.Range((int)0, players.Length) < impostors) ? PlayerType.impostor : PlayerType.crewmate) : PlayerType.crewmate;
                if (((i + 1) == players.Length) && (thisPlayType.Equals(PlayerType.crewmate)) && (impostors > 0))
                {
                    thisPlayType = PlayerType.impostor;
                }
                player.SetPlayerType(thisPlayType);
                Debug.Log(player.playerType);
                if (player.playerType == PlayerType.impostor)
                {
                    impostors--;
                    Debug.Log(impostors + " impostors left");
                }

                player.playerState = PlayerState.alive;
            }
        }
        void PopulateLists()
        {
            players = FindObjectsOfType<PlayerScript>();
            Debug.Log(players.Length + " players");
            foreach (PlayerScript player in players)
            {
                playersTransforms.Add(player.transform);
                playersSpriteTranforms.Add(player.spriteTransform);
            }
        }
        public int CheckImpostorQuantity()
        {
            if (players.Length < 5)
            {
                return 1;
            }
            return 2;
        }
        public int CheckCrewmateQuantity()
        {
            Debug.Log((players.Length - CheckImpostorQuantity()) + " crewmates");
            return players.Length - CheckImpostorQuantity();
        }
        public List<PlayerScript> Impostors()
        {
            List<PlayerScript> imps = new List<PlayerScript>();
            int impostorQuantity = CheckImpostorQuantity();
            foreach (PlayerScript player in players)
            {
                if (player.playerType == PlayerType.impostor)
                {
                    imps.Add(player);
                    impostorQuantity--;
                }
                if (impostorQuantity == 0)
                {
                    break;
                }
            }
            return imps;
        }
        public List<PlayerScript> Crewmates()
        {
            List<PlayerScript> crew = new List<PlayerScript>();
            int crewQuantity = CheckCrewmateQuantity();
            foreach (PlayerScript player in players)
            {
                if (player.playerType == PlayerType.crewmate)
                {
                    crew.Add(player);
                    crewQuantity--;
                }
                if (crewQuantity == 0)
                {
                    break;
                }
            }
            return crew;
        }
        public PlayerScript GetMyPlayer()
        {
            foreach (PlayerScript player in players)
            {
                if (player.isLocalPlayer)
                {
                    return player;
                }
            }
            return null;
        }
        public int GetAliveCrewmates()
        {
            List<PlayerScript> crew = Crewmates();
            int alives = 0;
            foreach (PlayerScript player in crew)
            {
                if (player.playerState.Equals(PlayerState.alive))
                {
                    alives++;
                }
            }
            return alives;
        }
        [Command(ignoreAuthority = true)]
        public void CmdCheckIfImpostorsKilledEnough()
        {
            bool killedEnough = GetAliveCrewmates() <= CheckImpostorQuantity();
            if (killedEnough)
            {
                FindObjectOfType<NetworkRoomManagerExt>().ServerEndGameShowResults(PlayerType.impostor);
            }
        }

        [ClientRpc]
        public void RpcChangeWinnerVar(PlayerType winnerType)
        {
            PlayerStaticVars.winner = winnerType;
        }
    }
}