using UnityEngine;
using UnityEngine.Playables;

public class MainMenuUI : MonoBehaviour
{
    [Header("Intro")]
    [SerializeField] private PlayableDirector introDirector;

    [Header("Panels")]
    [SerializeField] private GameObject saveSlotPanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        introDirector.Play();
    }

    public void OnClickStart()
    {
        saveSlotPanel.SetActive(true);
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
