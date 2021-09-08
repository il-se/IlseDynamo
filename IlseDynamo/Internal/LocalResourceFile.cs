using System;
using System.IO;

using Autodesk.DesignScript.Runtime;

namespace IlseDynamo.Internal
{
#pragma warning disable CS1591

    /// <summary>
    /// A local file resource base implementation.
    /// </summary>
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
        /// Returns the base folder name (if set) or the folder of file resource with an ensured separator slash at the end.
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
        /// Returns the local folder relative to the base path (if set) or empty string if the folder is the same as the <see cref="BaseFolder"/>.
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

        /// <summary>
        /// Replaces the name (without extension and folder).
        /// </summary>
        /// <param name="newName">A new name without extension</param>
        public virtual void SetName(string newName)
        {
            FileName = $"{Path.GetDirectoryName(FileName)}{Path.DirectorySeparatorChar}{newName}{Path.GetExtension(FileName)}";
        }
    }

#pragma warning restore CS1591
}
