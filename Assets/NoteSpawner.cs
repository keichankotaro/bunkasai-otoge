using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class NoteSpawner : MonoBehaviour
{
    [Header("Lane Settings")]
    [Tooltip("レーン番号 (1-4)")]
    public int laneIndex;

    [Header("Prefabs")]
    [Tooltip("通常ノーツのプレハブ")]
    public GameObject prefab;
    [Tooltip("ロングノーツのプレハブ")]
    public GameObject prefab_long;

    // --- 内部変数 ---
    // 生成したノーツのリスト
    private List<GameObject> objs;
    // 現在の生成数カウント
    private int now_count = 0;
    // 生成開始フラグ (staticではなくインスタンス変数に変更)
    private bool started = false;
    // 総ノーツ数
    private int noteCount;

    // 更新用
    void Update()
    {
        // 準備完了かつ未生成なら生成開始
        if (adata.ready_to_start)
        {
            if (!started)
            {
                StartSpawnTask();
            }
        }
        else
        {
            // リセット処理
            started = false;
            objs = null;
            now_count = 0;
        }
    }

    // 非同期でノーツ生成を開始
    private async void StartSpawnTask()
    {
        started = true;
        await SpawnNotes();
    }

    // ノーツ生成のメインロジック
    private async Task SpawnNotes()
    {
        // --- キャッシュデータの取得 ---
        Dictionary<int, adata.NoteInfo> noteCache = null;

        switch (laneIndex)  
        {
            case 1: noteCache = adata.L1Notes; break;
            case 2: noteCache = adata.L2Notes; break;
            case 3: noteCache = adata.L3Notes; break;
            case 4: noteCache = adata.L4Notes; break;
            default: Debug.LogError("[NoteSpawner] Invalid Lane Index: " + laneIndex); return;
        }

        if (noteCache == null) return;

        noteCount = noteCache.Count;
        objs = new List<GameObject>();

        // adataの各レーンノーツ数に加算
        switch (laneIndex)
        {
            case 1: adata.l1notes += noteCount; break;
            case 2: adata.l2notes += noteCount; break;
            case 3: adata.l3notes += noteCount; break;
            case 4: adata.l4notes += noteCount; break;
        }

        // --- ノーツの生成ループ ---
        // 最大同時表示数まで生成（または全ノーツ数がそれ以下なら全て）
        int spawnLimit = (noteCount > adata.maximum_notes) ? adata.maximum_notes : noteCount;

        for (int i = 1; i <= spawnLimit; i++)
        {
            if (noteCache.ContainsKey(i))
            {
                string type = noteCache[i].type;
                GameObject newNote = null;

                if (type == "tap")
                {
                    newNote = Instantiate(prefab);
                    newNote.name = $"L{laneIndex}_{i}";
                }
                else if (type == "long")
                {
                    newNote = Instantiate(prefab_long);
                    newNote.name = $"L{laneIndex}-long_{i}";
                }

                if (newNote != null)
                {
                    objs.Add(newNote);
                    
                    // LaneControllerにレーン情報を渡す（GetComponentの負荷軽減のため初期化時に渡すのが理想）
                    LaneController controller = newNote.GetComponent<LaneController>();
                    if (controller != null)
                    {
                        controller.laneIndex = this.laneIndex;
                    }

                    // 最終IDの更新
                    switch (laneIndex)
                    {
                        case 1: adata.LastID_L1 = i; break;
                        case 2: adata.LastID_L2 = i; break;
                        case 3: adata.LastID_L3 = i; break;
                        case 4: adata.LastID_L4 = i; break;
                    }
                }
            }
        }

        now_count += spawnLimit;
        
        await Task.CompletedTask;
    }
}
