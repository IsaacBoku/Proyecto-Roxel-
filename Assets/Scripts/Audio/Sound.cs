using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string name;
    [Tooltip("Lista de clips para este sonido. Si hay más de uno, se elegirá uno aleatoriamente.")]
    public AudioClip[] clips;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
}
