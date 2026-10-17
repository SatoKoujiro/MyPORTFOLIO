using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Stage", menuName = "Tower Defense/Stage")]
public class StageDefinition : ScriptableObject
{
    [Serializable]
    public class EnemyRouteDefinition
    {
        [Tooltip("ルートの管理用名称です。")]
        public string displayName;
        [Tooltip("敵が順番に通過するマス座標です。最初が出現地点、最後が自軍拠点です。")]
        public Vector2Int[] waypoints;
    }

    [Header("ステージ")]
    [Tooltip("HUDに表示するステージ名です。")]
    public string displayName;
    [Tooltip("横方向に配置できるマスの数です。")]
    [Min(1)] public int columns = 7;
    [Tooltip("ルートの数です。1なら一本道、2なら上下2ルートになります。")]
    [Min(1)] public int laneCount = 1;
    [Tooltip("横方向の隣り合うマスの間隔（ワールド単位）です。")]
    [Min(0.1f)] public float cellSpacing = 1f;
    [Tooltip("上下ルート間の間隔（ワールド単位）です。")]
    [Min(0.1f)] public float laneSpacing = 1.25f;
    [Tooltip("配置レーン全体を上下にずらす量です。マップの通路位置に合わせます。")]
    public float laneOffsetY;
    [Tooltip("盤面を上から並べたタイル列です。#は壁、.は床、Bは自軍、Eは敵の出現位置です。")]
    public string[] mapLayout;
    [Tooltip("マップタイル1個の横幅（ワールド単位）です。")]
    [Min(0.1f)] public float mapCellSpacing = 1f;
    [Tooltip("マップタイル1個の高さ（ワールド単位）です。")]
    [Min(0.1f)] public float mapRowSpacing = 1f;
    [Tooltip("ウェーブから選択する敵の移動ルートです。各座標はマップ左上を(0,0)とします。")]
    public EnemyRouteDefinition[] enemyRoutes;
    [Header("ウェーブ")]
    [Tooltip("このステージで順番に実行するウェーブです。敵の種類はGameConfigのEnemies配列から選びます。")]
    public GameConfig.WaveDefinition[] waves;

    public int MapWidth
    {
        get
        {
            int width = 0;
            if (mapLayout != null)
            {
                for (int index = 0; index < mapLayout.Length; index++)
                {
                    if (mapLayout[index] != null)
                    {
                        width = Mathf.Max(width, mapLayout[index].Length);
                    }
                }
            }

            return width > 0 ? width : columns;
        }
    }

    public int MapHeight => mapLayout != null && mapLayout.Length > 0 ? mapLayout.Length : laneCount;
}