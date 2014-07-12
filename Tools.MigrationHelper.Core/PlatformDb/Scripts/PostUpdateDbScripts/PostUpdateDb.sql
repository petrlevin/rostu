-- Выполняется после применения каждого пакета обновления, включенного в дистрибутив, в процессе обновления БД. Для сравнения см. PostUpdate.sql.
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'Unique_isDescription')
ALTER INDEX [Unique_isDescription] ON ref.EntityField REBUILD;
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'Unique_isCaption')
ALTER INDEX [Unique_isCaption] ON ref.EntityField REBUILD;