using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Mirror;

public class PlayerDetection : MonoBehaviour
{
    public PlayerScript thisPlayer;

    private void Start()
    {
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(10);
        while (true)
        {
            GetTargets();
            yield return new WaitForSeconds(0.5f);
        }
    }
    void GetTargets()
    {
        thisPlayer.killAction = null;
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D collision in targetsInViewRadius)
        {
            if (collision.gameObject.tag.Equals("Player"))
            {
                PlayerScript player = collision.GetComponent<PlayerScript>();
                if (player != thisPlayer)
                {
                    if (thisPlayer.playerType.Equals(PlayerType.impostor))
                    {
                        thisPlayer.killAction = player.CmdGetKilled;
                    }
                }
            }
        }
    }
}
