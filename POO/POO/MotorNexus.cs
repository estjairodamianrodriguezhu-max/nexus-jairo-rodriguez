using System;

namespace ProyectoNEXUS
{
    public class MotorNexus
    {
        private Explorador usuario;
        private Realidad[] realidades;
        private Anomalia anomaliaActual;

        public MotorNexus(Explorador usuario)
        {
            this.usuario = usuario;
            this.anomaliaActual = new Anomalia("Interferencia Cuántica IRIS", 40);
            InicializarRealidades();
        }

        private void InicializarRealidades()
        {
            realidades = new Realidad[]
            {
        new Realidad("IRIS-07", "Valle de Cristal", new string[,] {
            { "·", "·", "◆", "·", "·", "·", "?", "?", "?", "?" },
            { "·", "⌂", "·", "·", "⚠", "·", "·", "◆", "?", "?" },
            { "·", "·", "📍", "·", "·", "●", "·", "·", "·", "?" },
            { "≈", "≈", "·", "·", "·", "·", "⚠", "·", "·", "·" },
            { "≈", "≈", "≈", "·", "·", "◆", "·", "·", "⌂", "·" }
        }),
        new Realidad("IRIS-12", "Sector Abisal", new string[,] {
            { "≈", "≈", "≈", "≈", "·", "·", "◆", "·", "?", "?" },
            { "≈", "⌂", "·", "≈", "≈", "⚠", "·", "·", "?", "?" },
            { "·", "·", "📍", "·", "≈", "≈", "●", "·", "·", "·" },
            { "·", "⚠", "·", "·", "·", "≈", "≈", "≈", "◆", "·" },
            { "·", "·", "◆", "·", "·", "·", "≈", "≈", "⌂", "·" }
        })
            };
        }

        public void IniciarSimulacion()
        {
            bool activo = true;
            while (activo)
            {
                Console.Clear();
                MostrarPanelUI();
                Console.WriteLine("1. Explorar Realidad Dimensional");
                Console.WriteLine("2. Analizar e Intervenir Anomalía IRIS");
                Console.WriteLine("3. Ver Mapas Cartografiados");
                Console.WriteLine("4. Recargar Energía Vital");
                Console.WriteLine("0. Salir de NEXUS");
                Console.Write("\nSeleccione operación: ");

                string opcion = Console.ReadLine()?.Trim() ?? "";
                switch (opcion)
                {
                    case "1":
                        if (usuario.Energia >= 20)
                        {
                            usuario.ModificarEnergia(-15);
                            usuario.ModificarEstabilidad(-5);
                            Console.WriteLine("\n[ÉXITO]: Inmersión completada con éxito.");
                        }
                        else
                        {
                            Console.WriteLine("\n[ADVERTENCIA]: Energía insuficiente.");
                        }
                        break;
                    case "2":
                        if (usuario.Energia >= 15)
                        {
                            usuario.ModificarEnergia(-10);
                            anomaliaActual.ReducirInterferencia(15);
                            Console.WriteLine($"\n[ANÁLISIS]: Anomalía {anomaliaActual.Tipo} reducida a {anomaliaActual.NivelInterferencia}%.");
                        }
                        break;
                    case "3":
                        MostrarSubmenuMapas();
                        break;
                    case "4":
                        usuario.RecargarEnergia(30);
                        Console.WriteLine("\n[RECARGA]: Reservas energéticas restauradas.");
                        break;
                    case "0":
                        activo = false;
                        break;
                }

                if (activo)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }

        private void MostrarSubmenuMapas()
        {
            Console.Clear();
            Console.WriteLine("--- REALIDADES REGISTRADAS EN NEXUS ---");
            for (int i = 0; i < realidades.Length; i++)
            {
                Console.WriteLine($"[{i + 1}] {realidades[i].Codigo} - {realidades[i].Nombre}");
            }
            Console.Write("\nSeleccione mapa a visualizar (0 para cancelar): ");
            if (int.TryParse(Console.ReadLine(), out int sel) && sel > 0 && sel <= realidades.Length)
            {
                realidades[sel - 1].MostrarMapa();
            }
        }

        private void MostrarPanelUI()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("             SISTEMA DE SIMULACIÓN NEXUS          ");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            Console.WriteLine($"CADETE     : {usuario.Nombre} | EDAD: {usuario.Edad}");
            Console.WriteLine($"ENERGÍA    : {usuario.Energia}% | ESTABILIDAD: {usuario.Estabilidad}%");
            Console.WriteLine($"ANOMALÍA   : {anomaliaActual.Tipo} ({anomaliaActual.NivelInterferencia}% de peligro)");
            Console.WriteLine("--------------------------------------------------");
        }
    }
}