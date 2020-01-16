# Analisador Léxico e Sintático para Python
Trabalho realizado para disciplina de Linguagens Formais na Universidade de Caxias do Sul (UCS) - 2019/4.

### Objetivo
Fazer um programa que leia um arquivo fonte em python e efetue a análise léxica e sintática do mesmo.
O subconjunto da linguagem Python que deverá ser reconhecido é composto de:
1. Referência a variáveis inteiras, float e strings.
2. Expressões: Com a lista completa de operadores aritméticos, relacionais e lógicos.
3. Comandos de atribuição, while, for..range e if
4. Declarações e chamadas de Funções

### Funcionamento
O programa foi feito em C# usando o console. O arquivo de testes deverá estar na pasta C:\temp. Feito isso, existem as opções:
- Sintático: irá gerar um arquivo Saida.lex na pasta C:\temp contendo todos os tokens encontrados no código fonte - e após isso será realizada a análise sintática em cima desse arquivo gerado. 
- Léxico: irá apenas gerar o arquivo Saida.lex.
