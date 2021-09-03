using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using IlseDynamo.Data.Allplan;

namespace IlseDynamo.Allplan
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
    /// Allplan attribute definitions.
    /// </summary>
    public class Attributes
    {
        #region Internals

        internal string FileName { get; set; }
        internal AttributeDefinitionCollection AttributeCollection { get; set; }

        internal Attributes()
        {
        }

        internal Attributes NewWithSameRegion(IEnumerable<AttributeDefinition> attributeDefinitions)
        {
            return new Attributes
            {
                AttributeCollection = new AttributeDefinitionCollection
                {
                    Region = AttributeCollection?.Region,
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
        public static Dictionary<string, Attributes> Diff(Attributes left, Attributes right)
        {
            var leftIfNrMap = left.AttributeCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);
            var rightIfNrMap = right.AttributeCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);

            if (!EqualityComparer<string>.Default.Equals(left.AttributeCollection.Region, right.AttributeCollection.Region))
                throw new Exception($"Regions don't match ({left.AttributeCollection.Region} != {right.AttributeCollection.Region})");

            List<AttributeDefinition> leftConflicts = new List<AttributeDefinition>();
            List<AttributeDefinition> rightConflicts = new List<AttributeDefinition>();
            List<AttributeDefinition> equals = new List<AttributeDefinition>();
            List<AttributeDefinition> onlyLeft = new List<AttributeDefinition>();
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

            return new Dictionary<string, Attributes>()
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
        public static Dictionary<string, object> Merge(Attributes left, Attributes right, MergeStrategy mergeStrategy)
        {
            var leftByIfNR = left
                .AttributeCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);
            var rightByIfNR = right
                .AttributeCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);
            // Sequence of definitons without duplicate IfNR keys
            var lrUniqueIfNR = leftByIfNR
                .Where(l => !rightByIfNR.ContainsKey(l.Key))
                .Concat(rightByIfNR.Where(r => !leftByIfNR.ContainsKey(r.Key)))
                .Select(a => a.Value)                
                .ToList();

            var report = new List<string[]>();

            foreach (var ifnr in leftByIfNR.Keys.Where(ifnr => rightByIfNR.ContainsKey(ifnr)))
            {
                var leftAttribute = leftByIfNR[ifnr];
                var rightAttribute = rightByIfNR[ifnr];

                if (leftAttribute.Equals(rightAttribute))
                {
                    switch (mergeStrategy)
                    {
                        case MergeStrategy.ThrowExceptionOnFirst:
                        case MergeStrategy.TryCaseInvariantPreferLeft:
                        case MergeStrategy.UseLeftSideByDefault:
                            lrUniqueIfNR.Add(leftAttribute);
                            break;
                        case MergeStrategy.TryCaseInvariantPreferRight:
                        case MergeStrategy.UseRightSideByDefault:
                            lrUniqueIfNR.Add(rightAttribute);
                            break;
                    }
                }
                else
                {
                    switch(mergeStrategy)
                    {
                        case MergeStrategy.ThrowExceptionOnFirst:
                            throw new Exception($"IfNr #{ifnr} causes conflicts (left name '{leftAttribute.Text}' != '{rightAttribute.Text}'");
                        case MergeStrategy.TryCaseInvariantPreferRight:
                        case MergeStrategy.TryCaseInvariantPreferLeft:
                            if (string.Equals(leftAttribute.Text, rightAttribute.Text, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (mergeStrategy == MergeStrategy.TryCaseInvariantPreferLeft)
                                    lrUniqueIfNR.Add(leftAttribute);
                                else
                                    lrUniqueIfNR.Add(rightAttribute);

                                report.Add(new string[] { $"{leftAttribute.Ifnr}", $"Case invariant match '{leftAttribute.Text}' vs. '{rightAttribute.Text}'" });
                            }
                            else
                                throw new Exception($"IfNr #{ifnr} causes conflicts (left name '{leftAttribute.Text}' != '{rightAttribute.Text}'");
                            break;
                        case MergeStrategy.UseLeftSideByDefault:
                            lrUniqueIfNR.Add(leftAttribute);
                            report.Add(new string[] { $"{leftAttribute.Ifnr}", $"Using left: '{leftAttribute.Text}' vs. '{rightAttribute.Text}'" });
                            break;
                        case MergeStrategy.UseRightSideByDefault:
                            lrUniqueIfNR.Add(rightAttribute);
                            report.Add(new string[] { $"{leftAttribute.Ifnr}", $"Using right: '{leftAttribute.Text}' vs. '{rightAttribute.Text}'" });
                            break;
                    }
                }
            }

            return new Dictionary<string, object>()
            {
                { "attributeDefintion", new Attributes
                    {
                        AttributeCollection = new AttributeDefinitionCollection
                        {
                            Region = left.AttributeCollection.Region,
                            AttributeDefinition = lrUniqueIfNR
                        }
                    }
                },
                { "report", report.ToArray() }
            };
        }

        /// <summary>
        /// Reads a new attribute definition by file name.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>An attribute definition</returns>
        public static Attributes ByFileName(string fileName)
        {
            return new Attributes
            {
                FileName = fileName,
                AttributeCollection = AttributeDefinitionCollection.ReadFrom(fileName)
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
                { "ifnr", AttributeCollection.AttributeDefinition.Select(a => a.Ifnr).ToArray() },
                { "name", AttributeCollection.AttributeDefinition.Select(a => a.Text).ToArray() },
                { "datatype", AttributeCollection.AttributeDefinition.Select(a => a.Datatype).ToArray() },
                { "parentGroup", AttributeCollection.AttributeDefinition.Select(a => a.ParentUserDirCode).ToArray() },
            };
        }

        /// <summary>
        /// Runs a validation on attribute definitions. Checks for unique names.
        /// </summary>
        /// <param name="ignoreCase">Whether being case sensitive</param>
        /// <param name="trimStart">Trim whitespaces and separators at start</param>
        /// <param name="trimEnd">Trim whitespaces and separators at end</param>
        /// <returns></returns>
        [MultiReturn("attributeName", "ifnr")]
        public Dictionary<string, object> Validate(bool ignoreCase, bool trimStart = true, bool trimEnd = true)        
        {
            Func<AttributeDefinition, string> nameUnifier = (d) =>
            {
                var key = ignoreCase ? d.Text.ToUpperInvariant() : d.Text;
                if (trimStart)
                    key = key.TrimStart(' ', '-', '_');
                if (trimEnd)
                    key = key.TrimEnd(' ', '-', '_');
                return key;
            };

            var duplicates = AttributeCollection.AttributeDefinition
                .ToLookup(nameUnifier)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(d => new Tuple<string, long>(d.Text, d.Ifnr)));
            return new Dictionary<string, object>()
            {
                { "attributeName", duplicates.Select(t => t.Item1).ToArray() },
                { "ifnr", duplicates.Select(t => t.Item2).ToArray() },
            };
        }

        /// <summary>
        /// Generates a custom string representation.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Path.GetFileName(FileName)} ({AttributeCollection.Region}, {AttributeCollection.AttributeDefinition.Count} attributes)";
        }
    }
}
