
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("참조")]
    public GameObject nodePrefab;
    public GameObject connectionPrefab;
    public MapGenerator generator;
    public MapIconLibrary iconLibrary;
    public MapScenePreset scenePreset;

    [Header("현재 챕터 프리셋")]
    public ChapterPreset currentChapter;
    [SerializeField] private ChapterDatabase chapterDatabase;

    public static List<MapNode> LoadedMapData { get; set; }

    private Dictionary<int, MapNode> nodeDict = new();
    public Dictionary<int, MapNodeView> viewDict = new();
    private int currentNodeId = -1;

    private MapScenePreset nextScenePreset;
    private MapIconLibrary nextIconLibrary;
    private Sprite nextBackgroundSprite;

    private bool isMapGenerated = false;

    private GameObject mapUI;
    private Transform nodeContainer;
    private Transform connectionContainer;
    private Image backgroundImageUI;
    [SerializeField] private MapDragController dragController;

    private MapUIController mapUIController;

    public void RegisterUIController(MapUIController controller)
    {
        mapUIController = controller;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            TryRestoreChapter();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mapUI = GameObject.FindGameObjectWithTag("MapUI");
        nodeContainer = GameObject.FindGameObjectWithTag("MapNodeContainer")?.transform;
        connectionContainer = GameObject.FindGameObjectWithTag("MapLineContainer")?.transform;
        backgroundImageUI = GameObject.FindGameObjectWithTag("MapBackground")?.GetComponent<Image>();

        if (dragController != null)
        {
            dragController.enabled = false;
            dragController.SetPositionInstant(0);
        }

        isMapGenerated = false;

        if (nodeContainer != null && LoadedMapData != null && LoadedMapData.Count > 0)
        {
            GenerateMap();
        }
        else
        {
            Debug.Log("[MapManager] 맵 생성 생략 또는 저장된 맵 없음");
        }
    }

    private void TryRestoreChapter()
    {
        if (currentChapter == null)
        {
            string id = PlayerPrefs.GetString("CurrentChapterId", "");
            if (!string.IsNullOrEmpty(id))
            {
                ChapterDatabase chapterDb = Resources.Load<ChapterDatabase>("ChapterDatabase");
                if (chapterDb != null)
                {
                    var preset = chapterDb.GetChapterById(id);
                    if (preset != null)
                    {
                        SetChapter(preset);
                        Debug.Log($"[MapManager] 챕터 복원 완료: {preset.chapterName}");
                    }
                    else Debug.LogWarning($"[MapManager] 해당 ID의 챕터가 없습니다: {id}");
                }
                else Debug.LogError("[MapManager] ChapterDatabase를 Resources에서 찾을 수 없습니다.");
            }
        }
    }

    public void SetChapter(ChapterPreset chapter)
    {
        currentChapter = chapter;
        iconLibrary = chapter.iconLibrary;
        scenePreset = chapter.mapScenePreset;

        PlayerPrefs.SetString("CurrentChapterId", chapter.chapterId);

        if (generator != null)
            generator.scenePreset = chapter.mapScenePreset;
    }

    public string GetSceneForNode(NodeType type)
    {
        if (currentChapter != null && currentChapter.mapScenePreset != null)
            return currentChapter.mapScenePreset.GetSceneNameForNode(type);
        return "BattleScene";
    }

    public void ShowMap()
    {
        if (mapUIController != null)
        {
            mapUIController.ShowMap();
        }
        else
        {
            Debug.LogWarning("[MapManager] MapUIController가 등록되지 않았습니다.");
        }
    }

    public void GenerateMap()
    {
        if (isMapGenerated || nodeContainer == null)
        {
            Debug.Log("[MapManager] 맵 생성 생략 (이미 생성되었거나 UI 없음)");
            return;
        }

        if (LoadedMapData != null && LoadedMapData.Count > 0)
        {
            Debug.Log("[MapManager] 저장된 맵 데이터를 사용하여 복원");

            generator.RegisterUsedScenes(LoadedMapData); // 씬 중복 방지용
            GenerateFromData(LoadedMapData);

            var startY = nodeDict.Values.Min(n => n.y);
            int savedId = PlayerPrefs.GetInt("PendingNodeId", -1);
            bool hasSaved = savedId != -1 && nodeDict.ContainsKey(savedId);

            currentNodeId = hasSaved
                ? savedId
                : nodeDict.Values.First(n => n.y == startY).id;

            if (nodeDict.TryGetValue(currentNodeId, out var currentNode))
            {
                currentNode.isUnlocked = true;
                viewDict[currentNode.id].UpdateState();
            }

            isMapGenerated = true;
            Debug.Log("[MapManager] 맵 복원 완료");
        }
        else
        {
            Debug.Log("[MapManager] 저장된 맵 없음 - 새 맵 생성 시도");
            GenerateNewMap();
        }
    }



    public void GenerateFromData(List<MapNode> savedNodes)
    {
        nodeDict.Clear();
        viewDict.Clear();

        // ?? 기존 UI 요소 정리
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in connectionContainer) Destroy(child.gameObject);

        // ?? 노드 복원
        foreach (var node in savedNodes)
            nodeDict[node.id] = node;

        // ?? 노드 UI 생성
        foreach (var node in nodeDict.Values)
        {
            var viewObj = Instantiate(nodePrefab, nodeContainer);
            var view = viewObj.GetComponent<MapNodeView>();
            view.Setup(node, iconLibrary.GetIcon(node.type));
            view.SetupPosition(node.x, node.y);
            view.SetManager(this);
            viewDict[node.id] = view;
        }

        // ?? 연결선 생성
        foreach (var node in nodeDict.Values)
        {
            var fromView = viewDict[node.id];
            foreach (int toId in node.connectedNodeIds ?? new List<int>())
            {
                if (!viewDict.ContainsKey(toId)) continue;

                var toView = viewDict[toId];
                var conn = Instantiate(connectionPrefab, connectionContainer);
                var connUI = conn.GetComponent<MapConnectionUI>();
                connUI.from = fromView.GetComponent<RectTransform>();
                connUI.to = toView.GetComponent<RectTransform>();
            }
        }

        // ?? 배경 이미지 적용
        if (scenePreset != null && backgroundImageUI != null)
        {
            backgroundImageUI.sprite = scenePreset.backgroundSprite;
        }

        Debug.Log("[MapManager] 맵 생성 or 복원 완료");
    }

    public void GenerateNewMap()
    {
        // 맵 생성
        var newMap = generator.Generate();
        LoadedMapData = newMap;

        // 초기화
        nodeDict.Clear();
        viewDict.Clear();

        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in connectionContainer) Destroy(child.gameObject);

        GenerateFromData(newMap);

        // 시작 노드 선택
        var startY = nodeDict.Values.Min(n => n.y);
        currentNodeId = nodeDict.Values.First(n => n.y == startY).id;

        if (nodeDict.TryGetValue(currentNodeId, out var currentNode))
        {
            currentNode.isUnlocked = true;
            viewDict[currentNode.id].UpdateState();
        }

        isMapGenerated = true;
        Debug.Log("[MapManager] 새 맵 생성 완료");
    }


    public void EnterNode(MapNode node)
    {
        currentNodeId = node.id;
        node.visited = true;
        node.isCleared = true;

        int currentY = node.y;

        // 같은 라인 이하 노드 모두 잠금 (자기 자신 제외)
        foreach (var otherNode in nodeDict.Values)
        {
            if (otherNode.y <= currentY && otherNode.id != node.id)
                otherNode.isUnlocked = false;
        }

        UpdateAllViewStates();

        PlayerPrefs.SetInt("PendingNodeId", node.id);
        PlayerPrefs.SetInt("CurrentSlotIndex", SaveManager.Instance.currentSlotIndex);
        PlayerPrefs.Save();

        // ? 저장 후 씬 전환
        StartCoroutine(SaveAndLoadScene(node.sceneName));
    }

    private IEnumerator SaveAndLoadScene(string sceneName)
    {
        Debug.Log("[MapManager] 씬 전환 전 저장 시작");

        // 저장 수행 (세이브 중간에 처리할 작업 있다면 여기 추가)
        SaveManager.Instance.SaveGame(SaveManager.Instance.CurrentSlotIndex);

        // 저장 시간 보장 (디스크 반영 여유)
        yield return new WaitForSeconds(0.1f); // 필요시 조정

        Debug.Log("[MapManager] 저장 완료 → 씬 전환: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public int GetCurrentNodeId() => currentNodeId;
    public bool CanEnter(MapNode node) => node.isUnlocked && !node.isCleared;
    public List<MapNode> GetCurrentMapData() => new(nodeDict.Values);

    public MapNode GetCurrentNode()
    {
        if (nodeDict.TryGetValue(currentNodeId, out var node))
            return node;

        Debug.LogWarning("[MapManager] currentNodeId에 해당하는 노드를 찾을 수 없습니다.");
        return null;
    }

    public void UnlockNextNodes(MapNode node)
    {
        foreach (var nextId in node.connectedNodeIds)
        {
            if (nodeDict.TryGetValue(nextId, out var nextNode))
                nextNode.isUnlocked = true;
        }

        UpdateAllViewStates();
    }

    private void UpdateAllViewStates()
    {
        foreach (var view in viewDict.Values)
            view.UpdateState();
    }


    public void SetNextChapter(MapScenePreset preset, MapIconLibrary iconLib, Sprite background)
    {
        nextScenePreset = preset;
        nextIconLibrary = iconLib;
        nextBackgroundSprite = background;
    }

    public void ClearMap(bool clearSavedData = false)
    {
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in connectionContainer) Destroy(child.gameObject);

        nodeDict.Clear();
        viewDict.Clear();

        if (clearSavedData)
        {
            Debug.Log("[MapManager] 저장된 맵 데이터도 초기화함");
            LoadedMapData = null;
        }

        isMapGenerated = false;
    }


    public void ReloadMap()
    {
        ClearMap();
        GenerateMap();
    }

    public void LoadNextChapterMap()
    {
        Debug.Log("[MapManager] 다음 챕터 맵 로드");

        if (nextScenePreset != null)
            scenePreset = nextScenePreset;
        if (nextIconLibrary != null)
            iconLibrary = nextIconLibrary;

        nextScenePreset = null;
        nextIconLibrary = null;

        if (nextBackgroundSprite != null && backgroundImageUI != null)
            backgroundImageUI.sprite = nextBackgroundSprite;

        LoadedMapData = null;

        mapUI = GameObject.FindWithTag("MapUI");
        nodeContainer = GameObject.FindWithTag("MapNodeContainer")?.transform;
        connectionContainer = GameObject.FindWithTag("MapLineContainer")?.transform;
        backgroundImageUI = GameObject.FindWithTag("MapBackground")?.GetComponent<Image>();

        ClearMap();
        GenerateMap();
    }

    public void SetSlotIndexAndGenerate(int index)
    {
        PlayerPrefs.SetInt("CurrentSlotIndex", index);
        PlayerPrefs.Save();

        StartCoroutine(GenerateMapWithDelay());
    }

    private IEnumerator GenerateMapWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ClearMap();
        GenerateMap();
        ShowMap();
    }

    public static void LoadChapterById(string id)
    {
        var preset = ChapterDatabase.Instance?.GetChapterById(id);
        if (preset != null)
        {
            Instance.SetChapter(preset);                  // 챕터 전체 설정
            Instance.iconLibrary = preset.iconLibrary;    // 아이콘 라이브러리 재지정 (명시적)
            Debug.Log("[MapManager] ChapterPreset 복원 완료: " + id);
        }
    }



    public string GetCurrentChapterId()
    {
        return scenePreset?.chapterId;
    }

    public List<SerializableMapNode> GetSerializableMapData()
    {
        return nodeDict.Values.Select(n => new SerializableMapNode
        {
            id = n.id,
            x = n.x,
            y = n.y,
            type = n.type,
            connectedNodeIds = n.connectedNodeIds,
            visited = n.visited,
            sceneName = n.sceneName,
            isUnlocked = n.isUnlocked,
            isCleared = n.isCleared
        }).ToList();
    }

    public static List<MapNode> ConvertToMapNodes(List<SerializableMapNode> serializable)
    {
        return serializable.Select(n => new MapNode
        {
            id = n.id,
            x = n.x,
            y = n.y,
            type = n.type,
            connectedNodeIds = n.connectedNodeIds ?? new List<int>(),
            visited = n.visited,
            sceneName = n.sceneName,
            isUnlocked = n.isUnlocked,
            isCleared = n.isCleared
        }).ToList();
    }
}