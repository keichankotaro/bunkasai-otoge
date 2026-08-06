using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

[System.Serializable]
public class ChartData
{
    public MainData maindata = new MainData();
    public Dictionary<string, Dictionary<string, NoteData>> chartdata = new Dictionary<string, Dictionary<string, NoteData>>();
    public SpeedData speeddata = new SpeedData();
    public List<TimeSignatureEvent> timeSignatures = new List<TimeSignatureEvent>();

    public ChartData()
    {
        chartdata["L1"] = new Dictionary<string, NoteData>();
        chartdata["L2"] = new Dictionary<string, NoteData>();
        chartdata["L3"] = new Dictionary<string, NoteData>();
        chartdata["L4"] = new Dictionary<string, NoteData>();
        timeSignatures.Add(new TimeSignatureEvent { time = 0, numerator = 4, denominator = 4 });
    }
}

[System.Serializable]
public class TimeSignatureEvent
{
    public float time;
    public int numerator;
    public int denominator;
}

[System.Serializable]
public class MainData
{
    public string music = "audio.wav";
    public float bpm = 120;
    public float offset = 0;
    public string composer = "";
    public string level = "1";
    public string difficulty = "Hard";
}

[System.Serializable]
public class NoteData
{
    public int aid;
    public float time;
    public float speed;
    public string type = "tap";
    public float endtime; // For long notes
    public bool s_change = false;
    public List<SpeedChangeData> changes = new List<SpeedChangeData>();
}

[System.Serializable]
public class SpeedData
{
    public List<SpeedChangeEvent> changes = new List<SpeedChangeEvent>();
}

[System.Serializable]
public class SpeedChangeData 
{
    public float triggerTime;
    public float multiplier;
}

[System.Serializable]
public class SpeedChangeEvent 
{
    public string type; // "time" or "formula"
    public float triggerTime;
    public float value1; // multiplier or duration
    public float value2; // targetMultiplier (for formula)
    public float x1, y1, x2, y2;
}

public static class ChartFileManager
{
    public static ChartData Load(string jsonContent)
    {
        ChartData data = new ChartData();
        JObject root = JObject.Parse(jsonContent);

        // Load MainData
        if (root["maindata"] != null)
        {
            data.maindata.music = root["maindata"]["music"]?.ToString();
            data.maindata.bpm = (float?)root["maindata"]["bpm"] ?? 120;
            data.maindata.offset = (float?)root["maindata"]["offset"] ?? 0;
            data.maindata.composer = root["maindata"]["composer"]?.ToString();
            data.maindata.level = root["maindata"]["level"]?.ToString();
        }

        // Load ChartData
        if (root["chartdata"] != null)
        {
            foreach (var laneKey in new[] { "L1", "L2", "L3", "L4" })
            {
                if (root["chartdata"][laneKey] != null)
                {
                    JToken laneJson = root["chartdata"][laneKey];
                    foreach (JToken child in laneJson.Children())
                    {
                        if (child is JProperty prop && int.TryParse(prop.Name, out int id))
                        {
                            var n = prop.Value;
                            NoteData nd = new NoteData
                            {
                                aid = (int?)n["aid"] ?? id,
                                time = (float?)n["time"] ?? 0,
                                speed = (float?)n["speed"] ?? 1.0f,
                                type = n["type"]?.ToString() ?? "tap",
                                endtime = (float?)n["endtime"] ?? 0,
                                s_change = (bool?)n["s_change"] ?? false
                            };
                            data.chartdata[laneKey][id.ToString()] = nd;
                        }
                    }
                }
            }
        }
        
        // Load SpeedData
        if (root["speeddata"] != null && root["speeddata"]["changes"] is JArray changesArr)
        {
            foreach(var c in changesArr)
            {
                SpeedChangeEvent ev = new SpeedChangeEvent();
                if (c[0].Type == JTokenType.String && c[0].ToString() == "formula")
                {
                    ev.type = "formula";
                    ev.triggerTime = (float)c[1];
                    ev.value1 = (float)c[2]; 
                    ev.value2 = (float)c[3]; 
                    ev.x1 = (float)c[4]; ev.y1 = (float)c[5];
                    ev.x2 = (float)c[6]; ev.y2 = (float)c[7];
                }
                else
                {
                    ev.type = "time";
                    ev.triggerTime = (float)c[0];
                    ev.value1 = (float)c[1]; 
                }
                data.speeddata.changes.Add(ev);
            }
        }

        // Load TimeSignatures
        if (root["timesignatures"] != null && root["timesignatures"] is JArray tsArr)
        {
            data.timeSignatures.Clear();
            foreach(var t in tsArr)
            {
                data.timeSignatures.Add(new TimeSignatureEvent {
                    time = (float)t["time"],
                    numerator = (int)t["numerator"],
                    denominator = (int)t["denominator"]
                });
            }
        }
        if (data.timeSignatures.Count == 0) data.timeSignatures.Add(new TimeSignatureEvent { time = 0, numerator = 4, denominator = 4 });

        return data;
    }

    public static string Save(ChartData data)
    {
        JObject root = new JObject();

        // MainData
        JObject main = new JObject();
        main["music"] = data.maindata.music;
        main["bpm"] = data.maindata.bpm;
        main["offset"] = data.maindata.offset;
        main["composer"] = data.maindata.composer;
        main["level"] = data.maindata.level;
        root["maindata"] = main;

        // ChartData
        JObject chart = new JObject();
        foreach (var laneKey in new[] { "L1", "L2", "L3", "L4" })
        {
            JObject laneObj = new JObject();
            JObject meta = new JObject();
            meta["notes"] = data.chartdata[laneKey].Count;
            laneObj[laneKey + "data"] = meta;

            int index = 1;
            var sortedNotes = data.chartdata[laneKey].Values.OrderBy(n => n.time).ToList();
            
            foreach (var note in sortedNotes)
            {
                JObject n = new JObject();
                n["aid"] = note.aid; 
                n["speed"] = note.speed;
                n["time"] = note.time;
                n["type"] = note.type;
                if (note.type == "long") n["endtime"] = note.endtime;
                if (note.s_change)
                {
                    n["s_change"] = true;
                    n["changes"] = new JArray(); 
                }
                laneObj[index.ToString()] = n;
                index++;
            }
            chart[laneKey] = laneObj;
        }
        root["chartdata"] = chart;

        // SpeedData
        JObject speed = new JObject();
        JArray changes = new JArray();
        foreach(var ev in data.speeddata.changes)
        {
            JArray c = new JArray();
            if(ev.type == "formula")
            {
                c.Add("formula");
                c.Add(ev.triggerTime);
                c.Add(ev.value1);
                c.Add(ev.value2);
                c.Add(ev.x1); c.Add(ev.y1); c.Add(ev.x2); c.Add(ev.y2);
            }
            else
            {
                c.Add(ev.triggerTime);
                c.Add(ev.value1);
            }
            changes.Add(c);
        }
        speed["changes"] = changes;
        root["speeddata"] = speed;

        // TimeSignatures
        JArray tsArr = new JArray();
        foreach(var t in data.timeSignatures)
        {
            JObject to = new JObject();
            to["time"] = t.time;
            to["numerator"] = t.numerator;
            to["denominator"] = t.denominator;
            tsArr.Add(to);
        }
        root["timesignatures"] = tsArr;

        return root.ToString();
    }
}