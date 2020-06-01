using System;
using System.Collections.Generic;
using System.Linq;
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
            var lastLine = TokensList.Last();

            for(int line = 1; line <= lastLine.Line; line++)
            {
                foreach(var found in GetObjects(line))
                {
                    var bytecodeFound = new BytecodeFound
                    {
                        Line = line,
                        Address = address,
                        FriendlyInterpretation = found.FriendlyInterpretation,
                        OpName = found.OpName
                    };

                    BytecodeFounds.Add(bytecodeFound);
                    address += 2;
                }               
            }

            return true;
        }

        private List<BytecodeFound> GetObjects(int line)
        {
            var tokens = TokensList.FindAll(x => x.Line.Equals(line));
            var bytecodeFoundList = new List<BytecodeFound>();

            for(int index = tokens.Count - 1; index >= 0; index--)
            {
                var bytecodeFound = new BytecodeFound();

                if (tokens[index].Token.Equals("TOKEN.STRING") || tokens[index].Token.Equals("TOKEN.FLOAT") 
                    || tokens[index].Token.Equals("TOKEN.INTEGER"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                    bytecodeFoundList.Add(bytecodeFound);
                    continue;
                }

                if (tokens[index].Token.Equals("TOKEN.IGUAL"))
                {
                    var indexAux = index - 1;
                    if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.ID"))
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFound.OpName = opName;
                        bytecodeFound.FriendlyInterpretation = tokens[indexAux].Lexema;
                        bytecodeFoundList.Add(bytecodeFound);
                        continue;
                    }
                }

                if (tokens[index].Token.Equals("TOKEN.ID"))
                {
                    var indexAux = index - 1;
                    if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.IGUAL"))
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFound.OpName = opName;
                        bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                        bytecodeFoundList.Add(bytecodeFound);
                        continue;
                    }
                    else if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.MAIS"))
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFound.OpName = opName;
                        bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                        bytecodeFoundList.Add(bytecodeFound);
                        continue;
                    }
                }

                if (tokens[index].Token.Equals("TOKEN.MAIS"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFoundList.Add(bytecodeFound);
                    continue;
                }


            }

            return bytecodeFoundList;
        }


    }
}
