using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineEndSceneLoader : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "0-2. Library";
    public int autoSaveSlot = 0;

    void Start()
    {
        if (director != null)
            director.stopped += OnTimelineEnd;
    }

    private void OnTimelineEnd(PlayableDirector pd)
    {
        if (pd == director)
        {
            AutoSave(); // ?? 자동 저장 추가
            LoadingManager.sceneToLoad = nextSceneName;
            SceneManager.LoadScene("0. Loading Scene");
        }
    }

    private void AutoSave()
    {
        SaveData data = new SaveData();
        data.sceneName = nextSceneName;
        data.displaySceneName = SaveManager.Instance.GetDisplaySceneName(nextSceneName);
        data.playerPosition = new float[] { 0f, 0f, 0f };
        data.playerLevel = 1;
        data.gold = 0;
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        SaveManager.Instance.SaveToSlot(autoSaveSlot, data);
    }
}
