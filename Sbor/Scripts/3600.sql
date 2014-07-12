update PPO
set PPO.UsedGMZ = case WHEN PPOMM.idModule = -1610612703 then 0 else 1 end
from ref.PublicLegalFormation as PPO
left join ref.PublicLegalFormationModule as PPOM on ppo.id = PPOM.idPublicLegalFormation
left join ml.PublicLegalFormationModule_Module as PPOMM on PPOM.id = PPOMM.idPublicLegalFormationModule and PPOMM.idModule = -1610612703