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
    public static Element ToNative(this Structural1DElement myBeam)
    {
      return null;
    }

    public static List<SpeckleObject> AnalyticalStickToSpeckle(Autodesk.Revit.DB.FamilyInstance myFamily)
    {
      // Generate Analytical Model
      var myStick = (Autodesk.Revit.DB.Structure.AnalyticalModelStick)myFamily.GetAnalyticalModel();

      if (!myStick.IsEnabled())
        return new List<SpeckleObject>();

      var myElement = new Structural1DElement();
      myElement.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine)SpeckleCore.Converter.Serialise(myStick.GetCurve());

      var coordinateSystem = myStick.GetLocalCoordinateSystem();
      myElement.ZAxis = new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z });

      // Property
      var mySection = new Structural1DProperty();

      mySection.Name = myFamily.Symbol.GetStructuralSection().StructuralSectionShapeName;
      mySection.StructuralId = myFamily.Symbol.GetStructuralSection().StructuralSectionShapeName;

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
          mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection());
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralR:
          mySection.Shape = Structural1DPropertyShape.Circular;
          mySection.Hollow = true;
          mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection());
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralF:
          mySection.Shape = Structural1DPropertyShape.Rectangular;
          mySection.Hollow = false;
          break;
        case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralS:
          mySection.Shape = Structural1DPropertyShape.Circular;
          mySection.Hollow = false;
          break;
        default:
          mySection.Shape = Structural1DPropertyShape.Generic;
          mySection.Hollow = false;
          break;
      }

      // Generate section profile
      var profile = myFamily.GetSweptProfile().GetSweptProfile();

      if (mySection.Shape == Structural1DPropertyShape.Circular)
      {
        var myProfile = new SpeckleCircle();

        var sectionArc = SpeckleCore.Converter.Serialise(profile.Curves.get_Item(0)) as SpeckleArc;
        double radius = Math.Round(Math.Sqrt(sectionArc.StartPoint.Value.Sum(x => x * x)));

        myProfile.Radius = radius;
        myProfile.Plane = new SpecklePlane(new SpecklePoint(0, 0, 0),
          new SpeckleVector(0, 0, 1),
          new SpeckleVector(1, 0, 0),
          new SpeckleVector(0, 1, 0));
        myProfile.GenerateHash();

        mySection.Profile = myProfile;
      }
      else
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

      mySection.GenerateHash();
      myElement.GenerateHash();

      myElement.ApplicationId = myStick.UniqueId;
      mySection.ApplicationId = myStick.UniqueId + "_section";

      return new List<SpeckleObject>() { myElement, mySection };
    }
  }
}
