Update 
	tp.PlanActivity_ActivityVolume 
Set Volume = 0
	Where Volume Is Null 

Update 
	tp.PlanActivity_ActivityVolumeAUBU
Set 
	Volume = 0
Where 
	Volume Is Null 

Update
	tp.PlanActivity_IndicatorQualityActivityValue
Set 
	Value = 0
Where 
	Value Is Null

