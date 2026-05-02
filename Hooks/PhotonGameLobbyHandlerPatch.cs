using HarmonyLib;
using MyceliumNetworking;

namespace AlwaysPlayFinalDay.Patches;

[HarmonyPatch(typeof(PhotonGameLobbyHandler))]
public class PhotonGameLobbyHandlerPatch
{
    // this method is run on every client, but only the host should be able to do things with it
    [HarmonyPatch(nameof(PhotonGameLobbyHandler.SetCurrentObjective))]
    [HarmonyPrefix]
    private static void SetCurrentObjective_Prefix(PhotonGameLobbyHandler __instance, ref Objective objective)
    {
        if (MyceliumNetwork.IsHost
            && AlwaysPlayFinalDay.Instance.PlayFinalDayEvenIfQuotaNotMet
            && AlwaysPlayFinalDay.Instance.IsFinalDayAndQuotaNotMet())
        {
            // intercept when returning from InitSurface, set objective to Extract video
            if (AlwaysPlayFinalDay.Instance.Debug_InitSurfaceActive)
            {
                objective = new ExtractVideoObjective();
            }
            // intercept when finished watching TV, inform crew they failed quota
            // the text seems almost identical, so this is just cosmetic future-proofing
            if (objective is GoToBedSuccessObjective)
            {
                objective = new GoToBedFailedObjective();
            }
        }
    }
}
