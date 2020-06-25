using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    public class BytecodeFound
    {
        public int Line { get; set; }
        public int Address { get; set; }
        public string OpName { get; set; }
        public string Argument { get; set; }
        public string FriendlyInterpretation { get; set; }
    }
}
