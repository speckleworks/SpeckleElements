using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleElementsClasses
{
  public enum CurveType
  {
    ModelCurve,
    DetailCurve,
    RoomBounding
  }

  // pruned from: https://docs.google.com/spreadsheets/d/1uNa77XYLjeN-1c63gsX6C5D5Pvn_3ZB4B0QMgPeloTw/edit#gid=1549586957
  public enum Category
  {
    Casework,
    Ceilings,
    Columns,
    CurtainPanels,
    CurtainSystems,
    CurtainWallMullions,
    Doors,
    Entourage,
    Fascias,
    Floors,
    Furniture,
    FurnitureSystems,
    GenericModels,
    Gutters,
    Landings,
    Mass,
    Railings,
    Ramps,
    Roads,
    RoofSoffits,
    Roofs,
    Runs,
    Site,
    SpecialtyEquipment,
    Stairs,
    StructuralAreaReinforcement,
    StructuralBeamSystems,
    StructuralColumns,
    StructuralConnections,
    StructuralFoundations,
    StructuralFraming,
    StructuralRebar,
    StructuralStiffeners,
    StructuralTrusses,
    Supports,
    Walls,
    Windows
  }

  public static class BuiltInCategories
  {
    public static string GetFromCategory(Category c)
    {
      return List[(int)c];
    }

    internal static List<string> List = new List<string>
  {
    "OST_Casework",
    "OST_Ceilings",
    "OST_Columns",
    "OST_CurtainWallPanels",
    "OST_CurtaSystem",
    "OST_CurtainWallMullions",
    "OST_Doors",
    "OST_Entourage",
    "OST_Fascia",
    "OST_Floors",
    "OST_Furniture",
    "OST_FurnitureSystems",
    "OST_GenericModel",
    "OST_Gutter",
    "OST_StairsLandings",
    "OST_Mass",
    "OST_StairsRailing",
    "OST_Ramps",
    "OST_Roads",
    "OST_RoofSoffit",
    "OST_Roofs",
    "OST_StairsRuns",
    "OST_Site",
    "OST_SpecialityEquipment",
    "OST_Stairs",
    "OST_AreaRein",
    "OST_StructuralFramingSystem",
    "OST_StructuralColumns",
    "OST_StructConnections",
    "OST_StructuralFoundation",
    "OST_StructuralFraming",
    "OST_Rebar",
    "OST_StructuralStiffener",
    "OST_StructuralTruss",
    "OST_StairsSupports",
    "OST_Walls",
    "OST_Windows"
  };
  }
}


