using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    class FunctionExpression : Node
    {
        public FunctionExpression(LexLocation location)
            :base(location)
        {

        }

        //public Identifier Identifier { get; set; }
        public FunctionCall FunctionCall { get; set; }
        //public Node Arguments { get; set; }
        public Block Block { get; set; }

        public override IEnumerable<Node> GetChildNodes()
        {
            //yield return Identifier;
            yield return FunctionCall;
            yield return Block;
        }
    }
}
