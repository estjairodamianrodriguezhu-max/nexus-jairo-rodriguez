using System;

namespace ProyectoNEXUS
{
    public class Realidad
    {
        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        private string[,] mapa;

        public Realidad(string codigo, string nombre, string[,] mapa)
        {
            Codigo = codigo;
            Nombre = nombre;
            this.mapa = mapa;
        }

        public void MostrarMapa()
        {
            Console.Clear();
            int filas = mapa.GetLength(0);
            int columnas = mapa.GetLength(1);
            int totalCasillas = filas * columnas;
            int casillasExploradas = 0;

            for (int f = 0; f < filas; f++)
            {
                for (int c = 0; c < columnas; c++)
                {
                    if (mapa[f, c] != "?") casillasExploradas++;
                }
            }

            double porcentaje = ((double)casillasExploradas / totalCasillas) * 100;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ REALIDAD ENFOCADA: {Codigo} // {Nombre,-27} ║");
            Console.WriteLine($"║ CARTOGRAFÍA REGISTRADA: {porcentaje,5:F1}% ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");

            for (int f = 0; f < filas; f++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"║ {f} ");
                for (int c = 0; c < columnas; c++)
                {
                    string elemento = mapa[f, c];
                    Console.Write($" {elemento} ");
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(" ║");
            }

            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine("\nPresione cualquier tecla para regresar...");
            Console.ReadKey();
        }
    }
}