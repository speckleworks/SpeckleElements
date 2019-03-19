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
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser( ) { }

    /// <summary>
    /// Revit doc will be injected in here by the revit plugin. 
    /// To create a similar kit, make sure you declare this property in your initialiser class. 
    /// </summary>
    public static UIApplication RevitApp { get; set; }
    public static List<SpeckleStream> LocalRevitState { get; set; }
    public static double RevitScale = 3.2808399;
  }

  public static partial class Conversions
  {
    static double Scale { get => Initialiser.RevitScale; }
    static Document Doc { get => Initialiser.RevitApp.ActiveUIDocument.Document; }


    public static Element GetExistingElementById( string _id )
    {
      foreach ( var stream in Initialiser.LocalRevitState )
      {
        var found = stream.Objects.FirstOrDefault( s => s.ApplicationId == _id );
        if ( found != null )
          return Doc.GetElement( found.Properties[ "revitUniqueId" ] as string );
      }
      return null;
    }

    public static Grid ToNative( this GridLine myGridLine )
    {
      var existing = GetExistingElementById( myGridLine.ApplicationId );
      if ( existing != null )
      {
        var myGrid = existing as Grid;

        var oldStart = myGrid.Curve.GetEndPoint( 0 );
        var oldEnd = myGrid.Curve.GetEndPoint( 1 );

        var newStart = new XYZ( myGridLine.Value[ 0 ] * Scale, myGridLine.Value[ 1 ] * Scale, myGridLine.Value[ 2 ] * Scale );
        var newEnd = new XYZ( myGridLine.Value[ 3 ] * Scale, myGridLine.Value[ 4 ] * Scale, myGridLine.Value[ 5 ] * Scale );

        var translate = newStart.Subtract( oldStart );
        ElementTransformUtils.MoveElement( Doc, myGrid.Id, translate );

        var currentDirection = myGrid.Curve.GetEndPoint( 0 ).Subtract( myGrid.Curve.GetEndPoint( 1 ) ).Normalize();
        var newDirection = newStart.Subtract( newEnd ).Normalize();

        var angle = newDirection.AngleTo( currentDirection );
        var crossProd = newDirection.CrossProduct( currentDirection ).Z;

        ElementTransformUtils.RotateElement( Doc, myGrid.Id, Line.CreateUnbound( newStart, XYZ.BasisZ ), crossProd < 0 ? angle : -angle );

        //TODO: set end points
        myGrid.SetCurveInView( DatumExtentType.Model, Doc.ActiveView, Line.CreateBound( newStart, newEnd ) );

        return myGrid;
      }
      else
      {
        var res = Grid.Create( Doc, Line.CreateBound( new XYZ( myGridLine.Value[ 0 ] * Scale, myGridLine.Value[ 1 ] *Scale, myGridLine.Value[ 2 ] * Scale ), new XYZ( myGridLine.Value[ 3 ] * Scale, myGridLine.Value[ 4 ] * Scale, myGridLine.Value[ 5 ] * Scale ) ) );

        return res;
      }
    }
  }
}
