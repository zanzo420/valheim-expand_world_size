using System.Collections.Generic;
using HarmonyLib;

namespace ExpandWorldSize;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
    new DebugCommands();
  }
}

public class DebugCommands
{
  public DebugCommands()
  {
    new Terminal.ConsoleCommand("ew_seeds", "- Prints different seeds.", args =>
    {
      var wg = WorldGenerator.m_instance;
      if (wg == null) return;
      List<string> lines = new() {
        "Main: " + wg.m_world.m_seedName,
        "Generator: " + wg.m_world.m_worldGenVersion,
        "World: " + wg.m_world.m_seed,
        "Offset X: " + wg.m_offset0,
        "Offset Y: " + wg.m_offset1,
        "Height: " + wg.m_offset3,
        "Meadows: " + wg.m_offset3,
        "Black forest: " + wg.m_offset2,
        "Swamp: "+ wg.m_offset0,
        "Plains: "+ wg.m_offset1,
        "Mistlands: " + wg.m_offset4
      };
      ZLog.Log(string.Join("\n", lines));
      args.Context.AddString(string.Join("\n", lines));
    }, true);
  }
}