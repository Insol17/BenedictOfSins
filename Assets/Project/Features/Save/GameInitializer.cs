// GameInitializer.cs
using UnityEngine;
using TMPro;

public class GameInitializer : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;

    void Start()
    {
        var sm = SaveManager.Instance;

        Debug.Log($"[Init] PlayerName: {sm.currentPlayerName}, CutsceneWatched: {sm.cutsceneWatched}");

        if (playerNameText != null)
            playerNameText.text = sm.currentPlayerName;

        if (sm.cutsceneWatched)
            SkipCutscene();
    }

    void SkipCutscene()
    {
        // ÄÆ¾ÀÀ» ½ºÅµÇÏ´Â ½ÇÁ¦ ·ÎÁ÷À» ¿©±â¿¡ ÀÛ¼º
        Debug.Log("ÄÆ¾À ½ºÅµµÊ");
    }
}
