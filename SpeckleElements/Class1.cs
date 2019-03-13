using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElements
{
  public interface ISpeckleElement
  {
    Dictionary<string, object> parameters { get; set; }
  }

  public class GridLine : SpeckleLine, ISpeckleElement
  {
    public override string Type { get => "GridLine"; set => base.Type = value; }

    public Dictionary<string, object> parameters { get => Properties; set => Properties = value; }
  }
}
