using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    public class Bytecode
    {
        private List<TokensFound> TokensList { get; set; }
        public List<BytecodeFound> BytecodeFounds { get; private set; }
        private int Id { get; set; }

        public Bytecode(List<TokensFound> tokensList)
        {
            TokensList = tokensList;
            Id = 0;
        }

        public bool BytecodeAnalysis()
        {
            BytecodeFounds = new List<BytecodeFound>();
            var address = 0;
            var lastLine = 0;
            foreach (TokensFound token in TokensList)
            {
                var bytecodeFound = new BytecodeFound();
                bytecodeFound.Line = token.Line;
                bytecodeFound.Address = address;
                address += 2;

                OperationNames();

            }

            return false;
        }

        private void OperationNames()
        {

        }

    }
}
