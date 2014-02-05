using System.Runtime.InteropServices;

namespace ParaEngine.Tools.Services
{
    [Guid("9C7E3398-6BAC-4dcd-AB37-EF4817B309AD")]
    public interface ILuaLanguageService
    {
        /// <summary>
        /// Adds a FrameXML file to the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void AddFrameXmlFile(string path);

        /// <summary>
        /// Removes a FrameXML file from the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void RemoveFrameXmlFile(string path);

        /// <summary>
        /// Adds a Lua file to the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void AddLuaFile(string path);

        /// <summary>
        /// Removes a Lua file from the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void RemoveLuaFile(string path);

        /// <summary>
        /// Clears all files from the list of files to be parsed.
        /// </summary>
        void Clear();
    }
}
