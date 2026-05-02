using HarmonyLib;
using MyceliumNetworking;
using UnityEngine;

namespace AlwaysPlayFinalDay.Patches;

[HarmonyPatch(typeof(SurfaceNetworkHandler))]
public class SurfaceNetworkHandlerPatch
{
    // this method is run on every client
    [HarmonyPatch(nameof(SurfaceNetworkHandler.InitSurface))]
    [HarmonyPrefix]
    private static void InitSurface_Prefix(SurfaceNetworkHandler __instance)
    {
        AlwaysPlayFinalDay.Instance.Debug_InitSurfaceActive = true;
    }

    [HarmonyPatch(nameof(SurfaceNetworkHandler.InitSurface))]
    [HarmonyPostfix]
    private static void InitSurface_Postfix(SurfaceNetworkHandler __instance)
    {
        // if quota not met on final day in InitSurface
        // spawn players and let them extract camera.
        if (MyceliumNetwork.IsHost
            && AlwaysPlayFinalDay.Instance.PlayFinalDayEvenIfQuotaNotMet
            && AlwaysPlayFinalDay.Instance.IsFinalDayAndQuotaNotMet())
        {
            Debug.Log($"[{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}] Spawn players, even on final day.");
            if (!Player.justDied)
            {
                SpawnHandler.Instance.SpawnLocalPlayer(Spawns.DiveBell);
                // 2.6.24 - hospital spawns are handled by SpawnHandler.Start(), so we don't have to handle it here
            }
        }

        AlwaysPlayFinalDay.Instance.Debug_InitSurfaceActive = false;
    }

    // 7.6.24: This method is interesting, because it is Called by every client's OnSlept, but also on the final day in the Evening if quota fails as you return from spelunking.
    // Dev note: this is the most fragile part of the code in this mod, as it is vulnerable to breaking if
    // the Content Warning devs change or update how the game handles when quota is not met
    // if quota failed as crew return to surface on final day
    // This method is called by host in InitSurface.
    // The game tries to jump straight to 'Sleeping', runs _NextDay (this method) and immediately game overs players (without letting them watch their last video)
    [HarmonyPatch(nameof(SurfaceNetworkHandler.NextDay))]
    [HarmonyPrefix]
    private static bool NextDay_Prefix(SurfaceNetworkHandler __instance)
    {
        // Do not run NextDay if quota not met on final day DURING InitSurface (as this ends the run immediately)
        if (MyceliumNetwork.IsHost
            && AlwaysPlayFinalDay.Instance.PlayFinalDayEvenIfQuotaNotMet
            && AlwaysPlayFinalDay.Instance.IsFinalDayAndQuotaNotMet()
            && AlwaysPlayFinalDay.Instance.Debug_InitSurfaceActive)
        {
            // early out - we do not want to skip to 'quota failed' in InitSurface before players get to watch their video.
            Debug.Log($"[{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}] Cancelled gameover, to allow players to watch their video on the final day when they didn't meet quota");
            return false; // skip original
        }

        return true; // run original
    }
}
