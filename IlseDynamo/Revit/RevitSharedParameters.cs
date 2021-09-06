using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using IlseDynamo.Internal;
using IlseDynamo.Data.Revit;

namespace IlseDynamo.Revit
{
    /// <summary>
    /// Revit Shared Parameter File Resource.
    /// </summary>
    public class RevitSharedParameters : LocalResourceFile
    {
        #region Internals

        internal RevitSharedParameter[] Parameter { get; set; }

        internal RevitSharedParameters()
        { }

        #endregion

        /// <inheritdoc/>
        public new string Folder { get => base.Folder; }
        /// <inheritdoc/>
        public new string Name { get => base.Name; }
        /// <inheritdoc/>
        public new string BaseFolder { get => base.BaseFolder; }

        /// <summary>
        /// Decomposes the parameter resource by group names.
        /// </summary>
        /// <returns>Dictionary of group names vs. decomposed parameters</returns>
        public Dictionary<string, RevitSharedParameters> DecomposeByGroup()
        {
            return Parameter
                .GroupBy(p => p.Group.Name)
                .ToDictionary(g => g.Key, g => new RevitSharedParameters 
                { 
                    FileName = Path.Combine(
                        Path.GetDirectoryName(FileName), $"{Path.GetFileNameWithoutExtension(FileName)}-{g.Key}{Path.GetExtension(FileName)}"),
                    BasePath = BasePath, 
                    Parameter = g.ToArray() 
                });
        }

        /// <summary>
        /// Extracts the parameter names.
        /// </summary>
        /// <returns>An array of names</returns>
        public string[] ToNames()
        {
            return Parameter.Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Extracts the parameter GUIDs.
        /// </summary>
        /// <returns>An array of paranmeter GUIDs.</returns>
        public Guid[] ToGuids()
        {
            return Parameter.Where(p => p.DefinitionId.HasValue).Select(p => p.DefinitionId.Value).ToArray();
        }

        /// <summary>
        /// Reads a Revit Shared Parameter file as local resource.
        /// </summary>
        /// <param name="filePathName">The file path.</param>
        /// <param name="basePath">An optional base path as relative base folder.</param>
        /// <returns>A Revit shared parameter resource</returns>
        public static RevitSharedParameters ByFile(string filePathName, string basePath = null)
        {
            return new RevitSharedParameters 
            { 
                FileName = filePathName,
                BasePath = basePath,
                Parameter = RevitSharedParameter.FromTextFile(filePathName).ToArray() 
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} ({Parameter.Length})";
        }
    }
}
