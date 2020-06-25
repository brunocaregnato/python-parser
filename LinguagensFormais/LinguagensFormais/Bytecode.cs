﻿using System.Collections.Generic;
using System.Linq;

namespace LinguagensFormais
{
    public class Bytecode
    {
        public List<BytecodeFound> BytecodeFounds { get; private set; }
        private List<TokensFound> TokensList { get; set; }
        private OperationsName OperationsName { get; set; }
        private int Size { get; set; }
        private List<Arguments> ArgumentsList { get; set; }        
        private List<int> LineArgumentsList { get; set; } = new List<int>();
        private Stack<int> IdentLevel { get; set; } = new Stack<int>();
        private int PreviousLevel { get; set; }


        public Bytecode(List<TokensFound> tokensList)
        {
            TokensList = tokensList;
        }

        public bool BytecodeAnalysis()
        {
            BytecodeFounds = new List<BytecodeFound>();
            OperationsName = new OperationsName();
            ArgumentsList = new List<Arguments>();

            var address = 0;
            var lastLine = TokensList.Last();

            for (int line = 1; line <= lastLine.Line; line++)
            {
                foreach (var found in GetObjects(line))
                {
                    var bytecodeFound = new BytecodeFound
                    {
                        Line = line,
                        Address = address,
                        OpName = found.OpName,
                        Argument = found.Argument,
                        FriendlyInterpretation = found.FriendlyInterpretation
                    };

                    BytecodeFounds.Add(bytecodeFound);
                    address += 3;
                }
            }

            //Ajusta argumentos dos jumps       
            var withoutArguments = BytecodeFounds.Where(x => x.Argument.Trim().Equals("")).ToList();
            withoutArguments.ForEach(argument =>
            {
                var name = argument.OpName;

                if (name.Contains("POP") || name.Contains("JUMP"))
                {
                    if(LineArgumentsList.Count > 0)
                    {
                        var line = LineArgumentsList.First();
                        var allResults = BytecodeFounds.Where(x => x.Line.Equals(line)).ToList();
                        var firstChar = BytecodeFounds.Where(x => x.Line.Equals(line) && !x.OpName.Contains("JUMP")).FirstOrDefault();
                        if(firstChar != null)
                        {
                            if (name.Contains("POP") || name.Equals("JUMP_ABSOLUTE"))
                            {
                                argument.Argument = firstChar.Address.ToString();
                            }
                            else if (name.Equals("JUMP_FORWARD"))
                            {
                                argument.Argument = "0";
                                argument.FriendlyInterpretation = "to " + firstChar.Address.ToString();
                            }

                        }
                        LineArgumentsList.RemoveAt(0);
                    }
                }
            });


            return true;
        }

        private List<BytecodeFound> GetObjects(int line, int index = 0)
        {
            var tokens = TokensList.FindAll(x => x.Line.Equals(line));
            var list = new List<BytecodeFound>();
            bool hasPlaceJumpForward = false;
            
            while (tokens[index].Token.Equals("TOKEN.INDENT"))
            {
                index++;
                IdentLevel.Push(line);
                if (tokens[index].Token.Equals("TOKEN.ELSE"))
                {
                    return list;
                }
            }
            while (tokens[index].Token.Equals("TOKEN.DEDENT"))
            {

                index++;
                if (tokens[index].Token.Equals("TOKEN.ELSE"))
                {
                    if (IdentLevel.Count > 0) IdentLevel.Pop();
                    return list;
                }
                if (IdentLevel.Count > 0) IdentLevel.Pop();
                int i = tokens[index].Token.Equals("TOKEN.ELSE") ? 0 : IdentLevel.Count;
                InsertIntoLineArgumentsList(tokens[index].Line, i, tokens[index].Token);                

                if (tokens[index].Token.Equals("TOKEN.IF") || tokens[index].Token.Equals("TOKEN.ELIF") || tokens[index].Token.Equals("TOKEN.ELSE"))
                {
                    InsertIntoLineArgumentsList(tokens[index].Line, i, tokens[index].Token);
                    if (!hasPlaceJumpForward)
                    {
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_FORWARD",
                            Argument = ""
                            
                        });

                        hasPlaceJumpForward = true;
                    }
                    else
                    {
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_ABSOLUTE",
                            Argument = ""
                        });
                    }
                    
                }
                else if(!tokens[index].Token.Equals("TOKEN.DEDENT"))
                {
                    InsertIntoLineArgumentsList(tokens[index].Line,IdentLevel.Count);
                    list.Add(new BytecodeFound
                    {
                        OpName = "JUMP_FORWARD",
                        Argument = ""
                    });
                }
                else
                {
                    InsertIntoLineArgumentsList(tokens[index].Line, IdentLevel.Count);
                    list.Add(new BytecodeFound
                    {
                        OpName = "JUMP_ABSOLUTE",
                        Argument = ""
                    });
                }
            }
            if (tokens[index].Token.Equals("TOKEN.ELSE"))
            {
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.ID") && tokens[index + 1].Token.Equals("TOKEN.IGUAL"))
            {
                IsAssign(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.IF") || tokens[index].Token.Equals("TOKEN.ELIF"))
            {
                IsIf(tokens).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.WHILE"))
            {
                IsWhile(tokens).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.EOF"))
            {
                var endProgram = new BytecodeFound()
                {
                    Argument = "0",
                    OpName = "LOAD_CONST",
                    FriendlyInterpretation = "None"
                };

                list.Add(endProgram);
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                list.Add(CreateBytecodeFoundObject(opName));
                return list;
            }



            return list;
        }

        private List<BytecodeFound> IsAssign(List<TokensFound> tokens, int indented = 0)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;

            for (int index = indented + 2; index < tokens.Count; index += Size)
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

            GetAssignBytecodeFound(tokens, indented + 1).ForEach(bytecode => bytecodeFoundList.Add(bytecode));

            if (tokens[tokens.Count - 1].Token.Equals("TOKEN.EOF"))
            {
                var endProgram = new BytecodeFound()
                {
                    Argument = "0",
                    OpName = "LOAD_CONST",
                    FriendlyInterpretation = "None"
                };

                bytecodeFoundList.Add(endProgram);
                OperationsName.OperationsNameList.TryGetValue(tokens[tokens.Count - 1].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
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
                        bytecodeFound.Argument = GetArgument(opName, bytecodeFound.FriendlyInterpretation);
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
                bytecodeFound.Argument = GetArgument(opName2, bytecodeFound.FriendlyInterpretation);
                bytecodeFoundList.Add(bytecodeFound);
                return bytecodeFoundList;
            }

            if (tokens[index].Token.Equals("TOKEN.IGUAL"))
            {
                var indexAux = index - 1;
                if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[indexAux].Lexema));
                    return bytecodeFoundList;
                }
            }

            if (tokens[index].Token.Equals("TOKEN.ID"))
            {
                var indexAux = index - 1;
                if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.IGUAL")) || tokens[index + 1].Token.Equals("TOKEN.IGUAL"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.MAIS")) || tokens[index + 1].Token.Equals("TOKEN.MAIS"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));

                    OperationsName.OperationsNameList.TryGetValue(index >= 0 ? tokens[indexAux].Token : tokens[index + 1].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

                    return bytecodeFoundList;
                }
            }

            if (IsBinaryOperation(tokens[index].Token))
            {
                if (tokens[index + 1].Token.Equals("TOKEN.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName2);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName2, tokens[index + 1].Lexema));
                }

                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
                return bytecodeFoundList;
            }

            return null;
        }

        private List<BytecodeFound> IsIf(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;

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
                if (IsType(tokens[index + 1].Token))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if (tokens[index + 1].Token.Equals("TOKENS.ID"))
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

        
        private List<BytecodeFound> IsWhile(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;

            for (int index = 0; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetWhileBytecodeFound(tokens, index);
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

            return bytecodeFoundList;
        }

        private List<BytecodeFound> GetWhileBytecodeFound(List<TokensFound> tokens, int index)
        {
            /*
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
                if (IsType(tokens[index + 1].Token))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if (tokens[index + 1].Token.Equals("TOKENS.ID"))
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
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName + " (" + IfStack.Peek().ToString() + ")"));
                return bytecodeFoundList;
            }
            */
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
            switch (operation)
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

        private void InsertIntoLineArgumentsList(int line, int identLevel, string token = "")
        {


            if(LineArgumentsList.Count < 2 || (identLevel == 0 && !token.Equals("TOKEN.ELIF")))
            {
                LineArgumentsList.Add(line);
                PreviousLevel = identLevel;
            }
            else
            {
                if (token.Equals("TOKEN.ELIF"))
                {                    
                    if(PreviousLevel != identLevel)
                    {
                        LineArgumentsList.Sort();
                        LineArgumentsList.Insert(0, line);
                    }
                    else LineArgumentsList.Add(line);
                    PreviousLevel = identLevel;
                }
                else if (LineArgumentsList.Count % 2 == 0)
                {
                    LineArgumentsList.Insert(0, line);
                    PreviousLevel = identLevel;
                }
                else
                {
                    LineArgumentsList.Add(line);
                    if(IdentLevel.Count > 0) IdentLevel.Pop();
                }                              
            }
        }

        private BytecodeFound CreateBytecodeFoundObject(string opName, string friendlyInterpretation = null)
        {
            var argument = GetArgument(opName, friendlyInterpretation);
            return new BytecodeFound
            {
                OpName = opName,
                Argument = argument,
                FriendlyInterpretation = friendlyInterpretation
            };
        }

        private string GetArgument(string opName, string friendlyInterpretation)
        {
            string value = "";
            if (friendlyInterpretation == null) return value;

            Arguments argument;
            if (opName.Contains("FAST"))
            {
                argument = ArgumentsList.Where((Arguments x) => x.OpName.Contains("FAST") && x.Value.Equals(friendlyInterpretation)).LastOrDefault();
            }
            else
            {
                argument = ArgumentsList.Where((Arguments x) => x.OpName.Equals(opName) && x.Value.Equals(friendlyInterpretation)).LastOrDefault();
            }

            if (argument != null)
            {
                return argument.Argument;
            }

            Arguments lastValue;            
            if (opName.Contains("FAST"))
            {
                lastValue = ArgumentsList.Where(x => x.OpName.Contains("FAST")).LastOrDefault();
                value = lastValue == null ? "0" : (int.Parse(lastValue.Argument) + 1).ToString();
            }
            else
            {
                if (opName.Contains("CONST"))
                {
                    lastValue = ArgumentsList.Where(x => x.OpName.Equals(opName)).LastOrDefault();
                    value = lastValue == null ? "1" : (int.Parse(lastValue.Argument) + 1).ToString();
                }
                else if (opName.Contains("COMPARE"))
                {
                    lastValue = ArgumentsList.Where(x => x.OpName.Equals(opName)).LastOrDefault();
                    value = lastValue == null ? "0" : (int.Parse(lastValue.Argument) + 1).ToString();
                }
            }

            argument = new Arguments
            {
                OpName = opName,
                Value = friendlyInterpretation,
                Argument = value
            };

            ArgumentsList.Add(argument);

            return value;
        }
    }

    class Arguments
    {
        public string OpName { get; set; }
        public string Value { get; set; }
        public string Argument { get; set; }
    }
}
