using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Play Audio Effect", fileName = "PlayAudioEffect")]
[System.Serializable]
public class PlayAudioEffect : ScriptableObject
{
    public AudioSource audioSourcePrefab;
    public List<AudioClip> audioClips = new List<AudioClip>();
    public Vector2 volumnRange = new Vector2(0f, 1f);
}
