using HarmonyLib;
using System.Reflection;
using AlwaysPlayFinalDay.Patches;
using MyceliumNetworking;
using Zorro.Settings;
using UnityEngine;

namespace AlwaysPlayFinalDay;

// since this alters the gameplay experience by playing through the final day,
// I have set it to "not vanilla"
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
public class AlwaysPlayFinalDaySteam
{
    // this static constructor is used to init the Steam version of this mod.
    static AlwaysPlayFinalDaySteam()
    {
        // Static constructors of types marked with ContentWarningPluginAttribute are automatically invoked on load.
        // Register callbacks, construct stuff, etc. here.
        InitMe();
    }

    public static void InitMe()
    {
        Debug.Log($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

        var gameObject = new GameObject("KeepCameraAfterDeathPluginSteam")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        gameObject.AddComponent<AlwaysPlayFinalDay>();

        // Jan 2025 - make sure CW update doesnt destroy this mod
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }
}

public class AlwaysPlayFinalDay : MonoBehaviour // prev. BaseUnityPlugin
{
    // Actual mod logic

    const uint myceliumNetworkModId = 61813; // meaningless, as long as it is the same between all the clients
    public static AlwaysPlayFinalDay Instance { get; private set; } = null!;

    private Harmony? _harmony;

    public bool PlayFinalDayEvenIfQuotaNotMet { get; private set; }

    public bool Debug_InitSurfaceActive; // helper boolean


    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        MyceliumNetwork.RegisterNetworkObject(Instance, myceliumNetworkModId);
    }

    void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        MyceliumNetwork.DeregisterNetworkObject(Instance, myceliumNetworkModId);
    }

    public bool IsFinalDayAndQuotaNotMet()
    {
        return SurfaceNetworkHandler.RoomStats != null && SurfaceNetworkHandler.RoomStats.IsQuotaDay && !SurfaceNetworkHandler.RoomStats.CalculateIfReachedQuota();
    }

    public void SetPlayFinalDayEvenIfQuotaNotMet(bool playFinalDayEvenIfQuotaNotMet)
    {
        PlayFinalDayEvenIfQuotaNotMet = playFinalDayEvenIfQuotaNotMet;
    }

    [ContentWarningSetting]
    public class EnableAllowCrewToWatchFootageEvenIfQuotaNotMetSetting : BoolSetting, IExposedSetting
    {
        public SettingCategory GetSettingCategory() => SettingCategory.Mods;

        public override void ApplyValue()
        {
            AlwaysPlayFinalDay.Instance.SetPlayFinalDayEvenIfQuotaNotMet(Value);
        }
       
        public string GetDisplayName() => "AlwaysPlayFinalDay: Allow crew to view their camera footage on final day, even if the footage won't reach quota (uses the host's game settings) \nWithout this setting, the third day ends immediately.";

        protected override bool GetDefaultValue() => true;
    }
}
