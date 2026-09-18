using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using EffExt;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TestIterator;

[BepInPlugin(GUID, Name, Version)]
sealed class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger;
    private static bool IsInit;
    public const string GUID = "hexahedron1.testiterator";
    public const string Name = "Test iterator";
    public const string Version = "0.2.31"; // TODO: version
    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += OnModsInit;
        Hooks.Apply();
    }

    public void OnDisable() {
        Hooks.Unapply();
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        Effects.Register();
        
        Logger.LogInfo(Version);
    }
}
