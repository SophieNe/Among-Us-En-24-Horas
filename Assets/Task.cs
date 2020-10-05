using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Game
{
    public abstract class Task : MonoBehaviour
    {
        public GameObject taskContent;
        public bool isCompleted;
        private void CheckTask(PlayerScript player)
        {
            if (!isCompleted)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    TaskContent(player);
                }
            }
        }
        public virtual void TaskContent(PlayerScript player)
        {
            isCompleted = false;
            taskContent.SetActive(true);
            PlayerStaticVars.state = clientState.showingUI;
        }
        public virtual void TaskCompleted(PlayerScript player)
        {
            isCompleted = true;
            taskContent.SetActive(false);
            PlayerStaticVars.state = clientState.showingGameplay;
            FindObjectOfType<TaskManager>().AddCompletedTask();
            player.RemoveTask(this);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
            if (player != null)
            {
                if (player.playerType.Equals(PlayerType.crewmate))
                {
                    if (player.playerTasks.Contains(this))
                    {
                        AddTaskToListener(player);
                    }
                }
            }
        }
        void AddTaskToListener(PlayerScript player)
        {
            if (player.isLocalPlayer)
            {
                player.taskAction += delegate { CheckTask(player); };
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
            if (player != null)
            {
                if (player.playerType.Equals(PlayerType.crewmate))
                {
                    if ((Array.Find(player.playerTasks.ToArray(), taskk => taskk == this)) != null)
                    {
                        DeleteTaskFromListener(player);
                    }
                }
            }
        }
        void DeleteTaskFromListener(PlayerScript player)
        {
            if (player.isLocalPlayer)
            {
                player.taskAction -= delegate { CheckTask(player); };
            }
        }
        public void SetButton(Button button, UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
