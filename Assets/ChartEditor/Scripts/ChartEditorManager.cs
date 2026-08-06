using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChartEditorManager : MonoBehaviour
{
    // --- 設定 ---
    public string initialChartPath = "Assets/Resources/Charts/test-chart.json";
    public AudioSource audioSource;
    
    // --- エディタ状態 ---
    public ChartData currentChart;
    public float currentTime = 0f;
    public bool isPlaying = false;
    public float playbackSpeed = 1.0f;
    
    public enum EditMode { Normal, Score }
    public EditMode currentMode = EditMode.Score;

    public enum EditorTool { Select, Pen, BPM, TimeSig }
    public EditorTool currentTool = EditorTool.Pen;

    public int selectedLane = 1;
    public string currentNoteType = "tap";
    
    // --- パレット / スコア設定 ---
    public enum NoteValue { Whole = 1, Half = 2, Quarter = 4, Eighth = 8, Sixteenth = 16, ThirtySecond = 32 }
    public NoteValue selectedNoteValue = NoteValue.Quarter;
    
    // --- 表示設定 ---
    public float pixelsPerSecond = 300f; 
    public float noteWidth = 100f;
    public float laneSpacing = 120f;
    public float hitLineY = 900f;
    public float toolbarWidth = 250f;

    private Rect editorRect;
    private Texture2D noteHeadTexture;

    // --- 操作状態 ---
    private NoteData draggingNote = null;
    private bool isDraggingEnd = false;
    private Vector2 contextMenuPos; 
    private bool showContextMenu = false;
    private NoteData contextNote = null; 
    private float contextTime = 0f;

    // BPMドラッグ用
    private SpeedChangeEvent draggingSpeedEvent = null;
    private bool isDraggingBPM = false;
    private Vector2 dragStartPos;

    // ハイライト用
    private object hoveredItem = null;

    // --- 速度/グラフエディタ状態 ---
    private bool showSpeedEditor = false;
    private SpeedChangeEvent editingSpeedEvent = null;
    private Rect speedEditorRect = new Rect(300, 100, 400, 300); // サイズ調整

    // --- 拍子エディタ状態 ---
    private bool showTimeSigEditor = false;
    private TimeSignatureEvent editingTimeSigEvent = null;
    private Rect timeSigEditorRect = new Rect(400, 200, 300, 200);

    void Start()
    {
        if (File.Exists(initialChartPath)) LoadChartInternal(initialChartPath);
        else currentChart = new ChartData();

        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        noteHeadTexture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++) {
            for (int x = 0; x < 32; x++) {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                noteHeadTexture.SetPixel(x, y, dist < 14 ? Color.black : Color.clear);
            }
        }
        noteHeadTexture.Apply();
    }

    void Update()
    {
        if (isPlaying && audioSource.clip != null) currentTime = audioSource.time;
        else if (isPlaying) currentTime += Time.deltaTime * playbackSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && !showSpeedEditor && !showTimeSigEditor) TogglePlay();

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                pixelsPerSecond = Mathf.Clamp(pixelsPerSecond + scroll * 100f, 50f, 2000f);
            }
            else
            {
                currentTime = Mathf.Max(0, currentTime - (scroll * 0.5f)); 
                if (isPlaying && audioSource.clip != null)
                {
                    audioSource.time = currentTime;
                }
            }
        }
    }

    public void LoadChartDialog() {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("チャート読込 (JSON)", "Assets/Resources/Charts", "json");
        if (!string.IsNullOrEmpty(path)) LoadChartInternal(path);
#endif
    }

    private void LoadChartInternal(string path) {
        if (File.Exists(path)) {
            string content = File.ReadAllText(path);
            currentChart = ChartFileManager.Load(content);
        }
    }

    public void SaveChartDialog() {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel("チャート保存 (JSON)", "Assets/Resources/Charts", "chart", "json");
        if (!string.IsNullOrEmpty(path)) {
            string content = ChartFileManager.Save(currentChart);
            File.WriteAllText(path, content);
        }
#endif
    }

    public void LoadAudioDialog() {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("音声ファイル読込", "", "wav,mp3,ogg");
        if (!string.IsNullOrEmpty(path)) StartCoroutine(LoadAudioCoroutine(path));
#endif
    }

    private IEnumerator LoadAudioCoroutine(string path) {
        string url = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN)) {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }

    public void TogglePlay() {
        isPlaying = !isPlaying;
        if (isPlaying && audioSource.clip != null) { audioSource.time = currentTime; audioSource.Play(); }
        else if (audioSource.clip != null) audioSource.Pause();
    }

    void OnGUI()
    {
        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        GUI.Box(new Rect(0, 0, toolbarWidth, Screen.height), "");
        
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f); 
        GUI.Box(new Rect(toolbarWidth, 0, Screen.width - toolbarWidth, Screen.height), "");
        GUI.backgroundColor = Color.white;

        editorRect = new Rect(toolbarWidth, 0, Screen.width - toolbarWidth, Screen.height);

        DrawPalette();

        if (showContextMenu && Event.current.type == EventType.MouseDown)
        {
            Rect menuRect = new Rect(contextMenuPos.x, contextMenuPos.y, 150, 100);
            if (!menuRect.Contains(Event.current.mousePosition))
            {
                showContextMenu = false;
                Event.current.Use();
            }
        }

        GUI.BeginGroup(editorRect);
        
        hoveredItem = null;

        if (currentMode == EditMode.Normal) DrawNormalMode();
        else DrawScoreMode();
        
        GUI.EndGroup();

        if (showContextMenu) DrawContextMenu();
        if (showSpeedEditor) speedEditorRect = GUI.Window(0, speedEditorRect, DrawSpeedEditorWindow, "BPM 設定"); // タイトル変更
        if (showTimeSigEditor) timeSigEditorRect = GUI.Window(1, timeSigEditorRect, DrawTimeSigEditorWindow, "拍子設定");
    }

    void DrawPalette()
    {
        GUILayout.BeginArea(new Rect(10, 10, toolbarWidth - 20, Screen.height - 20));
        GUIStyle header = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        
        GUILayout.Label("パレット", header);
        if (GUILayout.Button("チャートを開く")) LoadChartDialog();
        if (GUILayout.Button("保存")) SaveChartDialog();
        if (GUILayout.Button("音声読込")) LoadAudioDialog();
        
        GUILayout.Space(10);
        GUILayout.Label($"BPM: {GetBPMAt(currentTime):F0}", header); 
        GUILayout.Label($"時間: {currentTime:F2}s", header);
        if (GUILayout.Button(isPlaying ? "停止" : "再生")) TogglePlay();

        GUILayout.Space(15);
        GUILayout.Label("ツール", header);
        string[] tools = { "選択", "ペン (音符)", "BPM変更", "拍子変更" };
        EditorTool[] toolEnums = { EditorTool.Select, EditorTool.Pen, EditorTool.BPM, EditorTool.TimeSig };
        for(int i=0; i<tools.Length; i++) {
             if(GUILayout.Toggle(currentTool == toolEnums[i], tools[i], "Button")) currentTool = toolEnums[i];
        }

        if (currentTool == EditorTool.Pen)
        {
            GUILayout.Space(15);
            GUILayout.Label("音価 (グリッド)", header);
            string[] names = { "全音符", "2分", "4分", "8分", "16分", "32分" };
            NoteValue[] values = { NoteValue.Whole, NoteValue.Half, NoteValue.Quarter, NoteValue.Eighth, NoteValue.Sixteenth, NoteValue.ThirtySecond };
            for (int i = 0; i < names.Length; i++) {
                if (GUILayout.Toggle(selectedNoteValue == values[i], names[i], "Button")) selectedNoteValue = values[i];
            }

            GUILayout.Space(15);
            GUILayout.Label("ノート種類", header);
            if (GUILayout.Toggle(currentNoteType == "tap", "通常 (Tap)", "Button")) currentNoteType = "tap";
            if (GUILayout.Toggle(currentNoteType == "long", "ロング (Long)", "Button")) currentNoteType = "long";
        }

        GUILayout.Space(15);
        GUILayout.Label("表示モード", header);
        if (GUILayout.Toggle(currentMode == EditMode.Score, "楽譜モード", "Button")) currentMode = EditMode.Score;
        if (GUILayout.Toggle(currentMode == EditMode.Normal, "ノーマルモード", "Button")) currentMode = EditMode.Normal;

        GUILayout.EndArea();
    }

    float GetBPMAt(float t)
    {
        float bpm = currentChart.maindata.bpm;
        foreach(var ev in currentChart.speeddata.changes)
        {
            if (ev.triggerTime <= t && ev.type == "time")
            {
                bpm = currentChart.maindata.bpm * ev.value1;
            }
        }
        return bpm;
    }

    TimeSignatureEvent GetTimeSigAt(float t)
    {
        TimeSignatureEvent last = currentChart.timeSignatures[0];
        foreach(var ts in currentChart.timeSignatures)
        {
            if (ts.time <= t) last = ts;
        }
        return last;
    }

    float SnapToClosestMeasure(float targetTime)
    {
        float t = 0;
        float maxTime = targetTime + 10f; 
        float closestTime = 0;
        float minDiff = float.MaxValue;

        int safeCounter = 0;
        while (t < maxTime && safeCounter < 5000)
        {
            float diff = Mathf.Abs(targetTime - t);
            if (diff < minDiff)
            {
                minDiff = diff;
                closestTime = t;
            }

            float currentBPM = GetBPMAt(t);
            TimeSignatureEvent currentTS = GetTimeSigAt(t);
            float beatDuration = (60f / currentBPM) * (4f / currentTS.denominator);
            float measureDuration = beatDuration * currentTS.numerator;
            
            t += measureDuration;
            safeCounter++;
        }
        return closestTime;
    }

    void DrawScoreMode()
    {
        float staffSpacing = 80f;
        float startY = (Screen.height - (4 * staffSpacing)) / 2f;
        float drawOriginX = 100f - (currentTime * pixelsPerSecond);

        // --- BPM 表示・編集・ドラッグ ---
        foreach(var ev in currentChart.speeddata.changes)
        {
            float x = drawOriginX + (ev.triggerTime * pixelsPerSecond);
            if(x > -50 && x < editorRect.width)
            {
                string label = ev.type == "time" ? $"♩ = {currentChart.maindata.bpm * ev.value1:F0}" : "Formula";
                GUIStyle bpmStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
                Rect labelRect = new Rect(x, startY - 80, 100, 30);

                bool isHover = labelRect.Contains(Event.current.mousePosition);
                if (isHover) hoveredItem = ev;
                GUI.color = (hoveredItem == (object)ev) ? Color.yellow : Color.black;
                
                GUI.Label(labelRect, label, bpmStyle);
                
                // D&D 開始判定
                if (currentTool == EditorTool.Select && isHover)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        draggingSpeedEvent = ev;
                        isDraggingBPM = false; // まだドラッグ確定ではない
                        dragStartPos = Event.current.mousePosition;
                        Event.current.Use();
                    }
                }
            }
        }
        GUI.color = Color.white; 

        // --- 拍子 表示・編集 ---
        foreach(var ts in currentChart.timeSignatures)
        {
            float x = drawOriginX + (ts.time * pixelsPerSecond);
            if(x > -50 && x < editorRect.width)
            {
                GUIStyle tsStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                Rect tsRect = new Rect(x + 10, startY + staffSpacing * 1.5f, 40, 60);
                
                bool isHover = tsRect.Contains(Event.current.mousePosition);
                if (isHover) hoveredItem = ts;
                GUI.color = (hoveredItem == (object)ts) ? Color.yellow : Color.black;

                GUI.Label(tsRect, $"{ts.numerator}\n{ts.denominator}", tsStyle);
                
                if (currentTool == EditorTool.Select && isHover && GUI.Button(tsRect, "", GUIStyle.none))
                {
                    OpenTimeSigEditor(ts);
                }
            }
        }
        GUI.color = Color.white;

        // --- 譜面 ---
        for (int i = 1; i <= 4; i++)
        {
            float y = startY + (i - 1) * staffSpacing;
            GUI.color = Color.black;
            GUI.Label(new Rect(10, y - 10, 80, 20), $"Perc. {i}");
            GUI.Box(new Rect(90, y - 20, 10, 40), ""); 
            GUI.color = new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(0, y, editorRect.width, 2), Texture2D.whiteTexture);
        }

        // --- 小節線 ---
        DrawMeasureLines(startY, drawOriginX, staffSpacing);

        // --- ノーツ ---
        NoteData hitNote = null;
        bool hitEnd = false;

        GUI.color = Color.black;
        foreach (var laneKey in new[] { "L1", "L2", "L3", "L4" })
        {
            if (!currentChart.chartdata.ContainsKey(laneKey)) continue;
            int laneIdx = int.Parse(laneKey.Substring(1));
            float y = startY + (laneIdx - 1) * staffSpacing;

            foreach (var note in currentChart.chartdata[laneKey].Values)
            {
                float x = drawOriginX + (note.time * pixelsPerSecond);
                if (x < -50 || x > editorRect.width + 50) continue;

                Rect headRect = new Rect(x - 8, y - 8, 16, 16);
                
                bool isNoteHover = headRect.Contains(Event.current.mousePosition);
                
                GUI.color = (note.type == "long") ? Color.green : Color.black;
                if (isNoteHover) 
                {
                    hoveredItem = note;
                    GUI.color = Color.yellow; 
                }

                GUI.DrawTexture(headRect, noteHeadTexture);
                GUI.DrawTexture(new Rect(x + 7, y - 35, 2, 35), Texture2D.whiteTexture);

                if (note.type == "long")
                {
                    float endX = drawOriginX + (note.endtime * pixelsPerSecond);
                    GUI.color = new Color(0, 0.8f, 0, 0.5f);
                    GUI.DrawTexture(new Rect(x, y - 2, endX - x, 4), Texture2D.whiteTexture);
                    Rect endRect = new Rect(endX - 8, y - 8, 16, 16);
                    
                    bool isEndHover = endRect.Contains(Event.current.mousePosition);
                    GUI.color = Color.green;
                    if (isEndHover)
                    {
                        hoveredItem = note;
                        GUI.color = Color.yellow;
                    }

                    GUI.DrawTexture(endRect, noteHeadTexture);

                    if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                    {
                         if (isEndHover) { hitNote = note; hitEnd = true; }
                         else if (isNoteHover) { hitNote = note; hitEnd = false; }
                    }
                }
                else
                {
                     if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && isNoteHover)
                     {
                         hitNote = note; hitEnd = false;
                     }
                }
            }
        }
        GUI.color = Color.white;

        // 再生ヘッド
        GUI.color = new Color(0.2f, 0.4f, 1f, 0.8f);
        GUI.DrawTexture(new Rect(100f, startY - 20, 2, staffSpacing * 4), Texture2D.whiteTexture);
        GUI.color = Color.white;

        HandleInput(100f, 0, false, drawOriginX, startY, staffSpacing, hitNote, hitEnd);
    }

    void DrawMeasureLines(float startY, float drawOriginX, float staffSpacing)
    {
        float t = 0;
        int measureIndex = 1;
        float maxTime = Mathf.Max(currentTime + 10f, 300f); 

        int safeCounter = 0;
        while (t < maxTime && safeCounter < 5000)
        {
            safeCounter++;
            float currentBPM = GetBPMAt(t);
            TimeSignatureEvent currentTS = GetTimeSigAt(t);
            float beatDuration = (60f / currentBPM) * (4f / currentTS.denominator);
            float measureDuration = beatDuration * currentTS.numerator;

            float x = drawOriginX + (t * pixelsPerSecond);
            if (x > -50 && x < editorRect.width)
            {
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(x, startY, 2, staffSpacing * 3 + 2), Texture2D.whiteTexture); 
                GUI.Label(new Rect(x + 5, startY - 20, 30, 20), measureIndex.ToString());
            }

            t += measureDuration;
            measureIndex++;
        }
    }

    void DrawNormalMode()
    {
        float totalLanesWidth = 4 * laneSpacing;
        float startX = (editorRect.width - totalLanesWidth) / 2f;
        float drawOriginY = hitLineY + (currentTime * pixelsPerSecond);

        for (int i = 1; i <= 4; i++) {
            float laneX = startX + (i - 1) * laneSpacing;
            GUI.color = new Color(0,0,0,0.1f); GUI.DrawTexture(new Rect(laneX + noteWidth/2, 0, 1, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.red; GUI.DrawTexture(new Rect(laneX, hitLineY, noteWidth, 4), Texture2D.whiteTexture);
            
            string laneKey = "L" + i;
            if (currentChart.chartdata.ContainsKey(laneKey))
            {
                foreach (var note in currentChart.chartdata[laneKey].Values)
                {
                    float yPos = drawOriginY - (note.time * pixelsPerSecond);
                    if (yPos < -200 || yPos > Screen.height + 200) continue;
                    
                    Rect noteRect = new Rect(laneX, yPos - 15, noteWidth, 30);
                    if (note.type == "long") { 
                        float endY = drawOriginY - (note.endtime * pixelsPerSecond);
                         noteRect = new Rect(laneX, endY, noteWidth, yPos - endY);
                    }
                    GUI.color = note.type == "long" ? new Color(0.2f, 1f, 0.2f) : new Color(0f, 1f, 1f);
                    GUI.Box(noteRect, "");
                }
            }
            GUI.color = Color.white;
        }
        
        HandleInput(startX, totalLanesWidth, true, drawOriginY, 0, 0, null, false);
    }

    void HandleInput(float startPos, float totalSize, bool isVertical, float origin, float staffY, float spacing, NoteData hitNote, bool hitEnd)
    {
        Event e = Event.current;

        // --- BPM D&D (MouseDrag & MouseUp here, independent of element detection) ---
        if (draggingSpeedEvent != null)
        {
            if (e.type == EventType.MouseDrag)
            {
                if (!isDraggingBPM && Vector2.Distance(e.mousePosition, dragStartPos) > 5f)
                {
                    isDraggingBPM = true;
                }

                if (isDraggingBPM)
                {
                    float t = isVertical ? (origin - e.mousePosition.y) / pixelsPerSecond : (e.mousePosition.x - origin) / pixelsPerSecond;
                    float snappedTime = SnapToClosestMeasure(t);
                    if (snappedTime >= 0) draggingSpeedEvent.triggerTime = snappedTime;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (!isDraggingBPM)
                {
                    OpenSpeedEditor(draggingSpeedEvent);
                }
                draggingSpeedEvent = null;
                isDraggingBPM = false;
                e.Use();
            }
            // 重要: ドラッグ中は他のイベントを処理しない
            return; 
        }

        // コンテキストメニュー
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            contextNote = hitNote;
            if (isVertical) contextTime = (origin - e.mousePosition.y) / pixelsPerSecond;
            else contextTime = (e.mousePosition.x - origin) / pixelsPerSecond;
            
            showContextMenu = true;
            contextMenuPos = new Vector2(e.mousePosition.x + editorRect.x, e.mousePosition.y + editorRect.y);
            e.Use();
            return;
        }

        // 左クリック
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (showContextMenu) return; 

            // ツール処理
            if (currentTool == EditorTool.BPM)
            {
                float t = isVertical ? (origin - e.mousePosition.y) / pixelsPerSecond : (e.mousePosition.x - origin) / pixelsPerSecond;
                if(t >= 0) {
                    t = SnapToClosestMeasure(t);
                    CreateSpeedEventAt(t);
                }
                e.Use();
                return;
            }
            if (currentTool == EditorTool.TimeSig)
            {
                float t = isVertical ? (origin - e.mousePosition.y) / pixelsPerSecond : (e.mousePosition.x - origin) / pixelsPerSecond;
                if(t >= 0) {
                    t = SnapToClosestMeasure(t);
                    CreateTimeSigEventAt(t);
                }
                e.Use();
                return;
            }

            // ペン・選択処理
            if (hitNote != null)
            {
                if (currentTool == EditorTool.Select || currentTool == EditorTool.Pen)
                {
                    if (hitNote.type == "long" && hitEnd)
                    {
                        draggingNote = hitNote;
                        isDraggingEnd = true;
                        e.Use();
                    }
                }
            }
            else
            {
                if (currentTool == EditorTool.Pen)
                {
                    PlaceNote(e.mousePosition, startPos, isVertical, origin, staffY, spacing);
                }
            }
        }

        // ドラッグ (ノーツ)
        if (e.type == EventType.MouseDrag && draggingNote != null && isDraggingEnd)
        {
            float time = 0;
            if (isVertical) time = (origin - e.mousePosition.y) / pixelsPerSecond;
            else time = (e.mousePosition.x - origin) / pixelsPerSecond;
            
            float currentBPM = GetBPMAt(time);
            TimeSignatureEvent currentTS = GetTimeSigAt(time);
            float beatDuration = (60f / currentBPM) * (4f / currentTS.denominator);
            
            float step = beatDuration * (4f / (int)selectedNoteValue);
            time = Mathf.Round(time / step) * step;

            if (time > draggingNote.time) draggingNote.endtime = time;
            e.Use();
        }

        // ドラッグ終了
        if (e.type == EventType.MouseUp)
        {
            draggingNote = null;
            isDraggingEnd = false;
        }
    }

    void PlaceNote(Vector2 mousePos, float startPos, bool isVertical, float origin, float staffY, float spacing)
    {
        float t = isVertical ? (origin - mousePos.y) / pixelsPerSecond : (mousePos.x - origin) / pixelsPerSecond;
        if (t < 0) return;

        float currentBPM = GetBPMAt(t);
        TimeSignatureEvent currentTS = GetTimeSigAt(t);
        float beatDuration = (60f / currentBPM) * (4f / currentTS.denominator);
        float step = beatDuration * (4f / (int)selectedNoteValue);
        
        t = Mathf.Round(t / step) * step;

        if (isVertical)
        {
            for (int i = 1; i <= 4; i++)
             {
                 float laneX = startPos + (i - 1) * laneSpacing;
                 if (mousePos.x >= laneX && mousePos.x <= laneX + noteWidth) AddNote(i, t);
             }
        }
        else
        {
            for (int i = 1; i <= 4; i++)
            {
                float y = staffY + (i - 1) * spacing;
                if (Mathf.Abs(mousePos.y - y) < 25) AddNote(i, t);
            }
        }
    }

    void AddNote(int lane, float time)
    {
        string laneKey = "L" + lane;
        if (currentChart.chartdata[laneKey].Values.Any(n => Mathf.Abs(n.time - time) < 0.01f)) return;

        int newId = 1;
        if (currentChart.chartdata[laneKey].Count > 0) newId = currentChart.chartdata[laneKey].Keys.Select(k => int.Parse(k)).Max() + 1;

        NoteData note = new NoteData { aid = newId, time = time, type = currentNoteType, speed = 1.0f };
        if (currentNoteType == "long") {
             float currentBPM = GetBPMAt(time);
             TimeSignatureEvent currentTS = GetTimeSigAt(time);
             float beatDuration = (60f / currentBPM) * (4f / currentTS.denominator);
             note.endtime = time + (beatDuration * (4f / (int)selectedNoteValue));
        }
        currentChart.chartdata[laneKey][newId.ToString()] = note;
    }

    void DrawContextMenu()
    {
        float w = 150;
        float h = contextNote != null ? 80 : 80;
        Rect r = new Rect(contextMenuPos.x, contextMenuPos.y, w, h);
        
        GUI.backgroundColor = Color.gray;
        GUI.Box(new Rect(r.x + 2, r.y + 2, r.width, r.height), "");
        GUI.backgroundColor = Color.white;
        GUI.Box(r, "メニュー");
        
        GUILayout.BeginArea(r);
        GUILayout.Space(20);

        if (contextNote != null)
        {
            if (GUILayout.Button("削除"))
            {
                DeleteNote(contextNote);
                showContextMenu = false;
            }
        }
        else
        {
            if (GUILayout.Button("BPM 変更を追加"))
            {
                CreateSpeedEventAt(SnapToClosestMeasure(contextTime));
                showContextMenu = false;
            }
        }
        GUILayout.EndArea();
    }

    void DeleteNote(NoteData note)
    {
        foreach(var lane in currentChart.chartdata.Values)
        {
            var item = lane.FirstOrDefault(x => x.Value == note);
            if (item.Value != null)
            {
                lane.Remove(item.Key);
                return;
            }
        }
    }

    void CreateSpeedEventAt(float time)
    {
        SpeedChangeEvent ev = new SpeedChangeEvent();
        ev.type = "time";
        ev.triggerTime = time;
        ev.value1 = 1.0f; 
        currentChart.speeddata.changes.Add(ev);
        OpenSpeedEditor(ev);
    }

    void OpenSpeedEditor(SpeedChangeEvent ev)
    {
        editingSpeedEvent = ev;
        showSpeedEditor = true;
    }

    void CreateTimeSigEventAt(float time)
    {
        TimeSignatureEvent ts = new TimeSignatureEvent { time = time, numerator = 4, denominator = 4 };
        currentChart.timeSignatures.Add(ts);
        OpenTimeSigEditor(ts);
    }

    void OpenTimeSigEditor(TimeSignatureEvent ts)
    {
        editingTimeSigEvent = ts;
        showTimeSigEditor = true;
    }

    void DrawSpeedEditorWindow(int windowID)
    {
        if (editingSpeedEvent == null) { showSpeedEditor = false; return; }

        GUILayout.BeginHorizontal();
        GUILayout.Label("タイプ:", GUILayout.Width(50));
        if (GUILayout.Toggle(editingSpeedEvent.type == "time", "固定", "Button")) editingSpeedEvent.type = "time";
        if (GUILayout.Toggle(editingSpeedEvent.type == "formula", "曲線 (Bezier)", "Button")) editingSpeedEvent.type = "formula";
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"時間: {editingSpeedEvent.triggerTime:F2}");
        GUILayout.EndHorizontal();

        if (editingSpeedEvent.type == "time")
        {
            // BPM入力の改善
            float currentBPM = currentChart.maindata.bpm * editingSpeedEvent.value1;
            GUILayout.Label("テンポ (BPM):");
            string input = GUILayout.TextField(currentBPM.ToString("F0"));
            if (float.TryParse(input, out float newBPM))
            {
                if (currentChart.maindata.bpm > 0)
                    editingSpeedEvent.value1 = newBPM / currentChart.maindata.bpm;
            }
        }
        else 
        {
            GUILayout.Label("期間 (秒):");
            editingSpeedEvent.value1 = GUILayout.HorizontalSlider(editingSpeedEvent.value1, 0.1f, 10.0f);
            
            float targetBPM = currentChart.maindata.bpm * editingSpeedEvent.value2;
            GUILayout.Label("目標テンポ (BPM):");
            string input = GUILayout.TextField(targetBPM.ToString("F0"));
            if (float.TryParse(input, out float newBPM))
            {
                if (currentChart.maindata.bpm > 0)
                    editingSpeedEvent.value2 = newBPM / currentChart.maindata.bpm;
            }

            Rect graphRect = GUILayoutUtility.GetRect(300, 200);
            GUI.Box(graphRect, "");
            
            Vector2 start = new Vector2(graphRect.x, graphRect.y + graphRect.height);
            Vector2 end = new Vector2(graphRect.x + graphRect.width, graphRect.y);
            Vector2 cp1 = new Vector2(start.x + (editingSpeedEvent.x1 * graphRect.width), start.y - (editingSpeedEvent.y1 * graphRect.height));
            Vector2 cp2 = new Vector2(start.x + (editingSpeedEvent.x2 * graphRect.width), start.y - (editingSpeedEvent.y2 * graphRect.height));

#if UNITY_EDITOR
            Handles.DrawBezier(start, end, cp1, cp2, Color.cyan, null, 2f);
#endif

            GUI.color = Color.red;
            GUI.Box(new Rect(cp1.x - 5, cp1.y - 5, 10, 10), "");
            GUI.Box(new Rect(cp2.x - 5, cp2.y - 5, 10, 10), "");
            GUI.color = Color.white;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("X1:"); editingSpeedEvent.x1 = float.Parse(GUILayout.TextField(editingSpeedEvent.x1.ToString()));
            GUILayout.Label("Y1:"); editingSpeedEvent.y1 = float.Parse(GUILayout.TextField(editingSpeedEvent.y1.ToString()));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("X2:"); editingSpeedEvent.x2 = float.Parse(GUILayout.TextField(editingSpeedEvent.x2.ToString()));
            GUILayout.Label("Y2:"); editingSpeedEvent.y2 = float.Parse(GUILayout.TextField(editingSpeedEvent.y2.ToString()));
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("削除"))
        {
            currentChart.speeddata.changes.Remove(editingSpeedEvent);
            showSpeedEditor = false;
        }

        if (GUILayout.Button("閉じる")) showSpeedEditor = false;
        
        GUI.DragWindow();
    }

    void DrawTimeSigEditorWindow(int windowID)
    {
        if (editingTimeSigEvent == null) { showTimeSigEditor = false; return; }

        GUILayout.Label($"時間: {editingTimeSigEvent.time:F2}");
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("分子 (拍数):");
        editingTimeSigEvent.numerator = int.Parse(GUILayout.TextField(editingTimeSigEvent.numerator.ToString()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("分母 (基準音符):");
        editingTimeSigEvent.denominator = int.Parse(GUILayout.TextField(editingTimeSigEvent.denominator.ToString()));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("削除"))
        {
            currentChart.timeSignatures.Remove(editingTimeSigEvent);
            showTimeSigEditor = false;
        }

        if (GUILayout.Button("閉じる")) showTimeSigEditor = false;
        GUI.DragWindow();
    }
}
