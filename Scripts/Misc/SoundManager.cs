using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    public AudioMixerGroup musicMixer;
    public AudioMixerGroup soundEffectMixer;

    private void Awake()
    {
        gameObject.name = "Sound Manager";
    }

    void Update()
    {
        
    }

    //Play Sound Effects
    public static void PlaySound3D(string name, Vector3 position, float volume, float pitch)
    {
        SoundManager soundManager = Get.SoundManager();
        //Find right audio clip
        AudioClip clip = null;
        foreach (Sound s in soundManager.sounds)
        {
            if (s.name == name)
            {
                clip = s.audio;
                break;
            }
        }
        if (clip == null)
        {
            Debug.LogError("Couldn't find a sound by the name of: " + name + ".");
            return;
        }

        GameObject newObj = new GameObject();
        GameObject sound = Instantiate(newObj, position, Quaternion.identity);
        Destroy(newObj);
        sound.name = name + " sound";
        sound.tag = "Sound";

        sound.AddComponent<AudioSource>();
        AudioSource audio = sound.GetComponent<AudioSource>();
        audio.volume = volume;
        audio.pitch = pitch;
        audio.clip = clip;
        audio.playOnAwake = false;
        audio.spatialBlend = 1;
        audio.outputAudioMixerGroup = soundManager.soundEffectMixer;
        audio.rolloffMode = AudioRolloffMode.Linear;
        audio.maxDistance = 20;
        audio.Play();

        sound.AddComponent<killObjectsAfterTime>();
        killObjectsAfterTime killScript = sound.GetComponent<killObjectsAfterTime>();
        killScript.lifetime = clip.length + 0.1f;
        killScript.killThisObject = true;
        killScript.unscaledTime = true;

        List<GameObject> gameObjects = new List<GameObject>();
        killScript.objectsToKill = gameObjects.ToArray();
    }
    public static void PlaySound2D(string name, Vector3 position, float volume, float pitch)
    {
        SoundManager soundManager = Get.SoundManager();

        if (GameObject.FindGameObjectsWithTag("Sound").Length < 20)
        {
            //Find right audio clip
            AudioClip clip = null;
            foreach (Sound s in soundManager.sounds)
            {
                if (s.name == name)
                {
                    clip = s.audio;
                    break;
                }
            }
            if (clip == null)
            {
                Debug.LogError("Couldn't find a sound by the name of: " + name + ".");
                return;
            }

            GameObject newObj = new GameObject();
            GameObject sound = Instantiate(newObj, position, Quaternion.identity);
            Destroy(newObj);
            sound.name = name + " sound";
            sound.tag = "Sound";

            sound.AddComponent<AudioSource>();
            AudioSource audio = sound.GetComponent<AudioSource>();
            audio.volume = volume;
            audio.pitch = pitch;
            audio.clip = clip;
            audio.playOnAwake = false;
            audio.outputAudioMixerGroup = soundManager.soundEffectMixer;
            audio.Play();

            sound.AddComponent<killObjectsAfterTime>();
            killObjectsAfterTime killScript = sound.GetComponent<killObjectsAfterTime>();
            killScript.lifetime = clip.length + 0.1f;
            killScript.killThisObject = true;
            killScript.unscaledTime = true;

            List<GameObject> gameObjects = new List<GameObject>();
            killScript.objectsToKill = gameObjects.ToArray();
        }
    }

    public void PlayUISound(string name)
    {
        SoundManager soundManager = Get.SoundManager();

        if (GameObject.FindGameObjectsWithTag("Sound").Length < 20)
        {
            //Find right audio clip
            AudioClip clip = null;
            foreach (Sound s in soundManager.sounds)
            {
                if (s.name == name)
                {
                    clip = s.audio;
                    break;
                }
            }
            if (clip == null)
            {
                Debug.LogError("Couldn't find a sound by the name of: " + name + ".");
                return;
            }

            GameObject newObj = new GameObject();
            GameObject sound = Instantiate(newObj, new Vector3(0, 0, 0), Quaternion.identity);
            Destroy(newObj);
            sound.name = name + " sound";
            sound.tag = "Sound";

            sound.AddComponent<AudioSource>();
            AudioSource audio = sound.GetComponent<AudioSource>();
            audio.volume = 1;
            audio.pitch = 1;
            audio.clip = clip;
            audio.playOnAwake = false;
            audio.outputAudioMixerGroup = soundManager.soundEffectMixer;
            audio.Play();

            sound.AddComponent<killObjectsAfterTime>();
            killObjectsAfterTime killScript = sound.GetComponent<killObjectsAfterTime>();
            killScript.lifetime = clip.length + 0.1f;
            killScript.killThisObject = true;
            killScript.unscaledTime = true;

            List<GameObject> gameObjects = new List<GameObject>();
            killScript.objectsToKill = gameObjects.ToArray();
        }
    }

    //Play Music
    public static void PlayMusic3D(string name, Vector3 position, float volume, float pitch, bool loop)
    {
        SoundManager soundManager = Get.SoundManager();
        
        //Find right audio clip
        AudioClip clip = null;
        foreach (Sound s in soundManager.sounds)
        {
            if (s.name == name)
            {
                clip = s.audio;
                break;
            }
        }
        if (clip == null)
        {
            Debug.LogError("Couldn't find a music track by the name of: " + name + ".");
            return;
        }

        GameObject newObj = new GameObject();
        GameObject sound = Instantiate(newObj, position, Quaternion.identity);
        Destroy(newObj);
        sound.name = name + " music";

        sound.AddComponent<AudioSource>();
        AudioSource audio = sound.GetComponent<AudioSource>();
        audio.volume = volume;
        audio.pitch = pitch;
        audio.clip = clip;
        audio.playOnAwake = false;
        audio.loop = loop;
        audio.spatialBlend = 1;
        audio.outputAudioMixerGroup = soundManager.musicMixer;
        audio.Play();

        if (loop)
            return;

        sound.AddComponent<killObjectsAfterTime>();
        killObjectsAfterTime killScript = sound.GetComponent<killObjectsAfterTime>();
        killScript.lifetime = clip.length + 0.1f;
        killScript.killThisObject = true;
        killScript.unscaledTime = true;

        List<GameObject> gameObjects = new List<GameObject>();
        killScript.objectsToKill = gameObjects.ToArray();
    }
    public static void PlayMusic2D(string name, Vector3 position, float volume, float pitch, bool loop)
    {
        SoundManager soundManager = Get.SoundManager();
        
        //Find right audio clip
        AudioClip clip = null;
        foreach (Sound s in soundManager.sounds)
        {
            if (s.name == name)
            {
                clip = s.audio;
                break;
            }
        }
        if (clip == null)
        {
            Debug.LogError("Couldn't find a music track by the name of: " + name + ".");
            return;
        }

        GameObject newObj = new GameObject();
        GameObject sound = Instantiate(newObj, position, Quaternion.identity);
        Destroy(newObj);
        sound.name = name + " music";

        sound.AddComponent<AudioSource>();
        AudioSource audio = sound.GetComponent<AudioSource>();
        audio.volume = volume;
        audio.pitch = pitch;
        audio.clip = clip;
        audio.playOnAwake = false;
        audio.loop = loop;
        audio.outputAudioMixerGroup = soundManager.musicMixer;
        audio.Play();

        if (loop)
            return;

        sound.AddComponent<killObjectsAfterTime>();
        killObjectsAfterTime killScript = sound.GetComponent<killObjectsAfterTime>();
        killScript.lifetime = clip.length + 0.1f;
        killScript.killThisObject = true;
        killScript.unscaledTime = true;

        List<GameObject> gameObjects = new List<GameObject>();
        killScript.objectsToKill = gameObjects.ToArray();
    }
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip audio;
}
