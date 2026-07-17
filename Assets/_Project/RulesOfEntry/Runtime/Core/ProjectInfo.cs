namespace RulesOfEntry.Core
{
    /// <summary>
    /// Stable project identity and repository paths shared by runtime and editor systems.
    /// </summary>
    public static class ProjectInfo
    {
        public const string GameTitle = "Rules of Entry";
        public const string ProjectCode = "ROE";
        public const string ExpectedUnityVersion = "6000.5.2f1";
        public const string CurrentMilestone =
            "Milestone 5 - Mission, ROE, and After-Action Review";
        public const int FoundationSchemaVersion = 1;

        public const string ProjectAssetRoot = "Assets/_Project/RulesOfEntry";
        public const string RuntimeAssetRoot = ProjectAssetRoot + "/Runtime";
        public const string OriginalTemplateScenePath = "Assets/OutdoorsScene.unity";
        public const string PrototypeScenePath =
            ProjectAssetRoot + "/Scenes/Prototype/ROE_Prototype.unity";
    }
}
