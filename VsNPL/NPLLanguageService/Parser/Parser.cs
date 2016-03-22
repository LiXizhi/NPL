using System;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// Static implementation part of lua parser. The language-specific
    /// part is generated based on parser.y.
    /// </summary>
    public partial class Parser
    {
        #region Public methods and properties.
        /// <summary>
        /// Gets or sets the request object the parser works with.
        /// </summary>
        public ParseRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the chunk parsed.
        /// </summary>
        public Chunk Chunk { get; private set; }
        #endregion

        #region Private methods and properties.

		/// <summary>
		/// Creates the text span.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns></returns>
        private TextSpan CreateTextSpan(LexLocation location)
        {
            return TextSpan(location.sLin, location.sCol, location.eLin, location.eCol);
        }

		/// <summary>
		/// Warnings the specified location.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="message">The message.</param>
        private void Warning(LexLocation location, string message)
        {
            ReportWarning(CreateTextSpan(location), message);
        }

		/// <summary>
		/// Errors the specified location.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="message">The message.</param>
        private void Error(LexLocation location, string message)
        {
            ReportError(CreateTextSpan(location), message);
        }

        public override void PrintError(string errorMsg)
        {
            if(location_stack.top>=1)
            {
                LexLocation location = location_stack.array[location_stack.top - 1];
                if (location != null)
                    ReportError(CreateTextSpan(location), errorMsg);
            }
        }

        /// <summary>
        /// Matches the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        private void Match(LexLocation left, LexLocation right)
        {
            if (IsSinkAvailable && Sink.BraceMatching)
            {
                Sink.MatchPair(CreateTextSpan(left), CreateTextSpan(right), 1);
                System.Diagnostics.Trace.WriteLine(String.Format("Match: {0}:{1}-{2}:{3}", left.sCol, left.eCol, right.sCol, right.eCol));
            }
        }

		/// <summary>
		/// Regions the specified location.
		/// </summary>
		/// <param name="location">The location.</param>
        private void Region(LexLocation location)
        {
            if (IsSinkAvailable && Sink.HiddenRegions)
            {
                Sink.ProcessHiddenRegions = true;
                Sink.AddHiddenRegion(CreateTextSpan(location));
                System.Diagnostics.Trace.WriteLine(String.Format("Region: line{0}:{1}-line{2}:{3}", location.sLin, location.sCol, location.eLin, location.eCol));
            }
        }

		/// <summary>
		/// Regions the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
        private void Region(LexLocation left, LexLocation right)
        {
            if (IsSinkAvailable && Sink.HiddenRegions)
                this.Region(this.Merge(left, right));
        }

		/// <summary>
		/// Starts the name.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="name">The name.</param>
        private void StartName(LexLocation location, string name)
        {
            if (IsSinkAvailable && Sink.FindNames)
            {
                Request.Sink.StartName(CreateTextSpan(location), name);
                System.Diagnostics.Trace.WriteLine("StartName: " + name);
            }
        }

		/// <summary>
		/// Qualifies the name.
		/// </summary>
		/// <param name="selectorLocation">The selector location.</param>
		/// <param name="nameLocation">The name location.</param>
		/// <param name="name">The name.</param>
        private void QualifyName(LexLocation selectorLocation, LexLocation nameLocation, string name)
        {
            if (IsSinkAvailable && Sink.FindNames)
            {
                Request.Sink.QualifyName(CreateTextSpan(selectorLocation), CreateTextSpan(nameLocation), name);
                System.Diagnostics.Trace.WriteLine("QualifyName: " + name);
            }
        }

		/// <summary>
		/// Starts the parameters.
		/// </summary>
		/// <param name="location">The location.</param>
        private void StartParameters(LexLocation location)
        {
            if (IsSinkAvailable && Sink.MethodParameters)
            {
                Request.Sink.StartParameters(CreateTextSpan(location));
                System.Diagnostics.Trace.WriteLine("StartParameters");
            }
        }

		/// <summary>
		/// Parameters the specified location.
		/// </summary>
		/// <param name="location">The location.</param>
        private void Parameter(LexLocation location)
        {
            if (IsSinkAvailable && Sink.MethodParameters)
            {
                Request.Sink.NextParameter(CreateTextSpan(location));
                System.Diagnostics.Trace.WriteLine("Parameter");
            }
        }

		/// <summary>
		/// Ends the parameters.
		/// </summary>
		/// <param name="location">The location.</param>
        private void EndParameters(LexLocation location)
        {
            if (IsSinkAvailable && Sink.MethodParameters)
            {
                Request.Sink.EndParameters(CreateTextSpan(location));
                System.Diagnostics.Trace.WriteLine("EndParameters");
            }
        }

        /// <summary>
        /// Append the right node to the left into a single linked list.
        /// </summary>
        /// <param name="left">The left node.</param>
        /// <param name="right">The right node.</param>
        /// <returns>The first node of a single linked list of nodes.</returns>
        private Node AppendNode(Node left, Node right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;
            if (left == null && right == null)
                return null;

            Node last = left;

            while (last.Next != null)
            {
                if (last == right)
                    throw new InvalidOperationException("Cannot append nodes, result would be circular.");

                last = last.Next;
            }

            last.Next = right;

            return left;
        }

        /// <summary>
        /// Append the nodes into a single linked list.
        /// </summary>
        /// <param name="nodes">The nodes to append.</param>
        /// <returns>The first node of a single linked list of nodes.</returns>
        private Node AppendNodes(params Node[] nodes)
        {
            Node node = nodes[0];

            for (int i = 1; i < nodes.Length; i++)
                node = this.AppendNode(node, nodes[i]);

            return node;
        }

		/// <summary>
		/// Merges the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
        private LexLocation Merge(LexLocation left, LexLocation right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;

            return left.Merge(right);
        }

        /// <summary>
        /// Indicates whether there is a request sink available.
        /// </summary>
        private bool IsSinkAvailable
        {
            get { return Request != null && Request.Sink != null; }
        }


        private bool m_bSuppressErrorSink = false;
        public bool IsSuppressErrorSink
        {
            get { return m_bSuppressErrorSink; }
            set { m_bSuppressErrorSink = value;  }
        }

        /// <summary>
        /// Contains information about the source being parsed.
        /// </summary>
        private AuthoringSink Sink
        {
            get { return Request.Sink; }
        }

        public override void OnPreprocess()
        {
            if(scanner is LuaScanner)
            {
                IsSuppressErrorSink = (scanner as LuaScanner).IsSuppressError;
            }
        }

        /// <summary>
        /// Reports the error.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        private void ReportError(TextSpan span, string message, Severity severity)
        {
            if (IsSinkAvailable && !IsSuppressErrorSink)
            {
                Sink.AddError(Request.FileName, message, span, severity);
                System.Diagnostics.Trace.WriteLine(String.Format("Report Error: Line{0}:{1} msg:{2}", span.iStartLine, span.iStartIndex, message));
            }
        }

		/// <summary>
		/// Reports the error.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="message">The message.</param>
        private void ReportError(TextSpan location, string message)
        {
            ReportError(location, message, Severity.Error);
        }

		/// <summary>
		/// Reports the warning.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="message">The message.</param>
        private void ReportWarning(TextSpan location, string message)
        {
            ReportError(location, message, Severity.Warning);
        }

		/// <summary>
		/// Texts the span.
		/// </summary>
		/// <param name="startLine">The start line.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endIndex">The end index.</param>
		/// <returns></returns>
        private TextSpan TextSpan(int startLine, int startIndex, int endIndex)
        {
            return TextSpan(startLine, startIndex, startLine, endIndex);
        }

		/// <summary>
		/// Texts the span.
		/// </summary>
		/// <param name="startLine">The start line.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="endLine">The end line.</param>
		/// <param name="endIndex">The end index.</param>
		/// <returns></returns>
        private TextSpan TextSpan(int startLine, int startIndex, int endLine, int endIndex)
        {
            TextSpan ts;
            ts.iStartLine = startLine - 1;
            ts.iStartIndex = startIndex;
            ts.iEndLine = endLine - 1;
            ts.iEndIndex = endIndex;
            return ts;
        }

        #endregion
    }
}