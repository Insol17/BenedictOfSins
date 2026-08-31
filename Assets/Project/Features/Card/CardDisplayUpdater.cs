using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardDisplayUpdater
{
    public static void RefreshAllCardDescriptions()
    {
        foreach (var card in GameObject.FindGameObjectsWithTag("Card"))
        {
            var display = card.GetComponent<CardDisplay>();
            if (display != null)
                display.RefreshDescription();
        }
    }
}
