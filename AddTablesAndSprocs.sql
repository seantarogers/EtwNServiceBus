CREATE TABLE [InfoDebugLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[Logger] [nvarchar](255) NOT NULL,
	[LogMessage] [nvarchar](max) NOT NULL,
	[ApplicationName] [nvarchar](100) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.InfoDebugLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [SignalRTraceLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[Logger] [nvarchar](255) NOT NULL,
	[LogMessage] [nvarchar](max) NOT NULL,
	[ApplicationName] [nvarchar](100) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.SignalRTraceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


CREATE TABLE [ErrorLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[Logger] [nvarchar](255) NOT NULL,
	[LogMessage] [nvarchar](max) NOT NULL,
	[ApplicationName] [nvarchar](100) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ErrorLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

--you will need to run as admin or add some execute permissions to the sprocs when testing
CREATE PROCEDURE [usp_Create_ErrorLog]
  @LogDate DATETIME,
  @Logger NVARCHAR(255),
  @LogMessage NVARCHAR(MAX),
  @ApplicationName NVARCHAR(100)
AS
BEGIN
	
	SET NOCOUNT ON;	    
		    
	INSERT INTO [ErrorLog]
           ([LogDate]
		   ,[Logger]
           ,[LogMessage]
		   ,[ApplicationName],
		    [CreateDate])
     VALUES
           (@LogDate
		   ,@Logger
           ,@LogMessage
		   ,@ApplicationName	
		   	,GETUTCDATE())
END
GO


CREATE PROCEDURE [usp_Create_InfoDebugLog]
  @LogDate DATETIME,
  @Logger NVARCHAR(255),
  @LogMessage NVARCHAR(MAX),
  @ApplicationName NVARCHAR(100)
AS
BEGIN
	
	SET NOCOUNT ON;	    
		    
	INSERT INTO [InfoDebugLog]
           ([LogDate]
		   ,[Logger]
           ,[LogMessage]
		   ,[ApplicationName],
		    [CreateDate])
     VALUES
           (@LogDate
		   ,@Logger
           ,@LogMessage
		   ,@ApplicationName	
		   	,GETUTCDATE())
END
GO

CREATE PROCEDURE [usp_Create_SignalRTraceLog]
  @LogDate DATETIME,
  @Logger NVARCHAR(255),
  @LogMessage NVARCHAR(MAX),
  @ApplicationName NVARCHAR(100)
AS
BEGIN
	
	SET NOCOUNT ON;	    
		    
	INSERT INTO [SignalRTraceLog]
           ([LogDate]
		   ,[Logger]
           ,[LogMessage]
		   ,[ApplicationName],
		    [CreateDate])
     VALUES
           (@LogDate
		   ,@Logger
           ,@LogMessage
		   ,@ApplicationName	
		   	,GETUTCDATE())
END
GO

