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
    public static Element ToNative(this Structural1DElement myBeam)
    {
      return null;
    }

    public static List<SpeckleObject> ToSpeckle(this Autodesk.Revit.DB.Structure.AnalyticalModelStick myStick)
    {
      if (!myStick.IsEnabled())
        return new List<SpeckleObject>();

      // Get the family
      var myFamily = (Autodesk.Revit.DB.FamilyInstance)Doc.GetElement(myStick.GetElementId());

      var myElement = new Structural1DElement();
      var line = (SpeckleCoreGeometryClasses.SpeckleLine)SpeckleCore.Converter.Serialise(myStick.GetCurve());
      myElement.baseLine = line;

      var coordinateSystem = myStick.GetLocalCoordinateSystem();
      myElement.ZAxis = new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z });

      // Property
      var mySection = new Structural1DProperty();

      mySection.Name = Doc.GetElement(myStick.GetElementId()).Name;
      mySection.StructuralId = mySection.Name;

      switch (myFamily.Symbol.GetStructuralSection().StructuralSectionGeneralShape)
      {
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralI:
          mySection.Shape = Structural1DPropertyShape.I;
          mySection.Hollow = false;
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralT:
          mySection.Shape = Structural1DPropertyShape.T;
          mySection.Hollow = false;
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralH:
          mySection.Shape = Structural1DPropertyShape.Rectangular;
          mySection.Hollow = true;
          mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection()) / Scale;
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralR:
          mySection.Shape = Structural1DPropertyShape.Circular;
          mySection.Hollow = true;
          mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection()) / Scale;
          mySection.Profile = new SpeckleCircle(
            new SpecklePlane(new SpecklePoint(0, 0, 0),
              new SpeckleVector(0, 0, 1),
              new SpeckleVector(1, 0, 0),
              new SpeckleVector(0, 1, 0)),
            (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("Diameter").GetValue(myFamily.Symbol.GetStructuralSection()) / 2 / Scale);
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralF:
          mySection.Shape = Structural1DPropertyShape.Rectangular;
          mySection.Hollow = false;
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralS:
          mySection.Shape = Structural1DPropertyShape.Circular;
          mySection.Profile = new SpeckleCircle(
            new SpecklePlane(new SpecklePoint(0, 0, 0),
              new SpeckleVector(0, 0, 1),
              new SpeckleVector(1, 0, 0),
              new SpeckleVector(0, 1, 0)),
            (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("Diameter").GetValue(myFamily.Symbol.GetStructuralSection()) / 2 / Scale);
          mySection.Hollow = false;
          break;
        default:
          mySection.Shape = Structural1DPropertyShape.Generic;
          mySection.Hollow = false;
          break;
      }

      // Generate section profile
      var profile = myFamily.GetSweptProfile().GetSweptProfile();

      if (mySection.Shape != Structural1DPropertyShape.Circular)
      {
        var myProfile = new SpecklePolyline();

        myProfile.Value = new List<double>();

        for (int i = 0; i < profile.Curves.Size; i++)
        {
          var sectionCurves = SpeckleCore.Converter.Serialise(profile.Curves.get_Item(i));

          var sectionCoordinates = new List<double>();
          var nextCoordinates = new List<double>();

          if (sectionCurves is SpeckleLine)
          {
            sectionCoordinates = (sectionCurves as SpeckleLine).Value.Select(x => Math.Round(x, 10)).ToList();

            if (myProfile.Value.Count == 0)
            {
              myProfile.Value = sectionCoordinates;
              continue;
            }

            if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual(sectionCoordinates.Take(3)))
              nextCoordinates = sectionCoordinates.Skip(3).ToList();
            else
              break;
          }
          else if (sectionCurves is SpeckleArc)
          {
            if (myProfile.Value.Count == 0)
            {
              myProfile.Value = (sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10))
                .Concat((sectionCurves as SpeckleArc).MidPoint.Value.Select(x => Math.Round(x, 10)))
                .Concat((sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10)))
                .ToList();
              continue;
            }

            if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual((sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10))))
              nextCoordinates = (sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10)).ToList();
            else if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual((sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10))))
              nextCoordinates = (sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10)).ToList();
            else
              break;
          }

          if (nextCoordinates.SequenceEqual(myProfile.Value.Take(3)))
          {
            myProfile.Closed = true;
            break;
          }
          else
            myProfile.Value.AddRange(nextCoordinates);
        }
        
        myProfile.GenerateHash();

        mySection.Profile = myProfile;
      }

      myElement.PropertyRef = mySection.StructuralId;

      // Material
      var matType = myFamily.StructuralMaterialType;

      SpeckleObject myMaterial = null;

      switch(matType)
      {
        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
          var concMat = new StructuralMaterialConcrete();
          concMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
          concMat.Name = concMat.StructuralId;
          myMaterial = concMat;
          break;
        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
          var steelMat = new StructuralMaterialSteel();
          steelMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
          steelMat.Name = steelMat.StructuralId;
          myMaterial = steelMat;
          break;
        default:
          var defMat = new StructuralMaterialSteel();
          defMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
          defMat.Name = defMat.StructuralId;
          myMaterial = defMat;
          break;
      }

      mySection.MaterialRef = (myMaterial as IStructural).StructuralId;

      myMaterial.GenerateHash();
      mySection.GenerateHash();
      myElement.GenerateHash();

      mySection.ApplicationId = myStick.UniqueId + "_material";
      mySection.ApplicationId = myStick.UniqueId + "_section";
      myElement.ApplicationId = myStick.UniqueId;

      return new List<SpeckleObject>() { myElement, mySection, myMaterial };
    }
  }
}
