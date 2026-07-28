using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSceneLoader : MonoBehaviour
{
    public void LoadSceneForNode(MapNode node)
    {
        if (!string.IsNullOrEmpty(node.sceneName))
        {
            Debug.Log($"[MapSceneLoader] 씬 로드: {node.sceneName}");
            SceneManager.LoadScene(node.sceneName);
        }
        else
        {
            Debug.LogWarning($"[MapSceneLoader] 씬 이름이 비어있음. 노드 타입: {node.type}");
        }
    }
}
