using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Waves Config")]
public class WavesConfig : ScriptableObject
{
    [SerializeField] private List<Wave> _waves;
    [SerializeField] private float _timeBetweenWaves;
    [SerializeField] private int _startingWaveIndex = 0;

    public IEnumerable<Wave> GetWaves() => _waves;
    public float GetTimeBetweenWaves() => _timeBetweenWaves;
    public int GetStartingWaveIndex() => _startingWaveIndex;

    private void OnValidate()
    {
        if (_startingWaveIndex > _waves.Count)
        {
            _startingWaveIndex = _waves.Count;
        } else if (_startingWaveIndex < 0)
        {
            _startingWaveIndex = 0;
        }
        
        if (_timeBetweenWaves < 0)
        {
            _timeBetweenWaves = 0;
        }
    }
}
