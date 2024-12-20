create database BDTIENDATELAS
GO

use BDTIENDATELAS
go

-- Creación de las tablas
CREATE TABLE usuarios (
    id_usuario INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(100) NOT NULL,
    correo NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE telas (
    cod_tel CHAR(5) PRIMARY KEY,
    tip_tel NVARCHAR(50) NOT NULL,
    col_tel NVARCHAR(30) NOT NULL,
    pre_tel DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL
);

CREATE TABLE carrito (
    id_carrito INT PRIMARY KEY IDENTITY(1,1),
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    estado NVARCHAR(20) DEFAULT 'pendiente',
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE detalle_carrito (
    id_detalle INT PRIMARY KEY IDENTITY(1,1),
    id_carrito INT NOT NULL,
    cod_tel CHAR(5) NOT NULL,
    cantidad INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (id_carrito) REFERENCES carrito(id_carrito),
    FOREIGN KEY (cod_tel) REFERENCES telas(cod_tel)
);

CREATE TABLE compras (
    id_compra INT PRIMARY KEY IDENTITY(1,1),
    id_usuario INT NOT NULL,
    fecha_compra DATETIME DEFAULT GETDATE(),
    total DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

-- Store procedure para listar las telas
CREATE PROCEDURE sp_ListarTelas
AS
BEGIN
    SELECT cod_tel, tip_tel, col_tel, pre_tel, stock
    FROM telas;
END;


-- Store procedure para buscar una tela por ID
CREATE PROCEDURE sp_BuscarTelaPorID
    @cod_tel CHAR(5)
AS
BEGIN
    SELECT cod_tel, tip_tel, col_tel, pre_tel, stock
    FROM telas
    WHERE cod_tel = @cod_tel;
END;


-- Store procedure para buscar telas por las iniciales de tip_tel
CREATE PROCEDURE sp_BuscarTelaPorIniciales
    @tip_tel_iniciales NVARCHAR(50)
AS
BEGIN
    SELECT cod_tel, tip_tel, col_tel, pre_tel, stock
    FROM telas
    WHERE tip_tel LIKE @tip_tel_iniciales + '%';
END;

----------------------------------------------------------------------------

CREATE PROCEDURE sp_AgregarProductoAlCarrito
    @id_usuario INT,
    @cod_tel CHAR(5),
    @cantidad INT
AS
BEGIN
    DECLARE @stock_disponible INT;
    
    -- Verificar el stock disponible
    SELECT @stock_disponible = stock
    FROM telas
    WHERE cod_tel = @cod_tel;

    IF @stock_disponible >= @cantidad
    BEGIN
        -- Verificar si ya existe un carrito pendiente para el usuario
        IF NOT EXISTS (SELECT 1 FROM carrito WHERE id_usuario = @id_usuario AND estado = 'pendiente')
        BEGIN
            -- Crear un carrito nuevo si no existe
            INSERT INTO carrito (id_usuario, estado) VALUES (@id_usuario, 'pendiente');
        END
        
        DECLARE @id_carrito INT;
        -- Obtener el id del carrito pendiente
        SELECT @id_carrito = id_carrito
        FROM carrito
        WHERE id_usuario = @id_usuario AND estado = 'pendiente';

        -- Insertar en detalle_carrito
        INSERT INTO detalle_carrito (id_carrito, cod_tel, cantidad, subtotal)
        VALUES (@id_carrito, @cod_tel, @cantidad, (SELECT pre_tel FROM telas WHERE cod_tel = @cod_tel) * @cantidad);

        -- Actualizar el stock de la tela
        UPDATE telas
        SET stock = stock - @cantidad
        WHERE cod_tel = @cod_tel;
    END
    ELSE
    BEGIN
        RAISERROR('No hay suficiente stock de la tela seleccionada.', 16, 1);
    END
END;

---------------------------------------------------------------------------
CREATE PROCEDURE sp_ActualizarCantidadProductoEnCarrito
    @id_usuario INT,
    @cod_tel CHAR(5),
    @nueva_cantidad INT
AS
BEGIN
    DECLARE @stock_disponible INT, @id_carrito INT, @cantidad_actual INT, @subtotal DECIMAL(10,2);
    
    -- Verificar stock disponible
    SELECT @stock_disponible = stock
    FROM telas
    WHERE cod_tel = @cod_tel;

    IF @stock_disponible >= @nueva_cantidad
    BEGIN
        -- Obtener el id del carrito pendiente
        SELECT @id_carrito = id_carrito
        FROM carrito
        WHERE id_usuario = @id_usuario AND estado = 'pendiente';

        -- Verificar si el producto ya está en el carrito
        SELECT @cantidad_actual = cantidad
        FROM detalle_carrito
        WHERE id_carrito = @id_carrito AND cod_tel = @cod_tel;

        IF @cantidad_actual IS NOT NULL
        BEGIN
            -- Actualizar la cantidad y subtotal
            UPDATE detalle_carrito
            SET cantidad = @nueva_cantidad, 
                subtotal = (SELECT pre_tel FROM telas WHERE cod_tel = @cod_tel) * @nueva_cantidad
            WHERE id_carrito = @id_carrito AND cod_tel = @cod_tel;
            
            -- Actualizar stock de la tela
            UPDATE telas
            SET stock = stock + @cantidad_actual - @nueva_cantidad
            WHERE cod_tel = @cod_tel;
        END
        ELSE
        BEGIN
            RAISERROR('El producto no existe en el carrito.', 16, 1);
        END
    END
    ELSE
    BEGIN
        RAISERROR('No hay suficiente stock de la tela seleccionada.', 16, 1);
    END
END;

---------------------------------------------------------------------------

CREATE PROCEDURE sp_EliminarProductoDelCarrito
    @id_usuario INT,
    @cod_tel CHAR(5)
AS
BEGIN
    DECLARE @id_carrito INT, @cantidad INT;
    
    -- Obtener el id del carrito pendiente
    SELECT @id_carrito = id_carrito
    FROM carrito
    WHERE id_usuario = @id_usuario AND estado = 'pendiente';

    -- Verificar si el producto está en el carrito
    SELECT @cantidad = cantidad
    FROM detalle_carrito
    WHERE id_carrito = @id_carrito AND cod_tel = @cod_tel;

    IF @cantidad IS NOT NULL
    BEGIN
        -- Eliminar el producto del carrito
        DELETE FROM detalle_carrito
        WHERE id_carrito = @id_carrito AND cod_tel = @cod_tel;
        
        -- Restaurar el stock de la tela
        UPDATE telas
        SET stock = stock + @cantidad
        WHERE cod_tel = @cod_tel;
    END
    ELSE
    BEGIN
        RAISERROR('El producto no existe en el carrito.', 16, 1);
    END
END;

---------------------------------------------------------------------------

CREATE PROCEDURE sp_RealizarCompra
    @id_usuario INT
AS
BEGIN
    DECLARE @id_carrito INT, @total DECIMAL(10,2) = 0;

    -- Obtener el carrito pendiente
    SELECT @id_carrito = id_carrito
    FROM carrito
    WHERE id_usuario = @id_usuario AND estado = 'pendiente';

    IF @id_carrito IS NOT NULL
    BEGIN
        -- Calcular el total de la compra
        SELECT @total = SUM(subtotal)
        FROM detalle_carrito
        WHERE id_carrito = @id_carrito;

        -- Crear un registro de compra
        INSERT INTO compras (id_usuario, total)
        VALUES (@id_usuario, @total);

        -- Actualizar el estado del carrito a 'comprado'
        UPDATE carrito
        SET estado = 'comprado'
        WHERE id_carrito = @id_carrito;

        -- Actualizar el stock de las telas en el carrito
        DECLARE @cod_tel CHAR(5), @cantidad INT;
        DECLARE carrito_cursor CURSOR FOR
        SELECT cod_tel, cantidad
        FROM detalle_carrito
        WHERE id_carrito = @id_carrito;

        OPEN carrito_cursor;
        FETCH NEXT FROM carrito_cursor INTO @cod_tel, @cantidad;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            UPDATE telas
            SET stock = stock - @cantidad
            WHERE cod_tel = @cod_tel;

            FETCH NEXT FROM carrito_cursor INTO @cod_tel, @cantidad;
        END;

        CLOSE carrito_cursor;
        DEALLOCATE carrito_cursor;

        -- Eliminar los detalles del carrito
        DELETE FROM detalle_carrito
        WHERE id_carrito = @id_carrito;
    END
    ELSE
    BEGIN
        RAISERROR('No hay carrito pendiente para este usuario.', 16, 1);
    END
END;

-----------------------------------------------------------

CREATE PROCEDURE sp_ListarDetalleCarritoPendiente
    @id_usuario INT
AS
BEGIN
    SELECT 
        dc.id_detalle AS IdDetalle,
        dc.id_carrito AS IdCarrito,
        dc.cod_tel AS CodigoTela,
        t.tip_tel AS TipoTela,
        t.col_tel AS ColorTela,
        dc.cantidad AS Cantidad,
        dc.subtotal AS Subtotal,
        t.pre_tel AS PrecioTela
    FROM 
        detalle_carrito dc
    INNER JOIN 
        carrito c ON dc.id_carrito = c.id_carrito
    INNER JOIN 
        telas t ON dc.cod_tel = t.cod_tel
    WHERE 
        c.id_usuario = @id_usuario
        AND c.estado = 'pendiente';
END;
