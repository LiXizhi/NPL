using System.Collections.Generic;


namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Provides static declarations for Lua keywords.
    /// </summary>
    public class KeywordDeclarationProvider : ICodeSenseDeclarationProvider
    {
        private static readonly Declaration[] declarations = CreateKeywordDeclarations();

        private static Declaration[] CreateKeywordDeclarations()
        {
            return new[] 
                      {
                          CreateKeywordDeclaration("and"),
                          CreateKeywordDeclaration("break"),
                          CreateKeywordDeclaration("do"),
                          CreateKeywordDeclaration("end"),
                          CreateKeywordDeclaration("else"),
//                        CreateKeywordDeclaration("elseif"),
//                        CreateKeywordDeclaration("for"),
                          CreateKeywordDeclaration("false"),
//                        CreateKeywordDeclaration("function"),
//                        CreateKeywordDeclaration("if"),
                          CreateKeywordDeclaration("in"),
                          CreateKeywordDeclaration("local"),
                          CreateKeywordDeclaration("nil"),
                          CreateKeywordDeclaration("not"),
                          CreateKeywordDeclaration("or"),
                          CreateKeywordDeclaration("repeat"),
                          CreateKeywordDeclaration("return"),
                          CreateKeywordDeclaration("self"),
                          CreateKeywordDeclaration("then"),
                          CreateKeywordDeclaration("this"),
                          CreateKeywordDeclaration("true"),
                          CreateKeywordDeclaration("until"),
                          CreateKeywordDeclaration("while")
                      };
        }
        
        /// <summary>
        /// Gets the declarations for a CompleteWord request for the given scope.
        /// </summary>
        /// <returns>
        /// An enumeration of <see cref="Declaration"/> instances.
        /// </returns>
        public IEnumerable<Declaration> GetCompleteWordDeclarations()
        {
            return declarations;
        }

        /// <summary>
        /// Gets the declarations for a MemberSelect request.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the variable to select members.</param>
        /// <returns>
        /// An enumeration of <see cref="Declaration"/> instances.
        /// </returns>
        public IEnumerable<Declaration> GetMemberSelectDeclarations(string qualifiedName)
        {
            yield break;
        }

        /// <summary>
        /// Gets the methods for a MethodTip request with the given qualified name.
        /// </summary>
        /// <param name="qualifiedName"></param>
        /// <returns></returns>
        public IEnumerable<Method> GetMethods(string qualifiedName)
        {
            yield break;
        }

		/// <summary>
		/// Creates the keyword declaration.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
        private static Declaration CreateKeywordDeclaration(string name)
        {
            return new Declaration
                    {
                        DeclarationType = DeclarationType.Keyword,
                        Name = name
                    };
        }
    }
}