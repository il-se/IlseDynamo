using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using IlseDynamo.Allplan;
using IlseDynamo.Data.Revit;

namespace IlseDynamo.Revit
{
    public class RevitSharedParameters
    {
        #region Internals

        internal string FileName { get; set; }

        internal string BasePathName { get; set; }

        internal RevitSharedParameter[] Parameter { get; set; }

        internal RevitSharedParameters()
        { }

        #endregion

        public string FavouriteName() => FileName;

        public Dictionary<string, RevitSharedParameters> SplitByGroup()
        {
            return Parameter
                .GroupBy(p => p.Group.Name)
                .ToDictionary(g => g.Key, g => new RevitSharedParameters { FileName = FileName, Parameter = g.ToArray() });
        }

        public string[] ToNames()
        {
            return Parameter.Select(p => p.Name).ToArray();
        }

        public Guid[] ToGuids()
        {
            return Parameter.Where(p => p.DefinitionId.HasValue).Select(p => p.DefinitionId.Value).ToArray();
        }

        public static RevitSharedParameters ByFile(string filePathName)
        {
            return new RevitSharedParameters 
            { 
                FileName = Path.GetFileNameWithoutExtension(filePathName),
                Parameter = RevitSharedParameter.FromTextFile(filePathName).ToArray() 
            };
        }

    }
}
