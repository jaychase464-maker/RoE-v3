namespace RulesOfEntry.Core
{
    /// <summary>
    /// Stable project identity and repository paths shared by runtime and editor systems.
    /// </summary>
    public static class ProjectInfo
    {
        public const string GameTitle = "Rules of Entry";
        public const string StudioName = "Trooper Studios";
        public const string ProjectCode = "ROE";
        public const string ExpectedUnityVersion = "6000.5.2f1";
        public const string CurrentMilestone =
            "Milestone 5.5 / 6A / 6B / 6C / 7A / 7B / 7C / 7D - Front-End through Campaign Archive";
        public const int FoundationSchemaVersion = 1;

        public const string ProjectAssetRoot = "Assets/_Project/RulesOfEntry";
        public const string RuntimeAssetRoot = ProjectAssetRoot + "/Runtime";
        public const string OriginalTemplateScenePath = "Assets/OutdoorsScene.unity";
        public const string FrontEndScenePath =
            ProjectAssetRoot + "/Scenes/FrontEnd/ROE_FrontEnd.unity";
        public const string HeadquartersScenePath =
            ProjectAssetRoot + "/Scenes/Headquarters/ROE_Headquarters.unity";
        public const string PrototypeScenePath =
            ProjectAssetRoot + "/Scenes/Prototype/ROE_Prototype.unity";
    }
}
