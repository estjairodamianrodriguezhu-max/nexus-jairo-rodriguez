using System;

namespace ProyectoNEXUS
{
    public class Anomalia
    {
        public string Tipo { get; private set; }
        public int NivelInterferencia { get; private set; }

        public Anomalia(string tipo, int nivelInterferencia)
        {
            Tipo = tipo;
            NivelInterferencia = Math.Clamp(nivelInterferencia, 1, 100);
        }

        public void ReducirInterferencia(int cantidad)
        {
            NivelInterferencia = Math.Max(0, NivelInterferencia - cantidad);
        }
    }
}