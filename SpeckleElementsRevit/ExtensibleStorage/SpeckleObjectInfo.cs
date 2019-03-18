using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SpeckleElementsRevit.ExtensibleStorage
{
  public class SpeckleObjectInfo
  {
    public string GUID;
    public string Hash;
    public List<string> Streams = new List<string>();
  }

  public static class SpeckleObjectInfoSchema
  {
    readonly static Guid schemaGuid = new Guid( "{A0060917-9023-45DB-A9F5-C37B86AC2030}" );
    public static Schema GetSchema( )
    {
      var schema = Schema.Lookup( schemaGuid );
      if ( schema != null ) return schema;

      var builder = new SchemaBuilder( schemaGuid );
      builder.SetSchemaName( "SpeckleObjectInfo" );

      builder.AddSimpleField( "GUID", typeof( string ) );
      builder.AddSimpleField( "Hash", typeof( string ) );
      builder.AddArrayField( "Streams", typeof( string ) );

      return builder.Finish();
    }
  }

}
