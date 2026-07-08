using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine.Networking;

namespace HamburburConsole.Console;

[HarmonyPatch(typeof(VRRig))]
internal static class TelemetryManagement
{
    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPostfix]
    private static void OnGetRigCosmetics(VRRig __instance)
    {
        NetPlayer player = __instance.creator;

        if (__instance == null || player.GetPlayerRef() == PhotonNetwork.LocalPlayer ||
            HamburburData.Admins.ContainsKey(player.UserId))
            return;

        Dictionary<string, Dictionary<string, string>> data = new()
        {
                [player.UserId] = new Dictionary<string, string>
                {
                        {
                                "nickname",
                                CleanString(player.NickName, 12)
                        },
                        {
                                "cosmetics",
                                __instance._playerOwnedCosmetics.Concat()
                        },
                        {
                                "color",
                                $"{Math.Round(__instance.playerColor.r * 255)} {Math.Round(__instance.playerColor.g * 255)} {Math.Round(__instance.playerColor.b * 255)}"
                        },
                        {
                                "platform",
                                IsOnSteam(__instance) ? "STEAM" : "QUEST"
                        },
                },
        };

        Plugin.Instance.StartCoroutine(SendPlayerDataSync(
                data,
                PhotonNetwork.CurrentRoom.Name,
                PhotonNetwork.CloudRegion, NetworkSystem.Instance.GameModeString));
    }

    private static IEnumerator SendPlayerDataSync(Dictionary<string, Dictionary<string, string>> data, string directory,
                                                  string                                         region, string gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = CleanString(directory, 12, ['@', ]),
                region    = CleanString(region,    3),
                gameMode  = CleanString(gameMode,  128, [';', ]),
                data,
                playersCount = PhotonNetwork.PlayerList.Length,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new("https://hamburbur.org/syncdata", "POST");
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }

    private static string CleanString(string input, int maxLength, char[] ignoredChars = null)
    {
        input = new string(Array.FindAll(input.ToCharArray(), character =>
                                                                      Utils.IsASCIILetterOrDigit(character) ||
                                                                      ignoredChars != null && Array.IndexOf(ignoredChars, character) != -1));

        if (input.Length > maxLength)
        {
            input = input[..maxLength];
        }

        return input.ToUpper();
    }

    private static bool IsOnSteam(VRRig Player)
    {
        string concat           = Player._playerOwnedCosmetics.Concat();
        int    customPropsCount = Player.Creator.GetPlayerRef().CustomProperties.Count;

        return concat.Contains("S. FIRST LOGIN") || concat.Contains("FIRST LOGIN") || customPropsCount >= 2;
    }

    public static IEnumerator TelemetryRequest(string directory, string identity,    string region, string userid,
                                               bool   isPrivate, int    playerCount, string gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = CleanString(directory, 12, ['@', ]),
                identity  = CleanString(identity,12),
                region    = CleanString(region, 3),
                userid    = CleanString(userid, 20),
                isPrivate,
                playerCount,
                gameMode       = CleanString(gameMode, 128, [';', ]),
                consoleVersion = "NaN",
                menuName       = Constants.Name,
                menuVersion    = Constants.Version,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest hamburburRequest = new("https://hamburbur.org/telemetry", "POST");
        hamburburRequest.uploadHandler = new UploadHandlerRaw(raw);
        hamburburRequest.SetRequestHeader("Content-Type", "application/json");
        hamburburRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return hamburburRequest.SendWebRequest();

        UnityWebRequest seralythRequest = new("https://menu.seralyth.software/telemetry", "POST");
        seralythRequest.uploadHandler = new UploadHandlerRaw(raw);
        seralythRequest.SetRequestHeader("Content-Type", "application/json");
        seralythRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return seralythRequest.SendWebRequest();
    }
}