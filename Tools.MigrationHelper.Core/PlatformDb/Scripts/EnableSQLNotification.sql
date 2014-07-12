--use [Master];

-- Ensuring that Service Broker is enabled - replace the Database with the desired Database name
--ALTER DATABASE [{0}] SET ENABLE_BROKER;
--GO 

-- Switching to our database - replace the Database with the desired Database name
exec sp_changedbowner 'sa';

IF NOT EXISTS(SELECT 1 FROM sys.sysusers WHERE NAME='bis')
BEGIN
  CREATE USER [bis] FOR LOGIN [bis] 
  WITH DEFAULT_SCHEMA = [bis]
END;

--ALTER USER [bis] WITH DEFAULT_SCHEMA = [bis];

ALTER AUTHORIZATION ON SCHEMA::[bis] TO [bis];

IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE NAME='sql_dependency_subscriber' AND type ='R')
BEGIN
CREATE ROLE sql_dependency_subscriber AUTHORIZATION [bis]
END;

ALTER AUTHORIZATION ON ROLE::[sql_dependency_subscriber] TO [bis];

-- Permissions needed for [sql_dependency_subscriber]
GRANT CREATE PROCEDURE to [sql_dependency_subscriber];
GRANT CREATE QUEUE to [sql_dependency_subscriber];
GRANT CREATE SERVICE to [sql_dependency_subscriber];
GRANT REFERENCES on CONTRACT::[http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification] to [sql_dependency_subscriber];
GRANT VIEW DEFINITION TO [sql_dependency_subscriber];

-- Permissions needed for [sql_dependency_subscriber] 
GRANT SELECT to [sql_dependency_subscriber];
GRANT SUBSCRIBE QUERY NOTIFICATIONS TO [sql_dependency_subscriber];
GRANT RECEIVE ON QueryNotificationErrorsQueue TO [sql_dependency_subscriber];
GRANT REFERENCES on CONTRACT::[http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification] to [sql_dependency_subscriber];

-- Making sure that my users are member of the correct role.
EXEC sp_addrolemember 'sql_dependency_subscriber', 'bis';