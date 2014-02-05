using System.Collections.Generic;
using System.Linq;

namespace ParaEngine.Tools.Lua.AST
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class LuaDeclaredType : Node
	{
		public static readonly LuaDeclaredType Unknown = new LuaDeclaredType("Unknown");
		public static readonly LuaDeclaredType Nil = new LuaDeclaredType("Nil");
		public static readonly LuaDeclaredType Boolean = new LuaDeclaredType("Boolean");
		public static readonly LuaDeclaredType Number = new LuaDeclaredType("Number");
		public static readonly LuaDeclaredType String = new LuaDeclaredType("String");
		public static readonly LuaDeclaredType Function = new LuaDeclaredType("Function");
		public static readonly LuaDeclaredType Userdata = new LuaDeclaredType("Userdata");
		public static readonly LuaDeclaredType Thread = new LuaDeclaredType("Thread");
		public static readonly LuaDeclaredType Table = new LuaDeclaredType("Table");

		private static IEnumerable<LuaDeclaredType> luaDeclaredTypeList =
			new List<LuaDeclaredType>(
				new[] {Unknown, Nil, Boolean, Number, String, Function, Userdata, Thread, Table});

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaDeclaredType"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public LuaDeclaredType(string name) : base(null)
		{
			Name = name;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the lua types.
		/// </summary>
		/// <value>The lua types.</value>
		public static IEnumerable<LuaDeclaredType> LuaTypes
		{
			get { return luaDeclaredTypeList; }
			set { luaDeclaredTypeList = value; }
		}

		/// <summary>
		/// Find the specified LuaDeclaredType by its name.
		/// </summary>
		/// <param name="name">Name of LuaDeclaredType.</param>
		/// <returns>LuaDeclaredType Or Null</returns>
		public static LuaDeclaredType Find(string name)
		{
			if (string.IsNullOrEmpty((name)))
				return Unknown;

			return LuaTypes.SingleOrDefault(luaType => luaType.Name == name);
		}
	}
}