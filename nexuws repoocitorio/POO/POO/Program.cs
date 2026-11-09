using System;

namespace ProyectoNEXUS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "NEXUS SYSTEM 1.1 - 100% POO";

            Console.Write("Ingrese Nombre del Explorador: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Ingrese Edad: ");
            int.TryParse(Console.ReadLine(), out int edad);

            Explorador cadete = new Explorador(nombre, edad, 80, 90);

            if (!cadete.PuedeIngresar())
            {
                Console.WriteLine("[ERROR]: El cadete no cumple las condiciones mínimas para entrar a NEXUS.");
                return;
            }

            MotorNexus nexus = new MotorNexus(cadete);
            nexus.IniciarSimulacion();
        }
    }
}