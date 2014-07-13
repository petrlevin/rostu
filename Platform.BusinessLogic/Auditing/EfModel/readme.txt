Контекст, сущностные классы и маппинг генерированы на базе шаблона
EntityFramework Reverse POCO Generator
http://visualstudiogallery.msdn.microsoft.com/ee4fcff9-0c4c-4179-afd9-7a2fb90f5838

 
Platform.EF.Reverse.Core.ttinclude -- в функции InitConnectionString() сейчас костыль -- строка подключения должна быть указана в шаблоне AuditDatabase.tt в параметре ConnectionStringAudit.
TODO: разобраться почему не получается использовать внутри Core.ttinclude контейнер IoC 
