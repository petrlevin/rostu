while exists (
	select d.id
	from 
		doc.PlanActivity as d
		left join doc.PlanActivity as c on c.id = d.idParent
	where d.Number in (
		select Number from doc.PlanActivity group by Number having Count(*) > 1
	) 
	or (d.id is not null and 
		case
			when charindex('.', c.Number) > 0 then substring(c.Number, 1, charindex('.', c.Number)-1) + '.' + cast(cast(substring(c.Number, charindex('.', c.Number)+1, len(c.Number)) as int)+1 as nvarchar(100))
			else c.Number + '.1'
		end != d.Number
	)
)
begin
	update d set Number = 
		isnull(case
			when charindex('.', c.Number) > 0 then substring(c.Number, 1, charindex('.', c.Number)-1) + '.' + cast(cast(substring(c.Number, charindex('.', c.Number)+1, len(c.Number)) as int)+1 as nvarchar(100))
			else c.Number + '.1'
		end, d.Number)
	from 
		doc.PlanActivity as d
		left join doc.PlanActivity as c on c.id = d.idParent
	where d.Number in (
		select Number from doc.PlanActivity group by Number having Count(*) > 1
	) 
	or (d.id is not null and 
		case
			when charindex('.', c.Number) > 0 then substring(c.Number, 1, charindex('.', c.Number)-1) + '.' + cast(cast(substring(c.Number, charindex('.', c.Number)+1, len(c.Number)) as int)+1 as nvarchar(100))
			else c.Number + '.1'
		end != d.Number
	)
end


update doc.PlanActivity set Caption =
	'План деятельности № ' +Number+ ' от ' +  right('00'+cast(day([Date]) as nvarchar(2)),2) + '.' +  right('00'+cast(month([Date]) as nvarchar(2)),2) + '.' + right('0000'+cast(year([Date]) as nvarchar(4)),4)
from doc.PlanActivity
where Caption != 'План деятельности № ' +Number+ ' от ' +  right('00'+cast(day([Date]) as nvarchar(2)),2) + '.' +  right('00'+cast(month([Date]) as nvarchar(2)),2) + '.' + right('0000'+cast(year([Date]) as nvarchar(4)),4)

