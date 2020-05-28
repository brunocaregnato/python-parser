using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    public class Bytecode
    {
        public List<BytecodeFound> BytecodeFounds { get; private set; }
        private List<TokensFound> TokensList { get; set; }
        private OperationsName OperationsName { get; set; }

        public Bytecode(List<TokensFound> tokensList)
        {
            TokensList = tokensList;            
        }

        public bool BytecodeAnalysis()
        {
            BytecodeFounds = new List<BytecodeFound>();
            OperationsName = new OperationsName();
            var address = 0;

            foreach (TokensFound token in TokensList)
            {
                var bytecodeFound = new BytecodeFound
                {
                    Line = token.Line,
                    Address = address                    
                };

                if (OperationsName.OperationsNameList.TryGetValue(token.Token, out string opName))
                {
                    bytecodeFound.OpName = opName;
                }

                bytecodeFound.FriendlyInterpretation = GetFriendlyInterpretation(token.Line, token.Token);

                BytecodeFounds.Add(bytecodeFound);

                address += 2;
            }

            return true;
        }

        private string GetFriendlyInterpretation(int line, string tokenFound)
        {
            var tokens = TokensList.FindAll(x => x.Line.Equals(line));
            int id = 0;
            foreach(var token in tokens)
            {
                
            }


            return "";
        }


    }
}
