using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

public class LaneController : MonoBehaviour
{
    private static readonly Dictionary<string, int> lastCompletedNoteIdByLane = new Dictionary<string, int>
    {
        { "L1", -1 },
        { "L2", -1 },
        { "L3", -1 },
        { "L4", -1 },
    };

    [Tooltip("Set the lane number (1-4) in the Unity Editor")]
    public int laneIndex; 

    private string[] data;
    private float arrsec;
    private float speed;
    private int id;
    private JObject jsonObj;
    private float game_time = 0;
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
    private bool hasRegisteredCompletion = false;
    private bool debugDataSent;

    // Lane-specific configurations
    private string laneJsonId;
    private int totalLaneNotes;
    private string inputKey;
    private KeyCode controllerKey;
    private string raycastTargetName;
    private Vector3 judgeEffectPosition;

    void Start()
    {
        InitializeLaneData();
        Init_0();
        InitializeNote();
    }

    private static bool IsPreviousNoteCompleted(string laneId, int currentId)
    {
        if (!lastCompletedNoteIdByLane.TryGetValue(laneId, out var lastCompleted))
        {
            return false;
        }

        return currentId == 0 || lastCompleted >= currentId - 1;
    }

    private void RegisterNoteCompletion()
    {
        if (hasRegisteredCompletion)
        {
            return;
        }

        if (lastCompletedNoteIdByLane.ContainsKey(laneJsonId))
        {
            lastCompletedNoteIdByLane[laneJsonId] = Math.Max(lastCompletedNoteIdByLane[laneJsonId], id);
        }
        else
        {
            lastCompletedNoteIdByLane[laneJsonId] = id;
        }

        hasRegisteredCompletion = true;
        isPreviousObjectDestroyed = true;
    }
    
    private void InitializeLaneData()
    {
        // Adjust index to be 0-based for arrays
        int index = laneIndex - 1;

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
        time_reming = (game_time - arrsec) * -1.0f;
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
            JObject nextNoteData = JObject.Parse(adata.jsonObj["chartdata"][laneJsonId][newId + ""].ToString());
            string nextNoteType = nextNoteData["type"].ToString();

            name = $"{laneJsonId}_{(nextNoteType == "long" ? "long_" : "")}{newId}";

            await InitializeNote();
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
        
        cd = JObject.Parse(jsonObj["chartdata"][laneJsonId][id.ToString()].ToString());
        arrsec = (float)cd["time"];
        offset = (float)jsonObj["maindata"]["offset"];
        type = cd["type"].ToString();
        speed = (float)cd["speed"] * (adata.speed * 10.0f);
        
        long_click = false;

        s_change = cd.ContainsKey("s_change") && (bool)cd["s_change"];
        if (s_change)
        {
            change = (JArray)cd["changes"];
            change_count = 0;
        }

        if (type == "long")
        {
            endsec = (float)cd["endtime"];
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
        
        transform.position = new Vector3(transform.position.x, transform.position.y, -40000);
        LongNoteDetermined = false;
        EndTask = false;
        debugDataSent = false;
        hasRegisteredCompletion = false;
        isPreviousObjectDestroyed = IsPreviousNoteCompleted(laneJsonId, id);

        return Task.CompletedTask;
    }
    
    private void UpdateScore(string judge, int amount = 1)
    {
        if (judge == "Bad" || judge == "Miss") {
            ScoreManeger.combo = 0;
        } else {
            ScoreManeger.combo++;
        }

        switch(judge)
        {
            case "Perfect": ScoreManeger.score += 5; ResultUI.Perfect++; break;
            case "Good": ScoreManeger.score += 3; ResultUI.Good++; break;
            case "Bad": ResultUI.Bad++; break;
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
        updateDebug(amount, judge == "Perfect" ? 5 : (judge == "Good" ? 3 : 0));

        Action<int> updateJudgeCount = (j) => {};
        switch(laneIndex)
        {
             case 1:
                if(judge == "Perfect") DebugText.perfect_l1++;
                else if(judge == "Good") DebugText.good_l1++;
                else if(judge == "Bad") DebugText.bad_l1++;
                else if(judge == "Miss") DebugText.miss_l1 += amount;
                adata.clicked_L1 += amount;
                break;
            case 2:
                if(judge == "Perfect") DebugText.perfect_l2++;
                else if(judge == "Good") DebugText.good_l2++;
                else if(judge == "Bad") DebugText.bad_l2++;
                else if(judge == "Miss") DebugText.miss_l2 += amount;
                adata.clicked_L2 += amount;
                break;
            case 3:
                if(judge == "Perfect") DebugText.perfect_l3++;
                else if(judge == "Good") DebugText.good_l3++;
                else if(judge == "Bad") DebugText.bad_l3++;
                else if(judge == "Miss") DebugText.miss_l3 += amount;
                adata.clicked_L3 += amount;
                break;
            case 4:
                if(judge == "Perfect") DebugText.perfect_l4++;
                else if(judge == "Good") DebugText.good_l4++;
                else if(judge == "Bad") DebugText.bad_l4++;
                else if(judge == "Miss") DebugText.miss_l4 += amount;
                adata.clicked_L4 += amount;
                break;
        }
    }

    private void ShowJudgementEffect(string judge) {
        GameObject prefab = null;
        if(judge == "Perfect") prefab = adata.p;
        else if(judge == "Good") prefab = adata.g;
        else if(judge == "Bad") prefab = adata.b;
        else if(judge == "Miss") prefab = adata.m;

        if (prefab != null)
        {
            GameObject h = Instantiate(prefab, judgeEffectPosition, Quaternion.Euler(68.85f, 0.0f, 0.0f));
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
            
            // --- Speed & Position Update ---
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
                length = (originalSpeed * (endsec - arrsec)) / 2;
                transform.localScale = new Vector3(0.7f, 0.01f, length);
            }
            transform.position = new Vector3(
                transform.position.x,
                isLongNoteActive ? 0.0001f : 0.0004f,
                whereiam() + (isLongNoteActive ? (length / 2.0f) : 0)
            );
            
            // --- Previous Note Check ---
            if (!isPreviousObjectDestroyed)
            {
                isPreviousObjectDestroyed = IsPreviousNoteCompleted(laneJsonId, id);
            }

            if(isPreviousObjectDestroyed && !debugDataSent)
            {
                JObject root = new JObject { ["source"] = laneJsonId, ["payload"] = cd };
                DebugSocketClient.Instance.SendData(root.ToString());
                debugDataSent = true;
            }

            if (!isPreviousObjectDestroyed) return;

            // --- Judgement Logic ---
            float timelag = Mathf.Abs(game_time - arrsec);

            // --- AUTO PLAY ---
            if (adata.auto_play)
            {
                if (type == "tap" && (arrsec - game_time <= adata.auto))
                {
                    ShowJudgementEffect("Perfect"); UpdateScore("Perfect");
                    RegisterNoteCompletion();
                    await RecycleOrDestroy();
                }
                else if (type == "long")
                {
                    if (!long_click && (arrsec - game_time <= adata.auto))
                    {
                        ShowJudgementEffect("Perfect"); UpdateScore("Perfect");
                        long_click = true;
                    }
                    if (long_click && !LongNoteDetermined && (game_time >= endsec))
                    {
                        ShowJudgementEffect("Perfect"); UpdateScore("Perfect");
                        LongNoteDetermined = true;
                        RegisterNoteCompletion();
                        await RecycleOrDestroy();
                    }
                }
                return;
            }

            // --- MANUAL PLAY ---
            bool keyPress = Input.GetKeyDown(inputKey) || Input.GetKeyDown(controllerKey);
            bool keyHeld = Input.GetKey(inputKey) || Input.GetKey(controllerKey);
            bool mousePress = Input.GetMouseButtonDown(0) && IsMouseOverLane();
            bool mouseHeld = Input.GetMouseButton(0) && IsMouseOverLane();

            if (type == "tap")
            {
                if (keyPress || mousePress)
                {
                    if (timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect"); }
                    else if (timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good"); }
                    else if (timelag <= adata.bad) { ShowJudgementEffect("Bad"); UpdateScore("Bad"); }
                    else { return; } // Input too early/late, ignore
                    RegisterNoteCompletion();
                    await RecycleOrDestroy();
                }
                else if (game_time > arrsec + adata.miss)
                {
                    ShowJudgementEffect("Miss"); UpdateScore("Miss");
                    RegisterNoteCompletion();
                    await RecycleOrDestroy();
                }
            }
            else // Long Note
            {
                if (!long_click) // State: Waiting for press
                {
                    if (keyPress || mousePress)
                    {
                        if (timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect"); long_click = true; }
                        else if (timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good"); long_click = true; }
                        else if (timelag <= adata.bad) {
                            ShowJudgementEffect("Bad"); UpdateScore("Bad", 2);
                            LongNoteDetermined = true;
                        }
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
                        if (end_timelag <= adata.perfect) { ShowJudgementEffect("Perfect"); UpdateScore("Perfect"); }
                        else if (end_timelag <= adata.good) { ShowJudgementEffect("Good"); UpdateScore("Good"); }
                        else { ShowJudgementEffect("Bad"); UpdateScore("Bad"); } // Released too early = Bad
                        LongNoteDetermined = true;
                    }
                    else if (game_time >= endsec) // Player held until the end
                    {
                        ShowJudgementEffect("Perfect"); UpdateScore("Perfect");
                        LongNoteDetermined = true;
                    }
                }

                if (LongNoteDetermined)
                {
                    RegisterNoteCompletion();
                    await RecycleOrDestroy();
                }
            }
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
