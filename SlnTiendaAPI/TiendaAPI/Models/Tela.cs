using System;
using System.Collections.Generic;

namespace TiendaAPI.Models;

public partial class Tela
{
    public string CodTel { get; set; } = null!;

    public string TipTel { get; set; } = null!;

    public string ColTel { get; set; } = null!;

    public decimal PreTel { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<DetalleCarrito> DetalleCarritos { get; set; } = new List<DetalleCarrito>();
}
