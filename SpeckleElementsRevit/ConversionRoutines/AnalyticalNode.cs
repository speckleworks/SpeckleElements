using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative(this StructuralNode myBeam)
    {
      return null;
    }

    public static List<SpeckleObject> ToSpeckle(this Autodesk.Revit.DB.Structure.BoundaryConditions myRestraint)
    {
      List<XYZ> points = new List<XYZ>();

      var restraintType = myRestraint.GetBoundaryConditionsType();

      if (restraintType == BoundaryConditionsType.Point)
      {
        var point = myRestraint.Point;
        points.Add(point);
      }
      else if (restraintType == BoundaryConditionsType.Line)
      {
        var curve = myRestraint.GetCurve();
        points.Add(curve.GetEndPoint(0));
        points.Add(curve.GetEndPoint(1));
      }
      else if (restraintType == BoundaryConditionsType.Area)
      {
        var loops = myRestraint.GetLoops();
        foreach (CurveLoop loop in loops)
        {
          foreach (Curve curve in loop)
          {
            points.Add(curve.GetEndPoint(0));
            points.Add(curve.GetEndPoint(1));
          }
        }
        points = points.Distinct().ToList();
      }

      var coordinateSystem = myRestraint.GetDegreesOfFreedomCoordinateSystem();
      var axis = new StructuralAxis(
        new StructuralVectorThree(new double[] { coordinateSystem.BasisX.X, coordinateSystem.BasisX.Y, coordinateSystem.BasisX.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisY.X, coordinateSystem.BasisY.Y, coordinateSystem.BasisY.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z })
      );

      var state = myRestraint.GetParameters("State")[0].AsValueString();
      var restraint = new StructuralVectorBoolSix();
      if (state == "Fixed")
      {
        restraint.Value = new List<bool>() { true, true, true, true, true, true };
        restraint.GenerateHash();
      }
      else if (state == "Pinned")
      {
        restraint.Value = new List<bool>() { true, true, true, false, false, false };
        restraint.GenerateHash();
      }

      List<SpeckleObject> myNodes = new List<SpeckleObject>();

      foreach(XYZ point in points)
      {
        var myPoint = (SpeckleCoreGeometryClasses.SpecklePoint)SpeckleCore.Converter.Serialise(point);
        var myNode = new StructuralNode();
        myNode.basePoint = myPoint;
        myNode.Axis = axis;
        myNode.Restraint = restraint;
        myNodes.Add(myNode);
      }
      
      return myNodes;
    }
  }
}
