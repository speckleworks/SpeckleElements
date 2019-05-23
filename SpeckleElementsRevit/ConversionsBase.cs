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
    public Initialiser() { }

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
    public static double RevitScale { get; set; } = 3.2808399;

    /// <summary>
    /// Keeps track of the missing families and their types from the bake process. It's a quite roundabout way of doing things, but it keeps concerns separated. 
    /// More and more doubts about this architecture every day...
    /// </summary>
    public static HashSet<string> MissingFamiliesAndTypes { get; set; } = new HashSet<string>();
  }

  public static partial class Conversions
  {
    static double Scale { get => Initialiser.RevitScale; }
    static Document Doc { get => Initialiser.RevitApp.ActiveUIDocument.Document; }
    static HashSet<string> MissingFamiliesAndTypes { get => Initialiser.MissingFamiliesAndTypes; }

    public static GenericElement ToSpeckle( this Element myElement )
    {
      var generic = new GenericElement();

      (generic.Faces, generic.Vertices) = GetFaceVertexArrayFromElement( myElement );

      generic.parameters = GetElementParams( myElement );

      generic.ApplicationId = myElement.UniqueId;
      generic.GenerateHash();
      return generic;
    }

    /// <summary>
    /// Gets a dictionary representation of all this element's parameters.
    /// TODO: manage (somehow!) units; essentially set them back to whatever the current document
    /// setting is (meters, millimiters, etc). 
    /// </summary>
    /// <param name="myElement"></param>
    /// <returns></returns>
    public static Dictionary<string, object> GetElementParams( Element myElement )
    {
      var myParamDict = new Dictionary<string, object>();
      var unitsDict = new Dictionary<string, string>();

      foreach( Parameter p in myElement.Parameters )
      {
        switch( p.StorageType )
        {
          case StorageType.Double:
          // NOTE: do not use p.AsDouble() as direct input for unit utils conversion, it doesn't work. 
          // ¯\_(ツ)_/¯
          var val = p.AsDouble();
          try
          {
            myParamDict[ p.Definition.Name ] = UnitUtils.ConvertFromInternalUnits( val, p.DisplayUnitType );
            myParamDict[ "____units_" + p.Definition.Name ] = p.DisplayUnitType.ToString();
          }
          catch( Exception e )
          {
            myParamDict[ p.Definition.Name ] = val;
          }
          break;
          case StorageType.Integer:
          myParamDict[ p.Definition.Name ] = p.AsInteger();
          //myParamDict[ p.Definition.Name ] = UnitUtils.ConvertFromInternalUnits( p.AsInteger(), p.DisplayUnitType);
          break;
          case StorageType.String:
          myParamDict[ p.Definition.Name ] = p.AsString();
          break;
          case StorageType.ElementId:
          // TODO: Properly get ref elemenet and serialise it in here.
          // NOTE: Too much garbage for too little info...
          //var docEl = Doc.GetElement( p.AsElementId() );
          //var spk = SpeckleCore.Converter.Serialise( docEl );
          //if( !(spk is SpeckleNull) ) {
          //  myParamDict[ p.Definition.Name + "_el" ] = spk;
          //  myParamDict[ p.Definition.Name ] = p.AsValueString();
          //} else
          myParamDict[ p.Definition.Name ] = p.AsValueString();
          break;
          case StorageType.None:
          break;
        }
      }

      // TODO: Sanitise keys
      // ...
      // myParamDict["__units"] = unitsDict;
      // TODO: BIG CORE PROBLEM: failure to serialise things with nested dictionary (like the line above).
      return myParamDict;
    }

    public static void SetElementParams( Element myElement, Dictionary<string, object> parameters )
    {
      // TODO: Set parameters please
      if( myElement == null ) return;
      if( parameters == null ) return;

      //myElement.LookupParameter
      foreach( var kvp in parameters )
      {
        try
        {
          var myParam = myElement.LookupParameter( kvp.Key );
          if( myParam == null ) continue;
          switch( myParam.StorageType )
          {
            case StorageType.Double:
              //TODO: Set Double param, risky as it's potentially overriding things?
            break;
            case StorageType.Integer:
            myParam.Set( (int) kvp.Value );
            break;
            case StorageType.String:
            myParam.Set( (string) kvp.Value );
            break;
            case StorageType.ElementId:
              //TODO
            break;
          }
        }catch(Exception e) { }
      }

    }

    /// <summary>
    /// Gets an element by its type and name. If nothing found, returns the first one.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Element GetElementByName( Type type, string name )
    {
      var collector = new FilteredElementCollector( Doc ).OfClass( type );

      if( name == null ) return collector.FirstElement();

      foreach( var myElement in collector.ToElements() )
        if( myElement.Name == name ) return myElement;

      // now returning the first type, which means we didn't find the type we were actually looking for.
      MissingFamiliesAndTypes.Add( "Wall type " + name );
      return collector.FirstElement();
    }

    /// <summary>
    /// Returns, if found, the corresponding doc element and its corresponding local state object.
    /// The doc object can be null if the user deleted it. 
    /// </summary>
    /// <param name="ApplicationId"></param>
    /// <returns></returns>
    public static (Element, SpeckleObject) GetExistingElementByApplicationId( string ApplicationId, string ObjectType )
    {
      foreach( var stream in Initialiser.LocalRevitState )
      {
        var found = stream.Objects.FirstOrDefault( s => s.ApplicationId == ApplicationId && (string) s.Properties[ "__type" ] == ObjectType );
        if( found != null )
          return (Doc.GetElement( found.Properties[ "revitUniqueId" ] as string ), (SpeckleObject) found);
      }
      return (null, null);
    }

    public static (List<Element>, List<SpeckleObject>) GetExistingElementsByApplicationId( string ApplicationId, string ObjectType )
    {
      var allStateObjects = (from p in Initialiser.LocalRevitState.SelectMany( s => s.Objects ) select p).ToList();

      var found = allStateObjects.Where( obj => obj.ApplicationId == ApplicationId && (string) obj.Properties[ "__type" ] == ObjectType );
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
      switch( crv )
      {
        case SpeckleLine line:
        myCurves.Add( (Line) SpeckleCore.Converter.Deserialise( line, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
        return myCurves;

        case SpeckleArc arc:
        myCurves.Add( (Arc) SpeckleCore.Converter.Deserialise( arc, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
        return myCurves;

        case SpeckleCurve nurbs:
        myCurves.Add( (Curve) SpeckleCore.Converter.Deserialise( nurbs, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
        return myCurves;

        case SpecklePolyline poly:
        if( poly.Value.Count == 6 )
        {
          myCurves.Add( (Line) SpeckleCore.Converter.Deserialise( new SpeckleLine( poly.Value ), excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
        }
        else
        {
          List<SpecklePoint> pts = new List<SpecklePoint>();
          for( int i = 0; i < poly.Value.Count; i += 3 )
          {
            pts.Add( new SpecklePoint( poly.Value[ i ], poly.Value[ i + 1 ], poly.Value[ i + 2 ] ) );
          }

          for( int i = 1; i < pts.Count; i++ )
          {
            var speckleLine = new SpeckleLine( new double[ ] { pts[ i - 1 ].Value[ 0 ], pts[ i - 1 ].Value[ 1 ], pts[ i - 1 ].Value[ 2 ], pts[ i ].Value[ 0 ], pts[ i ].Value[ 1 ], pts[ i ].Value[ 2 ] } );

            myCurves.Add( (Line) SpeckleCore.Converter.Deserialise( speckleLine, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
          }

          if( poly.Closed )
          {
            var speckleLine = new SpeckleLine( new double[ ] { pts[ pts.Count - 1 ].Value[ 0 ], pts[ pts.Count - 1 ].Value[ 1 ], pts[ pts.Count - 1 ].Value[ 2 ], pts[ 0 ].Value[ 0 ], pts[ 0 ].Value[ 1 ], pts[ 0 ].Value[ 2 ] } );
            myCurves.Add( (Line) SpeckleCore.Converter.Deserialise( speckleLine, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } ) );
          }
        }
        return myCurves;

        case SpecklePolycurve plc:
        foreach( var seg in plc.Segments )
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
      if( name == null )
        return collector.FirstElement();

      foreach( var e in collector.ToElements() )
        if( e.Name == name )
          return e;

      return collector.FirstElement();
    }

    public static FamilySymbol GetFamilySymbolByFamilyNameAndTypeAndCategory( string familyName, string typeName, BuiltInCategory category )
    {
      var collectorElems = new FilteredElementCollector( Doc ).WhereElementIsElementType().OfClass( typeof( FamilySymbol ) ).OfCategory( category ).ToElements().Cast<FamilySymbol>();

      if( (familyName == null || typeName == null) && collectorElems.Count() > 0 )
        return collectorElems.First();

      foreach( var e in collectorElems )
      {
        if( e.FamilyName == familyName && e.Name == typeName )
          return e;
      }

      if( collectorElems.Count() > 0 )
      {
        MissingFamiliesAndTypes.Add( familyName + " " + typeName );
        return collectorElems.First();
      }

      return null;
    }
  }
}
