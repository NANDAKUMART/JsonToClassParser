using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonParser
{
    public class SyntaxTree
    {
        public bool isArray { get; set; }

        public int Startpos { get; set; }

        public List<SyntaxTree> ChildList { get; set; }

        public int EndPos { get; set; }
    }
}
