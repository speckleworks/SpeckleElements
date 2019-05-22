# SpeckleElements

[![Build status](https://ci.appveyor.com/api/projects/status/e9yeqdpjbmiv3pc4?svg=true)](https://ci.appveyor.com/project/SpeckleWorks/speckleelements)


A work in progress, revit-centric, object model and its conversion routines to and from speckle. Includes the following, in progress, object defintions: 

- GridLine
- Level
- Wall
- Floor
- Column
- Beam
- Shaft
- Topography
- FamilyInstance (from revit only) - covers all families without an explicit conversion routine (ie, doors)
- GenericElement (from revit only) - covers everything else not explicitely covered
- Room

These are early days and the following are not battle tested. 

# Notes & Dev Log: 

Most of these elements inherit from a SpeckleMesh (from SpeckleCoreGeometry). While this sounds strange, the reason behind it is that this allows us to easily get these elements out of revit in a way that it can be previewed somewhere else (web, rhino, etc). Actual object properties are still accessible, so you can do machine learning and stuff on your wall elevations if you want to.

**Grids**: 
- Question: are non-horizontal grids allowed / do they exist? A: Yes, and they are currently not supported, see below.
- Default `z` coordinate for anything grid is ZERO. See above for problems.

**Walls**:
- bottom and top constraints are supported (revit to revit) 
- offset and hegiht are captured and are supported, so is the wall type (will use default if not present)
- there's quite a bit of cruft in the wall generation part due to trying to support one to many generation (polyline -> n walls).
- finally nailed arcs 
- curtain walls get exported, but there's no way to create them now; sorry.

**Levels**: 
- kind of work 
- when creating a level, we check if there is one already at that elevation; if yes, we reuse that
- if no, we create a new level at that specific elevation

**Floors**:
- kind of work (at least from gh)
- need a load of checking and exception handling

**Rooms**:
- Only out of revit please. 

**GenericElement**: 
- anything that we don't know how to convert goes in here.
- ie, whatever poop you select from revit ends up like this, unless there's an explicit ToNative() conversion for its type.

**Family instance**: 
- again, any family that we do not know how to convert goes in here. 
- Beams and Columns have special cases and are exported as such.

## TODOs: 

- Go on holidays - afterwards, we can talk.
- Support more elements. Depending on what the crowd wants. 
- Integrate analytical models - for all the elements that have a sane analytical model...

# License
MIT
