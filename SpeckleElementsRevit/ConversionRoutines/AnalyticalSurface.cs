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
      List<SpeckleObject> returnObjects = new List<SpeckleObject>();

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
      var axis = coordinateSystem == null ? null : new StructuralAxis(
        new StructuralVectorThree(new double[] { coordinateSystem.BasisX.X, coordinateSystem.BasisX.Y, coordinateSystem.BasisX.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisY.X, coordinateSystem.BasisY.Y, coordinateSystem.BasisY.Z }),
        new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z })
      );

      // Property
      string sectionID = null;
      try
      {
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

        try
        {
          // Material
          Material myMat = null;
          StructuralAsset matAsset = null;

          if (myRevitElement is Autodesk.Revit.DB.Floor)
          {
            var myFloor = myRevitElement as Autodesk.Revit.DB.Floor;
            myMat = Doc.GetElement(myFloor.FloorType.StructuralMaterialId) as Material;
          }
          else if (myRevitElement is Autodesk.Revit.DB.Wall)
          {
            var myWall = myRevitElement as Autodesk.Revit.DB.Wall;
            myMat = Doc.GetElement(myWall.WallType.GetParameters("Structural Material")[0].AsElementId()) as Material;
          }

          SpeckleObject myMaterial = null;

          matAsset = ((Autodesk.Revit.DB.PropertySetElement)Doc.GetElement(myMat.StructuralAssetId)).GetStructuralAsset();

          string matType = myMat.MaterialClass;

          switch (matType)
          {
            case "Concrete":
              var concMat = new StructuralMaterialConcrete();
              concMat.StructuralId = Doc.GetElement(myMat.StructuralAssetId).Name;
              concMat.Name = concMat.StructuralId;
              concMat.YoungsModulus = matAsset.YoungModulus.X;
              concMat.ShearModulus = matAsset.ShearModulus.X;
              concMat.PoissonsRatio = matAsset.PoissonRatio.X;
              concMat.Density = matAsset.Density;
              concMat.CoeffThermalExpansion = matAsset.ThermalExpansionCoefficient.X;
              concMat.CompressiveStrength = matAsset.ConcreteCompression;
              concMat.MaxStrain = 0;
              concMat.AggragateSize = 0;
              myMaterial = concMat;
              break;
            case "Steel":
              var steelMat = new StructuralMaterialSteel();
              steelMat.StructuralId = Doc.GetElement(myMat.StructuralAssetId).Name;
              steelMat.Name = steelMat.StructuralId;
              steelMat.YoungsModulus = matAsset.YoungModulus.X;
              steelMat.ShearModulus = matAsset.ShearModulus.X;
              steelMat.PoissonsRatio = matAsset.PoissonRatio.X;
              steelMat.Density = matAsset.Density;
              steelMat.CoeffThermalExpansion = matAsset.ThermalExpansionCoefficient.X;
              steelMat.YieldStrength = matAsset.MinimumYieldStress;
              steelMat.UltimateStrength = matAsset.MinimumTensileStrength;
              steelMat.MaxStrain = 0;
              myMaterial = steelMat;
              break;
            default:
              var defMat = new StructuralMaterialSteel();
              defMat.StructuralId = Doc.GetElement(myMat.StructuralAssetId).Name;
              defMat.Name = defMat.StructuralId;
              myMaterial = defMat;
              break;
          }

          myMaterial.GenerateHash();
          myMaterial.ApplicationId = mySurface.UniqueId + "_material";
          mySection.MaterialRef = (myMaterial as IStructural).StructuralId;

          returnObjects.Add(myMaterial);
        }
        catch { }

        mySection.GenerateHash();
        mySection.ApplicationId = mySurface.UniqueId + "_section";

        sectionID = mySection.StructuralId;

        returnObjects.Add(mySection);
      }
      catch { }
      
      foreach(double[] coor in polylines)
      {
        var dummyMesh = new Structural2DElementMesh(coor, null, type, null, null, null);
        
        int numFaces = 0;
        for (int i = 0; i < dummyMesh.Faces.Count(); i++)
        {
          numFaces++;
          i += dummyMesh.Faces[i] + 3;
        }

        var mesh = new Structural2DElementMesh();
        mesh.Vertices = dummyMesh.Vertices;
        mesh.Faces = dummyMesh.Faces;
        mesh.Colors = dummyMesh.Colors;
        mesh.ElementType = type;
        if (sectionID != null)
          mesh.PropertyRef = sectionID;
        if (axis != null)
          mesh.Axis = Enumerable.Repeat(axis, numFaces).ToList();
        mesh.Offset = Enumerable.Repeat(0, numFaces).Cast<double>().ToList(); //TODO

        mesh.GenerateHash();
        mesh.ApplicationId = mySurface.UniqueId; // THIS IS NOT UNIQUE ANYMORE

        returnObjects.Add(mesh);
      }

      return returnObjects;
    }
  }
}
