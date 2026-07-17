using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.World
{
    public enum ScenePurpose
    {
        Bootstrap = 0,
        Prototype = 1,
        Test = 2
    }

    /// <summary>
    /// Identifies a project-owned scene and its serialized foundation version.
    /// It deliberately contains no Update loop or scene-loading behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneFoundationMarker : MonoBehaviour
    {
        [SerializeField] private string sceneId = "roe_prototype";
        [SerializeField] private string displayName = "Rules of Entry Prototype";
        [SerializeField] private ScenePurpose purpose = ScenePurpose.Prototype;
        [SerializeField, Min(1)] private int schemaVersion = ProjectInfo.FoundationSchemaVersion;

        public string SceneId => sceneId;
        public string DisplayName => displayName;
        public ScenePurpose Purpose => purpose;
        public int SchemaVersion => schemaVersion;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                sceneId = "roe_prototype";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "Rules of Entry Prototype";
            }

            schemaVersion = Mathf.Max(1, schemaVersion);
        }
    }
}
