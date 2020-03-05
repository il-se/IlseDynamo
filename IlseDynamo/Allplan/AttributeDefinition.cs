using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using Allplan.Data;

namespace Allplan
{
    /// <summary>
    /// Merge strategy of attribute collection merge request.
    /// </summary>
    public enum MergeStrategy
    {
        /// <summary>
        /// Throw an exception if definitions don't match. The merge node will get a warning.
        /// </summary>
        ThrowExceptionOnFirst,
        /// <summary>
        /// Try case invariant name tests before throwing an exception. On success left side is prefered.
        /// </summary>
        TryCaseInvariantPreferLeft,
        /// <summary>
        /// Try case invariant name tests before throwing an exception. On success right side is prefered.
        /// </summary>
        TryCaseInvariantPreferRight,
        /// <summary>
        /// Use left hand of merge by default but report conflicts.
        /// </summary>
        UseLeftSideByDefault,
        /// <summary>
        /// Use right hand of merge by default but report conflicts.
        /// </summary>
        UseRightSideByDefault,
    }

    /// <summary>
    /// Node collection for handling Allplan Attribute Definitions
    /// </summary>
    public class AttributeDefinition
    {
        #region Internals

        internal string FileName { get; set; }
        internal AttributeDefinitionCollection DefinitionCollection { get; set; }

        internal AttributeDefinition()
        {
        }

        internal AttributeDefinition NewWithSameRegion(IEnumerable<Data.AttributeDefinition> attributeDefinitions)
        {
            return new AttributeDefinition
            {
                DefinitionCollection = new AttributeDefinitionCollection
                {
                    Region = DefinitionCollection?.Region,
                    AttributeDefinition = attributeDefinitions.ToList()
                }
            };
        }

        #endregion

        /// <summary>
        /// Compares two definition collection against eachother and returns equal definitions
        /// and conflicts of left and right separatly.
        /// </summary>
        /// <param name="left">The left hand definition</param>
        /// <param name="right">The right hand definition</param>
        /// <returns></returns>
        [MultiReturn("equal", "leftConflicts", "rightConflicts", "onlyLeft", "onlyRight")]
        public static Dictionary<string, AttributeDefinition> Diff(AttributeDefinition left, AttributeDefinition right)
        {
            var leftIfNrMap = left.DefinitionCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);
            var rightIfNrMap = right.DefinitionCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);

            if (!EqualityComparer<string>.Default.Equals(left.DefinitionCollection.Region, right.DefinitionCollection.Region))
                throw new Exception($"Regions don't match ({left.DefinitionCollection.Region} != {right.DefinitionCollection.Region})");

            List<Data.AttributeDefinition> leftConflicts = new List<Data.AttributeDefinition>();
            List<Data.AttributeDefinition> rightConflicts = new List<Data.AttributeDefinition>();
            List<Data.AttributeDefinition> equals = new List<Data.AttributeDefinition>();
            List<Data.AttributeDefinition> onlyLeft = new List<Data.AttributeDefinition>();
            foreach (var leftIfNr in leftIfNrMap.Keys)
            {
                if (!rightIfNrMap.ContainsKey(leftIfNr))
                {
                    onlyLeft.Add(leftIfNrMap[leftIfNr]);                    
                } 
                else if (!leftIfNrMap[leftIfNr].Equals(rightIfNrMap[leftIfNr]))
                {
                    leftConflicts.Add(leftIfNrMap[leftIfNr]);
                    rightConflicts.Add(rightIfNrMap[leftIfNr]);
                }
                else
                {
                    equals.Add(leftIfNrMap[leftIfNr]);
                }
            }

            return new Dictionary<string, AttributeDefinition>()
            {
                { "equal", left.NewWithSameRegion(equals) },
                { "leftConflicts", left.NewWithSameRegion(leftConflicts) },
                { "rightConflicts", right.NewWithSameRegion(rightConflicts) },
                { "onlyLeft", left.NewWithSameRegion(onlyLeft) },
                { "onlyRight", right.NewWithSameRegion(rightIfNrMap.Where(r => !leftIfNrMap.ContainsKey(r.Key)).Select(r => r.Value)) }
            };
        }

        /// <summary>
        /// Runs a merge on two given collections using a given strategy.
        /// </summary>
        /// <param name="left">The left hand</param>
        /// <param name="right">The right hand</param>
        /// <param name="mergeStrategy">The strategy</param>
        /// <returns></returns>
        [MultiReturn("attributeDefintion", "report")]
        public static Dictionary<string, object> Merge(AttributeDefinition left, AttributeDefinition right, MergeStrategy mergeStrategy)
        {
            var leftIfNrMap = left.DefinitionCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);
            var rightIfNrMap = right.DefinitionCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);

            var finalMerge = leftIfNrMap
                .Where(a => !rightIfNrMap.ContainsKey(a.Key))
                .Concat(rightIfNrMap.Where(a => !leftIfNrMap.ContainsKey(a.Key)))
                .Select(a => a.Value)
                .ToList();

            var report = new List<string[]>();

            foreach (var ifnr in leftIfNrMap.Keys.Where(k => rightIfNrMap.ContainsKey(k)))
            {
                var la = leftIfNrMap[ifnr];
                var ra = rightIfNrMap[ifnr];
                if (!la.Equals(ra))
                {
                    switch(mergeStrategy)
                    {
                        case MergeStrategy.ThrowExceptionOnFirst:
                            throw new Exception($"IfNr #{ifnr} causes conflicts (left name '{la.Text}' != '{ra.Text}'");
                        case MergeStrategy.TryCaseInvariantPreferRight:
                        case MergeStrategy.TryCaseInvariantPreferLeft:
                            if (string.Equals(la.Text, ra.Text, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (mergeStrategy == MergeStrategy.TryCaseInvariantPreferLeft)
                                    finalMerge.Add(la);
                                else
                                    finalMerge.Add(ra);

                                report.Add(new string[] { $"{la.Ifnr}", $"Case invariant match '{la.Text}' vs. '{ra.Text}'" });
                            }
                            else
                                throw new Exception($"IfNr #{ifnr} causes conflicts (left name '{la.Text}' != '{ra.Text}'");
                            break;
                        case MergeStrategy.UseLeftSideByDefault:
                            finalMerge.Add(la);
                            report.Add(new string[] { $"{la.Ifnr}", $"Using left: '{la.Text}' vs. '{ra.Text}'" });
                            break;
                        case MergeStrategy.UseRightSideByDefault:
                            finalMerge.Add(ra);
                            report.Add(new string[] { $"{la.Ifnr}", $"Using right: '{la.Text}' vs. '{ra.Text}'" });
                            break;
                    }
                }
            }

            return new Dictionary<string, object>()
            {
                { "attributeDefintion", left.NewWithSameRegion(finalMerge) },
                { "report", report.ToArray() }
            };
        }

        /// <summary>
        /// Reads a new attribute definition by file name.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>An attribute definition</returns>
        public static AttributeDefinition ByFileName(string fileName)
        {
            return new AttributeDefinition
            {
                FileName = fileName,
                DefinitionCollection = AttributeDefinitionCollection.ReadFrom(fileName)
            };
        }

        /// <summary>
        /// Splits the definition into single attribute data groups.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("ifnr", "name", "datatype", "parentGroup")]
        public Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>() 
            {
                { "ifnr", DefinitionCollection.AttributeDefinition.Select(a => a.Ifnr).ToArray() },
                { "name", DefinitionCollection.AttributeDefinition.Select(a => a.Text).ToArray() },
                { "datatype", DefinitionCollection.AttributeDefinition.Select(a => a.Datatype).ToArray() },
                { "parentGroup", DefinitionCollection.AttributeDefinition.Select(a => a.ParentUserDirCode).ToArray() },
            };
        }
    }
}
