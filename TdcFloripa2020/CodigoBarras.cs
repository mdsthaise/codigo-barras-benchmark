using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TdcFloripa2020
{
    [MemoryDiagnoser]
    public class CodigoBarras
    {
        private const string espaco = " ";
        private const string ponto = ".";

        [Params("39993000000000014993739040736027668911000002")]
        public string Barra { get; set; }

        #region Código Original
        [Benchmark]
        public string CalcularLinhaOriginal()
        {
            var campo1 = new StringBuilder();
            var campo2 = new StringBuilder();
            var campo3 = new StringBuilder();
            var linhaDigitavel = new StringBuilder();

            // primeiro conjunto
            campo1.Append(Barra.Substring(0, 4));
            campo1.Append(Barra.Substring(19, 5));
            linhaDigitavel.Append(campo1.ToString());
            linhaDigitavel.Append(CalcularModulo10Original(campo1.ToString()));
            linhaDigitavel.Insert(5, '.');
            linhaDigitavel.Append(' ');

            // segundo conjunto
            campo2.Append(Barra.Substring(24, 10));
            linhaDigitavel.Append(campo2.ToString());
            linhaDigitavel.Append(CalcularModulo10Original(campo2.ToString()));
            linhaDigitavel.Insert(17, '.');
            linhaDigitavel.Append(' ');

            // terceiro conjunto
            campo3.Append(Barra.Substring(34, 10));
            linhaDigitavel.Append(campo3.ToString());
            linhaDigitavel.Append(CalcularModulo10Original(campo3.ToString()));
            linhaDigitavel.Insert(30, '.');
            linhaDigitavel.Append(' ');

            // dv geral do código de barras
            linhaDigitavel.Append(Barra.Substring(4, 1));
            linhaDigitavel.Append(' ');

            // vencimento e valor
            if ((Barra.Substring(5, 14).Length < 5) || (Barra.Substring(5, 14) == string.Format("").PadLeft(14, '0')))
                linhaDigitavel.Append("000");
            else
                linhaDigitavel.Append(Barra.Substring(5, 14));

            return linhaDigitavel.ToString();
        }

        private string CalcularModulo10Original(string numero)
        {
            try
            {
                var valores = numero.ToCharArray();
                var peso = 2;
                var soma = 0;

                var builder = new StringBuilder();

                for (var i = valores.Length - 1; i >= 0; i--)
                {
                    builder.Append(int.Parse(valores[i].ToString()) * peso);

                    peso = peso == 1 ? 2 : 1;
                }

                valores = builder.ToString().ToCharArray();

                for (var i = 0; i <= valores.Length - 1; i++)
                {
                    soma += int.Parse(valores[i].ToString());
                }

                soma = 10 - (soma % 10);

                if (soma <= 9)
                {
                    return soma.ToString();
                }

                return "0";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro no Modulo10 ({numero})!", ex);
            }
        }
        #endregion

        #region Código Intermediário
        [Benchmark]
        public string CalcularLinhaAlterado()
        {
            ReadOnlySpan<char> barraSpan = Barra.AsSpan();
            Span<char> campo1 = stackalloc char[9];
            Span<char> campo2 = stackalloc char[10];
            Span<char> campo3 = stackalloc char[10];
            Span<char> linhaDigitavel = stackalloc char[54];

            // primeiro conjunto
            barraSpan.Slice(0, 4).CopyTo(campo1);
            barraSpan.Slice(19, 5).CopyTo(campo1.Slice(4));

            barraSpan.Slice(0, 4).CopyTo(linhaDigitavel);
            barraSpan.Slice(19, 1).CopyTo(linhaDigitavel.Slice(4));
            ponto.AsSpan().CopyTo(linhaDigitavel.Slice(5));
            barraSpan.Slice(20, 4).CopyTo(linhaDigitavel.Slice(6));
            CalcularModulo10Alterado(campo1).AsSpan().CopyTo(linhaDigitavel.Slice(10));
            espaco.AsSpan().CopyTo(linhaDigitavel.Slice(11));

            // segundo conjunto
            barraSpan.Slice(24, 10).CopyTo(campo2);

            barraSpan.Slice(24, 5).CopyTo(linhaDigitavel.Slice(12));
            ponto.AsSpan().CopyTo(linhaDigitavel.Slice(17));
            barraSpan.Slice(29, 5).CopyTo(linhaDigitavel.Slice(18));
            CalcularModulo10Alterado(campo2).AsSpan().CopyTo(linhaDigitavel.Slice(23));
            espaco.AsSpan().CopyTo(linhaDigitavel.Slice(24));

            // terceiro conjunto
            barraSpan.Slice(34, 10).CopyTo(campo3);

            barraSpan.Slice(34, 5).CopyTo(linhaDigitavel.Slice(25));
            ponto.AsSpan().CopyTo(linhaDigitavel.Slice(30));
            barraSpan.Slice(39, 5).CopyTo(linhaDigitavel.Slice(31));
            CalcularModulo10Alterado(campo3).AsSpan().CopyTo(linhaDigitavel.Slice(36));
            espaco.AsSpan().CopyTo(linhaDigitavel.Slice(37));

            // dv geral do código de barras
            barraSpan.Slice(4, 1).CopyTo(linhaDigitavel.Slice(38));
            espaco.AsSpan().CopyTo(linhaDigitavel.Slice(39));

            // vencimento e valor
            if ((barraSpan.Slice(5, 14) == string.Format("").PadLeft(14, '0')))
            {
                Span<char> zeros = stackalloc char[3];
                zeros.Fill('0');
                zeros.CopyTo(linhaDigitavel.Slice(40));
            }
            else
                barraSpan.Slice(5, 14).CopyTo(linhaDigitavel.Slice(40));

            return linhaDigitavel.ToString();
        }

        private string CalcularModulo10Alterado(ReadOnlySpan<char> valoresSpan)
        {
            try
            {
                var peso = 2;
                var soma = 0;

                Span<int> builderSpan = stackalloc int[12];
                int j = 0;
                for (var i = valoresSpan.Length - 1; i >= 0; i--)
                {
                    int mult = int.Parse(valoresSpan.Slice(i, 1)) * peso;
                    if (mult > 9)
                    {
                        builderSpan.Slice(j).Fill(1);
                        j++;
                        builderSpan.Slice(j).Fill(mult - 10);
                    }
                    else
                        builderSpan.Slice(j).Fill(mult);

                    peso = peso == 1 ? 2 : 1;
                    j++;
                }

                for (int i = 0; j > i; i++)
                    soma += builderSpan[i];

                soma = 10 - (soma % 10);

                if (soma <= 9)
                    return soma.ToString();

                return "0";
            }
            catch (Exception ex)
            {
                throw new Exception("Erro no Modulo10!", ex);
            }
        }
        #endregion
    }
}