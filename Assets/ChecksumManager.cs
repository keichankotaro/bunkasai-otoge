using System.Collections.Generic;
using System.IO;
using UnityEngine;
// Json.NET���C�u�������K�v�ł��B
// Unity��PackageManager���� "Newtonsoft Json" ���C���X�g�[�����Ă��������B
using Newtonsoft.Json;

/// <summary>
/// ���[�J���ɕۑ����ꂽ�t�@�C���̃`�F�b�N�T�����Ǘ�����ÓI�N���X�B
/// </summary>
public static class ChecksumManager
{
    private const string JacketChecksumFile = "jacket_checksums.json";
    private const string AudioChecksumFile = "audio_checksums.json";

    private static Dictionary<string, string> _jacketChecksums;
    private static Dictionary<string, string> _audioChecksums;

    /// <summary>
    /// �A�v���P�[�V�����N�����Ɉ�x�����Ăяo���AJSON�t�@�C������`�F�b�N�T����ǂݍ��݂܂��B
    /// </summary>
    public static void Initialize()
    {
        _jacketChecksums = LoadFromFile(JacketChecksumFile);
        _audioChecksums = LoadFromFile(AudioChecksumFile);
        Debug.Log($"[ChecksumManager] Initialized. Loaded {_jacketChecksums.Count} jacket and {_audioChecksums.Count} audio checksums from local files.");
    }

    /// <summary>
    /// �A�v���P�[�V�����I�����ɌĂяo���A��������̃`�F�b�N�T����JSON�t�@�C���ɕۑ����܂��B
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
    /// �w�肳�ꂽ�y�Ȃ̃W���P�b�g�̃��[�J���`�F�b�N�T�����擾���܂��B
    /// </summary>
    /// <param name="musicName">�y�Ȗ�</param>
    /// <returns>�`�F�b�N�T��������B���݂��Ȃ��ꍇ��null�B</returns>
    public static string GetJacketChecksum(string musicName)
    {
        if (_jacketChecksums == null) return null;
        _jacketChecksums.TryGetValue(musicName, out string checksum);
        return checksum;
    }

    /// <summary>
    /// �w�肳�ꂽ�y�Ȃ̃v���r���[�����̃��[�J���`�F�b�N�T�����擾���܂��B
    /// </summary>
    /// <param name="musicName">�y�Ȗ�</param>
    /// <returns>�`�F�b�N�T��������B���݂��Ȃ��ꍇ��null�B</returns>
    public static string GetAudioChecksum(string musicName)
    {
        if (_audioChecksums == null) return null;
        _audioChecksums.TryGetValue(musicName, out string checksum);
        return checksum;
    }

    /// <summary>
    /// �w�肳�ꂽ�y�Ȃ̃W���P�b�g�̃`�F�b�N�T�����X�V���܂��B
    /// </summary>
    /// <param name="musicName">�y�Ȗ�</param>
    /// <param name="newChecksum">�V�����`�F�b�N�T��</param>
    public static void UpdateJacketChecksum(string musicName, string newChecksum)
    {
        if (_jacketChecksums == null) return;
        _jacketChecksums[musicName] = newChecksum;
    }

    /// <summary>
    /// �w�肳�ꂽ�y�Ȃ̃v���r���[�����̃`�F�b�N�T�����X�V���܂��B
    /// </summary>
    /// <param name="musicName">�y�Ȗ�</param>
    /// <param name="newChecksum">�V�����`�F�b�N�T��</param>
    public static void UpdateAudioChecksum(string musicName, string newChecksum)
    {
        if (_audioChecksums == null) return;
        _audioChecksums[musicName] = newChecksum;
    }

    /// <summary>
    /// �w�肳�ꂽ�t�@�C���p�X����`�F�b�N�T���̎�����ǂݍ��݂܂��B
    /// </summary>
    private static Dictionary<string, string> LoadFromFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                // �t�@�C������̏ꍇ�ł�null��Ԃ��Ȃ��悤�ɂ���
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChecksumManager] Failed to load or parse {fileName}: {e.Message}. A new dictionary will be created.");
                return new Dictionary<string, string>();
            }
        }
        // �t�@�C�������݂��Ȃ��ꍇ�͐V����������Ԃ�
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// �`�F�b�N�T���̎������w�肳�ꂽ�t�@�C���p�X��JSON�`���ŕۑ����܂��B
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
