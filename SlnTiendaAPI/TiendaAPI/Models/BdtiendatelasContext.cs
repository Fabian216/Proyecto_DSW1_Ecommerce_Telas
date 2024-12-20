using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TiendaAPI.Models;

public partial class BdtiendatelasContext : DbContext
{
    public BdtiendatelasContext()
    {
    }

    public BdtiendatelasContext(DbContextOptions<BdtiendatelasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCarrito> DetalleCarritos { get; set; }

    public virtual DbSet<Tela> Telas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    //public virtual DbSet<CarritoProducto> CarritoProductos { get; set; }

    

    // Metodo para la lista de todas las telas
    public async Task<List<Tela>> ListarTelasAsync()
    {
        // Ejecuta el procedimiento almacenado sp_ListarArticulosActivos
        return await this.Telas.FromSqlRaw("EXEC sp_ListarTelas").ToListAsync();
    }

    // buscar por id
    public async Task<Tela?> BuscarTelaPorIDAsync(string codTel)
    {
        var parametro = new SqlParameter("@cod_tel", codTel);

        // Ejecutamos el procedimiento almacenado
        var result = await this.Telas
            .FromSqlRaw("EXEC sp_BuscarTelaPorID @cod_tel", parametro)
            .ToListAsync();

        // Retornamos el primer resultado (si existe) o null si no hay resultados
        return result.FirstOrDefault();
    }

    //listar por iniciales
    public async Task<List<Tela>> BuscarTelaPorInicialesAsync(string iniciales)
    {
        var parametro = new SqlParameter("@tip_tel_iniciales", iniciales);

        // Ejecutamos el procedimiento almacenado
        return await this.Telas
            .FromSqlRaw("EXEC sp_BuscarTelaPorIniciales @tip_tel_iniciales", parametro)
            .ToListAsync();
    }

    // METODOS DE OPERACIONES------------------------------

    public async Task<string> AgregarProductoAlCarritoAsync(int idUsuario, string codTel, int cantidad)
    {
        var parametros = new[]
        {
        new SqlParameter("@id_usuario", idUsuario),
        new SqlParameter("@cod_tel", codTel),
        new SqlParameter("@cantidad", cantidad)
    };

        // Ejecutamos el procedimiento almacenado
        var resultado = await this.Database.ExecuteSqlRawAsync("EXEC sp_AgregarProductoAlCarrito @id_usuario, @cod_tel, @cantidad", parametros);

        // Si se ejecuta correctamente, el procedimiento no devuelve nada
        return "Producto agregado al carrito exitosamente";
    }

    //eliminar producto
    public async Task<string> EliminarProductoDelCarritoAsync(int idUsuario, string codTel)
    {
        var parametros = new[]
        {
        new SqlParameter("@id_usuario", idUsuario),
        new SqlParameter("@cod_tel", codTel)
    };

        // Ejecutamos el procedimiento almacenado
        var resultado = await this.Database.ExecuteSqlRawAsync("EXEC sp_EliminarProductoDelCarrito @id_usuario, @cod_tel", parametros);

        return "Producto eliminado del carrito exitosamente";
    }

    //actualizar 
    public async Task<string> ActualizarCantidadProductoEnCarritoAsync(int idUsuario, string codTel, int nuevaCantidad)
    {
        var parametros = new[]
        {
        new SqlParameter("@id_usuario", idUsuario),
        new SqlParameter("@cod_tel", codTel),
        new SqlParameter("@nueva_cantidad", nuevaCantidad)
    };

        // Ejecutamos el procedimiento almacenado
        var resultado = await this.Database.ExecuteSqlRawAsync("EXEC sp_ActualizarCantidadProductoEnCarrito @id_usuario, @cod_tel, @nueva_cantidad", parametros);

        // Si no hay errores, retornamos un mensaje de éxito
        return "Cantidad del producto actualizada exitosamente";
    }

    //listar detalle
    public async Task<List<DetalleCarritoViewModel>> ListarDetalleCarritoPendienteAsync(int idUsuario)
    {
        var parametro = new SqlParameter("@id_usuario", idUsuario);

        // Ejecutamos el procedimiento almacenado y mapeamos los resultados a DetalleCarritoViewModel
        var detallesCarrito = await this.Database
            .SqlQueryRaw<DetalleCarritoViewModel>("EXEC sp_ListarDetalleCarritoPendiente @id_usuario", parametro)
            .AsNoTracking() // No necesitamos hacer seguimiento de las entidades en este caso
            .ToListAsync();

        return detallesCarrito;
    }









    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("server=.;database=BDTIENDATELAS;integrated security=true;TrustServerCertificate=false;Encrypt=false;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.IdCarrito).HasName("PK__carrito__83A2AD9CE5C51CAA");

            entity.ToTable("carrito");

            entity.Property(e => e.IdCarrito).HasColumnName("id_carrito");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__carrito__id_usua__3E52440B");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PK__compras__C4BAA60434F1F060");

            entity.ToTable("compras");

            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_compra");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__compras__id_usua__45F365D3");
        });

        modelBuilder.Entity<DetalleCarrito>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__detalle___4F1332DED2560645");

            entity.ToTable("detalle_carrito");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CodTel)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("cod_tel");
            entity.Property(e => e.IdCarrito).HasColumnName("id_carrito");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.CodTelNavigation).WithMany(p => p.DetalleCarritos)
                .HasForeignKey(d => d.CodTel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__detalle_c__cod_t__4222D4EF");

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.DetalleCarritos)
                .HasForeignKey(d => d.IdCarrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__detalle_c__id_ca__412EB0B6");
        });

        modelBuilder.Entity<Tela>(entity =>
        {
            entity.HasKey(e => e.CodTel).HasName("PK__telas__F2805A6CE5FC255B");

            entity.ToTable("telas");

            entity.Property(e => e.CodTel)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("cod_tel");
            entity.Property(e => e.ColTel)
                .HasMaxLength(30)
                .HasColumnName("col_tel");
            entity.Property(e => e.PreTel)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pre_tel");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.TipTel)
                .HasMaxLength(50)
                .HasColumnName("tip_tel");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuarios__4E3E04AD70966B48");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "UQ__usuarios__2A586E0B5FA7AE8A").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
