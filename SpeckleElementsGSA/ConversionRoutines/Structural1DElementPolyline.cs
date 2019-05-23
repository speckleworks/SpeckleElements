using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
    [GSAObject("MEMB.7", new string[] { }, "elements", true, true, new Type[] { typeof(GSA1DElement) }, new Type[] { })]
    public class GSA1DElementPolyline : IGSASpeckleContainer
    {
        public string GWACommand { get; set; }
        public List<string> SubGWACommand { get; set; } = new List<string>();
        public dynamic Value { get; set; } = new Structural1DElementPolyline();
        
        public void SetGWACommand(GSAInterfacer GSA)
        {
            if (this.Value == null)
                return;

            Structural1DElementPolyline obj = this.Value as Structural1DElementPolyline;

            int group = GSA.Indexer.ResolveIndex(typeof(GSA1DElementPolyline), obj);

            Structural1DElement[] elements = obj.Explode();

            foreach (Structural1DElement element in elements)
            {
                if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
                    new GSA1DElement() { Value = element }.SetGWACommand(GSA, group);
                else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
                    new GSA1DMember() { Value = element }.SetGWACommand(GSA, group);
            }
        }
    }
    
    public static partial class Conversions
    {
        public static bool ToNative(this Structural1DElementPolyline poly)
        {
            new GSA1DElementPolyline() { Value = poly }.SetGWACommand(GSA);

            return true;
        }

        public static SpeckleObject ToSpeckle(this GSA1DElementPolyline poly)
        {
            return new SpeckleNull();
        }
    }
}
