using System;
using System.Collections.Generic;

namespace TiendaAPI.Models;

public partial class DetalleCarrito
{
    public int IdDetalle { get; set; }

    public int IdCarrito { get; set; }

    public string CodTel { get; set; } = null!;

    public int Cantidad { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Tela CodTelNavigation { get; set; } = null!;

    public virtual Carrito IdCarritoNavigation { get; set; } = null!;
}
