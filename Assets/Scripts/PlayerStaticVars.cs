using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public static class PlayerStaticVars
{
    public static clientState state = clientState.showingGameplay;
    public static PlayerType winner;
}
public enum clientState
{
    showingGameplay,
    showingUI
}