/*
	Удалить дубликаты из tp.PlanActivity_IndicatorQualityActivityValue
	по полям iav.idMaster, iav.idHierarchyPeriod
	мешающие созданию индекса
*/

declare @id int;
while (1=1)
	begin
		
		set @id= 
				(select top 1 MAX(id) as id
				from
					tp.PlanActivity_IndicatorQualityActivityValue as iav
				group by iav.idMaster, iav.idHierarchyPeriod
				having COUNT(*)>1)
		
		if (@id is null)
			break;
			
		delete from tp.PlanActivity_IndicatorQualityActivityValue
		where id = @id
		
	end;