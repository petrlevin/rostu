declare @idA int
set @idA=0
select top 1 @idA=id from ref.AnalyticalCodeStateProgram
if (@idA<>0)
begin
update doc.StateProgram set idAnalyticalCodeStateProgram=@idA where idAnalyticalCodeStateProgram is null
end