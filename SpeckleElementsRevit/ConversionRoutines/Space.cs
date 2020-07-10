using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElementsClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Text;

namespace SpeckleElementsRevit
{

  public static partial class Conversions //: IConversions<SpeckleElementsClasses.Space, Autodesk.Revit.DB.Mechanical.Space>//: Inheritance from base type? Singleton & converter instance rather than static?
  {

    public static Space ToSpeckle(this Autodesk.Revit.DB.Mechanical.Space mySpace)
    {
      var speckleSpace = new SpeckleElementsClasses.Space();

      //Name & number
      speckleSpace.spaceName    =         mySpace.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NAME).AsString();
      speckleSpace.spaceNumber  =         mySpace.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NUMBER).AsString();
      
      //Location
      var locPt = ((Autodesk.Revit.DB.LocationPoint)mySpace.Location).Point;
      speckleSpace.spaceLocation = new SpecklePoint(locPt.X / Scale, locPt.Y / Scale, locPt.Z / Scale);
      speckleSpace.levelElementId = mySpace.LevelId.IntegerValue.ToString();
      speckleSpace.phaseElementId = mySpace.GetParameters("Phase Id").FirstOrDefault().AsElementId().ToString();
      
      //3d geometry
      (speckleSpace.Faces, speckleSpace.Vertices) = GetFaceVertexArrayFromElement(mySpace);

      //2d boundary curve
      var seg = mySpace.GetBoundarySegments(new Autodesk.Revit.DB.SpatialElementBoundaryOptions());
      var myPolyCurve = new SpecklePolycurve() { Segments = new List<SpeckleCore.SpeckleObject>() };
      foreach (BoundarySegment segment in seg[0])
      {
        var crv = segment.GetCurve();
        var converted = SpeckleCore.Converter.Serialise(crv);
        myPolyCurve.Segments.Add(converted as SpeckleObject);
      }
      speckleSpace.baseCurve = myPolyCurve;

      //parameters
      speckleSpace.parameters = GetElementParams(mySpace);

      //try get type parameters
      if (mySpace.IsValidType(mySpace.GetTypeId()))
        speckleSpace.typeParameters = GetElementTypeParams(mySpace);

      //global parameters
      speckleSpace.ApplicationId = mySpace.UniqueId;
      speckleSpace.elementId = mySpace.Id.ToString();
      speckleSpace.GenerateHash();

      return speckleSpace;
    }

    public static Autodesk.Revit.DB.Mechanical.Space ToNative(this SpeckleElementsClasses.Space mySpace)
    {
      // Creation of space boundary, not necessary
      //CurveArray curveArr = new CurveArray();
      //SpecklePolycurve myPolyCurve = (SpecklePolycurve)mySpace.baseCurve;
      //foreach (SpeckleObject segment in myPolyCurve.Segments)
      //{
      //  var converted = SpeckleCore.Converter.Deserialise(segment);
      //  curveArr.Append(converted as Autodesk.Revit.DB.Curve);
      //}
      //ModelCurveArray crArr = Doc.Create.NewSpaceBoundaryLines(Doc.ActiveView.SketchPlane, curveArr, Doc.ActiveView);

      // Get element level
      ElementId elemLevelId = new ElementId(Int32.Parse(mySpace.levelElementId));
      Autodesk.Revit.DB.Level level = Doc.GetElement(elemLevelId) as Autodesk.Revit.DB.Level;

      // Get element phase
      ElementId elemPhaseId = new ElementId(Int32.Parse(mySpace.phaseElementId));
      Phase phase = Doc.GetElement(elemPhaseId) as Phase;

      //Get element location point
      Autodesk.Revit.DB.UV locPoint = new Autodesk.Revit.DB.UV(mySpace.spaceLocation.Value[0] * Scale, mySpace.spaceLocation.Value[1] * Scale);
      
      //TODO: check view fro space creation - must be FloorPlan
      //if (Doc.ActiveView.ViewType == Autodesk.Revit.DB.ViewType.FloorPlan)

      // Create Revit space
      Autodesk.Revit.DB.Mechanical.Space revitSpace = Doc.Create.NewSpace(level, phase, locPoint);

      //Set element parameters
      //TODO: Check if all parameters set up
      SetElementParams(revitSpace, mySpace.parameters);

      //Add space tag (if one been loaded to project)
      Autodesk.Revit.DB.Mechanical.SpaceTag tag = Doc.Create.NewSpaceTag(revitSpace, locPoint, Doc.ActiveView);

      return revitSpace;
    }
  }
}
