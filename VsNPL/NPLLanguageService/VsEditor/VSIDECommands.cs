using System;
using System.Collections.Generic;
using System.Reflection;

namespace ParaEngine.Tools.Lua.VsEditor
{
    internal sealed class VSIDECommands
    {
        public static readonly Guid CLSID_StandardCommandSet97 = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");
		public static readonly Guid CMDSETID_StandardCommandSet2K = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");
        // Deleted by LiXizhi, this fixed a bug that page download and redo is not usable.  
		//public static readonly int cmdidDelete = 0x11;
        //public static readonly int cmdidRedo = 73;
        //public static readonly int cmdidUndo = 71; // Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UNDO;
        public static readonly int ECMD_SHOWCONTEXTMENU = 102;
        private static readonly Dictionary<int, string> commands = new Dictionary<int, string>();

        

		/// <summary>
		/// Initializes a new instance of the <see cref="VSIDECommands"/> class.
		/// </summary>
        private VSIDECommands()
        {
        }

		/// <summary>
		/// Initializes the <see cref="VSIDECommands"/> class.
		/// </summary>
        static VSIDECommands()
        {
            FieldInfo[] fields = typeof (VSIDECommands).GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.FieldType == typeof (int))
                {
                    object value = fieldInfo.GetValue(null);
                    string name = fieldInfo.Name;
                    if (value != null)
                        commands.Add((int) value, name);
                }
            }
        }

		/// <summary>
		/// Gets the command id.
		/// </summary>
		/// <param name="cmdGroup">The CMD group.</param>
		/// <param name="cmd">The CMD.</param>
		/// <returns></returns>
        internal static string GetCommandId(Guid cmdGroup, uint cmd)
        {
			if (cmdGroup == CMDSETID_StandardCommandSet2K || cmdGroup == CLSID_StandardCommandSet97)
            {
                if (commands.ContainsKey((int) cmd))
                    return commands[(int) cmd];
            }
            return null;
        }

		/// <summary>
		/// Determines whether [is right click] [the specified CMD group].
		/// </summary>
		/// <param name="cmdGroup">The CMD group.</param>
		/// <param name="cmd">The CMD.</param>
		/// <returns>
		/// 	<c>true</c> if [is right click] [the specified CMD group]; otherwise, <c>false</c>.
		/// </returns>
        public static bool IsRightClick(Guid cmdGroup, uint cmd)
        {
            // Deleted by LiXizhi, this fixed a bug that page down and redo is not usable.  
            return ((cmdGroup == CMDSETID_StandardCommandSet2K) && (cmd == ECMD_SHOWCONTEXTMENU));
            // return false;
        }
    }
}