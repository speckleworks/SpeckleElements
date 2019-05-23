﻿using System;
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
    [GSAObject("LOAD_TITLE.2", new string[] { }, "loads", true, true, new Type[] { }, new Type[] { })]
    public class GSALoadCase : IGSASpeckleContainer
    {
        public string GWACommand { get; set; }
        public List<string> SubGWACommand { get; set; } = new List<string>();
        public dynamic Value { get; set; } = new StructuralLoadCase();

        public void ParseGWACommand(GSAInterfacer GSA)
        {
            if (this.GWACommand == null)
                return;

            StructuralLoadCase obj = new StructuralLoadCase();
            
            string[] pieces = this.GWACommand.ListSplit(",");

            int counter = 1; // Skip identifier

            obj.StructuralId = pieces[counter++];
            obj.Name = pieces[counter++];

            string type = pieces[counter++];
            switch (type)
            {
                case "DEAD":
                    obj.CaseType = StructuralLoadCaseType.Dead;
                    break;
                case "LC_VAR_IMP":
                    obj.CaseType = StructuralLoadCaseType.Live;
                    break;
                case "WIND":
                    obj.CaseType = StructuralLoadCaseType.Wind;
                    break;
                case "SNOW":
                    obj.CaseType = StructuralLoadCaseType.Snow;
                    break;
                case "SEISMIC":
                    obj.CaseType = StructuralLoadCaseType.Earthquake;
                    break;
                case "LC_PERM_SOIL":
                    obj.CaseType = StructuralLoadCaseType.Soil;
                    break;
                default:
                    obj.CaseType = StructuralLoadCaseType.Generic;
                    break;
            }

            // Rest is unimportant

            this.Value = obj;
        }

        public void SetGWACommand(GSAInterfacer GSA)
        {
            if (this.Value == null)
                return;

            StructuralLoadCase loadCase = this.Value as StructuralLoadCase;
            
            string keyword = typeof(GSALoadCase).GetGSAKeyword();

            int index = GSA.Indexer.ResolveIndex(typeof(GSALoadCase), loadCase);

            List<string> ls = new List<string>();

            ls.Add("SET");
            ls.Add(keyword + ":" + GSA.GenerateSID(loadCase));
            ls.Add(index.ToString());
            ls.Add(loadCase.Name == null || loadCase.Name == "" ? " " : loadCase.Name);
            switch (loadCase.CaseType)
            {
                case StructuralLoadCaseType.Dead:
                    ls.Add("DEAD");
                    break;
                case StructuralLoadCaseType.Live:
                    ls.Add("LC_VAR_IMP");
                    break;
                case StructuralLoadCaseType.Wind:
                    ls.Add("WIND");
                    break;
                case StructuralLoadCaseType.Snow:
                    ls.Add("SNOW");
                    break;
                case StructuralLoadCaseType.Earthquake:
                    ls.Add("SEISMIC");
                    break;
                case StructuralLoadCaseType.Soil:
                    ls.Add("LC_PERM_SOIL");
                    break;
                default:
                    ls.Add("UNDEF");
                    break;
            }
            ls.Add("1"); // Source
            ls.Add("~"); // Category
            ls.Add("NONE"); // Direction
            ls.Add("INC_BOTH"); // Include

            GSA.RunGWACommand(string.Join("\t", ls));
        }
    }

    public static partial class Conversions
    {
        public static bool ToNative(this StructuralLoadCase load)
        {
            new GSALoadCase() { Value = load }.SetGWACommand(GSA);

            return true;
        }

        public static SpeckleObject ToSpeckle(this GSALoadCase dummyObject)
        {
            if (!GSASenderObjects.ContainsKey(typeof(GSALoadCase)))
                GSASenderObjects[typeof(GSALoadCase)] = new List<object>();

            List<GSALoadCase> loadCases = new List<GSALoadCase>();

            string keyword = typeof(GSALoadCase).GetGSAKeyword();
            string[] subKeywords = typeof(GSALoadCase).GetSubGSAKeyword();

            string[] lines = GSA.GetGWARecords("GET_ALL," + keyword);
            List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL," + keyword).ToList();
            foreach (string k in subKeywords)
                deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL," + k));

            // Remove deleted lines
            GSASenderObjects[typeof(GSALoadCase)].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
            foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
                kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

            // Filter only new lines
            string[] prevLines = GSASenderObjects[typeof(GSALoadCase)].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
            string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

            foreach (string p in newLines)
            {
                GSALoadCase loadCase = new GSALoadCase() { GWACommand = p };
                loadCase.ParseGWACommand(GSA);
                loadCases.Add(loadCase);
            }

            GSASenderObjects[typeof(GSALoadCase)].AddRange(loadCases);

            if (loadCases.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

            return new SpeckleNull();
        }
    }
}
