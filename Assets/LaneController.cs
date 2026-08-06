using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

public class LaneController : MonoBehaviour
{
    [Tooltip("Set the lane number (1-4) in the Unity Editor")]
    public int laneIndex; 

    private string[] data;
    private float arrsec;
    private float speed;
    private int id;
    private JObject jsonObj;
    private float game_time = 0;
    private adata.NoteInfo currentNoteInfo;
    private JObject cd;
    private float time_reming;
    private float now;
    private bool long_click = false;
    private float endsec;
    private float length;
    private float offset;

    private bool isLongNoteActive = false;
    private bool isPreviousObjectDestroyed = false;
    private bool EndTask = false;
    private string type;

    private bool s_change;
    private int change_count = 0;
    private JArray change;

    private bool LongNoteDetermined = false;
    private bool debugDataSent;

    // Lane-specific configurations
    private string laneJsonId;
    private int totalLaneNotes;
    private string inputKey;
    private KeyCode controllerKey;
    private string raycastTargetName;
    private Vector3 judgeEffectPosition;

    // Cached data reference
    private Dictionary<int, adata.NoteInfo> currentLaneNotes;

    // Helper to access static anti-ghosting variables
    private int LastJudgedFrame
    {
        get
        {
            switch (laneIndex)
            {
                case 1: return adata.lastJudgedFrame_L1;
                case 2: return adata.lastJudgedFrame_L2;
                case 3: return adata.lastJudgedFrame_L3;
                case 4: return adata.lastJudgedFrame_L4;
                default: return -1;
            }
        }
        set
        {
            switch (laneIndex)
            {
                case 1: adata.lastJudgedFrame_L1 = value; break;
                case 2: adata.lastJudgedFrame_L2 = value; break;
                case 3: adata.lastJudgedFrame_L3 = value; break;
                case 4: adata.lastJudgedFrame_L4 = value; break;
            }
        }
    }

    void Start()
    {
        InitializeLaneData();
        Init_0();
        InitializeNote();
    }
    
    private void InitializeLaneData()
    {
        // Adjust index to be 0-based for arrays
        int index = laneIndex - 1;

        switch (laneIndex)
        {
            case 1: currentLaneNotes = adata.L1Notes; break;
            case 2: currentLaneNotes = adata.L2Notes; break;
            case 3: currentLaneNotes = adata.L3Notes; break;
            case 4: currentLaneNotes = adata.L4Notes; break;
            default: Debug.LogError("Invalid Lane Index"); break;
        }

        string[] laneJsonIds = { "L1", "L2", "L3", "L4" };
        int[] totalNotes = { adata.l1notes, adata.l2notes, adata.l3notes, adata.l4notes };
        string[] inputKeys = { "d", "f", "j", "k" }; 
        KeyCode[] controllerKeys = { adata.ControlerLine1, adata.ControlerLine2, adata.ControlerLine3, adata.ControlerLine4 };
        string[] raycastNames = { "CL1", "CL2", "CL3", "CL4" };
        Vector3[] judgePositions = {
            new Vector3(8.875f, 0.2f, -11.9f),
            new Vector3(9.48f, 0.2f, -11.9f),
            new Vector3(10.08f, 0.2f, -11.9f),
            new Vector3(10.68f, 0.2f, -11.9f)
        };

        if (index < 0 || index >= laneJsonIds.Length)
        {
            Debug.LogError("Invalid laneIndex. Please set it to a value between 1 and 4.");
            this.enabled = false;
            return;
        }

        laneJsonId = laneJsonIds[index];
        totalLaneNotes = totalNotes[index];
        inputKey = inputKeys[index];
        controllerKey = controllerKeys[index];
        raycastTargetName = raycastNames[index];
        judgeEffectPosition = judgePositions[index];
    }

    float whereiam()
    {
        float effectiveArrsec = (type == "long" && long_click) ? Mathf.Min(game_time, endsec) : arrsec;
        time_reming = (game_time - effectiveArrsec) * -1.0f;
        now = (time_reming * (speed / 2)) - 9.51f;
        return now;
    }

    private async Task RecycleOrDestroy()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -40000f);
        EndTask = true; // Make sure task is ended before recycling

        int newId = id + adata.maximum_notes;

        if (newId > totalLaneNotes)
        {
            Destroy(gameObject);
        }
        else
        {
            // Optimized: Get type from cache without parsing
            if (currentLaneNotes.TryGetValue(newId, out adata.NoteInfo nextNote))
            {
                string nextNoteType = nextNote.type;
                name = $"{laneJsonId}_{(nextNoteType == "long" ? "long_" : "")}{newId}";
                await InitializeNote();
            }
            else
            {
                Debug.LogError($"Next note {newId} not found in cache.");
                Destroy(gameObject);
            }
        }
    }

    private int GetIdFromName(int AttemptCount)
    {
        // Unity renames objects with (Clone) so we need to handle that
        string objectName = name.Split('(')[0];
        data = objectName.Split('_');
        try
        {
            // For "L1_1", data is ["L1", "1"] -> id = 1
            // For "L1-long_1", data is ["L1-long", "1"] -> id = 1
            id = int.Parse(data[data.Length - 1]);
        }
        catch
        {
            if(AttemptCount > 50) {
                Debug.LogError($"Could not parse ID from name: {name}");
                Destroy(this.gameObject);
                return 0;
            }
            return GetIdFromName(AttemptCount + 1);
        }
        
        if (id == 0 && AttemptCount < 50)
        {
             return GetIdFromName(AttemptCount + 1);
        }

        return id;
    }

    private void Init_0()
    {
        jsonObj = adata.jsonObj;
    }

    private Task InitializeNote()
    {
        game_time = 0.0f;
        id = GetIdFromName(0);
        
        if (currentLaneNotes.TryGetValue(id, out adata.NoteInfo note))
        {
            currentNoteInfo = note;
            cd = note.rawData;
            
            arrsec = note.time;
            offset = (float)jsonObj["maindata"]["offset"];
            type = note.type;
            speed = note.speed * (adata.speed * 10.0f);
            
            long_click = false;

            s_change = note.s_change;
            if (s_change)
            {
                change = note.changes;
                change_count = 0;
            }

            if (type == "long")
            {
                endsec = note.endtime;
                length = (speed / 2) * (endsec - arrsec) / 2;
                GetComponent<Renderer>().material.color = new Color32(90, 255, 96, 255);
                this.transform.localScale = new Vector3(0.7f, 0.01f, length);
                isLongNoteActive = true;
            }
            else // tap
            {
                isLongNoteActive = false;
                GetComponent<Renderer>().material.color = new Color32(0, 255, 232, 255);
                transform.localScale = new Vector3(0.7f, 0.01f, 0.1f);
            }
        }
        else
        {
            Debug.LogError($"Note {id} not found in cache for lane {laneIndex}");
            Destroy(gameObject);
            return Task.CompletedTask;
        }
        
        transform.position = new Vector3(transform.position.x, transform.position.y, -40000);
        LongNoteDetermined = false;
        EndTask = false;
        debugDataSent = false;
        isPreviousObjectDestroyed = false; // Reset this flag

        return Task.CompletedTask;
    }
    
    private void UpdateScore(string judge, int amount = 1, bool isLate = false, bool isAuto = false)
    {
        if (judge == "Bad" || judge == "Miss") {
            ScoreManeger.combo = 0;
        } else {
            if (judge != "Good")
            {
                ScoreManeger.combo += amount;
            }
        }

        switch(judge)
        {
            case "PerfectPlus": 
                ScoreManeger.score += 21; 
                ResultUI.PerfectPlus++; 
                if (!isAuto) { if (isLate) ResultUI.PerfectPlusLate++; else ResultUI.PerfectPlusFast++; }
                break;
            case "Perfect": 
                ScoreManeger.score += 20; 
                ResultUI.Perfect++; 
                if (!isAuto) { if (isLate) ResultUI.PerfectLate++; else ResultUI.PerfectFast++; }
                break;
            case "Great": 
                ScoreManeger.score += 16; 
                ResultUI.Great++; 
                if (!isAuto) { if (isLate) ResultUI.GreatLate++; else ResultUI.GreatFast++; }
                break;
            case "Good": 
                ScoreManeger.score += 8; 
                ResultUI.Good++; 
                if (!isAuto) { if (isLate) ResultUI.GoodLate++; else ResultUI.GoodFast++; }
                break;
            case "Bad": 
                ScoreManeger.score += 0; 
                ResultUI.Bad++; 
                if (!isAuto) { if (isLate) ResultUI.BadLate++; else ResultUI.BadFast++; }
                break;
            case "Miss": ResultUI.Miss += amount; break;
        }

        Action<int, int> updateDebug = (j, s) => {};
        switch(laneIndex)
        {
            case 1: updateDebug = (j, s) => { DebugText.judged_l1 += j; DebugText.score_l1 += s; }; break;
            case 2: updateDebug = (j, s) => { DebugText.judged_l2 += j; DebugText.score_l2 += s; }; break;
            case 3: updateDebug = (j, s) => { DebugText.judged_l3 += j; DebugText.score_l3 += s; }; break;
            case 4: updateDebug = (j, s) => { DebugText.judged_l4 += j; DebugText.score_l4 += s; }; break;
        }
        updateDebug(amount, judge == "PerfectPlus" ? 21 : (judge == "Perfect" ? 20 : (judge == "Great" ? 16 : (judge == "Good" ? 8 : (judge == "Bad" ? 0 : 0)))));

        Action<int> updateJudgeCount = (j) => {};
        switch(laneIndex)
        {
             case 1:
                if(judge == "PerfectPlus") DebugText.perfect_plus_l1++;
                else if(judge == "Perfect") DebugText.perfect_l1++;
                else if(judge == "Great") DebugText.great_l1++;
                else if(judge == "Good") DebugText.good_l1++;
                else if(judge == "Bad") DebugText.bad_l1++;
                else if(judge == "Miss") DebugText.miss_l1 += amount;
                adata.clicked_L1 += amount;
                break;
            case 2:
                if(judge == "PerfectPlus") DebugText.perfect_plus_l2++;
                else if(judge == "Perfect") DebugText.perfect_l2++;
                else if(judge == "Great") DebugText.great_l2++;
                else if(judge == "Good") DebugText.good_l2++;
                else if(judge == "Bad") DebugText.bad_l2++;
                else if(judge == "Miss") DebugText.miss_l2 += amount;
                adata.clicked_L2 += amount;
                break;
            case 3:
                if(judge == "PerfectPlus") DebugText.perfect_plus_l3++;
                else if(judge == "Perfect") DebugText.perfect_l3++;
                else if(judge == "Great") DebugText.great_l3++;
                else if(judge == "Good") DebugText.good_l3++;
                else if(judge == "Bad") DebugText.bad_l3++;
                else if(judge == "Miss") DebugText.miss_l3 += amount;
                adata.clicked_L3 += amount;
                break;
            case 4:
                if(judge == "PerfectPlus") DebugText.perfect_plus_l4++;
                else if(judge == "Perfect") DebugText.perfect_l4++;
                else if(judge == "Great") DebugText.great_l4++;
                else if(judge == "Good") DebugText.good_l4++;
                else if(judge == "Bad") DebugText.bad_l4++;
                else if(judge == "Miss") DebugText.miss_l4 += amount;
                adata.clicked_L4 += amount;
                break;
        }
    }

    private void ShowJudgementEffect(string judge) {
        GameObject prefab = null;
        if(judge == "Perfect" || judge == "PerfectPlus") prefab = adata.p;
        else if(judge == "Great") prefab = adata.gr;
        else if(judge == "Good") prefab = adata.g;
        else if(judge == "Bad") prefab = adata.b;
        else if(judge == "Miss") prefab = adata.m;

        if (prefab != null)
        {
            GameObject h = Instantiate(prefab, judgeEffectPosition, Quaternion.Euler(68.85f, 0.0f, 0.0f));
            if (judge == "PerfectPlus")
            {
                var sr = h.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color32(236, 255, 0, 255);
                }
                else
                {
                    // Fallback to Image if it's UI based
                    var img = h.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        img.color = new Color32(236, 255, 0, 255);
                    }
                }
            }
            h.GetComponent<goodbye>().SetInstantiatorName(id.ToString(), laneIndex);
        }
    }


    async void Update()
    {
        try
        {
            if (!adata.ready_to_start || EndTask)
            {
                return;
            }
            
            // ここからスピード・位置更新
            game_time = adata.game_time;
            float originalSpeed = (float)cd["speed"] * (adata.speed * 10.0f);
            speed = originalSpeed;

            if (s_change && change != null && change_count < change.Count)
            {
                // ... (speed change logic remains the same)
            }

            if (type == "long")
            {
                endsec = (float)cd["endtime"];
                float effectiveArrsec = long_click ? Mathf.Min(game_time, endsec) : arrsec;
                length = (originalSpeed * (endsec - effectiveArrsec)) / 2;
                transform.localScale = new Vector3(0.7f, 0.01f, length);
            }
            transform.position = new Vector3(
                transform.position.x,
                isLongNoteActive ? 0.0001f : 0.0004f,
                whereiam() + (isLongNoteActive ? (length / 2.0f) : 0)
            );
            // ここまでスピード・位置更新
            
            // ここから前のノーツ確認
            if (!isPreviousObjectDestroyed)
            {
                 if (id > 0)
                 {
                    string prev_id = (id - 1).ToString();
                    // This check is inefficient. For better performance, use an event-based system.
                    if (GameObject.Find($"{laneJsonId}_{prev_id}") == null && GameObject.Find($"{laneJsonId}_long_{prev_id}") == null)
                    {
                        isPreviousObjectDestroyed = true;
                    }
                 }
                 else
                 {
                    isPreviousObjectDestroyed = true;
                 }
            }

            if(isPreviousObjectDestroyed && !debugDataSent)
            {
                JObject root = new JObject { ["source"] = laneJsonId, ["payload"] = cd };
                DebugSocketClient.Instance.SendData(root.ToString());
                debugDataSent = true;
            }

            if (!isPreviousObjectDestroyed) return;
            // ここまで前のノーツ確認

            // ここから判定ロジック
            float timelag = Mathf.Abs(game_time - arrsec);

            // ここからオートプレイ
            if (adata.auto_play)
            {
                if (type == "tap" && (arrsec - game_time <= adata.auto))
                {
                    ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, false, true);
                    await RecycleOrDestroy();
                }
                else if (type == "long")
                {
                    if (!long_click && (arrsec - game_time <= adata.auto))
                    {
                        ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, false, true);
                        long_click = true;
                    }
                    if (long_click && !LongNoteDetermined && (game_time >= endsec))
                    {
                        ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, false, true);
                        LongNoteDetermined = true;
                        await RecycleOrDestroy();
                    }
                }
                return;
            }
            // ここまでオートプレイ

            // ここから手動プレイ
            bool keyPress = Input.GetKeyDown(inputKey) || Input.GetKeyDown(controllerKey);
            bool keyHeld = Input.GetKey(inputKey) || Input.GetKey(controllerKey);
            bool mousePress = Input.GetMouseButtonDown(0) && IsMouseOverLane();
            bool mouseHeld = Input.GetMouseButton(0) && IsMouseOverLane();
            bool isLate = (game_time - arrsec) > 0;

            // Anti-Ghosting: If input was already used this frame for this lane, ignore it for this note.
            if ((keyPress || mousePress) && LastJudgedFrame == Time.frameCount)
            {
                keyPress = false;
                mousePress = false;
            }

            if (type == "tap")
            {
                if (keyPress || mousePress)
                {
                    if (timelag <= adata.perfect_plus) { ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    else if (timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    else if (timelag <= adata.great) { ShowJudgementEffect("Great"); UpdateScore("Great", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    else if (timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    //else if (timelag <= adata.bad) { ShowJudgementEffect("Bad"); UpdateScore("Bad", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    else if (timelag <= adata.miss) { ShowJudgementEffect("Miss"); UpdateScore("Miss", 1, isLate); LastJudgedFrame = Time.frameCount; }
                    else { return; } // Input too early/late, ignore
                    await RecycleOrDestroy();
                }
                else if (game_time > arrsec + adata.miss)
                {
                    ShowJudgementEffect("Miss"); UpdateScore("Miss");
                    await RecycleOrDestroy();
                }
            }
            else // Long Note
            {
                if (!long_click) // State: Waiting for press
                {
                    if (keyPress || mousePress)
                    {
                        if (timelag <= adata.perfect_plus) { ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, isLate); long_click = true; LastJudgedFrame = Time.frameCount; }
                        else if (timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect", 1, isLate); long_click = true; LastJudgedFrame = Time.frameCount; }
                        else if (timelag <= adata.great) { ShowJudgementEffect("Great"); UpdateScore("Great", 1, isLate); long_click = true; LastJudgedFrame = Time.frameCount; }
                        else if (timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good", 1, isLate); long_click = true; LastJudgedFrame = Time.frameCount; }
                        //else if (timelag <= adata.bad) { ShowJudgementEffect("Bad"); UpdateScore("Bad", 2, isLate); LongNoteDetermined = true; LastJudgedFrame = Time.frameCount; }
                        else if (timelag <= adata.miss) { ShowJudgementEffect("Miss"); UpdateScore("Miss", 2, isLate); LongNoteDetermined = true; LastJudgedFrame = Time.frameCount; }
                        else { return; } // Input too early/late, ignore
                    }
                    else if (game_time > arrsec + adata.miss)
                    {
                        ShowJudgementEffect("Miss"); UpdateScore("Miss", 2);
                        LongNoteDetermined = true;
                    }
                }
                else if (!LongNoteDetermined) // State: Holding
                {
                    bool isHolding = keyHeld || mouseHeld;

                    if (!isHolding) // Player released key
                    {
                        float end_timelag = Mathf.Abs(game_time - endsec);
                        bool endIsLate = (game_time - endsec) > 0;
                        if (end_timelag <= adata.perfect_plus) { ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, endIsLate); }
                        else if (end_timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect", 1, endIsLate); }
                        else if (end_timelag <= adata.great) { ShowJudgementEffect("Great"); UpdateScore("Great", 1, endIsLate); }
                        else if (end_timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good", 1, endIsLate); }
                        else if (end_timelag <= adata.miss) { ShowJudgementEffect("Miss"); UpdateScore("Miss", 1, endIsLate); } // Released too early = Miss
                        LongNoteDetermined = true;
                    }
                    else if (game_time >= endsec) // Player held until the end
                    {
                        ShowJudgementEffect("PerfectPlus"); UpdateScore("PerfectPlus", 1, false, true);
                        LongNoteDetermined = true;
                    }
                }

                if (LongNoteDetermined)
                {
                    await RecycleOrDestroy();
                }
            }
            // ここまで手動プレイ
            // ここまで判定ロジック
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in Lane {laneIndex} note {id} ({name}): {e}");
            Destroy(gameObject);
        }
    }

    private bool IsMouseOverLane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        return Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == raycastTargetName;
    }
}
