insert into tp.ActivityOfSBP_SBPBlankActual (
idOwner,
idSBP_BlankHistory
)
select doc.id, sbh.id
from doc.ActivityOfSBP as doc
join ref.SBP as s on s.id=doc.idSBP
join tp.SBP_BlankHistory as sbh on sbh.idBlankType=7 and (s.idSBPType=1 and sbh.idOwner=s.id or s.idSBPType=2 and sbh.idOwner=s.idParent)
