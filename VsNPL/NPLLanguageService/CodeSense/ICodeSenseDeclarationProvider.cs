using System.Collections.Generic;


namespace ParaEngine.Tools.Lua
{
    public interface ICodeSenseDeclarationProvider
    {
        /// <summary>
        /// Get declarations for a CompleteWord request.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Declaration> GetCompleteWordDeclarations();

        /// <summary>
        /// Get declarations for a MemberSelect request for the given qualified name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the type.</param>
        /// <returns></returns>
        IEnumerable<Declaration> GetMemberSelectDeclarations(string qualifiedName);

        /// <summary>
        /// Gets the methods for a MethodTip request with the given qualified name.
        /// </summary>
        /// <param name="qualifiedName"></param>
        /// <returns></returns>
        IEnumerable<Method> GetMethods(string qualifiedName);
    }
}
