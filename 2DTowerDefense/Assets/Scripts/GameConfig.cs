using System;
using UnityEngine;

public enum OperatorRole
{
    Melee,
    Ranged,
    Tank,
    Healer
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Tower Defense/Game Config")]
public class GameConfig : ScriptableObject
{
    [Serializable]
    public class OperatorDefinition
    {
        [Tooltip("画面と選択ボタンに表示するユニット名です。")]
        public string displayName;
        [Tooltip("近距離は敵をブロック、遠距離は射程内の敵を攻撃、重装は多くブロック、回復は味方を回復します。")]
        public OperatorRole role;
        [Tooltip("配置時に消費する配置ポイントです。")]
        [Min(0)] public int deploymentCost = 3;
        [Tooltip("ユニットの最大HPです。")]
        [Min(1)] public int maxHealth = 100;
        [Tooltip("1回の攻撃で敵に与えるダメージです。回復役では使用しません。")]
        [Min(0)] public int attackDamage = 20;
        [Tooltip("攻撃または回復を行う間隔（秒）です。")]
        [Min(0.05f)] public float attackInterval = 1f;
        [Tooltip("攻撃または回復が届く距離です。ワールド座標の距離で指定します。")]
        [Min(0.1f)] public float attackRange = 1f;
        [Tooltip("同時に足止めできる敵の数です。遠距離と回復役は通常0にします。")]
        [Min(0)] public int blockCapacity = 1;
        [Tooltip("1回に味方へ回復するHPです。回復役で使用します。")]
        [Min(0)] public int healingAmount;
        [Tooltip("盤面に表示するユニットの色です。")]
        public Color color = Color.white;
    }

    [Serializable]
    public class EnemyDefinition
    {
        [Tooltip("画面表示や設定確認に使う敵の名前です。")]
        public string displayName;
        [Tooltip("敵の最大HPです。")]
        [Min(1)] public int maxHealth = 80;
        [Tooltip("ブロック中のユニットに与える1回分のダメージです。")]
        [Min(0)] public int attackDamage = 10;
        [Tooltip("ブロック中に攻撃する間隔（秒）です。")]
        [Min(0.05f)] public float attackInterval = 1f;
        [Tooltip("拠点方向へ進む速さです。ワールド座標の単位/秒で指定します。")]
        [Min(0f)] public float moveSpeed = 0.7f;
        [Tooltip("盤面に表示する敵の色です。")]
        public Color color = Color.red;
    }

    [Serializable]
    public class EnemyGroupDefinition
    {
        [Tooltip("GameConfigのEnemies配列の要素番号です。0が先頭の敵です。")]
        [Min(0)] public int enemyDefinitionIndex;
        [Tooltip("このグループから出現させる敵の数です。グループは配列順に処理します。")]
        [Min(1)] public int enemyCount = 1;
    }

    [Serializable]
    public class WaveDefinition
    {
        [Tooltip("ウェーブの管理用名称です。")]
        public string displayName;
        [Tooltip("GameConfigのEnemies配列の要素番号です。0が先頭の敵です。")]
        [Min(0)] public int enemyDefinitionIndex;
        [Tooltip("このウェーブで出現させる敵の数です。")]
        [Min(1)] public int enemyCount = 5;
        [Tooltip("敵を出現させる間隔（秒）です。")]
        [Min(0.1f)] public float spawnInterval = 2f;
        [Tooltip("敵の種類と数を順番に並べます。空の場合はEnemy Definition IndexとEnemy Countを使います。")]
        public EnemyGroupDefinition[] enemyGroups;
        [Tooltip("このウェーブで使うステージ内の敵ルート番号です。0がEnemy Routes配列の先頭です。")]
        [Min(0)] public int routeIndex;
        [Tooltip("出現先レーンの番号です。0が下側のレーンです。配列の順番で出現先を繰り返します。")]
        [Min(0)] public int[] lanes = { 0 };
    }

    [Header("バトル設定")]
    [Tooltip("拠点に敵が到達したときに減る耐久値の初期値です。0になると敗北します。")]
    [Min(1)] public int startingLives = 5;
    [Tooltip("バトル開始時に持っている配置ポイントです。")]
    [Min(0)] public int startingDeploymentPoints = 8;
    [Tooltip("配置ポイントの上限です。")]
    [Min(1)] public int maximumDeploymentPoints = 20;
    [Tooltip("配置ポイントが1回復するまでの時間（秒）です。")]
    [Min(0.1f)] public float deploymentPointInterval = 2f;
    [Tooltip("ウェーブ間の待ち時間（秒）です。")]
    [Min(0.1f)] public float timeBetweenWaves = 3f;
    [Tooltip("最初に遊ぶステージの番号です。0がStages配列の先頭です。")]
    [Min(0)] public int initialStageIndex;

    [Header("生成プレハブ")]
    [Tooltip("配置時に複製するユニット共通プレハブです。UnitActorを含めてください。")]
    public GameObject operatorPrefab;
    [Tooltip("ウェーブの敵出現時に複製する敵プレハブです。EnemyActorを含めてください。")]
    public GameObject enemyPrefab;

    [Header("ユニット一覧")]
    [Tooltip("配置できるユニットの定義です。要素ごとに役割・コスト・能力値を設定します。")]
    public OperatorDefinition[] operators;

    [Header("敵一覧")]
    [Tooltip("ウェーブから参照される敵の定義です。WaveのEnemy Definition Indexで要素を指定します。")]
    public EnemyDefinition[] enemies;

    [Header("ステージ一覧")]
    [Tooltip("遊ぶステージの順番です。クリア後に次の要素へ進みます。")]
    public StageDefinition[] stages;
}