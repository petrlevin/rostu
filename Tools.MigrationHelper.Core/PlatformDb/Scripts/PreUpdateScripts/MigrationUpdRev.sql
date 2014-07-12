IF(NOT EXISTS (SELECT TOP 1 * FROM reg.UpdateRevision AS ur))
insert into reg.UpdateRevision (Revision,[File],[Date])
select ur.revision, ur.[File], ur.[Date] from dbo.UpdateRevisions AS ur