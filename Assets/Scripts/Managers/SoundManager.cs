using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    float _bgmVolume = 0.5f;
    float _effectVolume = 0.5f;

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            // bgm, effect
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
                if ((Define.Sound)i == Define.Sound.Bgm)
                    _audioSources[i].volume = _bgmVolume;
                else if ((Define.Sound)i == Define.Sound.Effect)
                    _audioSources[i].volume = _effectVolume;
            }

            //BGM은 루프로 재생
            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    // Define.Sound type을 지정하지 않으면 effect가 기본
    public void Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAdudioClip(path, type);
       
        Play(audioClip, type, pitch);
    }

    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == Define.Sound.Bgm)
        {           
            AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }


    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    //오디오 클립을 캐싱하는 함수
    AudioClip GetOrAddAdudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
        //Sounds/찾고자 하는 sound
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";

        AudioClip audioClip = null;

        if (type == Define.Sound.Bgm)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing ! {path}");

        return audioClip;
    }

    public float GetVolume(Define.Sound type)
    {
        return _audioSources[(int)type]?.volume ?? 1f;
    }

    public void SetVolume(Define.Sound type, float volume)
    {
        if (type == Define.Sound.Bgm)
        {
            _bgmVolume = volume;
            _audioSources[(int)Define.Sound.Bgm].volume = _bgmVolume;
        }
        else if (type == Define.Sound.Effect)
        {
            _effectVolume = volume;
            _audioSources[(int)Define.Sound.Effect].volume = _effectVolume;
        }
    }
}
