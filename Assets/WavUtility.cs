using System;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] data, string name)
    {
        // WAV�t�@�C���̃w�b�_�[����͂���AudioClip���쐬���鏈�����������܂�
        int channels = BitConverter.ToInt16(data, 22);
        int sampleRate = BitConverter.ToInt32(data, 24);
        int byteRate = BitConverter.ToInt32(data, 28);
        int blockAlign = BitConverter.ToInt16(data, 32);
        int bitsPerSample = BitConverter.ToInt16(data, 34);

        // �f�[�^�`�����N�̊J�n�ʒu��T��
        int dataChunkOffset = FindDataChunkOffset(data);
        if (dataChunkOffset == -1)
        {
            Debug.LogError("Data chunk not found in WAV file.");
            return null;
        }

        // �f�[�^�`�����N�̃T�C�Y���擾
        int dataSize = BitConverter.ToInt32(data, dataChunkOffset + 4);

        // �I�[�f�B�I�f�[�^�̃T���v�������v�Z
        int sampleCount = dataSize / (bitsPerSample / 8);

        // AudioClip���쐬���ăf�[�^��ݒ肵�܂�
        AudioClip audioClip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
        float[] floatData = ConvertByteToFloat(data, dataChunkOffset + 8, sampleCount, bitsPerSample);
        audioClip.SetData(floatData, 0);

        return audioClip;
    }

    private static int FindDataChunkOffset(byte[] data)
    {
        // "data"�`�����N�̊J�n�ʒu��T��
        for (int i = 12; i < data.Length - 8; i += 2)
        {
            if (data[i] == 'd' && data[i + 1] == 'a' && data[i + 2] == 't' && data[i + 3] == 'a')
            {
                return i;
            }
        }
        return -1;
    }

    private static float[] ConvertByteToFloat(byte[] data, int offset, int sampleCount, int bitsPerSample)
    {
        float[] floatData = new float[sampleCount];
        int byteIndex = offset;

        if (bitsPerSample == 16)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                floatData[i] = BitConverter.ToInt16(data, byteIndex) / 32768.0f;
                byteIndex += 2;
            }
        }
        else if (bitsPerSample == 24)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                int sample = (data[byteIndex + 2] << 16) | (data[byteIndex + 1] << 8) | data[byteIndex];
                if ((sample & 0x800000) != 0)
                {
                    sample |= unchecked((int)0xFF000000);
                }
                floatData[i] = sample / 8388608.0f;
                byteIndex += 3;
            }
        }
        else if (bitsPerSample == 32)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                floatData[i] = BitConverter.ToInt32(data, byteIndex) / 2147483648.0f;
                byteIndex += 4;
            }
        }
        else
        {
            throw new NotSupportedException("Unsupported bits per sample: " + bitsPerSample);
        }

        return floatData;
    }
}
