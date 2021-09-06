using System;
using System.IO;

using Autodesk.DesignScript.Runtime;

namespace IlseDynamo.Internal
{
#pragma warning disable CS1591

    [IsVisibleInDynamoLibrary(false)]
    public abstract class LocalResourceFile
    {
        /// <summary>
        /// Full file path to local resource file.
        /// </summary>
        protected internal string FileName { get; set; }

        /// <summary>
        /// Relative path base or null.
        /// </summary>
        protected internal string BasePath { get; set; }

        /// <summary>
        /// Gets the file name (without extension and path).
        /// </summary>        
        public virtual string Name
        {
            get => Path.GetFileNameWithoutExtension(FileName);
        }

        /// <summary>
        /// Returns the base folder name (if set).
        /// </summary>
        public virtual string BaseFolder
        {
            get
            {
                var baseFolder = BasePath?.Trim() ?? Path.GetDirectoryName(FileName);
                if (!baseFolder.EndsWith($"{Path.DirectorySeparatorChar}"))
                    return $"{baseFolder}{Path.DirectorySeparatorChar}";
                else
                    return $"{baseFolder}";
            }
        }

        /// <summary>
        /// Returns the local folder relative to the base path.
        /// </summary>
        public virtual string Folder
        {
            get
            {
                var folder = $"{Path.GetDirectoryName(FileName)}{Path.DirectorySeparatorChar}".RelativePathOf(BaseFolder);
                if (string.IsNullOrEmpty(folder) || folder.EndsWith($"{Path.DirectorySeparatorChar}"))
                    return folder.Trim();
                else
                    return $"{folder}{Path.DirectorySeparatorChar}";
            }
        }
    }

#pragma warning restore CS1591
}
