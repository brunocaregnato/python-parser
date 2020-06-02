using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            
            //se for um assign, precisa avaliar a expressao da esquerda para direita depois do =
            if (tokens[0].Token.Equals("TOKEN.ID") && tokens[1].Token.Equals("TOKEN.IGUAL"))
            {
                return IsAssign(tokens);               
            } 

            return null;
        }


        private List<BytecodeFound> IsAssign(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            int size = 1;
            for (int index = 2; index < tokens.Count; index += size)
            {
                size = 0;
                var list = GetBytecodeFound(tokens, index);
                if (list != null)
                {
                    list.ForEach(bytecode =>
                    {
                        size++;
                        bytecodeFoundList.Add(bytecode);
                    });
                }
                else
                {
                    size++;
                }
            }

            GetBytecodeFound(tokens, 1).ForEach(bytecode =>
            {
                bytecodeFoundList.Add(bytecode);
            });

            if (tokens[tokens.Count - 1].Token.Equals("TOKEN.EOF"))
            {
                OperationsName.OperationsNameList.TryGetValue(tokens[tokens.Count - 1].Token, out string opName);
                var bytecodeFound = new BytecodeFound
                {
                    OpName = opName
                };
                bytecodeFoundList.Add(bytecodeFound);
            }

            return bytecodeFoundList;
        }

        private List<BytecodeFound> GetBytecodeFound(List<TokensFound> tokens, int index)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            var bytecodeFound = new BytecodeFound();

            if (tokens[index].Token.Equals("TOKEN.STRING") || tokens[index].Token.Equals("TOKEN.FLOAT")
                || tokens[index].Token.Equals("TOKEN.INTEGER"))
            {
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFound.OpName = opName;
                bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                bytecodeFoundList.Add(bytecodeFound);
                return bytecodeFoundList;
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
                    return bytecodeFoundList;
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
                    return bytecodeFoundList;
                }
                else if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.MAIS"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                    bytecodeFoundList.Add(bytecodeFound);
                    return bytecodeFoundList;
                }
            }

            if (tokens[index].Token.Equals("TOKEN.MAIS"))
            {
                if(tokens[index + 1].Token.Equals("TOKEN.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName2);
                    bytecodeFound.OpName = opName2;
                    bytecodeFound.FriendlyInterpretation = tokens[index + 1].Lexema;
                    bytecodeFoundList.Add(bytecodeFound);
                    bytecodeFound = new BytecodeFound();                    
                }

                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFound.OpName = opName;
                bytecodeFoundList.Add(bytecodeFound);
                return bytecodeFoundList;
            }

            return null;
        }
    }
}
