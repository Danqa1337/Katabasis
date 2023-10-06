using UnityEngine;

public class GenerationPreset : MonoBehaviour
{
    [SerializeField] private GenerationPresetName _generationPresetName;
    private GenerationModule[] _modules;

    public GenerationPresetName GenerationPresetName { get => _generationPresetName; set => _generationPresetName = value; }

    public void Generate(GenerationData generationData)
    {
        _modules = GetComponentsInChildren<GenerationModule>();
        foreach (var item in _modules)
        {
            if (item.enabled)
            {
                item.Generate(generationData);
            }
        }
    }
}