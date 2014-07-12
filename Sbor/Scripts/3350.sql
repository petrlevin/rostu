declare @id int
select top 1 @id=id from ref.ResponsibleExecutantType

update doc.StateProgram
set IdResponsibleExecutantType = @id
where IdResponsibleExecutantType is null