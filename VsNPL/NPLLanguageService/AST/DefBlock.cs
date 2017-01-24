using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    class DefBlock : Node
    {
        public DefBlock(LexLocation location)
            :base(location)
        {

        }

        public Node TokenList { get; set; }

        public override IEnumerable<Node> GetChildNodes()
        {
            yield return TokenList;
        }
    }
}
