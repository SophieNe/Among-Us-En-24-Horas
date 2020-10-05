using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class WaitTask : Task
    {
        public Button startCountdownButton;
        public Slider slider;
        public override void TaskContent(PlayerScript player)
        {
            base.TaskContent(player);
            StopAllCoroutines();
            startCountdownButton.interactable = true;
            slider.value = 0;
            SetButton(startCountdownButton, delegate { StartCountdown(player); });
        }
        public void StartCountdown(PlayerScript player)
        {
            startCountdownButton.interactable = false;
            StartCoroutine(Countdown(player));
        }
        IEnumerator Countdown(PlayerScript player)
        {
            slider.value = 0;
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForSeconds(0.2f);
                slider.value += 0.2f;
            }
            TaskCompleted(player);
        }
        public override void TaskCompleted(PlayerScript player)
        {
            StopAllCoroutines();
            slider.value = 0;
            startCountdownButton.interactable = true;
            base.TaskCompleted(player);
        }
    }
}