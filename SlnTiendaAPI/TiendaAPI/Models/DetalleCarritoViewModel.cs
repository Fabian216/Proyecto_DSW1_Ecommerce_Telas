namespace TiendaAPI.Models
{
    public class DetalleCarritoViewModel
    {
        public int IdDetalle { get; set; }
        public int IdCarrito { get; set; }
        public string CodigoTela { get; set; } = null!;
        public string TipoTela { get; set; } = null!;
        public string ColorTela { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PrecioTela { get; set; }
    }
}
