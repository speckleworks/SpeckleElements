using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleElements;
using Interop.Gsa_10_0;
using SQLite;

namespace SpeckleElementsGSA
{
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser() { }

    public static GSAInterfacer GSA { get; set; } = new GSAInterfacer();

    public static Dictionary<Type, List<object>> GSASenderObjects { get; set; }

    public static string GSAUnits { get; set; }

    public static double GSACoincidentNodeAllowance { get; set; }

    public static GSATargetLayer GSATargetLayer { get; set; }

    public static bool GSATargetDesignLayer { set => GSATargetLayer = value ? GSATargetLayer.Design : GSATargetLayer.Analysis; }

    public static bool GSATargetAnalysisLayer { set => GSATargetLayer = value ? GSATargetLayer.Analysis : GSATargetLayer.Design; }

    public static bool GSASendResults { get; set; }

    public static List<string> GSAResultCases { get; set; }

    public static bool GSAResultInLocalAxis { get; set; }
  }

  public static partial class Conversions
  {
    public static Dictionary<Type, List<object>> GSASenderObjects { get => Initialiser.GSASenderObjects; }

    public static GSAInterfacer GSA { get => Initialiser.GSA; }

    public static string GSAUnits { get => Initialiser.GSAUnits; }

    public static double GSACoincidentNodeAllowance { get => Initialiser.GSACoincidentNodeAllowance; }

    public static GSATargetLayer GSATargetLayer { get => Initialiser.GSATargetLayer; }

    public static bool GSASendResults { get => Initialiser.GSASendResults; }

    public static List<string> GSAResultCases { get => Initialiser.GSAResultCases; }

    public static bool GSAResultInLocalAxis { get => Initialiser.GSAResultInLocalAxis; }
  }
}
