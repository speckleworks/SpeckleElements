using System.Linq;
using Autodesk.Revit.DB;

namespace SpeckleElementsRevit
{

  public static partial class Conversions
  {
    /// <summary>
    /// Inspired by Grevit https://github.com/grevit-dev/Grevit/blob/483eb57ac1cc8669046572933362b378a272b89f/Grevit.Revit/CreateExtension.cs#L347
    /// </summary>
    /// <param name="myCurve"></param>
    /// <returns></returns>
    public static Element ToNative(this SpeckleElementsClasses.Curve myCurve)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myCurve.ApplicationId, myCurve.Type);

      // cheating a bit. 
      var segments = GetSegmentList(myCurve.baseCurve);
      var baseCurve = segments[0];

      if (docObj != null) // we have a document object already, so check if we can edit it.
      {
        //var type = Doc.GetElement(docObj.GetTypeId()) as ElementType;

        //// if family changed, tough luck - delete and rewind
        //if (myAdaptiveComponent.familyName != type.FamilyName)
        //{
        //  //delete and continue crating it
        //  Doc.Delete(docObj.Id);
        //}
        //// edit element
        //else
        //{
        //  var existingFamilyInstance = (Autodesk.Revit.DB.FamilyInstance)docObj;

        //  // check if type changed, and try and change it
        //  if (myAdaptiveComponent.familyType != null && (myAdaptiveComponent.familyType != type.Name))
        //  {
        //    existingFamilyInstance.ChangeTypeId(familySymbol.Id);
        //  }

        //  SetAdaptiveComponentPoints(existingFamilyInstance, myAdaptiveComponent.points);
        //  SetElementParams(existingFamilyInstance, myAdaptiveComponent.parameters);
        //  return existingFamilyInstance;
        //}
      }

      // if we don't have a document object
      else
      {
        Element el = null;
        if (myCurve.curveType == SpeckleElementsClasses.CurveType.DetailCurve)
          el = Doc.Create.NewDetailCurve(Doc.ActiveView, baseCurve);

        else if (myCurve.curveType == SpeckleElementsClasses.CurveType.RoomBounding)
        {
          CurveArray tmpca = new CurveArray();
          tmpca.Append(baseCurve);
          el = Doc.Create.NewRoomBoundaryLines(NewSketchPlaneFromCurve(Doc, baseCurve), tmpca, Doc.ActiveView).get_Item(0);
        }
        else
        {
          el = Doc.Create.NewModelCurve(baseCurve, NewSketchPlaneFromCurve(Doc, baseCurve));
        }
        SetElementParams(el, myCurve.parameters);
        return el;
      }


      return null;
    }

    /// <summary>
    /// Credits: Grevit
    /// Creates a new Sketch Plane from a Curve
    /// https://github.com/grevit-dev/Grevit/blob/3c7a5cc198e00dfa4cc1e892edba7c7afd1a3f84/Grevit.Revit/Utilities.cs#L402
    /// </summary>
    /// <param name="document">Active Document</param>
    /// <param name="curve">Curve to get plane from</param>
    /// <returns>Plane of the curve</returns>
    public static SketchPlane NewSketchPlaneFromCurve(Document document, Autodesk.Revit.DB.Curve curve)
    {
      XYZ startPoint = curve.GetEndPoint(0);
      XYZ endPoint = curve.GetEndPoint(1);

      // If Start end Endpoint are the same check further points.
      int i = 2;
      while (startPoint == endPoint && endPoint != null)
      {
        endPoint = curve.GetEndPoint(i);
        i++;
      }

      // Plane to return
      Plane plane;

      // If Z Values are equal the Plane is XY
      if (startPoint.Z == endPoint.Z)
      {
        plane = CreatePlane(document, XYZ.BasisZ, startPoint);
      }
      // If X Values are equal the Plane is YZ
      else if (startPoint.X == endPoint.X)
      {
        plane = CreatePlane(document, XYZ.BasisX, startPoint);
      }
      // If Y Values are equal the Plane is XZ
      else if (startPoint.Y == endPoint.Y)
      {
        plane = CreatePlane(document, XYZ.BasisY, startPoint);
      }
      // Otherwise the Planes Normal Vector is not X,Y or Z.
      // We draw lines from the Origin to each Point and use the Plane this one spans up.
      else
      {
        CurveArray curves = new CurveArray();
        curves.Append(curve);
        curves.Append(Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, 0), startPoint));
        curves.Append(Autodesk.Revit.DB.Line.CreateBound(endPoint, new XYZ(0, 0, 0)));
#if (Revit2015 || Revit2016 || Revit2017)
                plane = document.Application.Create.NewPlane(curves);
#else
        plane = Plane.CreateByThreePoints(startPoint, new XYZ(0, 0, 0), endPoint);
#endif
      }


      // return new Sketchplane
      return SketchPlane.Create(document, plane);
    }

    public static Plane CreatePlane(Document document, XYZ basis, XYZ startPoint)
    {
#if (Revit2015 || Revit2016 || Revit2017)
            return document.Application.Create.NewPlane(basis, startPoint);
#else
      return Plane.CreateByNormalAndOrigin(basis, startPoint);
#endif
    }
  }
}
