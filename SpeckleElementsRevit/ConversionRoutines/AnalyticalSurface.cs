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
    public static Element ToNative(this Structural2DElementMesh myMesh)
    {
      return null;
    }

    public static List<SpeckleObject> ToSpeckle(this Autodesk.Revit.DB.Structure.AnalyticalModelSurface mySurface)
    {
      if (!mySurface.IsEnabled())
        return new List<SpeckleObject>();

      // Get the family
      var myRevitElement = Doc.GetElement(mySurface.GetElementId());

      Structural2DElementType type = Structural2DElementType.Generic;
      if (myRevitElement is Autodesk.Revit.DB.Floor)
        type = Structural2DElementType.Slab;
      else if (myRevitElement is Autodesk.Revit.DB.Wall)
        type = Structural2DElementType.Wall;

      List<double[]> polylines = new List<double[]>();

      var loops = mySurface.GetLoops(AnalyticalLoopType.External);
      foreach (CurveLoop loop in loops)
      {
        List<double> coor = new List<double>();
        foreach (Curve curve in loop)
        {
          var point = (SpeckleCoreGeometryClasses.SpecklePoint)SpeckleCore.Converter.Serialise(curve.GetEndPoint(0));
          coor.AddRange(point.Value);
        }

        polylines.Add(coor.ToArray());
      }

      var coordinateSystem = mySurface.GetLocalCoordinateSystem();
      var axis = new StructuralAxis(
        new StructuralVectorThree(new double[] { coordinateSystem.BasisX.X, coordinateSystem.BasisX.Y, coordinateSystem.BasisX.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisY.X, coordinateSystem.BasisY.Y, coordinateSystem.BasisY.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z })
      );

      
      // Property
      var mySection = new Structural2DProperty();

      mySection.Name = Doc.GetElement(mySurface.GetElementId()).Name;
      mySection.StructuralId = mySection.Name;

      if (myRevitElement is Autodesk.Revit.DB.Floor)
      {
        var myFloor = myRevitElement as Autodesk.Revit.DB.Floor;
        mySection.Thickness = myFloor.GetParameters("Thickness")[0].AsDouble() / Scale;
      }
      else if (myRevitElement is Autodesk.Revit.DB.Wall)
      {
        var myWall = myRevitElement as Autodesk.Revit.DB.Wall;
        mySection.Thickness = myWall.WallType.Width / Scale;
      }
      
      // Material
      string matType = "";
      string matID = "";
      if (myRevitElement is Autodesk.Revit.DB.Floor)
      {
        var myFloor = myRevitElement as Autodesk.Revit.DB.Floor;
        var myMat = Doc.GetElement(myFloor.FloorType.StructuralMaterialId) as Material;
        matType = myMat.MaterialClass;
        matID = Doc.GetElement(myMat.StructuralAssetId).Name;
      }
      else if (myRevitElement is Autodesk.Revit.DB.Wall)
      {
        var myWall = myRevitElement as Autodesk.Revit.DB.Wall;
        var myMat = Doc.GetElement(myWall.WallType.GetParameters("Structural Material")[0].AsElementId()) as Material;
        matType = myMat.MaterialClass;
        matID = Doc.GetElement(myMat.StructuralAssetId).Name;
      }

      SpeckleObject myMaterial = null;

      switch(matType)
      {
        case "Concrete":
          var concMat = new StructuralMaterialConcrete();
          concMat.StructuralId = matID;
          myMaterial = concMat;
          break;
        case "Steel":
          var steelMat = new StructuralMaterialSteel();
          steelMat.StructuralId = matID;
          myMaterial = steelMat;
          break;
        default:
          var defMat = new StructuralMaterialSteel();
          defMat.StructuralId = matID;
          myMaterial = defMat;
          break;
      }

      mySection.MaterialRef = (myMaterial as IStructural).StructuralId;
      
      myMaterial.GenerateHash();
      mySection.GenerateHash();

      mySection.ApplicationId = mySurface.UniqueId + "_material";
      mySection.ApplicationId = mySurface.UniqueId + "_section";

      List<SpeckleObject> meshes = new List<SpeckleObject>();

      foreach(double[] coor in polylines)
      {
        var dummyMesh = new Structural2DElementMesh(coor, null, type, mySection.StructuralId, axis, 0, null);
        var mesh = new Structural2DElementMesh();
        mesh.Vertices = dummyMesh.Vertices;
        mesh.Faces = dummyMesh.Faces;
        mesh.Colors = dummyMesh.Colors;
        mesh.ElementType = type;
        mesh.PropertyRef = mySection.StructuralId;
        mesh.Axis = axis;
        mesh.Offset = 0; //TODO

        mesh.GenerateHash();
        mesh.ApplicationId = mySurface.UniqueId; // THIS IS NOT UNIQUE ANYMORE

        meshes.Add(mesh);
      }

      return meshes.Concat(new List<SpeckleObject>() { mySection, myMaterial }).ToList();
    }
  }
}
