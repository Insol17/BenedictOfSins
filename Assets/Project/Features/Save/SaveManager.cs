using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 슬롯 기반 저장/로드/삭제 및 게임 상태를 전역 관리하는 SaveManager 싱글턴 클래스
/// </summary>
public class SaveManager : MonoBehaviour
{
    // ▶ 기본 필드
    public GameObject player;
    public int playerLevel;
    public string playerName;
    public bool cutsceneWatched;
    public string sceneName;

    // ▶ 싱글턴 및 상태 관리
    public static SaveManager Instance;
    public static SaveData lastLoadedData;

    // ▶ 현재 챕터 및 플레이어 이름
    public ChapterPreset currentChapter;
    public string currentPlayerName;

    // ▶ 플레이어 체력
    public int CurrentPlayerHP = 100;
    public int CurrentPlayerMaxHP = 100;

    [SerializeField] private SceneNameMapping sceneNameMapping;

    // ▶ 현재 슬롯 인덱스
    public int currentSlotIndex = -1;
    public int CurrentSlotIndex
    {
        get => currentSlotIndex;
        set
        {
            currentSlotIndex = value;
            PlayerPrefs.SetInt("CurrentSlotIndex", value);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] CurrentSlotIndex 설정됨: " + currentSlotIndex);
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentPlayerName = PlayerPrefs.GetString("PlayerName", "");
        currentSlotIndex = PlayerPrefs.GetInt("CurrentSlotIndex", -1);
        Debug.Log("[SaveManager] Singleton 초기화됨, currentPlayerName: " + currentPlayerName);
    }

    private void Start()
    {
        if (lastLoadedData != null)
            Debug.Log("[SaveManager] lastLoadedData 감지됨. 저장값이 적용될 예정입니다.");
    }

    public void OnChapterSelect(ChapterPreset selectedChapter)
    {
        currentChapter = selectedChapter;
        lastLoadedData = null; // ? 이것 추가

        Debug.Log($"[SaveManager] 챕터 선택됨: {currentChapter.chapterName}");
        ApplyChapterPreset();
    }


    public void ApplyChapterPreset()
    {
        if (currentChapter == null)
        {
            Debug.LogWarning("[SaveManager] ApplyChapterPreset 실패: currentChapter가 null입니다.");
            return;
        }

        // ?? 저장 데이터를 이미 불러온 경우 초기화 금지
        if (lastLoadedData != null)
        {
            Debug.Log("[SaveManager] 저장 데이터 존재 - ApplyChapterPreset 생략됨");
            return;
        }

        Debug.Log("[SaveManager] ApplyChapterPreset 호출됨: 챕터 초기화 실행");

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            var stats = playerObj.GetComponent<PlayerStats>();
            if (stats != null)
            {
                int maxHp = currentChapter.defaultPlayerMaxHP;

                stats.SetMaxHP(maxHp);
                stats.SetCurrentHP(maxHp);

                // ? SaveManager에 현재 HP도 동기화
                CurrentPlayerMaxHP = maxHp;
                CurrentPlayerHP = maxHp;
            }

            var energyManager = FindObjectOfType<EnergyManager>();
            if (energyManager != null)
                energyManager.SetEnergy(currentChapter.startingEnergy);
        }

        if (DeckManager.Instance != null && currentChapter.startingCards != null)
            DeckManager.Instance.SetDeck(currentChapter.startingCards);

        GoldManager.Instance?.SetGold(currentChapter.startingGold);

        if (RelicManager.Instance != null && currentChapter.startingRelics != null)
        {
            foreach (var relic in currentChapter.startingRelics)
                if (relic != null)
                    RelicManager.Instance.AddRelic(relic);
        }

        // ? 초기값 설정 후 즉시 자동 저장
        if (CurrentSlotIndex >= 0)
        {
            SaveGame(CurrentSlotIndex);
            Debug.Log("[SaveManager] ApplyChapterPreset 완료 후 자동 저장됨.");
        }
        else
        {
            Debug.LogWarning("[SaveManager] ApplyChapterPreset 저장 실패: CurrentSlotIndex가 유효하지 않음.");
        }
    }

    public static void SaveGame(int slotIndex, SaveData data)
    {
        Instance?.SaveToSlot(slotIndex, data);
    }

    public static SaveData LoadGame(int slotIndex)
    {
        SaveData data = Instance?.LoadFromSlot(slotIndex);
        if (data != null)
        {
            Instance.currentPlayerName = data.playerName;
            Instance.playerLevel = data.playerLevel;
            Instance.cutsceneWatched = data.cutsceneWatched;
            Instance.sceneName = data.sceneName;

            Instance.CurrentPlayerHP = data.playerCurrentHP;
            Instance.CurrentPlayerMaxHP = data.playerMaxHP;

            GoldManager.Instance?.SetGold(data.gold);
            lastLoadedData = data;

            if (!string.IsNullOrEmpty(data.chapterId))
                Instance.currentChapter = ChapterDatabase.Instance.GetChapterById(data.chapterId);

            PlayerPrefs.SetString("PlayerName", data.playerName);
            PlayerPrefs.SetInt("PendingNodeId", data.currentNodeId);
            PlayerPrefs.Save();

            MapManager.LoadedMapData = Instance.LoadMap(slotIndex);
            if (data.chapterId != null)
                MapManager.LoadChapterById(data.chapterId);

            // 전체 덱 복원
            if (data.allDeck != null && data.allDeck.Count > 0)
            {
                DeckManager.Instance?.LoadAllDeck(data.allDeck);
                Debug.Log($"[SaveManager] 덱 복원 완료: {data.allDeck.Count}종");
            }

            Debug.Log("[LoadGame] 상태 및 맵 데이터 불러오기 완료: " + data.playerName);
        }
        return data;
    }


    public void Load(int slotIndex)
    {
        SaveData data = LoadGame(slotIndex);
        if (data != null)
        {
            CurrentSlotIndex = slotIndex;
            Debug.Log("[Load] 슬롯 " + slotIndex + " 로드 성공: " + data.playerName);
        }
    }

    public SaveData CreateCurrentSaveData(string sceneName)
    {
        return new SaveData
        {
            playerName = currentPlayerName,
            sceneName = sceneName,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playerLevel = playerLevel,
            gold = GoldManager.Instance?.Gold ?? 0,
            cutsceneWatched = cutsceneWatched,
            currentNodeId = MapManager.Instance?.GetCurrentNodeId() ?? -1,
            chapterId = currentChapter?.chapterId
        };
    }

    public void SaveGame(int slotIndex)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        var stats = playerObj.GetComponent<PlayerStats>();
        List<MapNode> mapData = MapManager.Instance?.GetCurrentMapData();

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            displaySceneName = GetDisplaySceneName(SceneManager.GetActiveScene().name),
            playerPosition = Vector3ToFloatArray(playerObj.transform.position),
            gold = GoldManager.Instance?.Gold ?? 0,
            playerLevel = playerLevel,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playerName = currentPlayerName,
            cutsceneWatched = cutsceneWatched,
            savedMapNodes = ConvertToSerializable(mapData),
            currentNodeId = MapManager.Instance?.GetCurrentNodeId() ?? -1,
            chapterId = currentChapter?.chapterId,
            playerMaxHP = SaveManager.Instance.CurrentPlayerMaxHP,
            playerCurrentHP = SaveManager.Instance.CurrentPlayerHP,

            //  전체 덱 저장
            allDeck = DeckManager.Instance?.GetSavedAllDeck()
        };

        SaveToSlot(slotIndex, data);
        SaveMap(slotIndex, mapData);
        CurrentSlotIndex = slotIndex;

        lastLoadedData = data; // ? 이 한 줄이 매우 중요함
        Debug.Log($"[SaveManager] 저장 시점 CurrentHP: {CurrentPlayerHP}, MaxHP: {CurrentPlayerMaxHP}");

    }


    private List<SerializableMapNode> ConvertToSerializable(List<MapNode> mapNodes)
    {
        List<SerializableMapNode> list = new();
        foreach (var node in mapNodes)
        {
            list.Add(new SerializableMapNode
            {
                id = node.id,
                x = node.x,
                y = node.y,
                type = node.type,
                sceneName = node.sceneName,
                connectedNodeIds = node.connectedNodes.ConvertAll(n => n.id),
                visited = node.visited,
                isUnlocked = node.isUnlocked,
                isCleared = node.isCleared
            });
        }
        return list;
    }

    public void LoadGameAndChangeScene(int slotIndex)
    {
        SaveData data = LoadFromSlot(slotIndex);
        if (data == null) return;

        currentPlayerName = data.playerName;
        playerLevel = data.playerLevel;
        cutsceneWatched = data.cutsceneWatched;
        sceneName = data.sceneName;
        lastLoadedData = data;

        GoldManager.Instance?.SetGold(data.gold);

        if (!string.IsNullOrEmpty(data.chapterId))
            currentChapter = ChapterDatabase.Instance.GetChapterById(data.chapterId);

        MapManager.LoadedMapData = LoadMap(slotIndex);
        if (data.chapterId != null)
            MapManager.LoadChapterById(data.chapterId);

        PlayerPrefs.SetString("PlayerName", data.playerName);
        PlayerPrefs.SetInt("CurrentSlotIndex", slotIndex);
        PlayerPrefs.SetInt("PendingNodeId", data.currentNodeId);
        PlayerPrefs.SetInt("PlayerMaxHP", data.playerMaxHP);
        PlayerPrefs.SetInt("PlayerCurrentHP", data.playerCurrentHP);
        PlayerPrefs.Save();

        CurrentSlotIndex = slotIndex;

        LoadingManager.sceneToLoad = data.sceneName;
        SceneManager.LoadScene("0. Loading Scene");
    }

    public SaveData LoadFromSlot(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public void SaveToSlot(int slotIndex, SaveData data)
    {
        string path = GetSlotPath(slotIndex);
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log("[Save] 슬롯 " + slotIndex + " 저장 완료");
    }

    public void DeleteSlot(int slotIndex)
    {
        string path = GetSlotPath(slotIndex);
        if (File.Exists(path)) File.Delete(path);
    }

    private string GetSlotPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slotIndex}.json");
    }

    public string GetDisplaySceneName(string sceneName)
    {
        return sceneNameMapping?.GetDisplayName(sceneName) ?? sceneName;
    }

    public void SetPlayerName(string name)
    {
        currentPlayerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    public void ResetToNewGame()
    {
        GoldManager.Instance?.SetGold(0);
        playerLevel = 1;
        currentPlayerName = "";
        currentChapter = null;
        PlayerPrefs.SetString("PlayerName", "");
        PlayerPrefs.Save();
        playerName = "계란";

        player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = Vector3.zero;
    }

    private float[] Vector3ToFloatArray(Vector3 v) => new float[] { v.x, v.y, v.z };

    public void ClearCurrentData()
    {
        currentPlayerName = "";
        sceneName = "";
        playerLevel = 0;
        cutsceneWatched = false;
        currentChapter = null;
        GoldManager.Instance?.SetGold(0);
    }

    public void SaveMap(int slotIndex, List<MapNode> mapNodes)
    {
        MapSaveData saveData = new() { nodes = ConvertToSerializable(mapNodes) };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetMapPath(slotIndex), json);
    }

    public List<MapNode> LoadMap(int slotIndex)
    {
        string path = GetMapPath(slotIndex);
        if (!File.Exists(path)) return null;

        var saveData = JsonUtility.FromJson<MapSaveData>(File.ReadAllText(path));
        Dictionary<int, MapNode> nodeDict = new();

        foreach (var sNode in saveData.nodes)
        {
            nodeDict[sNode.id] = new MapNode
            {
                id = sNode.id,
                x = sNode.x,
                y = sNode.y,
                type = sNode.type,
                sceneName = sNode.sceneName,
                visited = sNode.visited,
                isUnlocked = sNode.isUnlocked,
                isCleared = sNode.isCleared,
                connectedNodeIds = sNode.connectedNodeIds,
                connectedNodes = new()
            };
        }

        foreach (var node in nodeDict.Values)
        {
            foreach (int targetId in node.connectedNodeIds)
                if (nodeDict.TryGetValue(targetId, out var targetNode))
                    node.connectedNodes.Add(targetNode);
        }

        return new List<MapNode>(nodeDict.Values);
    }

    private string GetMapPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slotIndex}_map.json");
    }

    public void SaveReward(Reward reward)
    {
        SaveData current = LoadFromSlot(CurrentSlotIndex);
        if (current == null) return;

        current.pendingReward = reward;
        current.isEnemyDead = true;
        SaveToSlot(CurrentSlotIndex, current);
    }

    public bool HasPendingReward(out Reward reward)
    {
        reward = null;
        SaveData current = LoadFromSlot(CurrentSlotIndex);
        if (current == null) return false;

        reward = current.pendingReward;
        return current.isEnemyDead && reward != null;
    }

    public void ClearReward()
    {
        SaveData current = LoadFromSlot(CurrentSlotIndex);
        if (current == null) return;

        current.pendingReward = null;
        current.isEnemyDead = false;
        SaveToSlot(CurrentSlotIndex, current);
    }

    public void ClearPendingReward()
    {
        SaveData saveData = LoadFromSlot(CurrentSlotIndex);
        if (saveData == null) return;

        saveData.pendingReward = null;
        saveData.isEnemyDead = false;
        SaveToSlot(CurrentSlotIndex, saveData);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedRestorePlayerStats());
    }

    private IEnumerator DelayedRestorePlayerStats()
    {
        yield return new WaitForSeconds(0.1f);

        // ? 새 게임이면 덮어쓰기 하지 않음
        if (lastLoadedData == null) yield break;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            var stats = playerObj.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.SetMaxHP(lastLoadedData.playerMaxHP);
                stats.SetCurrentHP(lastLoadedData.playerCurrentHP);
                Debug.Log($"[SaveManager] 체력 복원 완료: {stats.CurrentHP}/{stats.MaxHP}");
            }
        }
    }

    //플레이어 스탯과 연동되는것 playerstats.cs의 start에서 ready알림을 보내면 여기서 작동함.
    private PlayerStats cachedStats;

    public void OnPlayerStatsReady(PlayerStats stats)
    {
        cachedStats = stats;

        if (lastLoadedData == null && currentChapter != null)
        {
            Debug.Log("[SaveManager] PlayerStats 도착 - ApplyChapterPreset 실행");
            ApplyChapterPreset_Internal(); // 내부 로직만 따로 분리
        }
    }

    private void ApplyChapterPreset_Internal()
    {
        if (cachedStats == null) return;

        int maxHp = currentChapter.defaultPlayerMaxHP;
        cachedStats.SetMaxHP(maxHp);
        cachedStats.SetCurrentHP(maxHp);

        CurrentPlayerMaxHP = maxHp;
        CurrentPlayerHP = maxHp;

        GoldManager.Instance?.SetGold(currentChapter.startingGold);
        // 나머지 덱/유물 초기화 등도 여기에 계속
    }


}
