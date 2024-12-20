using System;
using System.Collections.Generic;

namespace TiendaAPI.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaCompra { get; set; }

    public decimal Total { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
