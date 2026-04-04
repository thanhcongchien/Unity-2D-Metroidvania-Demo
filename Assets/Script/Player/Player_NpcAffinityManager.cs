using System.Collections.Generic;
using UnityEngine;

public class Player_NpcAffinityManager : MonoBehaviour, ISaveable
{
    [Header("Affinity Range")]
    [SerializeField] private int minAffinity = -100;
    [SerializeField] private int maxAffinity = 100;
    [SerializeField] private int defaultAffinity = 0;

    private readonly Dictionary<string, int> npcAffinities = new Dictionary<string, int>();

    public int MinAffinity => minAffinity;
    public int MaxAffinity => maxAffinity;
    public int DefaultAffinity => defaultAffinity;

    public int GetAffinity(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return defaultAffinity;

        if (npcAffinities.TryGetValue(npcId, out int affinity))
            return affinity;

        return defaultAffinity;
    }

    public void AddAffinity(string npcId, int amount)
    {
        if (amount == 0 || string.IsNullOrWhiteSpace(npcId))
            return;

        int currentAffinity = GetAffinity(npcId);
        SetAffinity(npcId, currentAffinity + amount);
    }

    public void SetAffinity(string npcId, int value)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return;

        npcAffinities[npcId] = ClampAffinity(value);
    }

    public bool IsWithinRange(string npcId, int minInclusive, int maxInclusive)
    {
        int value = GetAffinity(npcId);
        return value >= minInclusive && value <= maxInclusive;
    }

    public float GetNormalizedAffinity(string npcId)
    {
        if (minAffinity == maxAffinity)
            return 1f;

        return Mathf.InverseLerp(minAffinity, maxAffinity, GetAffinity(npcId));
    }

    private int ClampAffinity(int value)
    {
        return Mathf.Clamp(value, minAffinity, maxAffinity);
    }

    public void LoadData(GameData data)
    {
        npcAffinities.Clear();

        if (data.npcAffinities == null)
            data.npcAffinities = new SerializableDictionary<string, int>();

        foreach (var pair in data.npcAffinities)
        {
            npcAffinities[pair.Key] = ClampAffinity(pair.Value);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.npcAffinities == null)
            data.npcAffinities = new SerializableDictionary<string, int>();

        data.npcAffinities.Clear();

        foreach (var pair in npcAffinities)
        {
            data.npcAffinities[pair.Key] = pair.Value;
        }
    }
}
