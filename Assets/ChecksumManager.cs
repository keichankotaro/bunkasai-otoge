using System.Collections.Generic;
using System.IO;
using UnityEngine;
// Json.NETライブラリが必要です。
// UnityのPackageManagerから "Newtonsoft Json" をインストールしてください。
using Newtonsoft.Json;

/// <summary>
/// ローカルに保存されたファイルのチェックサムを管理する静的クラス。
/// </summary>
public static class ChecksumManager
{
    private const string JacketChecksumFile = "jacket_checksums.json";
    private const string AudioChecksumFile = "audio_checksums.json";

    private static Dictionary<string, string> _jacketChecksums;
    private static Dictionary<string, string> _audioChecksums;

    /// <summary>
    /// アプリケーション起動時に一度だけ呼び出し、JSONファイルからチェックサムを読み込みます。
    /// </summary>
    public static void Initialize()
    {
        _jacketChecksums = LoadFromFile(JacketChecksumFile);
        _audioChecksums = LoadFromFile(AudioChecksumFile);
        Debug.Log($"[ChecksumManager] Initialized. Loaded {_jacketChecksums.Count} jacket and {_audioChecksums.Count} audio checksums from local files.");
    }

    /// <summary>
    /// アプリケーション終了時に呼び出し、メモリ上のチェックサムをJSONファイルに保存します。
    /// </summary>
    public static void SaveAll()
    {
        if (_jacketChecksums != null)
        {
            SaveToFile(JacketChecksumFile, _jacketChecksums);
        }
        if (_audioChecksums != null)
        {
            SaveToFile(AudioChecksumFile, _audioChecksums);
        }
        Debug.Log("[ChecksumManager] All checksums have been saved to local files.");
    }

    /// <summary>
    /// 指定された楽曲のジャケットのローカルチェックサムを取得します。
    /// </summary>
    /// <param name="musicName">楽曲名</param>
    /// <returns>チェックサム文字列。存在しない場合はnull。</returns>
    public static string GetJacketChecksum(string musicName)
    {
        if (_jacketChecksums == null) return null;
        _jacketChecksums.TryGetValue(musicName, out string checksum);
        return checksum;
    }

    /// <summary>
    /// 指定された楽曲のプレビュー音声のローカルチェックサムを取得します。
    /// </summary>
    /// <param name="musicName">楽曲名</param>
    /// <returns>チェックサム文字列。存在しない場合はnull。</returns>
    public static string GetAudioChecksum(string musicName)
    {
        if (_audioChecksums == null) return null;
        _audioChecksums.TryGetValue(musicName, out string checksum);
        return checksum;
    }

    /// <summary>
    /// 指定された楽曲のジャケットのチェックサムを更新します。
    /// </summary>
    /// <param name="musicName">楽曲名</param>
    /// <param name="newChecksum">新しいチェックサム</param>
    public static void UpdateJacketChecksum(string musicName, string newChecksum)
    {
        if (_jacketChecksums == null) return;
        _jacketChecksums[musicName] = newChecksum;
    }

    /// <summary>
    /// 指定された楽曲のプレビュー音声のチェックサムを更新します。
    /// </summary>
    /// <param name="musicName">楽曲名</param>
    /// <param name="newChecksum">新しいチェックサム</param>
    public static void UpdateAudioChecksum(string musicName, string newChecksum)
    {
        if (_audioChecksums == null) return;
        _audioChecksums[musicName] = newChecksum;
    }

    /// <summary>
    /// 指定されたファイルパスからチェックサムの辞書を読み込みます。
    /// </summary>
    private static Dictionary<string, string> LoadFromFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                // ファイルが空の場合でもnullを返さないようにする
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChecksumManager] Failed to load or parse {fileName}: {e.Message}. A new dictionary will be created.");
                return new Dictionary<string, string>();
            }
        }
        // ファイルが存在しない場合は新しい辞書を返す
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// チェックサムの辞書を指定されたファイルパスにJSON形式で保存します。
    /// </summary>
    private static void SaveToFile(string fileName, Dictionary<string, string> checksums)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            string json = JsonConvert.SerializeObject(checksums, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ChecksumManager] Failed to save {fileName}: {e.Message}");
        }
    }
}
