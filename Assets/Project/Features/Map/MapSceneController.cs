using UnityEngine;
using System.Collections;

public class MapSceneController : MonoBehaviour
{
    [SerializeField] private MapManager manager;

    IEnumerator Start()
    {
        yield return null; // 1 프레임 대기
        manager.GenerateMap();
    }
}
