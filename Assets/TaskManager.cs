using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Game;

public class TaskManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(ChangeTotalTasksSlider))] public int totalTasks;
    [SyncVar(hook = nameof(ChangeCurrentCompletedTasks))] public int completedTasks;

    public Slider tasksCompleted;

    public WaitTask[] waitTasks;
    public PasswordTask[] passTasks;

    public override void OnStartServer()
    {
        StartCoroutine(ServerStartDelay());
    }
    IEnumerator ServerStartDelay()
    {
        yield return new WaitForSeconds(7.3f);
        totalTasks = FindObjectOfType<PlayerList>().CheckCrewmateQuantity() * 5;
        Debug.Log(totalTasks + " tareas totales");
        completedTasks = 0;
        SetMaxValue(totalTasks);
        SetCurrentValue(completedTasks);
    }
    public override void OnStartClient()
    {
        StartCoroutine(LocalStartDelay());
    }
    IEnumerator LocalStartDelay()
    {
        yield return new WaitForSeconds(7.6f);
        PlayerScript myPlayer = FindObjectOfType<PlayerList>().myPlayer;
        Debug.Log("Asignando tareas a " + myPlayer.playerName);
        AssingTasks(myPlayer);
    }

    public void AssingTasks(PlayerScript player)
    {
        for (int i = 0; i < 2; i++)
        {
            WaitTask wait = null;
            bool isGenerated = false;
            while (isGenerated == false)
            {
                wait = waitTasks[Random.Range(0, waitTasks.Length)];
                if (!player.playerTasks.Contains(wait))
                {
                    isGenerated = true;
                }
            }
            player.playerTasks.Add(wait);
        }
        for (int i = 0; i < 3; i++)
        {
            PasswordTask pass = null;
            bool generated = false;
            while (generated == false)
            {
                pass = passTasks[Random.Range(0, passTasks.Length)];
                if (!player.playerTasks.Contains(pass))
                {
                    generated = true;
                }
            }
            player.playerTasks.Add(pass);
        }
    }
    [Command(ignoreAuthority = true)]
    public void AddCompletedTask()
    {
        completedTasks++;
        if (completedTasks == totalTasks)
        {
            NetworkRoomManagerExt room = FindObjectOfType<NetworkRoomManagerExt>();
            room.ServerEndGameShowResults(PlayerType.crewmate);
        }
    }
    public void ChangeTotalTasksSlider(int _, int newMax)
    {
        tasksCompleted.maxValue = newMax;
    }
    public void ChangeCurrentCompletedTasks(int _, int newCurrent)
    {
        tasksCompleted.value = newCurrent;
    }
    [ClientRpc]
    void SetMaxValue(int max)
    {
        totalTasks = max;
        tasksCompleted.maxValue = max;
    }
    [ClientRpc]
    void SetCurrentValue(int val)
    {
        completedTasks = val;
        tasksCompleted.value = val;
    }
}
