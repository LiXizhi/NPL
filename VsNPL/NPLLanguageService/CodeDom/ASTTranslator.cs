using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom
{
	/// <summary>
	/// Translates AST code model to Lua FileCodeModel.
	/// </summary>
	public sealed class ASTTranslator
	{
		private readonly DTE dte;
		private LuaCodeClass rootElement;
		private LuaCodeFunction rootClass;
		private readonly CodeElement parent;
		private bool isInStatement;

		/// <summary>
		/// Initializes a new instance of the <see cref="ASTTranslator"/> class.
		/// </summary>
		/// <param name="dte"></param>
		/// <param name="parent"></param>
		public ASTTranslator(DTE dte, CodeElement parent)
		{
			this.dte = dte;
			this.parent = parent;
		}


		/// <summary>
		/// Translates Chunk node to LuaCodeClass.
		/// </summary>
		/// <param name="chunk">Instance of Chunk.</param>
		/// <param name="name">Class name.</param>
		/// <returns></returns>
		public LuaCodeClass Translate(Chunk chunk, string name)
		{
			rootElement = new LuaCodeClass(dte, parent, name, chunk);
			rootClass = CreateVirtualFunction(rootElement);
			AddElementToParent(rootElement, rootClass);

			try
			{
				if (chunk.Block != null)
					foreach (Block member in chunk.Block)
					{
						if (member.StatementList != null)
							foreach (Node node in member.StatementList)
							{
								RecurseNode(node);
							}
					}
				//TraceDump(rootElement);
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex);
				dte.StatusBar.Text = "An error has occured during parsing source.";
				dte.StatusBar.Highlight(true);
			}
			return rootElement;
		}

		/// <summary>
		/// Translates Chunk node to LuaCodeClass with random name.
		/// </summary>
		/// <param name="chunk">Instance of Chunk.</param>
		/// <returns></returns>
		public LuaCodeClass Translate(Chunk chunk)
		{
			return Translate(chunk, String.Concat("LuaCodeClass::", Guid.NewGuid()));
		}


		/// <summary>
		/// Creates default virtual entry function in LuaCodeClass.
		/// </summary>
		/// <returns></returns>
		private LuaCodeFunction CreateVirtualFunction(CodeElement parentElement)
		{
			var rootFunction = LuaCodeElementFactory.CreateLuaCodeFunction(
				dte, parentElement, LuaFileCodeModel.ENTRYFUNCTION_NAME, LuaType.Nil, vsCMAccess.vsCMAccessPrivate,
				vsCMFunction.vsCMFunctionTopLevel, new FunctionDeclaration(new LexLocation()));
			return rootFunction;
		}

		/// <summary>
		/// Recurse top-level nodes in node.
		/// </summary>
		/// <param name="node">Instance of Node.</param>
		private void RecurseNode(Node node)
		{
			if (node is Assignment)
			{
				var variables = RecurseAssignmentNode(rootElement, node as Assignment);
				foreach (LuaCodeVariable variable in variables)
				{
					rootElement.AddVariable(variable);
				}
			}
			else if (node is FunctionDeclaration)
			{
				RecurseFunctionDeclarationNode(node as FunctionDeclaration);
			}
			else
			{
				RecurseStatement(rootClass, node);
			}
		}

		/// <summary>
		/// Recurse and translate in Assignment Node.
		/// </summary>
		/// <param name="assignmentNode">Instance of Assignment Node.</param>
		/// <param name="parentElement">Parent of CodeElement.</param>
		private IEnumerable<LuaCodeVariable> RecurseAssignmentNode(CodeElement parentElement, Assignment assignmentNode)
		{
			var variables = new List<LuaCodeVariable>();

			if (assignmentNode.ExpressionList == null)
			{
				assignmentNode.VariableList.ForEach(
					child =>
						{
							if (child is Identifier)
							{
								var identifier = (Identifier) child;
								var element = LuaCodeElementFactory.CreateLuaCodeElement<Identifier>
									(dte, identifier.Name, parentElement, identifier);
								AddElementToParent(parentElement, element);
							}
							else
							{
								Trace.WriteLine("ERROR IN VariableList. " + child);
							}
						});
			}
			else
			{
				var enumerator = new ParallelEnumerator<Node, Node>
					(assignmentNode.VariableList, assignmentNode.ExpressionList);

				while (enumerator.MoveNext())
				{
					if (enumerator.CurrentFirst is Identifier)
					{
						var identifier = (Identifier) enumerator.CurrentFirst;

						if (enumerator.CurrentSecond is FunctionCall)
						{
							LuaCodeVariable variable = LuaCodeElementFactory.CreateVariable
								(dte, parentElement, identifier.Name, LuaType.Unknown, assignmentNode.IsLocal,
								 new Variable(PrepareLocation(identifier.Location, enumerator.CurrentSecond.Location)) {Identifier = identifier});
							var functionCall = enumerator.CurrentSecond as FunctionCall;
							RecurseStatement(variable, functionCall);
							variable.InitExpression = variable.Children.Item(1);
							variable.Type = new LuaCodeTypeRef(dte, LuaDeclaredType.Unknown);
							variables.Add(variable);
						}
						else if (enumerator.CurrentSecond is TableConstructor)
						{
							var constructor = enumerator.CurrentSecond as TableConstructor;
							var variable = CreateTable(constructor, parentElement, identifier, assignmentNode.IsLocal);
							variables.Add(variable);
						}
						else if (enumerator.CurrentSecond is Literal)
						{
							var literal = enumerator.CurrentSecond as Literal;
							var variable = LuaCodeElementFactory.CreateVariable
								(dte, parentElement, identifier.Name, literal.Type, assignmentNode.IsLocal,
								 new Variable(PrepareLocation(identifier.Location, enumerator.CurrentSecond.Location)) {Identifier = identifier});
							variable.InitExpression = literal.Value;
							variable.Type = new LuaCodeTypeRef(dte, LuaDeclaredType.Find(literal.Type.ToString()));
							variables.Add(variable);
						}
						else if (enumerator.CurrentSecond is Variable)
						{
							var variableNode = enumerator.CurrentSecond as Variable;
							var variable = LuaCodeElementFactory.CreateVariable
								(dte, parentElement, identifier.Name, LuaType.Table, assignmentNode.IsLocal,
								 new Variable(PrepareLocation(identifier.Location, enumerator.CurrentSecond.Location)) {Identifier = identifier});

							//variable.GetChildNodes().ForEach(child => RecurseStatement(luaVariable, child));
							RecurseStatement(variable, variableNode.Identifier);
							RecurseStatement(variable, variableNode.PrefixExpression);
							RecurseStatement(variable, variableNode.Expression);
							variables.Add(variable);
						}
						else if (enumerator.CurrentSecond is Function)
						{
							var function = enumerator.CurrentSecond as Function;
							var variable = LuaCodeElementFactory.CreateVariable
								(dte, parentElement, identifier.Name, LuaType.Unknown, assignmentNode.IsLocal,
								 new Variable(PrepareLocation(identifier.Location, enumerator.CurrentSecond.Location)) {Identifier = identifier});
							RecurseFunctionNode(function, variable);
							variables.Add(variable);
						}
						else
						{
							var variable = LuaCodeElementFactory.CreateVariable
								(dte, parentElement, identifier.Name, LuaType.Unknown, assignmentNode.IsLocal,
								 new Variable(PrepareLocation(identifier.Location, enumerator.CurrentSecond.Location)) {Identifier = identifier});
							RecurseStatement(variable, enumerator.CurrentSecond);
							variables.Add(variable);
							//Trace.WriteLine("ERROR IN ASSIGNMENT. " + enumerator.CurrentSecond);
						}
					}
					else if (enumerator.CurrentFirst is Variable)
					{
						var variableNode = enumerator.CurrentFirst as Variable;
						string variableName = String.Empty;
						if (variableNode.PrefixExpression is Identifier)
						{
							variableName = variableNode.PrefixExpression == null
							               	? String.Empty
							               	: ((Identifier) variableNode.PrefixExpression).Name;
						}
						var variable = LuaCodeElementFactory.CreateVariable
							(dte, parentElement, variableName, LuaType.Table, assignmentNode.IsLocal, variableNode);
						RecurseStatement(variable, variableNode.Identifier);
						RecurseStatement(variable, variableNode.PrefixExpression);
						RecurseStatement(variable, variableNode.Expression);
						if (enumerator.CurrentSecond is Function)
						{
							RecurseFunctionNode(enumerator.CurrentSecond as Function, variable);
						}
						else
						{
							RecurseStatement(variable, enumerator.CurrentSecond);
						}
						variables.Add(variable);
					}
					else
					{
						Trace.WriteLine("ERROR IN FIRST ASSIGNMENT. " + enumerator.CurrentFirst);
					}
				}
			}
			return variables;
		}

		/// <summary>
		/// Hack - workaround for AST parser bug. LexLocation.eCol of qualfied name is incorrect.
		/// </summary>
		/// <param name="leftLocation"></param>
		/// <param name="rightLocation"></param>
		/// <returns></returns>
		private static LexLocation PrepareLocation(LexLocation leftLocation, LexLocation rightLocation)
		{
			int endcol = leftLocation.eCol;
			int endline = leftLocation.eLin;
			if (rightLocation.sCol > leftLocation.eCol) endcol = rightLocation.sCol - 1;
			//Trace.WriteLine(String.Format("PrepareLocation: Original:[el={0}, ec={1}], New:[el={2}, ec={3}]", leftLocation.eLin, leftLocation.eCol, endline, endcol));
			return new LexLocation(leftLocation.sLin, leftLocation.sCol, endline, endcol);
		}

		/// <summary>
		/// Creates LuaCodeVariableTable from TableConstructor node.
		/// </summary>
		/// <param name="constructor">TableConstructor node instance.</param>
		/// <param name="parentElement">Parent of LuaCodeVariable element.</param>
		/// <param name="name">Name of table.</param>
		/// <param name="isLocal">Indicates that element is declared locally.</param>
		/// <returns></returns>
		private LuaCodeVariable CreateTable(TableConstructor constructor, CodeElement parentElement,
		                                    string name, bool isLocal)
		{
			var variable = LuaCodeElementFactory.CreateLuaCodeVariableTable
				(dte, parentElement, name, LuaType.Table, isLocal, constructor);
			if (constructor.FieldList != null)
			{
				foreach (Field field in constructor.FieldList)
				{
					RecurseFieldInTable(variable as LuaCodeVariableTable, field);
				}
			}
			return variable;
		}

		/// <summary>
		/// Creates LuaCodeVariableTable from TableConstructor node.
		/// </summary>
		/// <param name="constructor">TableConstructor node instance.</param>
		/// <param name="parentElement">Parent of LuaCodeVariable element.</param>
		/// <param name="identifier">TableConstructor identifier.</param>
		/// <param name="isLocal">Indicates that element is declared locally.</param>
		/// <returns></returns>
		private LuaCodeVariable CreateTable(TableConstructor constructor, CodeElement parentElement,
		                                    Identifier identifier, bool isLocal)
		{
			var variable = (LuaCodeVariableTable) LuaCodeElementFactory.CreateLuaCodeVariableTable
			                                      	(dte, parentElement, identifier.Name,
			                                      	 LuaType.Table, isLocal, constructor);
			variable.IdentifierLocation = PrepareLocation(identifier.Location, constructor.Location);
			variable.IdentifierLocation.eCol = variable.IdentifierLocation.sCol + identifier.Name.Length;

			//var location = variable.IdentifierLocation;
			//Trace.WriteLine(String.Format("LuaCodeVariableTable Location: StartCol-{0} StartLine-{1} EndCol-{2} EndLine-{3}", location.sCol, location.sLin, location.eCol, location.eLin));
			if (constructor.FieldList != null)
			{
				foreach (Field field in constructor.FieldList)
				{
					RecurseFieldInTable(variable, field);
				}
			}
			return variable;
		}

		/// <summary>
		/// Creates variables for fileds in a specified LuaCodeVariableTable.
		/// </summary>
		/// <param name="field">Field enumeration.</param>
		/// <param name="parentElement">LuaCodeVariableTable as parent element.</param>
		private void RecurseFieldInTable(LuaCodeVariableTable parentElement, Field field)
		{
			LuaCodeVariable element = LuaCodeElementFactory.CreateVariable
				(dte, parentElement, String.Empty, LuaType.Unknown, vsCMAccess.vsCMAccessPrivate,
				 new Variable(field.Location) {Identifier = field.Identifier});

			if (field.Identifier != null)
			{
				element.Name = field.Identifier.Name;
			}

			if (field.LeftExpression is Literal)
			{
				var literal = field.LeftExpression as Literal;
				if (String.IsNullOrEmpty(element.Name)) element.Name = literal.Value;
				//element.Type = new LuaCodeTypeRef(dte, LuaDeclaredType.Find(literal.Type.ToString()));
			}
			if (field.Expression is Literal)
			{
				var literal = field.Expression as Literal;
				element.InitExpression = ((Literal) field.Expression).Value;
				element.Type = new LuaCodeTypeRef(dte, LuaDeclaredType.Find(literal.Type.ToString()));
			}
			else
			{
				RecurseStatement(element, field.Expression);
			}
			parentElement.AddInitVariable(element);
		}

		/// <summary>
		/// Recurse and translate FunctionDeclaration AST node.
		/// </summary>
		/// <param name="node">AST node instance.</param>
		/// <param name="parentElement">Parent of function.</param>
		private LuaCodeFunction RecurseFunctionDeclaration(CodeElement parentElement, FunctionDeclaration node)
		{
			LuaCodeFunction function = LuaCodeElementFactory.CreateLuaCodeFunction(
				dte, parentElement, node.Name, LuaType.Unknown, node.IsLocal, node);
			if (node.Body != null && node.Body.StatementList != null)
			{
				foreach (var statement in node.Body.StatementList)
				{
					RecurseStatement(function, statement);
				}
			}
			if (node.ParameterList != null)
			{
				foreach (Identifier param in node.ParameterList.IdentifierList)
				{
					function.AddParameter(param.Name, param, -1);
				}
			}
			return function;
		}

		/// <summary>
		/// Recurse and translate Function AST node.
		/// </summary>
		/// <param name="node">AST node instance.</param>
		/// <param name="parentElement">Parent of LuaCodeFunction element.</param>
		private void RecurseFunctionNode(Function node, CodeElement parentElement)
		{
			var functionNode = new FunctionDeclaration(node.Location)
			                   	{
			                   		Body = node.Body,
			                   		Name = node.Name,
			                   		Next = node.Next,
			                   		ParameterList = node.ParameterList,
			                   		IsLocal = true
			                   	};
			RecurseFunctionDeclarationNode(parentElement, functionNode);
		}

		/// <summary>
		/// Recurse and translate FunctionDeclaration AST node.
		/// </summary>
		/// <param name="node">AST node instance.</param>
		private void RecurseFunctionDeclarationNode(FunctionDeclaration node)
		{
			rootElement.AddFunction(RecurseFunctionDeclaration(rootElement, node));
		}

		/// <summary>
		/// Recurse and translate FunctionDeclaration AST node.
		/// </summary>
		/// <param name="node">AST node instance.</param>
		/// <param name="parentElement">Parent of element.</param>
		private void RecurseFunctionDeclarationNode(CodeElement parentElement, FunctionDeclaration node)
		{
			AddElementToParent(parentElement, RecurseFunctionDeclaration(parentElement, node));
		}

		/// <summary>
		/// Recurse and translate a statement.
		/// </summary>
		/// <param name="statement">Instance of statement.</param>
		/// <param name="parentElement">Parent of statement.</param>
		private void RecurseStatement(CodeElement parentElement, Node statement)
		{
			//Trace.WriteLine("RecurseStatement::" + statement);
			if (statement == null) return;

			//--------------------------------------------------------------------
			if (statement is Block)
			{
				var block = statement as Block;
				block.StatementList.ForEach(child => RecurseStatement(parentElement, child));
			}
				//--------------------------------------------------------------------
			else if (statement is Identifier)
			{
				var identifier = statement as Identifier;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<Identifier>
					(dte, identifier.Name, parentElement, identifier);
				if (!isInStatement)
				{
					Node nextNode = statement.Next;
					while (nextNode != null)
					{
						isInStatement = true;
						RecurseStatement(parentElement, nextNode);
						nextNode = nextNode.Next;
					}
					isInStatement = false;
				}
				//statement.GetChildNodes().ForEach(child => RecurseStatement(element, child));
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is Literal)
			{
				var literal = statement as Literal;
				var element = LuaCodeElementFactory.
					CreateLuaCodeElement<Literal>(dte, literal.Value, parentElement, literal);
				if (!isInStatement)
				{
					Node nextNode = statement.Next;
					while (nextNode != null)
					{
						isInStatement = true;
						RecurseStatement(parentElement, nextNode);
						nextNode = nextNode.Next;
					}
					isInStatement = false;
				}
				//literal.GetChildNodes().ForEach(child => RecurseStatement(element, child));
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is LocalDeclaration)
			{
				var localDeclaration = statement as LocalDeclaration;

				foreach (Identifier identifier in localDeclaration.IdentifierList)
				{
					var element = LuaCodeElementFactory.CreateVariable
						(dte, parentElement, identifier.Name, LuaType.Unknown, true,
						 new Variable(identifier.Location) {Identifier = identifier});
					AddElementToParent(parentElement, element);
				}
			}
				//--------------------------------------------------------------------
			else if (statement is Break)
			{
				var element = LuaCodeElementFactory.CreateLuaCodeElement<Break>
					(dte, "Break", parentElement, statement as Break);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is Assignment)
			{
				var variables = RecurseAssignmentNode(parentElement, statement as Assignment);
				foreach (LuaCodeVariable variable in variables)
				{
					AddElementToParent(parentElement, variable);
				}
			}
				//--------------------------------------------------------------------
			else if (statement is If)
			{
				var ifStatement = statement as If;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<If>
					(dte, "If", parentElement, ifStatement);
				RecurseStatement(element, ifStatement.Expression);
				RecurseStatement(element, ifStatement.ThenBlock);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is ThenBlock)
			{
				var thenBlock = statement as ThenBlock;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<ThenBlock>
					(dte, "ThenBlock", parentElement, thenBlock);
				//thenBlock.GetChildNodes().ForEach(child => RecurseStatement(element, child));
				RecurseStatement(element, thenBlock.Block);
				RecurseStatement(element, thenBlock.ElseIfBlockList);
				RecurseStatement(element, thenBlock.ElseBlock);
				AddElementToParent(parentElement, element);
			}
			else if (statement is ElseIfBlock)
			{
				var elseIfBlock = statement as ElseIfBlock;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<ElseIfBlock>
					(dte, "ThenBlock", parentElement, elseIfBlock);
				//thenBlock.GetChildNodes().ForEach(child => RecurseStatement(element, child));
				RecurseStatement(element, elseIfBlock.Expression);
				RecurseStatement(element, elseIfBlock.Block);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is Return)
			{
				var returnStatement = statement as Return;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<Return>
					(dte, "Return", parentElement, returnStatement);
				RecurseStatement(element, returnStatement.ExpressionList);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is FunctionCall)
			{
				var functionCall = statement as FunctionCall;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<FunctionCall>
					(dte, functionCall.Identifier == null ? "FunctionCall" : functionCall.Identifier.Name, parentElement, functionCall);
				functionCall.GetChildNodes().ForEach(child => RecurseStatement(element, child));
				var identifier = element.Children.OfType<LuaCodeElement<Identifier>>().FirstOrDefault();
				element.Name = identifier != null ? identifier.Name : string.Empty;
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is TableConstructor)
			{
				var tableNode = statement as TableConstructor;
				LuaCodeVariable variable = CreateTable(tableNode, parentElement, tableNode.Name, true);
				AddElementToParent(parentElement, variable);
			}
				//--------------------------------------------------------------------
			else if (statement is ForLoop)
			{
				var loop = statement as ForLoop;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<ForLoop>
					(dte, loop.Name, parentElement, loop);
				//statement.GetChildNodes().ForEach<Node>(child => RecurseStatement(element, child));
				RecurseStatement(element, loop.Identifier);
				RecurseStatement(element, loop.IdentifierList);
				RecurseStatement(element, loop.Expression);
				Node subExp = loop.Expression.Next;
				while (subExp != null)
				{
					RecurseStatement(element, subExp);
					subExp = subExp.Next;
				}
				RecurseStatement(element, loop.Block);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is Loop || statement is WhileLoop || statement is RepeatUntilLoop)
			{
				var loop = statement as Loop;
				var element = LuaCodeElementFactory.CreateLuaCodeElement<Loop>
					(dte, loop.Name, parentElement, loop);
				RecurseStatement(element, loop.Expression);
				RecurseStatement(element, loop.Block);
				AddElementToParent(parentElement, element);
			}
				//--------------------------------------------------------------------
			else if (statement is BinaryExpression)
			{
				var exp = statement as BinaryExpression;
				var codeStatement = LuaCodeElementFactory.CreateLuaCodeStatement
					(dte, String.Empty, parentElement, exp);
				RecurseStatement(codeStatement, (exp.LeftExpression));
				codeStatement.UserData = exp.Operator;
				RecurseStatement(codeStatement, (exp.RightExpression));
				AddElementToParent(parentElement, codeStatement);
			}
				//--------------------------------------------------------------------
			else if (statement is Variable)
			{
				var variable = statement as Variable;
				string variableName = String.Empty;
				if (variable.PrefixExpression is Identifier)
				{
					variableName = variable.PrefixExpression == null
					               	? String.Empty
					               	: ((Identifier) variable.PrefixExpression).Name;
				}
				var luaVariable = LuaCodeElementFactory.CreateVariable
					(dte, parentElement, variableName, LuaType.Table, vsCMAccess.vsCMAccessPrivate, variable);
				RecurseStatement(luaVariable, variable.Identifier);
				RecurseStatement(luaVariable, variable.PrefixExpression);
				RecurseStatement(luaVariable, variable.Expression);
				AddElementToParent(parentElement, luaVariable);
			}
				//--------------------------------------------------------------------
			else if (statement is UnaryExpression)
			{
				var exp = statement as UnaryExpression;
				var codeStatement = LuaCodeElementFactory.CreateLuaCodeStatement
					(dte, String.Empty, parentElement, exp);
				codeStatement.UserData = exp.Operator;
				exp.GetChildNodes().ForEach(child => RecurseStatement(codeStatement, child));
				AddElementToParent(parentElement, codeStatement);
			}
				//--------------------------------------------------------------------
			else if (statement is ExplicitBlock)
			{
				var block = statement as ExplicitBlock;
				block.Block.ForEach(child => RecurseStatement(parentElement, child));
			}
				//--------------------------------------------------------------------
			else if (statement is FunctionDeclaration)
			{
				var functionNode = statement as FunctionDeclaration;
				RecurseFunctionDeclarationNode(parentElement, functionNode);
			}
				//--------------------------------------------------------------------
			else if (statement is Function)
			{
				var function = statement as Function;
				var luaVariable = LuaCodeElementFactory.CreateVariable
					(dte, parentElement, function.Name, LuaType.Unknown, true,
					 new Variable(function.Location));
				RecurseFunctionNode(function, luaVariable);
				AddElementToParent(parentElement, luaVariable);
			}
			else
			{
				Trace.WriteLine("WARNING IN STATEMENT - " + statement);
			}
		}

		/// <summary>
		/// Adds element to Parent CodeElement.
		/// </summary>
		private static void AddElementToParent(CodeElement parentElement, CodeElement element)
		{
			if (parentElement is LuaCodeFunction)
			{
				(parentElement as LuaCodeFunction).FunctionBody.AddElement(element);
			}
			else if (parentElement is LuaCodeStatement)
			{
				(parentElement as LuaCodeStatement).Statements.AddElement(element);
			}
			else
			{
				((ICodeDomElement) parentElement).AddChildObject(element);
			}
		}

		#region Trace Functions

		/// <summary>
		/// Traces the dump.
		/// </summary>
		/// <param name="root">The root.</param>
		[Conditional("DEBUG")]
		private static void TraceDump(CodeClass root)
		{
			Trace.WriteLine(new String('=', 10));
			foreach (CodeElement element in root.Members)
			{
				TraceRecurseElements(element, 1);
			}
			Trace.WriteLine(new String('=', 10));
		}

		/// <summary>
		/// Traces the recurse elements.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="level">The level.</param>
		private static void TraceRecurseElements(CodeElement element, int level)
		{
			if (element == null) return;

			if (element is LuaCodeVariable)
			{
				var variable = element as LuaCodeVariable;
				Trace.WriteLine(String.Format("{0}AST::{4}::{1} = {2} [Type:{3}] [Access:{5}] [Kind:{6}]",
				                              new String(' ', 4*level), variable.Name,
				                              variable.InitExpression, variable.Type,
				                              variable.LuaASTNodeObject == null
				                              	? "Null"
				                              	: variable.LuaASTNodeObject.GetType().ToString(), variable.Access,
				                              variable.Kind));
			}
			else if (element is LuaCodeFunction)
			{
				var function = element as LuaCodeFunction;
				Trace.Write(String.Format("{0}Function::{1} [Type:{2}] [Access:{3}] [Kind:{4}]",
				                          new String(' ', 4*level), function.Name, function.Type, function.Access,
				                          function.Kind));
				if (function.Parameters != null)
				{
					foreach (LuaCodeElement<Identifier> param in function.Parameters)
					{
						Trace.Write(String.Format("{1}Param: {0}", param.Name, new String(' ', 5*level)));
					}
				}
			}
			else
			{
				Trace.WriteLine(String.Format("{0}AST::Node::{1} [Kind:{2}]", new String(' ', 4*level), element.Name,
				                              element.Kind));
			}
			if (element.Children != null)
			{
				foreach (CodeElement childElement in element.Children)
				{
					TraceRecurseElements(childElement, level + 1);
				}
			}
			//else { Trace.Write("  No Children"); }
		}

		#endregion
	}
}