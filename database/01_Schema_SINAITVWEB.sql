-- ============================================================================
-- SINAITVWEB - Base de Datos Central de Marcajes y Asistencia
-- Servidor: SQL Server | Integrado con Profit Nómina 3.1 (MasterProfitPro)
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SINAITVDB')
BEGIN
    CREATE DATABASE SINAITVDB;
END
GO

USE SINAITVDB;
GO

-- 1. Tabla de Roles para Control de Acceso en el Sistema Web
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RolID INT IDENTITY(1,1) PRIMARY KEY,
        NombreRol VARCHAR(50) NOT NULL UNIQUE, -- 'SoloLectura', 'Analista', 'Administrador'
        Descripcion VARCHAR(255) NULL
    );

    INSERT INTO Roles (NombreRol, Descripcion) VALUES 
    ('SoloLectura', 'Usuario auditor: Solo visualización de marcajes y reportes'),
    ('Analista', 'Usuario operativo: Consulta e inserción/modificación de justificaciones'),
    ('Administrador', 'Acceso total al sistema y configuración');
END
GO

-- 2. Permisos de Usuarios de Profit (Asocia Cod_Usuario de MasterProfitPro.dbo.MpUsuario a un Rol)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsuariosRoles')
BEGIN
    CREATE TABLE UsuariosRoles (
        Cod_Usuario CHAR(6) PRIMARY KEY, -- Relacionado con MasterProfitPro.dbo.MpUsuario.Cod_Usuario
        RolID INT NOT NULL FOREIGN KEY REFERENCES Roles(RolID),
        FechaAsignacion DATETIME DEFAULT GETDATE()
    );
END
GO

-- 3. Mapeo de Empleados entre Profit (snemple) y el Biométrico (Qclear-TC)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MapeoBiometricoEmpleado')
BEGIN
    CREATE TABLE MapeoBiometricoEmpleado (
        MapeoID INT IDENTITY(1,1) PRIMARY KEY,
        ID_Biometrico INT NOT NULL UNIQUE, -- ID del usuario registrado en el lector biométrico
        Cod_Empresa VARCHAR(20) NOT NULL,  -- Ej: 'TRANS_NNU' o 'SERPRA_N'
        Cod_Emp CHAR(17) NOT NULL,         -- Corresponde a snemple.cod_emp en la BD de la empresa
        Cedula VARCHAR(15) NOT NULL,        -- Corresponde a snemple.ci
        Estado BIT DEFAULT 1,              -- 1: Activo, 0: Inactivo
        CONSTRAINT UQ_Empresa_Empleado UNIQUE (Cod_Empresa, Cod_Emp)
    );
END
GO

-- 4. Tabla Central de Marcajes Brutos (Cargados por el servicio del biométrico)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MarcajesBrutos')
BEGIN
    CREATE TABLE MarcajesBrutos (
        MarcajeID BIGINT IDENTITY(1,1) PRIMARY KEY,
        ID_Biometrico INT NOT NULL,
        FechaHora DATETIME NOT NULL,
        TipoEvento INT DEFAULT 0, -- 0: Chequeo entrada/salida
        Origen VARCHAR(20) DEFAULT 'Biometrico',
        FechaRegistro DATETIME DEFAULT GETDATE()
    );
    
    CREATE NONCLUSTERED INDEX IX_Marcajes_Biometrico_Fecha ON MarcajesBrutos(ID_Biometrico, FechaHora);
END
GO

-- 5. Tabla de Justificaciones / Permisos (Insertados por Analistas)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Justificaciones')
BEGIN
    CREATE TABLE Justificaciones (
        JustificacionID INT IDENTITY(1,1) PRIMARY KEY,
        Cod_Empresa VARCHAR(20) NOT NULL,
        Cod_Emp CHAR(17) NOT NULL,
        FechaInicio DATETIME NOT NULL,
        FechaFin DATETIME NOT NULL,
        Motivo VARCHAR(255) NOT NULL,
        Cod_Usuario CHAR(6) NOT NULL, -- Usuario de Profit que registra la acción (MpUsuario)
        FechaCreacion DATETIME DEFAULT GETDATE()
    );
END
GO

-- 6. Tabla de Logs de Auditoría
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditoriaLogs')
BEGIN
    CREATE TABLE AuditoriaLogs (
        LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
        Cod_Usuario CHAR(6) NOT NULL,
        Accion VARCHAR(50) NOT NULL, -- 'INSERT', 'UPDATE', 'DELETE'
        TablaAfectada VARCHAR(50) NOT NULL,
        Detalle VARCHAR(MAX) NULL,
        FechaHora DATETIME DEFAULT GETDATE()
    );
END
GO