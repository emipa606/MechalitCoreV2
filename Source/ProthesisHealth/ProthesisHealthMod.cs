using System.Reflection;
using HarmonyLib;
using Verse;

namespace ProthesisHealth;

public class ProthesisHealthMod : Mod
{
    public ProthesisHealthMod(ModContentPack content)
        : base(content)
    {
        new Harmony("net.paragonteam.prothesishealth").PatchAll(Assembly.GetExecutingAssembly());
    }
}