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
        private Stack<bool> IfStack { get; set; } = new Stack<bool>();

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

        private List<BytecodeFound> GetObjects(int line, int index = 0)
        {
            var tokens = TokensList.FindAll(x => x.Line.Equals(line));
            var list = new List<BytecodeFound>();

            if (tokens[index].Token.Equals("TOKEN.INDENT"))
            {
                index++;
            }
            else if (tokens[index].Token.Equals("TOKEN.DEDENT"))
            {
                index++;
                if (IfStack.Count > 0)
                {
                    list.Add(new BytecodeFound
                    {
                        OpName = "JUMP_FORWARD"
                    });

                    IfStack.Pop();
                }
            }

            if (tokens[index].Token.Equals("TOKEN.ID") && tokens[index + 1].Token.Equals("TOKEN.IGUAL"))
            {
                IsAssign(tokens).ForEach(bytecode => list.Add(bytecode));
                return list; 
            }
            else if (tokens[index].Token.Equals("TOKEN.IF"))
            {
                return IsIf(tokens);
            }
            else if (tokens[index].Token.Equals("TOKEN.EOF"))
            {
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                list.Add(CreateBytecodeFoundObject(opName));
                return list;
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
                var list = GetAssignBytecodeFound(tokens, index);
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

            GetAssignBytecodeFound(tokens, 1).ForEach(bytecode => bytecodeFoundList.Add(bytecode));

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

        private List<BytecodeFound> GetAssignBytecodeFound(List<TokensFound> tokens, int index)
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
                if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.IGUAL")) || tokens[index + 1].Token.Equals("TOKEN.IGUAL"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                    bytecodeFoundList.Add(bytecodeFound);
                    return bytecodeFoundList;
                }
                else if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.MAIS")) || tokens[index + 1].Token.Equals("TOKEN.MAIS"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFound.OpName = opName;
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                    bytecodeFoundList.Add(bytecodeFound);

                    bytecodeFound = new BytecodeFound();
                    OperationsName.OperationsNameList.TryGetValue(index >= 0 ? tokens[indexAux].Token : tokens[index + 1].Token, out opName);
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
            Size = 1;
            IfStack.Push(true);

            for (int index = 1; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetIfBytecodeFound(tokens, index);
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

            OperationsName.OperationsNameList.TryGetValue("TOKEN.IF", out string opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

            return bytecodeFoundList;
        }

        private List<BytecodeFound> GetIfBytecodeFound(List<TokensFound> tokens, int index)
        {
            var bytecodeFoundList = new List<BytecodeFound>();

            if (tokens[index].Token.Equals("TOKEN.ID"))
            {
                if (IsBinaryOperation(tokens[index + 1].Token) || IsConditionalExpression(tokens[index + 1].Token))
                {
                    if (tokens[index + 2].Token.Equals("TOKEN.ID") || IsType(tokens[index + 2].Token))
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                        OperationsName.OperationsNameList.TryGetValue(tokens[index + 2].Token, out opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 2].Lexema));
                        OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, IsConditionalExpression(tokens[index + 1].Token) ? tokens[index + 1].Lexema : null));
                        return bytecodeFoundList;
                    }
                }
            }
            else if (IsConditionalExpression(tokens[index].Token))
            {
                if(IsType(tokens[index + 1].Token))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if(tokens[index + 1].Token.Equals("TOKENS.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
            }
            else if (IsAndOr(tokens[index].Token))
            {
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
                return bytecodeFoundList;
            }

            return null;
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

        private bool IsConditionalExpression(string token)
        {
            return token.Equals("TOKEN.IGUAL_IGUAL") || token.Equals("TOKEN.DIFERENTE") ||
                   token.Equals("TOKEN.MENOR_IGUAL") || token.Equals("TOKEN.MAIOR_IGUAL") ||
                   token.Equals("TOKEN.MAIOR") || token.Equals("TOKEN.MENOR");            
        }

        private bool IsAndOr(string token)
        {
            return token.Equals("TOKEN.AND") || token.Equals("TOKEN.OR");
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

        private BytecodeFound CreateBytecodeFoundObject(string opName, string friendlyInterpretation = null)
        {
            return new BytecodeFound
            {
                OpName = opName,
                FriendlyInterpretation = friendlyInterpretation
            };
        }
    }
}
