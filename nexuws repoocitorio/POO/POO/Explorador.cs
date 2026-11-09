using System;

namespace ProyectoNEXUS
{
    public class Explorador
    {
        private string nombre;
        private int edad;
        private int energia;
        private int estabilidad;

        public string Nombre => nombre;
        public int Edad => edad;

        public int Energia
        {
            get => energia;
            private set => energia = Math.Clamp(value, 0, 100);
        }

        public int Estabilidad
        {
            get => estabilidad;
            private set => estabilidad = Math.Clamp(value, 0, 100);
        }

        public Explorador(string nombre, int edad, int energiaInicial, int estabilidadInicial)
        {
            this.nombre = string.IsNullOrWhiteSpace(nombre) ? "Cadete Anónimo" : nombre;
            this.edad = Math.Clamp(edad, 18, 80);
            this.energia = Math.Clamp(energiaInicial, 0, 100);
            this.estabilidad = Math.Clamp(estabilidadInicial, 0, 100);
        }

        public bool PuedeIngresar()
        {
            return energia >= 20 && estabilidad >= 30;
        }

        public void ModificarEnergia(int cantidad)
        {
            Energia += cantidad;
        }

        public void ModificarEstabilidad(int cantidad)
        {
            Estabilidad += cantidad;
        }

        public void RecargarEnergia(int cantidad)
        {
            Energia += cantidad;
        }
    }
}