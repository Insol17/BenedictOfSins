using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // ▶ 저장된 씬 정보
    public string sceneName;             // 씬 이름 (실제 로드용)
    public string displaySceneName;      // 씬 표시 이름 (UI용)
    public string saveTime;              // 저장 시간 문자열
    public string playerName;            // 저장된 플레이어 이름

    // ▶ 플레이어 위치
    public float[] playerPosition = new float[3]; // [x, y, z]

    // ▶ 기본 상태 정보
    public int gold;                    // 보유 골드
    public int playerLevel;            // 레벨
    public bool cutsceneWatched;       // 컷씬 감상 여부

    // ▶ 맵 관련 정보
    public MapSaveData mapData;                         // 맵 상태
    public int currentMapNodeId;                        // 현재 선택된 맵 노드 ID
    public int currentNodeId;                           // 현재 위치한 노드 ID
    public List<int> visitedNodeIds = new();            // 방문한 노드 ID 리스트
    public List<SerializableMapNode> savedMapNodes;     // 저장용 맵 노드 목록
    public string chapterId;                            // 챕터 프리셋 ID

    // ▶ 보상/전투 상태
    public Reward pendingReward;          // 저장된 보상 (전투 후)
    public bool isEnemyDead;              // 적 사망 상태 여부

    // ▶ 플레이어 체력 저장용 (PlayerStats 상태 저장)
    public int playerMaxHP;               // 최대 체력
    public int playerCurrentHP;           // 현재 체력

    // ▶ 게임 상태
    public List<string> relicIds;         // 유물 ID 리스트

    // ▶ 카드 상태
    public List<SavedCardStack> allDeck = new();

    // ▶ 위치 벡터 변환 유틸리티
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
    }

    public void SetPlayerPosition(Vector3 pos)
    {
        playerPosition[0] = pos.x;
        playerPosition[1] = pos.y;
        playerPosition[2] = pos.z;
    }
}
