using RulesOfEntry.Core;

namespace RulesOfEntry.Editor.Foundation
{
    internal static class RulesOfEntryFoundationPaths
    {
        internal const string InputActionAssetPath = "Assets/InputSystem_Actions.inputactions";
        internal const string SceneMarkerScriptPath =
            ProjectInfo.RuntimeAssetRoot + "/World/SceneFoundationMarker.cs";

        internal static readonly string[] RequiredFolders =
        {
            ProjectInfo.ProjectAssetRoot,
            ProjectInfo.ProjectAssetRoot + "/Art",
            ProjectInfo.ProjectAssetRoot + "/Audio",
            ProjectInfo.ProjectAssetRoot + "/Data",
            ProjectInfo.ProjectAssetRoot + "/Data/Actors",
            ProjectInfo.ProjectAssetRoot + "/Data/Equipment",
            ProjectInfo.ProjectAssetRoot + "/Data/Missions",
            ProjectInfo.ProjectAssetRoot + "/Data/RulesOfEngagement",
            ProjectInfo.ProjectAssetRoot + "/Editor",
            ProjectInfo.ProjectAssetRoot + "/Input",
            ProjectInfo.ProjectAssetRoot + "/Prefabs",
            ProjectInfo.ProjectAssetRoot + "/Prefabs/Actors",
            ProjectInfo.ProjectAssetRoot + "/Prefabs/Environment",
            ProjectInfo.ProjectAssetRoot + "/Prefabs/Interactions",
            ProjectInfo.ProjectAssetRoot + "/Prefabs/UI",
            ProjectInfo.ProjectAssetRoot + "/Runtime",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Actors",
            ProjectInfo.ProjectAssetRoot + "/Runtime/AI",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Combat",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Commands",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Core",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Input",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Interaction",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Missions",
            ProjectInfo.ProjectAssetRoot + "/Runtime/Player",
            ProjectInfo.ProjectAssetRoot + "/Runtime/RulesOfEngagement",
            ProjectInfo.ProjectAssetRoot + "/Runtime/UI",
            ProjectInfo.ProjectAssetRoot + "/Runtime/World",
            ProjectInfo.ProjectAssetRoot + "/Scenes",
            ProjectInfo.ProjectAssetRoot + "/Scenes/Bootstrap",
            ProjectInfo.ProjectAssetRoot + "/Scenes/Prototype",
            ProjectInfo.ProjectAssetRoot + "/Scenes/Tests",
            ProjectInfo.ProjectAssetRoot + "/Tests",
            ProjectInfo.ProjectAssetRoot + "/Tests/EditMode",
            ProjectInfo.ProjectAssetRoot + "/Tests/PlayMode"
        };

        internal static readonly string[] RequiredAssemblyDefinitions =
        {
            ProjectInfo.RuntimeAssetRoot + "/RulesOfEntry.Runtime.asmdef",
            ProjectInfo.ProjectAssetRoot + "/Editor/RulesOfEntry.Editor.asmdef",
            ProjectInfo.ProjectAssetRoot + "/Tests/EditMode/RulesOfEntry.Tests.EditMode.asmdef",
            ProjectInfo.ProjectAssetRoot + "/Tests/PlayMode/RulesOfEntry.Tests.PlayMode.asmdef"
        };
    }
}
