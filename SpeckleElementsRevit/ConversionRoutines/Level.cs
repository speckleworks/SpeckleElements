using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.Level ToNative( this SpeckleElements.Level myLevel )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myLevel.ApplicationId, myLevel.Type );

      // if the new and old have the same id (hash equivalent) and the doc obj is not marked as being modified, return the doc object
      if( stateObj != null && docObj != null && myLevel._id == stateObj._id && (bool) stateObj.Properties[ "userModified" ] == false )
        return (Autodesk.Revit.DB.Level) docObj;

      if( docObj == null )
        docObj = ExistingLevelByName( myLevel.levelName );
      if( docObj == null )
        docObj = ExistingLevelByElevation( myLevel.elevation );

      // If no doc object, means we need to create it!
      if( docObj == null )
      {
        var myNewLevel = Autodesk.Revit.DB.Level.Create( Doc, myLevel.elevation * Scale );
        if( myLevel.levelName != null )
          try
          {
            myNewLevel.Name = myLevel.levelName;
          }
          catch { }

        if( myLevel.createView )
        {
          var vt = new FilteredElementCollector( Doc ).OfClass( typeof( ViewFamilyType ) ).Where( el => ((ViewFamilyType) el).ViewFamily == ViewFamily.FloorPlan ).First();

          var view = Autodesk.Revit.DB.ViewPlan.Create( Doc, vt.Id, myNewLevel.Id );
          try
          {
            view.Name = myLevel.levelName;
          }
          catch { }
        }

        SetElementParams( myNewLevel, myLevel.parameters );
        myNewLevel.Maximize3DExtents();
        return myNewLevel;
      }

      var existingLevel = docObj as Autodesk.Revit.DB.Level;
      existingLevel.Elevation = myLevel.elevation * Scale;
      existingLevel.Name = myLevel.levelName != null ? myLevel.levelName : "Level @ " + Math.Round( myLevel.elevation, 2 );

      SetElementParams( existingLevel, myLevel.parameters );
      existingLevel.Maximize3DExtents();
      return existingLevel;
    }

    public static SpeckleElements.Level ToSpeckle( this Autodesk.Revit.DB.Level myLevel )
    {
      // TODO
      var speckleLevel = new SpeckleElements.Level();
      speckleLevel.elevation = myLevel.Elevation / Scale; // UnitUtils.ConvertFromInternalUnits(myLevel.Elevation, DisplayUnitType.Meters)
      speckleLevel.levelName = myLevel.Name;
      speckleLevel.parameters = GetElementParams( myLevel );

      speckleLevel.ApplicationId = myLevel.UniqueId;
      speckleLevel.GenerateHash();
      return speckleLevel;
    }

    private static Autodesk.Revit.DB.Level ExistingLevelByName( string name )
    {
      var collector = new FilteredElementCollector( Doc ).OfClass( typeof( Autodesk.Revit.DB.Level ) ).ToElements();
      foreach( var l in collector )
      {
        if( ((Autodesk.Revit.DB.Level) l).Name == name )
          return (Autodesk.Revit.DB.Level) l;
      }

      return null;
    }

    private static Autodesk.Revit.DB.Level ExistingLevelByElevation( double elevation )
    {
      var collector = new FilteredElementCollector( Doc ).OfClass( typeof( Autodesk.Revit.DB.Level ) ).ToElements();
      foreach( var l in collector )
      {
        if( Math.Abs( ((Autodesk.Revit.DB.Level) l).Elevation - elevation * Scale ) < 0.1 )
          return (Autodesk.Revit.DB.Level) l;
      }

      return null;
    }
  }

}
