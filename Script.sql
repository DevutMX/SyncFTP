create table Registros
(
    Movimiento integer primary key autoincrement,
    Equipo varchar(50) not null,
    SO varchar(50) not null,
    Fecha DateTime not null,
    Tipo varchar(20) not null,
    Desde varchar(50) not null,
    Hacia varchar(50) not null
);