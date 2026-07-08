using BepInEx;
using HamburburConsole.Console;
using HarmonyLib;
using UnityEngine;

namespace HamburburConsole;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }

    private void Awake() => Instance = this;
    private void Start()
    {
        Harmony harmonyPatch = new(Constants.Guid);
        harmonyPatch.PatchAll();

        //HamburburData.cs calls Console.LoadConsole(); when admins have been loaded
        GameObject hamburburData = new("HamburburConsoleHamburburDataComponentHolder");
        hamburburData.AddComponent<HamburburData>();
    }
}