using BenchmarkDotNet.Running;
using System;

namespace TdcFloripa2020
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var summaryArray = BenchmarkRunner.Run<CodigoBarras>();

            //var codBarras = new CodigoBarras();
            //codBarras.Barra = "39993000000000014993739040736027668911000002";
            //codBarras.CalcularLinhaSpan1();
            Console.ReadLine();
        }
    }
}
