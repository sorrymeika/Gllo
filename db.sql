create table City (
CityID int primary key,
CityName varchar(50),
ProvinceID int,
Sort int
)
create table Province (
ProvinceID int primary key,
ProvinceName varchar(50),
Sort int,
Memo varchar(20)
)
create table Region (
RegionID int primary key,
RegionName varchar(50),
CityID int,
Sort int
)

alter table Orders add RegionID int