use BDTIENDATELAS
go

insert into telas values ('T0001','Gamuza','rojo','600',10)
go
insert into telas values ('T0002','Gamuza','verde','580',3)
go
insert into usuarios values ('fabian','fabian@gmail.com')
go

select * from telas
select * from usuarios
select * from detalle_carrito
select * from carrito
select * from compras

exec sp_ListarTelas

exec sp_BuscarTelaPorIniciales @tip_tel_iniciales = 'Gam';

exec sp_BuscarTelaPorID @cod_tel = 'T0001';

--------------------------PRUEBAS-----------------------------------
-- Agregar unidades al carrito del usuario 1
EXEC sp_AgregarProductoAlCarrito @id_usuario = 1, @cod_tel = 'T0001', @cantidad = 1;

-- Ver el contenido del carrito del usuario 1
EXEC sp_ListarDetalleCarritoPendiente @id_usuario = 1;

drop procedure sp_ListarDetalleCarritoPendiente

-- Actualizar la cantidad de T0001 (gamuza rojo) en el carrito del usuario 1
EXEC sp_ActualizarCantidadProductoEnCarrito @id_usuario = 1, @cod_tel = 'T0001', @nueva_cantidad = 2;

-- Eliminar T0001 (gamuza rojo) del carrito del usuario 1
EXEC sp_EliminarProductoDelCarrito @id_usuario = 1, @cod_tel = 'T0001';

-- Realizar la compra del usuario 1
EXEC sp_RealizarCompra @id_usuario = 1;