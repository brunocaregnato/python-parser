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
        private int Size { get; set; }

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
            if (tokens[0].Token.Equals("TOKEN.IF"))
            {
                return IsIf(tokens);
            }

            return null;
        }


        private List<BytecodeFound> IsAssign(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;
            
            for (int index = 2; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetBytecodeFound(tokens, index);
                if (list != null)
                {
                    list.ForEach(bytecode =>
                    {
                        Size++;
                        bytecodeFoundList.Add(bytecode);
                    });
                }
                else
                {
                    Size++;
                }
            }

            GetBytecodeFound(tokens, 1).ForEach(bytecode => bytecodeFoundList.Add(bytecode));

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

            if (IsType(tokens[index].Token))
            {
                while (!IsOutOfBounds(index + 1, tokens) && IsBinaryOperation(tokens[index + 1].Token))
                {
                    if (!IsOutOfBounds(index + 2, tokens) && IsType(tokens[index + 2].Token)) //se tem const op const, ja retorna o resultado
                    {
                        if (tokens[index].Token.Equals("TOKEN.STRING") &&
                            tokens[index + 1].Token.Equals("TOKEN.MAIS") &&
                            tokens[index + 2].Token.Equals("TOKEN.STRING"))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateStringOperation(tokens[index].Lexema, tokens[index + 2].Lexema);
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateStringOperation(bytecodeFound.FriendlyInterpretation, tokens[index + 2].Lexema);
                                Size += 2;
                                index += 2;
                            }
                        }
                        else if (tokens[index].Token.Equals("TOKEN.INTEGER") &&
                                 tokens[index + 2].Token.Equals("TOKEN.INTEGER"))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateIntOperation(
                                    int.Parse(tokens[index].Lexema),
                                    int.Parse(tokens[index + 2].Lexema),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateIntOperation(
                                    int.Parse(bytecodeFound.FriendlyInterpretation),
                                    int.Parse(tokens[index + 2].Lexema),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 2;
                                index += 2;
                            }
                        }
                        else if ((tokens[index].Token.Equals("TOKEN.FLOAT") || tokens[index].Token.Equals("TOKEN.INTEGER")) &&
                                 (tokens[index + 2].Token.Equals("TOKEN.FLOAT") || tokens[index + 2].Token.Equals("TOKEN.INTEGER")))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateFloatOperation(
                                    float.Parse(tokens[index].Lexema.Replace(".", ",")),
                                    float.Parse(tokens[index + 2].Lexema.Replace(".", ",")),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateFloatOperation(
                                    float.Parse(bytecodeFound.FriendlyInterpretation.Replace(".", ",")),
                                    float.Parse(tokens[index + 2].Lexema.Replace(".", ",")),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 2;
                                index += 2;
                            }
                        }
                        else
                        {
                            break;
                        }
                   }
                    else
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFound.OpName = opName;
                        if (bytecodeFound.FriendlyInterpretation == null)
                        {
                            bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                        }
                        bytecodeFoundList.Add(bytecodeFound);
                        return bytecodeFoundList;
                    }
                }

                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName2);
                bytecodeFound.OpName = opName2;
                if (bytecodeFound.FriendlyInterpretation == null)
                {
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                }
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

                    bytecodeFound = new BytecodeFound();
                    OperationsName.OperationsNameList.TryGetValue(tokens[indexAux].Token, out opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFoundList.Add(bytecodeFound);

                    return bytecodeFoundList;
                }
            }
            
            if (IsBinaryOperation(tokens[index].Token))
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

        private List<BytecodeFound> IsIf(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();

            return bytecodeFoundList;
        }

        private bool IsBinaryOperation(string token)
        {
            return token.Equals("TOKEN.MAIS") || token.Equals("TOKEN.MENOS") || token.Equals("TOKEN.VEZES") ||
                   token.Equals("TOKEN.BARRA") || token.Equals("TOKEN.NOME_PARAMETRO") || token.Equals("TOKEN.ARROBA") ||
                   token.Equals("TOKEN.BARRA_DUPLA") || token.Equals("TOKEN.PORCENTO") || token.Equals("TOKEN.SHIFT_LEFT") ||
                   token.Equals("TOKEN.SHIFT_RIGHT") || token.Equals("TOKEN.ECOMERCIAL") || token.Equals("TOKEN.PIPE") ||
                   token.Equals("TOKEN.CIRCUMFLEXO");
        }

        private bool IsType(string token)
        {
            return token.Equals("TOKEN.STRING") || token.Equals("TOKEN.FLOAT") || token.Equals("TOKEN.INTEGER");
        }

        private string CalculateStringOperation(string to1, string to2)
        {
            return "\"" + to1.Replace("\"", "") + to2.Replace("\"", "") + "\"";
        }

        private int CalculateIntOperation(int to1, int to2, string operation)
        {
            switch(operation)
            {
                case "+": return to1 + to2;
                case "-": return to1 - to2;
                case "*": return to1 * to2;
                case "/": return to1 / to2;
                case "%": return to1 % to2;
                case "&": return to1 & to2;
                case "|": return to1 | to2;
                case "<<": return to1 << to2;
                case ">>": return to1 >> to2;
                default:
                    break;
            }

            return 0;
        }

        private float CalculateFloatOperation(float to1, float to2, string operation)
        {
            switch (operation)
            {
                case "+": return to1 + to2;
                case "-": return to1 - to2;
                case "*": return to1 * to2;
                case "/": return to1 / to2;
                case "%": return to1 % to2;
                default:
                    break;
            }

            return 0;
        }

        private bool IsOutOfBounds(int index, List<TokensFound> tokens)
        {
            return !(index <= tokens.Count - 1);
        }

    }
}
