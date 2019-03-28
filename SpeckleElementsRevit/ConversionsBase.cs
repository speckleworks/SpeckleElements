using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElements;
using SpeckleCoreGeometryClasses;

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

    /// <summary>
    /// Local revit state (existing objects coming from a bake) will be injected here.
    /// </summary>
    public static List<SpeckleStream> LocalRevitState { get; set; }

    /// <summary>
    /// Scale will be set here by each individual stream bake. 
    /// TODO: Potential race condition when we simulatenously start baking two or more streams that have different scales.
    /// </summary>
    public static double RevitScale = 3.2808399;
  }

  public static partial class Conversions
  {
    static double Scale { get => Initialiser.RevitScale; }
    static Document Doc { get => Initialiser.RevitApp.ActiveUIDocument.Document; }

    /// <summary>
    /// Returns, if found, the corresponding doc element and its corresponding local state object.
    /// The doc object can be null if the user deleted it. 
    /// </summary>
    /// <param name="ApplicationId"></param>
    /// <returns></returns>
    public static (Element, SpeckleObject) GetExistingElementByApplicationId( string ApplicationId, string ObjectType )
    {
      foreach ( var stream in Initialiser.LocalRevitState )
      {
        var found = stream.Objects.FirstOrDefault( s => s.ApplicationId == ApplicationId && s.Type == ObjectType );
        if ( found != null )
          return (Doc.GetElement( found.Properties[ "revitUniqueId" ] as string ), ( SpeckleObject ) found);
      }
      return (null, null);
    }

    public static (List<Element>, List<SpeckleObject>) GetExistingElementsByApplicationId( string ApplicationId, string ObjectType )
    {
      var allStateObjects = ( from p in Initialiser.LocalRevitState.SelectMany( s => s.Objects ) select p ).ToList();

      var found = allStateObjects.Where( obj => obj.ApplicationId == ApplicationId && obj.Type == ObjectType );
      var revitObjs = found.Select( obj => Doc.GetElement( obj.Properties[ "revitUniqueId" ] as string ) );

      return (revitObjs.ToList(), found.ToList());
    }

    /// <summary>
    /// Recursively creates an ordered list of curves from a polycurve/polyline.
    /// Please note that a polyline is broken down into lines.
    /// </summary>
    /// <param name="crv">A speckle curve.</param>
    /// <returns></returns>
    public static List<Curve> GetSegmentList( object crv )
    {
      List<Curve> myCurves = new List<Curve>();
      switch ( crv )
      {
        case SpeckleLine line:
          myCurves.Add( ( Line ) SpeckleCore.Converter.Deserialise( line ) );
          return myCurves;

        case SpeckleArc arc:
          myCurves.Add( ( Arc ) SpeckleCore.Converter.Deserialise( arc ) );
          return myCurves;

        case SpeckleCurve nurbs:
          myCurves.Add( ( Curve ) SpeckleCore.Converter.Deserialise( nurbs ) );
          return myCurves;

        case SpecklePolyline poly:
          if ( poly.Value.Count == 6 )
          {
            myCurves.Add( ( Line ) SpeckleCore.Converter.Deserialise( new SpeckleLine( poly.Value ) ) );
          }
          else
          {
            List<SpecklePoint> pts = new List<SpecklePoint>();
            for ( int i = 0; i < poly.Value.Count; i += 3 )
            {
              pts.Add( new SpecklePoint( poly.Value[ i ], poly.Value[ i + 1 ], poly.Value[ i + 2 ] ) );
            }

            for ( int i = 1; i < pts.Count; i++ )
            {
              var speckleLine = new SpeckleLine( new double[ ] { pts[ i - 1 ].Value[ 0 ], pts[ i - 1 ].Value[ 1 ], pts[ i - 1 ].Value[ 2 ], pts[ i ].Value[ 0 ], pts[ i ].Value[ 1 ], pts[ i ].Value[ 2 ] } );

              myCurves.Add( ( Line ) SpeckleCore.Converter.Deserialise( speckleLine ) );
            }

            if ( poly.Closed )
            {
              var speckleLine = new SpeckleLine( new double[ ] { pts[ pts.Count - 1 ].Value[ 0 ], pts[ pts.Count - 1 ].Value[ 1 ], pts[ pts.Count - 1 ].Value[ 2 ], pts[ 0 ].Value[ 0 ], pts[ 0 ].Value[ 1 ], pts[ 0 ].Value[ 2 ] } );
              myCurves.Add( ( Line ) SpeckleCore.Converter.Deserialise( speckleLine ) );
            }
          }
          return myCurves;

        case SpecklePolycurve plc:
          foreach ( var seg in plc.Segments )
            myCurves.AddRange( GetSegmentList( seg ) );
          return myCurves;

      }
      return null;
    }


    /// <summary>
    /// Stolen from grevit.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Element GetElementByClassAndName( Type type, string name = null )
    {
      var collector = new FilteredElementCollector( Doc ).OfClass( type );

      // check against element name
      if ( name == null )
        return collector.FirstElement();

      foreach ( var e in collector.ToElements() )
        if ( e.Name == name )
          return e;

      return collector.FirstElement();
    }

  }
}
