delete a from tp.Activity_Indicator a
	left outer join ref.IndicatorActivity b on b.id=a.idIndicatorActivity
where b.id is null
