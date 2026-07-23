namespace OdinUndercroft.Patches;

public static class Functions
{
    /// <summary>
    /// Name of the custom EnvSetup this mod clones from "Crypt".
    ///
    /// WARNING: rolopogo's Basements mod registers an environment with this same name. Whichever
    /// mod's EnvMan.Awake postfix runs first wins; the other's tuning is silently discarded, and
    /// EnvMan.SetForceEnvironment resolves by name so a player in one mod's interior gets the
    /// other mod's environment.
    ///
    /// To make OdinsUndercroft fully independent, change this to something unique (e.g.
    /// "OU_Undercroft") AND update the SetForceEnvironment call in your own Basement component to
    /// match. Both must change together or the interior fix and the placement transpiler stop
    /// recognising the Undercroft.
    /// </summary>
    internal const string BasementEnvName = "Basement";

    /// <summary>
    /// Name of the vanilla environment we clone.
    /// </summary>
    internal const string SourceEnvName = "Crypt";

    internal static void RegisterAllSFX()
    {
        // Right now this is only one SFX, but it's here for future use to easily add more.
        PieceManager.PiecePrefabManager.RegisterPrefab("odins_undercroft", "SFX_UC_Destroyed");
    }

    /// <summary>
    /// Null-safe check for whether the current forced environment is our basement environment.
    /// EnvMan and its current environment can both be null during scene transitions, and the
    /// callers below run on hot paths, so every dereference here is guarded.
    /// </summary>
    internal static bool IsInBasementEnvironment()
    {
        EnvMan envMan = EnvMan.instance;
        if (envMan == null) return false;

        EnvSetup current = envMan.GetCurrentEnvironment();
        return current != null && current.m_name == BasementEnvName;
    }
}