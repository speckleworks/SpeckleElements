using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElements;

namespace SpeckleElementsGSA
{
    [GSAObject("ANAL_STAGE.3", new string[] { "LIST.1" }, "elements", false, true, new Type[] { }, new Type[] { typeof(GSA1DMember), typeof(GSA2DMember) })]
    public class GSAStageDefinition : IGSASpeckleContainer
    {
        public string GWACommand { get; set; }
        public List<string> SubGWACommand { get; set; } = new List<string>();
        public dynamic Value { get; set; } = new StructuralStageDefinition();

        public void ParseGWACommand(GSAInterfacer GSA)
        {
            if (this.GWACommand == null)
                return;

            StructuralStageDefinition obj = new StructuralStageDefinition();

            string[] pieces = this.GWACommand.ListSplit(",");

            int counter = 1; // Skip identifier

            obj.StructuralId = pieces[counter++];
            obj.Name = pieces[counter++];

            counter++; //Skip colour

            //Member list
            obj.MemberRefs = pieces[counter++].Trim().Split(' ').Select(g => g.Replace("G", "")).ToList();
            counter++; //Skip creep coefficient
            var intString = pieces[counter++];
            try
            {
                var converted = int.TryParse(intString, out int stageDays);
                if (converted)
                {
                    obj.StageDays = stageDays;
                }
            }
            catch { }

            this.Value = obj;
        }

        public void SetGWACommand(GSAInterfacer GSA)
        {
            if (this.Value == null)
                return;

            StructuralStageDefinition stageDef = this.Value as StructuralStageDefinition;
            
            string keyword = typeof(GSAStageDefinition).GetGSAKeyword();
            var subkeywords = typeof(GSAStageDefinition).GetSubGSAKeyword();

            int index = GSA.Indexer.ResolveIndex(typeof(GSAStageDefinition), stageDef);
            int elemListIndex = GSA.Indexer.ResolveIndex(subkeywords[0]);

            //The object mentions members by their structural Ids.  The corresponding members need to be queried, and their group IDs collated
            var groupIds = new List<int>();
            foreach (var memberRef in stageDef.MemberRefs)
            {
                var index1d = GSA.Indexer.LookupIndex(typeof(GSA1DMember), memberRef);
                if (index1d == null)
                {
                    var index2d = GSA.Indexer.LookupIndex(typeof(GSA2DMember), memberRef);
                    if (index2d != null)
                    {
                        groupIds.Add((int)index2d);
                    }
                }
                else
                {
                    groupIds.Add((int)index1d);
                }

            }

            var stageName = string.IsNullOrEmpty(stageDef.Name) ? " " : stageDef.Name;
            var groupsStr = (groupIds.Count() > 0) ? string.Join(" ", groupIds.Select(i => ("G" + i.ToString()))) : "";

            //Create the list of elements first
            List<string> ls = new List<string>
            {
                // Set ANAL_STAGE
                "SET",
                subkeywords[0],
                elemListIndex.ToString(),
                stageName, // Name
                "ELEMENT", // Type
                groupsStr //Elements by group name
            };

            GSA.RunGWACommand(string.Join("\t", ls));

            ls = new List<string>
            {
                // Set ANAL_STAGE
                "SET",
                keyword,
                index.ToString(),
                stageName, // Name
                "NO_RGB", // Colour
                groupsStr, //Elements by group name
                "0", //Creep factor
                stageDef.StageDays.ToString() // Stage
            };

            GSA.RunGWACommand(string.Join("\t", ls));
        }
    }

    public static partial class Conversions
    {
        public static bool ToNative(this StructuralStageDefinition loadTask)
        {
            new GSALoadTaskBuckling() { Value = loadTask }.SetGWACommand(GSA);

            return true;
        }

        public static SpeckleObject ToSpeckle(this GSAStageDefinition dummyObject)
        {
            if (!GSASenderObjects.ContainsKey(typeof(GSAStageDefinition)))
                GSASenderObjects[typeof(GSAStageDefinition)] = new List<object>();

            List<GSAStageDefinition> stageDefs = new List<GSAStageDefinition>();

            string keyword = typeof(GSAStageDefinition).GetGSAKeyword();
            string[] subKeywords = typeof(GSAStageDefinition).GetSubGSAKeyword();

            string[] lines = GSA.GetGWARecords("GET_ALL," + keyword);
            List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL," + keyword).ToList();
            foreach (string k in subKeywords)
                deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL," + k));

            // Remove deleted lines
            GSASenderObjects[typeof(GSAStageDefinition)].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
            foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
                kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));
       
            // Filter only new lines
            string[] prevLines = GSASenderObjects[typeof(GSAStageDefinition)].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
            string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

            foreach (string p in newLines)
            {
                GSAStageDefinition combo = new GSAStageDefinition() { GWACommand = p };
                combo.ParseGWACommand(GSA);
                stageDefs.Add(combo);
            }

            GSASenderObjects[typeof(GSAStageDefinition)].AddRange(stageDefs);

            if (stageDefs.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

            return new SpeckleNull();
        }
    }
}
