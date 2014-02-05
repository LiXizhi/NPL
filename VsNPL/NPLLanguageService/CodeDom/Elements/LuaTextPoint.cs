using System;
using EnvDTE;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Represents a TextPoint in Lua code document.
    /// </summary>
    public class LuaTextPoint : TextPoint
    {
        private readonly int x;
        private readonly int y;
        private readonly TextDocument parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaFileCodeModel"/> class.
        /// </summary>
        /// <param name="parent">Parent Document.</param>
        /// <param name="column">X coord of point.</param>
        /// <param name="row">Y coord of point.</param>
        public LuaTextPoint(TextDocument parent, int column, int row)
        {
            this.parent = parent;
            x = column == 0 ? 1 : column;
            y = row;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaFileCodeModel"/> class.
        /// </summary>
        /// <param name="parent">Parent Document.</param>
        /// <param name="location">LexLocation of node.</param>
        public LuaTextPoint(TextDocument parent, LexLocation location)
            : this(parent, location.sCol, location.sLin)
        {
        }

        #region TextPoint Members

        /// <summary>
        /// Gets the one-based character offset from the beginning of the document 
        /// to the <see cref="T:EnvDTE.TextPoint" /> object.
        /// </summary>
        public int AbsoluteCharOffset
        {
            get
            {
                var editPoint = parent.CreateEditPoint(this);
                return editPoint.AbsoluteCharOffset;
            }
        }

        /// <summary>
        /// Gets whether the object is at the end of the document.
        /// </summary>
        public bool AtEndOfDocument
        {
            get { return (parent.EndPoint.Line == y && parent.EndPoint.LineCharOffset == x); }
        }

        /// <summary>
        /// Gets whether or not object is at the end of a line.
        /// </summary>
        public bool AtEndOfLine
        {
            get
            {
                var editPoint = parent.CreateEditPoint(this);
                return editPoint.AtEndOfLine;
            }
        }

        /// <summary>
        /// Gets whether or not the object is at the beginning of the document.
        /// </summary>
        public bool AtStartOfDocument
        {
            get { return x == 1 && y == 1; }
        }

        /// <summary>
        /// Gets whether or not the object is at the beginning of a line.
        /// </summary>
        public bool AtStartOfLine
        {
            get { return x == 1; }
        }

        /// <summary>
        /// Creates and returns an <see cref="T:EnvDTE.EditPoint" /> object at the location of the calling object.
        /// </summary>
        /// <returns>An <see cref="T:EnvDTE.EditPoint" /> object.</returns>
        public EditPoint CreateEditPoint()
        {
            return parent.CreateEditPoint(this);
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public DTE DTE
        {
            get { return parent.DTE; }
        }

        /// <summary>
        /// Gets the number of the current displayed column 
        /// containing the <see cref="T:EnvDTE.TextPoint" /> object.
        /// </summary>
        public int DisplayColumn
        {
            get { return x; }
        }

        /// <summary>
        /// Returns whether the value of the given point object's 
        /// <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> property is 
        /// equal to that of the calling <see cref="T:EnvDTE.TextPoint" /> object.
        /// </summary>
        /// <param name="point">Required. A <see cref="T:EnvDTE.TextPoint" /> object to compare to 
        /// the calling point object.
        /// </param>
        /// <returns>A Boolean value indicating true if <paramref name="point" /> has the same <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> 
        /// property value as the calling point object.</returns>
        public bool EqualTo(TextPoint point)
        {
            var tp = point as LuaTextPoint;
            if (tp == null) return false;

            return tp.x == x && tp.y == y;
        }

        /// <summary>Indicates whether or not the value of the calling object's <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" />
        ///  property is greater than that of the given point object.</summary>
        /// <param name="point">Required. A <see cref="T:EnvDTE.TextPoint" /> object to 
        /// compare to the calling point object.
        /// </param>
        /// <returns>A Boolean value indicating true if <paramref name="point" /> 
        /// has a smaller <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> 
        /// property value compared to the calling point object's 
        /// <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> property. </returns>
        public bool GreaterThan(TextPoint point)
        {
            var tp = point as LuaTextPoint;
            if (tp == null) return false;

            return tp.y < y || (tp.y == y && tp.x < x);
        }

        /// <summary>Indicates whether or not the value of the called object's 
        /// <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> property is less than that of 
        /// the given object.
        /// </summary>
        /// <param name="point">Required. A <see cref="T:EnvDTE.TextPoint" /> to compare to 
        /// the calling point object.</param>
        /// <returns>A Boolean value indicating true if <paramref name="point" /> has a greater <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> property value compared to the calling point object's <see cref="P:EnvDTE.TextPoint.AbsoluteCharOffset" /> property. </returns>
        public bool LessThan(TextPoint point)
        {
            var tp = point as LuaTextPoint;
            if (tp == null) return false;

            return tp.y > y || (tp.y == y && tp.x > x);
        }

        /// <summary>
        /// Gets the line number of the object.
        /// </summary>
        public int Line
        {
            get { return y; }
        }

        /// <summary>
        /// Gets the character offset of the object.
        /// </summary>
        public int LineCharOffset
        {
            get { return x; }
        }

        /// <summary>
        /// Gets the number of characters in a line containing the object, 
        /// excluding the new line character.
        /// </summary>
        public int LineLength
        {
            get
            {
                var editPoint = parent.CreateEditPoint(this);
                return editPoint.LineLength;
            }
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.TextPoint" /> object.
        /// </summary>
        public TextDocument Parent
        {
            get { return parent; }
        }

        /// <summary>Attempts to display the text point's location.</summary>
        /// <returns>A Boolean value indicating true if the span of text fits within the current code editor, false if not.</returns>
        /// <param name="how">Optional. A <see cref="T:EnvDTE.vsPaneShowHow" /> constant that determines how the code is displayed.</param>
        /// <param name="pointOrCount">Optional. The endpoint of the selected range of text to be displayed. It can be either a <see cref="T:EnvDTE.TextPoint" /> or an integer.</param>
        public bool TryToShow(vsPaneShowHow how, object pointOrCount)
        {
            var editPoint = parent.CreateEditPoint(this);
            return editPoint.TryToShow(how, pointOrCount);
        }

        /// <summary>
        /// Gets the CodeElement in the specified scope.
        /// </summary>
        /// <param name="Scope"></param>
        /// <returns></returns>
        public CodeElement get_CodeElement(vsCMElement Scope)
        {
            throw new NotImplementedException();
        }

        #endregion

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
        public override string ToString()
        {
            return String.Format("LuaTextPoint: [X:{0}, Y:{1}]", x, y);
        }
    }
}