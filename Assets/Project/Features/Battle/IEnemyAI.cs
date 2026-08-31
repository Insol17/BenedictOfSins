using UnityEngine;

public interface IEnemyAI
{
    EnemyStats Self { get; }
    PlayerStats Player { get; }

    void OnTurnStart();
    void PerformAction();
    void OnTurnEnd();
    EnemyIntent CalculateNextIntent();
    void ForceUpdateIntentUI();

    /// <summary> 전체 턴 진행 흐름 </summary>
    void PerformTurn(); // ← 새로 추가
}
