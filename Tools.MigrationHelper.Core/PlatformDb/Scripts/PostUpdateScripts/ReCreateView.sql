IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReCreateView]') AND type in (N'P', N'PC'))
BEGIN
	declare @field1 int
	declare cur CURSOR LOCAL for
		SELECT e.id FROM ref.Entity e WHERE e.idEntityType = 8
	open cur
	fetch next from cur into @field1
	while @@FETCH_STATUS = 0 BEGIN
		EXEC dbo.ReCreateView @field1
		fetch next from cur into @field1
	END
	close cur
	deallocate cur
END