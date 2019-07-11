using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleElements
{
  public enum StructuralLoadCaseType
  {
    Generic,
    Dead,
    Soil,
    Live,
    Rain,
    Snow,
    Wind,
    Earthquake,
    Thermal
  }

  public enum StructuralLoadTaskType
  {
    LinearStatic,
    NonlinearStatic,
    Modal,
    Buckling
  }

  public enum StructuralLoadComboType
  {
    Envelope,
    LinearAdd
  }

  public enum StructuralMaterialType
  {
    Generic,
    Steel,
    Concrete
  }

  public enum Structural1DPropertyShape
  {
    Generic,
    Circular,
    Rectangular,
    I,
    T
  }

  public enum Structural2DPropertyReferenceSurface
  {
    Top,
    Middle,
    Bottom,
  }

  public enum Structural1DElementType
  {
    Generic,
    Column,
    Beam,
    Cantilever,
    Brace
  }

  public enum Structural2DElementType
  {
    Generic,
    Slab,
    Wall
  }

  public enum StructuralResultSource
  {
    Case,
    Task,
    Combo
  }

  public enum StructuralThermalLoadingType
  {
    Uniform,
    Gradient,
    General
  }
}
