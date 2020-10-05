using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PasswordTask : Task
    {
        public Text passwordHint, passwordWriter;
        public Button[] keypad;
        public Button deleteKey, acceptKey;

        public override void TaskContent(PlayerScript player)
        {
            base.TaskContent(player);
            passwordHint.text = Random.Range(10000, 100000).ToString();
            DeletePassword();
            SetUpKeyPadButtons();
            SetButton(deleteKey, DeletePassword);
            SetButton(acceptKey, delegate { CheckPassword(player); });
        }
        void SetUpKeyPadButtons()
        {
            for (int i = 0; i < keypad.Length; i++)
            {
                int num = i + 1;
                if (num == 10)
                {
                    num = 0;
                }
                SetButton(keypad[i], delegate { AddNumberToPassword(num); });
            }
        }
        public void AddNumberToPassword(int num)
        {
            passwordWriter.text += num.ToString();
        }
        public void DeletePassword()
        {
            passwordWriter.text = "";
        }
        public void CheckPassword(PlayerScript player)
        {
            if (passwordWriter.text.Equals(passwordHint.text))
            {
                Debug.Log("task completed!");
                DeletePassword();
                TaskCompleted(player);
            }
            else
            {
                DeletePassword();
            }
        }
    }
}
