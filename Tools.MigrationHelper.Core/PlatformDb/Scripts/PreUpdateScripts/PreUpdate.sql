-- Выполняется ОДИН раз перед обновления БД из дистрибутива. Для сравнения см. PreUpdateDb.sql.
ALTER DATABASE [{0}] SET TRUSTWORTHY ON;
ALTER AUTHORIZATION ON DATABASE::{0} TO sa;