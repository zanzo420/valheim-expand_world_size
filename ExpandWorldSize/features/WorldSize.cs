using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorldSize;

public class WorldSizeHelper
{
  public static IEnumerable<CodeInstruction> EdgeCheck(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10420f, () => Configuration.WorldTotalRadius - 80);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Ship), nameof(Ship.ApplyEdgeForce))]
public class ApplyEdgeForce
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);
}

[HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill))]
public class EdgeOfWorldKill
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);

  // Safer to simply skip when in dungeons.
  static bool Prefix(Player __instance) => __instance.transform.position.y < 4000f;
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestFactor))]
public class GetForestFactor
{
  static void Postfix(ref float __result)
  {
    var multiplier = Configuration.ForestMultiplier;
    if (multiplier == 0f)
      __result = 100f;
    else
      __result /= multiplier;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight))]
public class GetBaseHeight
{
  private static float Radius10000 = 10000f;
  private static float Radius10500 = 10500f;
  private static float Radius10490 = 10490f;
  public static void Refresh()
  {
    Radius10000 = Configuration.WorldRadius / Configuration.WorldStretch;
    Radius10500 = Configuration.WorldTotalRadius / Configuration.WorldStretch;
    Radius10490 = (Configuration.WorldTotalRadius - 10f) / Configuration.WorldStretch;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset0), (WorldGenerator instance) => instance.m_offset0);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset1), (WorldGenerator instance) => instance.m_offset1);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset0), (WorldGenerator instance) => Configuration.OffsetX ?? instance.m_offset0);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset1), (WorldGenerator instance) => Configuration.OffsetY ?? instance.m_offset1);
    matcher = Helper.Replace(matcher, 10000f, () => Radius10000);
    matcher = Helper.Replace(matcher, 10000f, () => Radius10000);
    matcher = Helper.Replace(matcher, 10500f, () => Radius10500);
    matcher = Helper.Replace(matcher, 10490f, () => Radius10490);
    matcher = Helper.Replace(matcher, 10500f, () => Radius10500);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.SetupMaterial))]
public class SetupMaterial
{
  public static void Refresh()
  {
    var objects = Object.FindObjectsOfType<WaterVolume>();
    foreach (var water in objects)
    {
      water.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
    }
  }
  public static void Prefix(WaterVolume __instance)
  {
    var obj = __instance;
    obj.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
  }
}

[HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
public class WaterLayerFix
{
  public static void Refresh(EnvMan obj)
  {
    var water = obj.transform.Find("WaterPlane")?.Find("watersurface");
    water?.GetComponent<MeshRenderer>()?.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
  }
  public static void Postfix(EnvMan __instance) => Refresh(__instance);
}

[HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateWind))]
public class UpdateWind
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldRadius);
    // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
    matcher = matcher
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldRadius);
    // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
    matcher = matcher
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.GetWaterSurface))]
public class GetWaterSurface
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius);
    return matcher.InstructionEnumeration();
  }
}