/* Script Date: 12-Dec-2024 */
/* Macsteel B2B Database (Nop-4.2) re-structure into NopStation B2B Database (Nop-4.7) */



-------------------------------------------- Create Independent New Entities --------------------------------------------
---------------------------------------------------------------------------------------------------------




---------------------- Create [Erp_Logs] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Logs]    Script Date: 10-Dec-24 09:51:14 PM 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Logs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpLogLevelId] [int] NOT NULL,
	[ErpSyncLevelId] [int] NOT NULL,
	[ShortMessage] [nvarchar](max) NULL,
	[FullMessage] [nvarchar](max) NULL,
	[IpAddress] [nvarchar](255) NOT NULL,
	[CustomerId] [int] NULL,
	[PageUrl] [nvarchar](255) NOT NULL,
	[ReferrerUrl] [nvarchar](255) NOT NULL,
	[CreatedOnUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Erp_Logs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO




---------------------- Create [Erp_Account_CustomerRegistrationForm] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Account_CustomerRegistrationForm]    Script Date: 10-Dec-24 09:47:30 PM  

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Account_CustomerRegistrationForm](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullRegisteredName] [nvarchar](100) NOT NULL,
	[RegistrationNumber] [nvarchar](50) NOT NULL,
	[VatNumber] [nvarchar](50) NOT NULL,
	[TelephoneNumber1] [nvarchar](50) NOT NULL,
	[TelephoneNumber2] [nvarchar](50) NULL,
	[TelefaxNumber] [nvarchar](50) NULL,
	[AccountsContactPersonNameSurname] [nvarchar](50) NOT NULL,
	[AccountsEmail] [nvarchar](50) NOT NULL,
	[AccountsTelephoneNumber] [nvarchar](50) NOT NULL,
	[AccountsCellphoneNumber] [nvarchar](50) NOT NULL,
	[BuyerContactPersonNameSurname] [nvarchar](50) NOT NULL,
	[BuyerEmail] [nvarchar](50) NOT NULL,
	[NatureOfBusiness] [nvarchar](50) NOT NULL,
	[RegisteredOfficeAddressId] [int] NOT NULL,
	[TypeOfBusiness] [nvarchar](50) NOT NULL,
	[EstimatePurchasesPerMonthZAR] [decimal](18, 4) NOT NULL,
	[CreditLimitRequired] [bit] NOT NULL,
	[IsApproved] [bit] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedOnUtc] [datetime2](6) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedOnUtc] [datetime2](6) NOT NULL,
	[UpdatedById] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Erp_Account_CustomerRegistrationForm] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO




---------------------- Create [Erp_Account_CustomerRegistration_TradeReferences] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Account_CustomerRegistration_TradeReferences]    Script Date: 10-Dec-24 09:48:47 PM  

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Account_CustomerRegistration_TradeReferences](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Telephone] [nvarchar](20) NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[Terms] [nvarchar](300) NOT NULL,
	[HowLong] [nvarchar](300) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedOnUtc] [datetime2](6) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedOnUtc] [datetime2](6) NOT NULL,
	[UpdatedById] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Erp_Account_CustomerRegistration_TradeReferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_TradeReferences]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Account_CustomerRegistration_TradeReferences_FormId_Erp_Account_CustomerRegistrationForm_Id] FOREIGN KEY([FormId])
REFERENCES [dbo].[Erp_Account_CustomerRegistrationForm] ([Id])
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_TradeReferences] CHECK CONSTRAINT [FK_Erp_Account_CustomerRegistration_TradeReferences_FormId_Erp_Account_CustomerRegistrationForm_Id]
GO




---------------------- Create [Erp_Account_CustomerRegistration_Premises] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Account_CustomerRegistration_Premises]    Script Date: 10-Dec-24 09:49:15 PM  

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Account_CustomerRegistration_Premises](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormId] [int] NOT NULL,
	[OwnedOrLeased] [bit] NOT NULL,
	[NameOfLandlord] [nvarchar](100) NOT NULL,
	[AddressOfLandlord] [nvarchar](300) NOT NULL,
	[EmailOfLandlord] [nvarchar](100) NOT NULL,
	[TelephoneNumberOfLandlord] [nvarchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedOnUtc] [datetime2](6) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedOnUtc] [datetime2](6) NOT NULL,
	[UpdatedById] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Erp_Account_CustomerRegistration_Premises] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_Premises]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Account_CustomerRegistration_Premises_FormId_Erp_Account_CustomerRegistrationForm_Id] FOREIGN KEY([FormId])
REFERENCES [dbo].[Erp_Account_CustomerRegistrationForm] ([Id])
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_Premises] CHECK CONSTRAINT [FK_Erp_Account_CustomerRegistration_Premises_FormId_Erp_Account_CustomerRegistrationForm_Id]
GO




---------------------- Create [Erp_Account_CustomerRegistration_PhysicalTradingAddress] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Account_CustomerRegistration_PhysicalTradingAddress]    Script Date: 10-Dec-24 09:49:44 PM  

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Account_CustomerRegistration_PhysicalTradingAddress](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormId] [int] NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Surname] [nvarchar](50) NOT NULL,
	[PhysicalTradingAddressId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedOnUtc] [datetime2](6) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedOnUtc] [datetime2](6) NOT NULL,
	[UpdatedById] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Erp_Account_CustomerRegistration_PhysicalTradingAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_PhysicalTradingAddress]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Account_CustomerRegistration_PhysicalTradingAddress_FormId_Erp_Account_CustomerRegistrationForm_Id] FOREIGN KEY([FormId])
REFERENCES [dbo].[Erp_Account_CustomerRegistrationForm] ([Id])
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_PhysicalTradingAddress] CHECK CONSTRAINT [FK_Erp_Account_CustomerRegistration_PhysicalTradingAddress_FormId_Erp_Account_CustomerRegistrationForm_Id]
GO




---------------------- Create [Erp_Account_CustomerRegistration_BankingDetails] Table ---------------------- No complexity

-- Object: Table [dbo].[Erp_Account_CustomerRegistration_BankingDetails]    Script Date: 10-Dec-24 09:50:21 PM 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Account_CustomerRegistration_BankingDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormId] [int] NOT NULL,
	[NameOfBanker] [nvarchar](100) NOT NULL,
	[AccountName] [nvarchar](100) NOT NULL,
	[AccountNumber] [nvarchar](50) NOT NULL,
	[BranchCode] [nvarchar](50) NOT NULL,
	[Branch] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedOnUtc] [datetime2](6) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedOnUtc] [datetime2](6) NOT NULL,
	[UpdatedById] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Erp_Account_CustomerRegistration_BankingDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_BankingDetails]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Account_CustomerRegistration_BankingDetails_FormId_Erp_Account_CustomerRegistrationForm_Id] FOREIGN KEY([FormId])
REFERENCES [dbo].[Erp_Account_CustomerRegistrationForm] ([Id])
GO

ALTER TABLE [dbo].[Erp_Account_CustomerRegistration_BankingDetails] CHECK CONSTRAINT [FK_Erp_Account_CustomerRegistration_BankingDetails_FormId_Erp_Account_CustomerRegistrationForm_Id]
GO



---------------------- Create [Erp_Data_Sync_Task] Table ---------------------- No complexity

/****** Object:  Table [dbo].[Erp_Data_Sync_Task]    Script Date: 10-Sep-25 09:06:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Data_Sync_Task](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[LastEnabledUtc] [datetime2](7) NULL,
	[Enabled] [bit] NULL,
	[LastStartUtc] [datetime2](7) NULL,
	[LastEndUtc] [datetime2](7) NULL,
	[LastSuccessUtc] [datetime2](7) NULL,
	[DayTimeSlots] [nvarchar](max) NULL,
	[IsRunning] [bit] NOT NULL,
	[IsIncremental] [bit] NOT NULL,
	[QuartzJobName] [nvarchar](255) NULL,
 CONSTRAINT [PK_Erp_Data_Sync_Task] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Data_Sync_Task] ADD  CONSTRAINT [DF_Erp_Data_Sync_Task_IsRunning]  DEFAULT ((0)) FOR [IsRunning]
GO

ALTER TABLE [dbo].[Erp_Data_Sync_Task] ADD  CONSTRAINT [DF_Erp_Data_Sync_Task_IsIncremental]  DEFAULT ((1)) FOR [IsIncremental]
GO




---------------------- Create Quartz Tables and Indexes ---------------------- No complexity

BEGIN
    -- drop indexes if they exist and rebuild if current ones
    DROP INDEX IF EXISTS [IDX_QRTZ_T_J] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_JG] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_C] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_G] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_G_J] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_STATE] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_N_STATE] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_N_G_STATE] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_NEXT_FIRE_TIME] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_MISFIRE] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST_MISFIRE] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP] ON [dbo].[QRTZ_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_TRIG_INST_NAME] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_J_G] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_JG] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_T_G] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_TG] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_G_J] ON [dbo].[QRTZ_FIRED_TRIGGERS];
    DROP INDEX IF EXISTS [IDX_QRTZ_FT_G_T] ON [dbo].[QRTZ_FIRED_TRIGGERS];

    IF OBJECT_ID(N'[dbo].[FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[QRTZ_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS];
    
    IF OBJECT_ID(N'[dbo].[FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS];
    
    IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QRTZ_JOB_LISTENERS_QRTZ_JOB_DETAILS]') AND parent_object_id = OBJECT_ID(N'[dbo].[QRTZ_JOB_LISTENERS]'))
    ALTER TABLE [dbo].[QRTZ_JOB_LISTENERS] DROP CONSTRAINT [FK_QRTZ_JOB_LISTENERS_QRTZ_JOB_DETAILS];
    
    IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QRTZ_TRIGGER_LISTENERS_QRTZ_TRIGGERS]') AND parent_object_id = OBJECT_ID(N'[dbo].[QRTZ_TRIGGER_LISTENERS]'))
    ALTER TABLE [dbo].[QRTZ_TRIGGER_LISTENERS] DROP CONSTRAINT [FK_QRTZ_TRIGGER_LISTENERS_QRTZ_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_CALENDARS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_CALENDARS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_CRON_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_CRON_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_BLOB_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_BLOB_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_FIRED_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_FIRED_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_PAUSED_TRIGGER_GRPS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS];
    
    IF  OBJECT_ID(N'[dbo].[QRTZ_JOB_LISTENERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_JOB_LISTENERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_SCHEDULER_STATE]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_SCHEDULER_STATE];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_LOCKS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_LOCKS];

    IF OBJECT_ID(N'[dbo].[QRTZ_TRIGGER_LISTENERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_TRIGGER_LISTENERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_JOB_DETAILS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_JOB_DETAILS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_SIMPLE_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_SIMPROP_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS];
    
    IF OBJECT_ID(N'[dbo].[QRTZ_TRIGGERS]', N'U') IS NOT NULL
    DROP TABLE [dbo].[QRTZ_TRIGGERS];
END 

CREATE TABLE [dbo].[QRTZ_CALENDARS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [CALENDAR_NAME] nvarchar(200) NOT NULL,
    [CALENDAR] varbinary(max) NOT NULL
);

CREATE TABLE [dbo].[QRTZ_CRON_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [CRON_EXPRESSION] nvarchar(120) NOT NULL,
    [TIME_ZONE_ID] nvarchar(80) 
);

CREATE TABLE [dbo].[QRTZ_FIRED_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [ENTRY_ID] nvarchar(140) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [INSTANCE_NAME] nvarchar(200) NOT NULL,
    [FIRED_TIME] bigint NOT NULL,
    [SCHED_TIME] bigint NOT NULL,
    [PRIORITY] int NOT NULL,
    [STATE] nvarchar(16) NOT NULL,
    [JOB_NAME] nvarchar(150) NULL,
    [JOB_GROUP] nvarchar(150) NULL,
    [IS_NONCONCURRENT] bit NULL,
    [REQUESTS_RECOVERY] bit NULL 
);

CREATE TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL 
);

CREATE TABLE [dbo].[QRTZ_SCHEDULER_STATE] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [INSTANCE_NAME] nvarchar(200) NOT NULL,
    [LAST_CHECKIN_TIME] bigint NOT NULL,
    [CHECKIN_INTERVAL] bigint NOT NULL
);

CREATE TABLE [dbo].[QRTZ_LOCKS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [LOCK_NAME] nvarchar(40) NOT NULL 
);

CREATE TABLE [dbo].[QRTZ_JOB_DETAILS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [JOB_NAME] nvarchar(150) NOT NULL,
    [JOB_GROUP] nvarchar(150) NOT NULL,
    [DESCRIPTION] nvarchar(250) NULL,
    [JOB_CLASS_NAME] nvarchar(250) NOT NULL,
    [IS_DURABLE] bit NOT NULL,
    [IS_NONCONCURRENT] bit NOT NULL,
    [IS_UPDATE_DATA] bit NOT NULL,
    [REQUESTS_RECOVERY] bit NOT NULL,
    [JOB_DATA] varbinary(max) NULL
);

CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [REPEAT_COUNT] int NOT NULL,
    [REPEAT_INTERVAL] bigint NOT NULL,
    [TIMES_TRIGGERED] int NOT NULL
);

CREATE TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [STR_PROP_1] nvarchar(512) NULL,
    [STR_PROP_2] nvarchar(512) NULL,
    [STR_PROP_3] nvarchar(512) NULL,
    [INT_PROP_1] int NULL,
    [INT_PROP_2] int NULL,
    [LONG_PROP_1] bigint NULL,
    [LONG_PROP_2] bigint NULL,
    [DEC_PROP_1] numeric(13,4) NULL,
    [DEC_PROP_2] numeric(13,4) NULL,
    [BOOL_PROP_1] bit NULL,
    [BOOL_PROP_2] bit NULL,
    [TIME_ZONE_ID] nvarchar(80) NULL 
);

CREATE TABLE [dbo].[QRTZ_BLOB_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [BLOB_DATA] varbinary(max) NULL
);

CREATE TABLE [dbo].[QRTZ_TRIGGERS] (
    [Id] [bigint] NULL,
    [SCHED_NAME] nvarchar(120) NOT NULL,
    [TRIGGER_NAME] nvarchar(150) NOT NULL,
    [TRIGGER_GROUP] nvarchar(150) NOT NULL,
    [JOB_NAME] nvarchar(150) NOT NULL,
    [JOB_GROUP] nvarchar(150) NOT NULL,
    [DESCRIPTION] nvarchar(250) NULL,
    [NEXT_FIRE_TIME] bigint NULL,
    [PREV_FIRE_TIME] bigint NULL,
    [PRIORITY] int NULL,
    [TRIGGER_STATE] nvarchar(16) NOT NULL,
    [TRIGGER_TYPE] nvarchar(8) NOT NULL,
    [START_TIME] bigint NOT NULL,
    [END_TIME] bigint NULL,
    [CALENDAR_NAME] nvarchar(200) NULL,
    [MISFIRE_INSTR] int NULL,
    [JOB_DATA] varbinary(max) NULL
);
GO

ALTER TABLE [dbo].[QRTZ_CALENDARS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_CALENDARS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [CALENDAR_NAME]
    );
GO

ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_CRON_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_FIRED_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_FIRED_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [ENTRY_ID]
    );
GO

ALTER TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_PAUSED_TRIGGER_GRPS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_SCHEDULER_STATE] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_SCHEDULER_STATE] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [INSTANCE_NAME]
    );
GO

ALTER TABLE [dbo].[QRTZ_LOCKS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [LOCK_NAME]
    );
GO

ALTER TABLE [dbo].[QRTZ_JOB_DETAILS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_JOB_DETAILS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_SIMPROP_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_BLOB_TRIGGERS] WITH NOCHECK ADD
    CONSTRAINT [PK_QRTZ_BLOB_TRIGGERS] PRIMARY KEY  CLUSTERED
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    );
GO

ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] ADD
    CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
    CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
    (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] ADD
    CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
    (
	[SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
    ) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[QRTZ_TRIGGERS] ADD
    CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS] FOREIGN KEY
    (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
    ) REFERENCES [dbo].[QRTZ_JOB_DETAILS] (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
    );
GO

-- Create indexes
CREATE INDEX [IDX_QRTZ_T_G_J]                 ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
CREATE INDEX [IDX_QRTZ_T_C]                   ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, CALENDAR_NAME);

CREATE INDEX [IDX_QRTZ_T_N_G_STATE]           ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
CREATE INDEX [IDX_QRTZ_T_STATE]               ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
CREATE INDEX [IDX_QRTZ_T_N_STATE]             ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
CREATE INDEX [IDX_QRTZ_T_NEXT_FIRE_TIME]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
CREATE INDEX [IDX_QRTZ_T_NFT_ST]              ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_STATE);
CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP]  ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_GROUP, TRIGGER_STATE);

CREATE INDEX [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
CREATE INDEX [IDX_QRTZ_FT_G_J]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
CREATE INDEX [IDX_QRTZ_FT_G_T]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_NAME);
GO



-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------




------------------------------- Restructure Entities and Create Dependent Entities -------------------------------
-------------------------------------------------------------------------------------------------------------------


-- 1.
----------------------  B2BAccount -> ErpAccount ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BAccount', 'Erp_Account';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BAccount', 'PK_Erp_Account';
GO

-- Rename columns
EXEC sp_rename 'Erp_Account.B2BSalesOrganisationId', 'ErpSalesOrgId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Account.BillingAddressId', 'BillingAddress_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Account.B2BAccountStatusTypeId', 'ErpAccountStatusTypeId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Account.LastAccountRefresh', 'LastErpAccountSyncDate', 'COLUMN';
GO

-- Add columns
ALTER TABLE Erp_Account ADD IsDefaultPaymentAccount BIT NOT NULL DEFAULT 0;
GO

-- Drop constraint
ALTER TABLE [dbo].[Erp_Account] DROP CONSTRAINT [FK_B2BAccount_B2BPriceGroupCode_B2BPriceGroupCodeId];
GO

-- FROM DbScript.sql start
--Alter Table [dbo].[Erp_Account]
--Alter Column [BillingSuburb] [nvarchar](max) NULL

--EXEC sp_rename 'Erp_Account.LastAccountRefresh', 'LastErpAccountSyncDate', 'COLUMN';

--ALTER TABLE [dbo].[Erp_Account]
--ADD [LastTimeOrderSyncOnUtc] [datetime2](7) NULL;
-- FROM DbScript.sql end

-- 2.
---------------------- B2BShipToAddress -> Erp_ShipToAddress ----------------------
    -- Rename table
    EXEC sp_rename 'B2BShipToAddress', 'Erp_ShipToAddress';
    GO

    -- Rename columns
    EXEC sp_rename 'Erp_ShipToAddress.AddressId', 'Address_Id', 'COLUMN';
    GO

    -- Rename the constraints
    EXEC sp_rename 'PK_B2BShipToAddress', 'PK_Erp_ShipToAddress';
    GO

    -- Add columns
    ALTER TABLE Erp_ShipToAddress 
    ADD LastShipToAddressSyncDate DATETIME2 NULL,
	    DeliveryOptionId INT NULL,
	    Latitude NVARCHAR(MAX) NULL,
	    Longitude NVARCHAR(MAX) NULL,
	    DistanceToNearestWareHouse DECIMAL(19, 5) NULL,
	    NearestWareHouseId INT NULL,
	    B2CShipToAddressId INT NULL;
    GO

    -- Alter columns
    ALTER TABLE Erp_ShipToAddress ALTER COLUMN OrderId INT NULL;
    ALTER TABLE Erp_ShipToAddress ALTER COLUMN B2BAccountId INT NULL;
    ALTER TABLE Erp_ShipToAddress ALTER COLUMN B2BSalesOrganisationId INT NULL;

    -- Drop constraints

    ALTER TABLE [dbo].[Erp_ShipToAddress] DROP CONSTRAINT [FK_B2BShipToAddress_B2BAccount_B2BAccountId];
    GO
    ALTER TABLE [dbo].[Erp_ShipToAddress] DROP CONSTRAINT [FK_B2BShipToAddress_B2BSalesOrganisation_B2BSalesOrganisationId];
    GO

    -- Add constraints

    ALTER TABLE [dbo].[Erp_ShipToAddress] ADD CONSTRAINT [FK_Erp_ShipToAddress_Address_Address_Id]
    FOREIGN KEY ([Address_Id]) REFERENCES [dbo].[Address] ([Id]);
    GO

    --Alter table Erp_ShipToAddress
    --DROP COLUMN Erpaccount_id

    Alter Table [dbo].[Erp_ShipToAddress]
    Alter Column [Suburb] [nvarchar](max) NULL
     --from DbScript.sql end

-- 3.
---------------------- Create [Erp_ShiptoAddress_Erp_Account_Map] Table ---------------------- No complexity
-- Object:  Table [dbo].[Erp_ShiptoAddress_Erp_Account_Map]    Script Date: 11-Dec-24 06:45:16 PM 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpAccountId] [int] NOT NULL,
	[ErpShiptoAddress_Id] [int] NOT NULL,
	[ErpShipToAddressCreatedByTypeId] [int] NULL,
 CONSTRAINT [PK_Erp_ShiptoAddress_Erp_Account_Map] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map]  WITH NOCHECK ADD  CONSTRAINT [FK_Erp_ShiptoAddress_Erp_Account_Map_Erp_Account_Erp_Account_Id] FOREIGN KEY([ErpAccountId])
REFERENCES [dbo].[Erp_Account] ([Id])
GO

ALTER TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map] CHECK CONSTRAINT [FK_Erp_ShiptoAddress_Erp_Account_Map_Erp_Account_Erp_Account_Id]
GO

ALTER TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map]  WITH NOCHECK ADD  CONSTRAINT [FK_Erp_ShiptoAddress_Erp_Account_Map_Erp_ShiptoAddress_Erp_ShipToAddress_Id] FOREIGN KEY([ErpShiptoAddress_Id])
REFERENCES [dbo].[Erp_ShipToAddress] ([Id])
GO

ALTER TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map] CHECK CONSTRAINT [FK_Erp_ShiptoAddress_Erp_Account_Map_Erp_ShiptoAddress_Erp_ShipToAddress_Id]
GO



-- 4.
---------------------- B2BActivityLog --> Erp_Activity_Logs ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BActivityLog', 'Erp_Activity_Logs';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BActivityLog', 'PK_Erp_Activity_Logs';
GO

-- Rename columns   
EXEC sp_rename 'Erp_Activity_Logs.EntityType', 'EntityName', 'COLUMN';
GO
EXEC sp_rename 'Erp_Activity_Logs.ActivityTypeId', 'ErpActivityLogTypeId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Activity_Logs.ChangedByCustomerId', 'Customer_Id', 'COLUMN';
GO

-- Add columns
ALTER TABLE Erp_Activity_Logs ADD Comment NVARCHAR(500) NULL;
GO



-- 5.
---------------------- B2BFinancialTransactionPerAccount -> Erp_Invoice ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BFinancialTransactionPerAccount', 'Erp_Invoice';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BFinancialTransactionPerAccount', 'PK_Erp_Invoice';
GO
EXEC sp_rename 'FK_B2BFinancialTransactionPerAccount_B2BAccount_B2BAccountId', 'FK_Erp_Invoice_Erp_Account_Erp_Account_Id';
GO

-- Rename columns
EXEC sp_rename 'Erp_Invoice.B2BAccountId', 'ErpAccountId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Invoice.ERPDocumentNumber', 'ErpDocumentNumber', 'COLUMN';
GO
EXEC sp_rename 'Erp_Invoice.ERPOrderNumber', 'ErpOrderNumber', 'COLUMN';
GO


-- 6.
---------------------- B2BSalesOrganisation -> Erp_Sales_Org ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BSalesOrganisation', 'Erp_Sales_Org';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BSalesOrganisation', 'PK_Erp_Sales_Org';
GO

-- Rename columns
EXEC sp_rename 'Erp_Sales_Org.SalesOrganisationCode', 'Code', 'COLUMN';
GO
EXEC sp_rename 'Erp_Sales_Org.SalesOrganisationName', 'Name', 'COLUMN';
GO
EXEC sp_rename 'Erp_Sales_Org.EmailAddresses', 'Email', 'COLUMN';
GO
EXEC sp_rename 'Erp_Sales_Org.AddressId', 'Address_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Sales_Org.B2BAccountIdForB2C', 'ErpAccountIdForB2C', 'COLUMN';
GO

-- add new columns:
ALTER TABLE Erp_Sales_Org
ADD 
    LastErpAccountSyncTimeOnUtc DATETIME2(7) NULL,
    LastErpGroupPriceSyncTimeOnUtc DATETIME2(7) NULL,
    LastErpShipToAddressSyncTimeOnUtc DATETIME2(7) NULL,
    LastErpProductSyncTimeOnUtc DATETIME2(7) NULL,
    LastErpStockSyncTimeOnUtc DATETIME2(7) NULL;
GO

-- 7.
---------------------- B2BPerAccountProductPricing -> Erp_Special_Price ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BPerAccountProductPricing', 'Erp_Special_Price';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BPerAccountProductPricing', 'PK_Erp_Special_Price';
GO

-- Rename columns
EXEC sp_rename 'Erp_Special_Price.B2BAccountId', 'ErpAccountId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Special_Price.ProductId', 'NopProductId', 'COLUMN';
GO

-- Remove columns
ALTER TABLE Erp_Special_Price DROP COLUMN CustomerItemNumber;
GO



-- 8.
---------------------- B2BPriceGroupCode -> Erp_Group_Price_Code ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BPriceGroupCode', 'Erp_Group_Price_Code';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BPriceGroupCode', 'PK_Erp_Group_Price_Code';
GO

-- Rename columns
EXEC sp_rename 'Erp_Group_Price_Code.PriceGroupCodeName', 'Code', 'COLUMN';
GO
EXEC sp_rename 'Erp_Group_Price_Code.LastPriceUpdatedOnUTC', 'LastUpdateTime', 'COLUMN';
GO

-- Add columns
ALTER TABLE Erp_Group_Price_Code
ADD IsActive BIT NOT NULL DEFAULT 1,
    CreatedOnUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedById INT NOT NULL DEFAULT 0,
    UpdatedOnUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedById INT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0;
GO



-- 9.
---------------------- B2BPriceGroupProductPricing -> Erp_Group_Price ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BPriceGroupProductPricing', 'Erp_Group_Price';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BPriceGroupProductPricing', 'PK_Erp_Group_Price';
GO
EXEC sp_rename 'FK_B2BPriceGroupProductPricing_B2BPriceGroupCode_B2BPriceGroupCodeId', 'FK_Erp_Group_Price_Erp_Group_Price_Code_Erp_Group_Price_Code_Id';
GO

-- Rename columns
EXEC sp_rename 'Erp_Group_Price.B2BPriceGroupCodeId', 'ErpNopGroupPriceCodeId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Group_Price.ProductId', 'NopProductId', 'COLUMN';
GO

-- Add columns
ALTER TABLE Erp_Group_Price
ADD IsActive BIT NOT NULL DEFAULT 1,
    CreatedOnUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedById INT NOT NULL DEFAULT 0,
    UpdatedOnUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedById INT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0;
GO




-- 10.
---------------------- B2BSalesRep -> Erp_Sales_Rep ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BSalesRep', 'Erp_Sales_Rep';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BSalesRep', 'PK_Erp_Sales_Rep';
GO

-- Rename columns
EXEC sp_rename 'Erp_Sales_Rep.NopCustomerId', 'NopCustomer_Id', 'COLUMN';
GO




-- 11.
---------------------- B2BSalesRepSalesOrg -> Erp_Sales_Rep_Sales_Org_Map ---------------------- No complexity
-- Rename the table
EXEC sp_rename 'B2BSalesRepSalesOrg', 'Erp_Sales_Rep_Sales_Org_Map';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BSalesRepSalesOrg', 'PK_Erp_Sales_Rep_Sales_Org_Map';
GO
EXEC sp_rename 'FK_B2BSalesRepSalesOrg_B2BSalesOrganisation_B2BSalesOrganisationId', 'FK_Erp_Sales_Rep_Sales_Org_Map_Erp_SalesOrg_Erp_SalesOrg_Id';
GO
EXEC sp_rename 'FK_B2BSalesRepSalesOrg_B2BSalesRep_B2BSalesRepId', 'FK_Erp_Sales_Rep_Sales_Org_Map_Erp_Sales_Rep_Erp_Sales_Rep_Id';
GO

-- Rename columns
EXEC sp_rename 'Erp_Sales_Rep_Sales_Org_Map.B2BSalesRepId', 'ErpSalesRep_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Sales_Rep_Sales_Org_Map.B2BSalesOrganisationId', 'ErpSalesOrgId', 'COLUMN';
GO



-- 12.
---------------------- B2BOrderPerAccount -> Erp_Order_Additional_Data ---------------------- No complexity

-- Rename table
EXEC sp_rename 'B2BOrderPerAccount', 'Erp_Order_Additional_Data';
GO

-- Rename columns
EXEC sp_rename 'Erp_Order_Additional_Data.NopOrderId', 'NopOrder_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BAccountId', 'ErpAccountId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BSalesOrganisationId', 'ErpSalesOrgId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BOrderOriginTypeId', 'ErpOrderOriginTypeId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BOrderTypeId', 'ErpOrderTypeId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.QuoteSalesOrderId', 'QuoteSalesOrder_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BShipToAddressId', 'ErpShipToAddress_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.ERPOrderNumber', 'ErpOrderNumber', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.OrderPlacedByNopCustomerId', 'OrderPlacedByNopCustomer_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Additional_Data.B2BOrderPlaceByCustomerTypeId', 'ErpOrderPlaceByCustomerTypeId', 'COLUMN';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BOrderPerAccount', 'PK_Erp_Order_Additional_Data';
GO

-- Add columns
ALTER TABLE Erp_Order_Additional_Data ADD 
	PaygateReferenceNumber NVARCHAR(MAX) NULL,
	CashRounding DECIMAL(18, 2) NULL,
	ShippingCost DECIMAL(18, 2) NULL,
	B2COrderPerUserId INT NULL;
GO

-- Drop old constraints
ALTER TABLE [dbo].[Erp_Order_Additional_Data] DROP CONSTRAINT [FK_B2BOrderPerAccount_B2BAccount_B2BAccountId];
GO
ALTER TABLE [dbo].[Erp_Order_Additional_Data] DROP CONSTRAINT [FK_B2BOrderPerAccount_B2BSalesOrganisation_B2BSalesOrganisationId];
GO
ALTER TABLE [dbo].[Erp_Order_Additional_Data] DROP CONSTRAINT [FK_B2BOrderPerAccount_B2BShipToAddress_B2BShipToAddressId];
GO

-- Drop index
DROP INDEX IX_B2BOrderPerAccount_B2BSalesOrganisationId ON [Erp_Order_Additional_Data];


-- Add new constraints
ALTER TABLE [dbo].[Erp_Order_Additional_Data] WITH CHECK ADD CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_Account_Erp_Account_Id] 
FOREIGN KEY ([ErpAccountId]) REFERENCES [dbo].[Erp_Account] ([Id]) ON DELETE CASCADE;
GO

/*ALTER TABLE [dbo].[Erp_Order_Additional_Data] WITH CHECK ADD CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_SalesOrg_Erp_SalesOrg_Id] 
FOREIGN KEY ([ErpSalesOrgId]) REFERENCES [dbo].[Erp_Sales_Org] ([Id]) ON DELETE CASCADE;
GO*/

ALTER TABLE [dbo].[Erp_Order_Additional_Data] WITH CHECK ADD CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_ShipToAddress_Erp_ShipToAddress_Id] 
FOREIGN KEY ([ErpShipToAddress_Id]) REFERENCES [dbo].[Erp_ShipToAddress] ([Id]);
GO


ALTER TABLE [dbo].[Erp_Order_Additional_Data] CHECK CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_Account_Erp_Account_Id];
GO
/*ALTER TABLE [dbo].[Erp_Order_Additional_Data] CHECK CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_SalesOrg_Erp_SalesOrg_Id];
GO*/
ALTER TABLE [dbo].[Erp_Order_Additional_Data] CHECK CONSTRAINT [FK_Erp_Order_Additional_Data_Erp_ShipToAddress_Erp_ShipToAddress_Id];
GO


/*-- Remove columns
ALTER TABLE Erp_Order_Additional_Data DROP COLUMN ErpSalesOrgId;
GO*/



-- 13.
---------------------- B2BOrderItem -> Erp_Order_Item_Additional_Data ---------------------- No complexity

-- Rename table
EXEC sp_rename 'B2BOrderItem', 'Erp_Order_Item_Additional_Data';
GO

-- Rename columns
EXEC sp_rename 'Erp_Order_Item_Additional_Data.NopOrderItemId', 'NopOrderItem_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPOrderLineNumber', 'ErpOrderLineNumber', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPSalesUoM', 'ErpSalesUoM', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPOrderLineStatus', 'ErpOrderLineStatus', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPDateRequired', 'ErpDateRequired', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPDateExpected', 'ErpDateExpected', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPDeliveryMethod', 'ErpDeliveryMethod', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPInvoiceNumber', 'ErpInvoiceNumber', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.ERPOrderLineNotes', 'ErpOrderLineNotes', 'COLUMN';
GO
EXEC sp_rename 'Erp_Order_Item_Additional_Data.LastERPUpdateUtc', 'LastErpUpdateUtc', 'COLUMN';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BOrderItem', 'PK_Erp_Order_Item_Additional_Data';
GO

-- Add columns
ALTER TABLE Erp_Order_Item_Additional_Data 
ADD ErpOrder_Id INT NULL, 
	Warehouse NVARCHAR(100) NULL,
	DiscountPercentage DECIMAL(19, 5) NULL,
	ListPricePerUnit DECIMAL(19, 5) NULL,
	TotalListPriceForQuantity DECIMAL(19, 5) NULL,
	BatchCode NVARCHAR(MAX) NULL,
	SpecialInstruction NVARCHAR(MAX) NULL,
	DeliveryDate DATETIME2 NULL,
	NopWarehouseId INT NULL;
GO

 --ALTER TABLE [dbo].[Erp_Order_Item_Additional_Data]
 --ADD [WareHouse] [nvarchar](255) NULL;
 --GO


-- 14.
---------------------- QO_QuickOrderItem -> Erp_Quick_Order_Item ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'QO_QuickOrderItem', 'Erp_Quick_Order_Item';
GO

-- Rename the constraints
EXEC sp_rename 'PK_QO_QuickOrderItem', 'PK_Erp_Quick_Order_Item';
GO
EXEC sp_rename 'FK_QO_QuickOrderItem_QO_QuickOrderTemplate_QuickOrderTemplateId', 'FK_Erp_Quick_Order_Item_Erp_Quick_Order_Template_Erp_Quick_Order_Template_Id';
GO

-- Rename columns
EXEC sp_rename 'Erp_Quick_Order_Item.ProductSku', 'Product_Sku', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Item.QuickOrderTemplateId', 'Quick_Order_Template_Id', 'COLUMN';
GO

-- Add columns
ALTER TABLE Erp_Quick_Order_Item ADD Attributes_Xml NVARCHAR(MAX) NULL;
GO



-- 15.
---------------------- QO_QuickOrderTemplate -> Erp_Quick_Order_Template ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'QO_QuickOrderTemplate', 'Erp_Quick_Order_Template';
GO

-- Rename the constraints
EXEC sp_rename 'PK_QO_QuickOrderTemplate', 'PK_Erp_Quick_Order_Template';
GO

-- Rename columns
EXEC sp_rename 'Erp_Quick_Order_Template.CustomerId', 'Customer_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Template.LastOrderDate', 'Last_Order_Date', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Template.EditedOnUtc', 'Edited_On_Utc', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Template.CreatedOnUtc', 'Created_On_Utc', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Template.LastPriceCalculatedOnUtc', 'Last_Price_Calculated_On_Utc', 'COLUMN';
GO
EXEC sp_rename 'Erp_Quick_Order_Template.IsDeleted', 'Deleted', 'COLUMN';
GO

-- 16.
---------------------- B2BCustomerConfiguration -> Erp_Customer_Configuration ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BCustomerConfiguration', 'Erp_Customer_Configuration';
GO



-- 17.
---------------------- B2BPriceListDownloadTrack -> Erp_Price_List_Download_Track ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BPriceListDownloadTrack', 'Erp_Price_List_Download_Track';
GO




-- 18.
---------------------- B2BProductNotePerSalesOrg -> Erp_Product_Note_Per_Sales_Org ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BProductNotePerSalesOrg', 'Erp_Product_Note_Per_Sales_Org';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BProductNotePerSalesOrg', 'PK_Erp_Product_Note_Per_Sales_Org';
GO



-- 19.
---------------------- B2BShipToCodeChange -> Erp_Ship_To_Code_Change ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BShipToCodeChange', 'Erp_Ship_To_Code_Change';
GO


-- 20.
---------------------- B2BSpecialIncludesAndExcludes -> Erp_Special_Includes_And_Excludes ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BSpecialIncludesAndExcludes', 'Erp_Special_Includes_And_Excludes';
GO

-- Rename columns
EXEC sp_rename 'Erp_Special_Includes_And_Excludes.B2BAccountId', 'ErpAccountId', 'COLUMN';
GO
EXEC sp_rename 'Erp_Special_Includes_And_Excludes.SalesOrgId', 'ErpSalesOrgId', 'COLUMN';
GO

-- Rename the constraints
EXEC sp_rename 'PK_SpecialIncludesAndExcludes', 'PK_Erp_Special_Includes_And_Excludes';
GO
EXEC sp_rename 'FK_SpecialIncludesAndExcludes_B2BAccount', 'FK_Erp_Special_Includes_And_Excludes_Erp_Account_Erp_AccountId';
GO
EXEC sp_rename 'FK_SpecialIncludesAndExcludes_B2BSalesOrganisation', 'FK_Erp_Special_Includes_And_Excludes_Erp_SalesOrg_Erp_SalesOrg_Id';
GO
EXEC sp_rename 'FK_SpecialIncludesAndExcludes_Product', 'FK_Erp_Special_Includes_And_Excludes_Product_Product_Id';
GO


-- 21.
---------------------- CustomPictureBinaryForERP -> Parallel_CustomPictureBinaryForERP ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'CustomPictureBinaryForERP', 'Parallel_CustomPictureBinaryForERP';
GO




-- 22.
---------------------- ERPProductCategoryMap -> ERPProductCategoryMap ---------------------- No complexity
-- Rename tables
--EXEC sp_rename 'ERPProductCategoryMap', 'ERPProductCategoryMap';
--GO



-- 23.
---------------------- ErpAccountPricing -> Parallel_ErpAccountPricing ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpAccountPricing', 'Parallel_ErpAccountPricing';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpAccountPricing', 'PK_Parallel_ErpAccountPricing';
GO


-- 24.
---------------------- ErpB2BAccount -> Parallel_ErpAccount ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpB2BAccount', 'Parallel_ErpAccount';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpB2BAccount', 'PK_Parallel_ErpAccount';
GO


-- 25.
---------------------- ErpB2BFinancialTransaction -> Parallel_ErpInvoice ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpB2BFinancialTransaction', 'Parallel_ErpInvoice';
GO



-- 26.
---------------------- ErpB2BShipToAddress -> Parallel_ErpShipToAddress ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpB2BShipToAddress', 'Parallel_ErpShipToAddress';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpB2BShipToAddress', 'PK_Parallel_ErpShipToAddress';
GO


-- 27.
---------------------- ErpOrder -> Parallel_ErpOrder ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpOrder', 'Parallel_ErpOrder';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpOrder', 'PK_Parallel_ErpOrder';
GO


-- 28.
---------------------- ErpProduct -> Parallel_ErpProduct ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpProduct', 'Parallel_ErpProduct';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpProduct', 'PK_Parallel_ErpProduct';
GO


-- 29.
---------------------- ErpStock -> Parallel_ErpStock ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'ErpStock', 'Parallel_ErpStock';
GO

-- Rename the constraints
EXEC sp_rename 'PK_ErpStock', 'PK_Parallel_ErpStock';
GO



-- 30.
---------------------- B2BUserFavourite -> Erp_User_Favourite ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BUserFavourite', 'Erp_User_Favourite';
GO

-- Rename the constraints
EXEC sp_rename 'PK_B2BUserFavourite', 'PK_Erp_User_Favourite';
GO
EXEC sp_rename 'FK_B2BUserFavourite_B2BUserInformation_B2BUserInformationId', 'FK_Erp_User_Favourite_Erp_User_Information_Erp_User_Information_Id';
GO

-- Rename columns
EXEC sp_rename 'Erp_User_Favourite.B2BUserInformationId', 'ErpNopUser_Id', 'COLUMN';
GO
EXEC sp_rename 'Erp_User_Favourite.NopCustomerId', 'NopCustomer_Id', 'COLUMN';
GO




    -- 31.
    ---------------------- B2BUserInformation -> Erp_Nop_User ---------------------- Be careful
    -- Rename tables
    EXEC sp_rename 'B2BUserInformation', 'Erp_Nop_User';
    GO

    -- Rename the constraints
    EXEC sp_rename 'PK_B2BUserInformation', 'PK_Erp_Nop_User';
    GO
    EXEC sp_rename 'FK_B2BUserInformation_B2BAccount_B2BAccountId', 'FK_Erp_Nop_User_Erp_Account_Erp_Account_Id';
    GO

    -- Rename columns
    EXEC sp_rename 'Erp_Nop_User.B2BAccountId', 'ErpAccountId', 'COLUMN';
    EXEC sp_rename 'Erp_Nop_User.NopCustomerId', 'NopCustomer_Id', 'COLUMN';
    EXEC sp_rename 'Erp_Nop_User.B2BShipToAddressId', 'ErpShipToAddress_Id', 'COLUMN';
    EXEC sp_rename 'Erp_Nop_User.ShippingB2BShipToAddressId', 'ShippingErpShipToAddress_Id', 'COLUMN';
    EXEC sp_rename 'Erp_Nop_User.BillingB2BShipToAddressId', 'BillingErpShipToAddress_Id', 'COLUMN';
    GO

    -- Add columns
    ALTER TABLE Erp_Nop_User 
    ADD ErpUserTypeId INT NULL,
	    B2CUser_Id INT NULL,
	    LastWarehouseCalculationTimeUtc DATETIME2 NULL,	
	    TotalSavingsForthisYear [decimal](18, 4) NULL,
	    TotalSavingsForthisYearUpdatedOnUtc DATETIME2 NULL,
	    TotalSavingsForAllTime [decimal](18, 4) NULL,
	    TotalSavingsForAllTimeUpdatedOnUtc DATETIME2 NULL
    GO

    -- Add constraints
    ALTER TABLE [dbo].[Erp_Nop_User]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Nop_User_ErpShipToAddress_Id_Erp_ShipToAddress_Id] FOREIGN KEY([ErpShipToAddress_Id])
    REFERENCES [dbo].[Erp_ShipToAddress] ([Id])
    GO

    ALTER TABLE [dbo].[Erp_Nop_User] CHECK CONSTRAINT [FK_Erp_Nop_User_ErpShipToAddress_Id_Erp_ShipToAddress_Id]
    GO





-- 32.
---------------------- Create [Erp_Sales_Rep_Erp_Account_Map] Table ---------------------- No complexity

-- Object:  Table [dbo].[Erp_Sales_Rep_Erp_Account_Map]    Script Date: 12-Dec-24 06:20:07 PM
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Sales_Rep_Erp_Account_Map](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpSalesRep_Id] [int] NOT NULL,
	[ErpAccountId] [int] NOT NULL,
 CONSTRAINT [PK_Erp_Sales_Rep_Erp_Account_Map] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Sales_Rep_Erp_Account_Map]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Sales_Rep_Erp_Account_Map_ErpAccountId_Erp_Account_Id] FOREIGN KEY([ErpAccountId])
REFERENCES [dbo].[Erp_Account] ([Id])
GO

ALTER TABLE [dbo].[Erp_Sales_Rep_Erp_Account_Map] CHECK CONSTRAINT [FK_Erp_Sales_Rep_Erp_Account_Map_ErpAccountId_Erp_Account_Id]
GO

ALTER TABLE [dbo].[Erp_Sales_Rep_Erp_Account_Map]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Sales_Rep_Erp_Account_Map_ErpSalesRep_Id_Erp_Sales_Rep_Id] FOREIGN KEY([ErpSalesRep_Id])
REFERENCES [dbo].[Erp_Sales_Rep] ([Id])
GO

ALTER TABLE [dbo].[Erp_Sales_Rep_Erp_Account_Map] CHECK CONSTRAINT [FK_Erp_Sales_Rep_Erp_Account_Map_ErpSalesRep_Id_Erp_Sales_Rep_Id]
GO





-- 33.
---------------------- Create [Erp_Nop_User_Account_Map] Table ---------------------- Be careful

-- Object:  Table [dbo].[Erp_Nop_User_Account_Map]    Script Date: 12-Dec-24 06:31:52 PM
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Nop_User_Account_Map](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpAccountId] [int] NOT NULL,
	[ErpUser_Id] [int] NOT NULL,
 CONSTRAINT [PK_Erp_Nop_User_Account_Map] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Nop_User_Account_Map]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Nop_User_Account_Map_ErpAccountId_Erp_Account_Id] FOREIGN KEY([ErpAccountId])
REFERENCES [dbo].[Erp_Account] ([Id])
GO

ALTER TABLE [dbo].[Erp_Nop_User_Account_Map] CHECK CONSTRAINT [FK_Erp_Nop_User_Account_Map_ErpAccountId_Erp_Account_Id]
GO

ALTER TABLE [dbo].[Erp_Nop_User_Account_Map]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Nop_User_Account_Map_ErpUser_Id_Erp_Nop_User_Id] FOREIGN KEY([ErpUser_Id])
REFERENCES [dbo].[Erp_Nop_User] ([Id])
GO

ALTER TABLE [dbo].[Erp_Nop_User_Account_Map] CHECK CONSTRAINT [FK_Erp_Nop_User_Account_Map_ErpUser_Id_Erp_Nop_User_Id]
GO

-- Make the ErpNopUserAccountMap CustomerRoleIds column nullable
--ALTER TABLE [Erp_Nop_User_Account_Map]
--ALTER COLUMN [CustomerRoles_Ids] NVARCHAR(MAX) NULL;
--GO

-- 34.
---------------------- Create [Erp_Warehouse_Additional_Data] Table ---------------------- Be careful
-- Object:  Table [dbo].[Erp_Warehouse_Additional_Data]    Script Date: 12-Dec-24 07:11:10 PM 

-- we dont need this table anymore




-- 35.
---------------------- Create [Erp_Warehouse_Sales_Org_Map] Table ---------------------- Be careful
-- Object:  Table [dbo].[Erp_Warehouse_Sales_Org_Map]    Script Date: 12-Dec-24 07:10:36 PM
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[Erp_Warehouse_Sales_Org_Map](
	[Id] INT IDENTITY(1,1) NOT NULL,
    [NopWarehouseId] INT NOT NULL,
    [ErpSalesOrgId] INT NOT NULL,
    [WarehouseCode] NVARCHAR(MAX) NULL,
    [IsB2CWarehouse] BIT NOT NULL DEFAULT(0),
    [LastSyncedOnUtc] DATETIME2 NULL,
 CONSTRAINT [PK_Erp_Warehouse_Sales_Org_Map] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Erp_Warehouse_Sales_Org_Map]  WITH CHECK ADD  CONSTRAINT [FK_Erp_Warehouse_Sales_Org_Map_ErpSalesOrgId_Erp_Sales_Org_Id] FOREIGN KEY([ErpSalesOrgId])
REFERENCES [dbo].[Erp_Sales_Org] ([Id])
GO

ALTER TABLE [dbo].[Erp_Warehouse_Sales_Org_Map] CHECK CONSTRAINT [FK_Erp_Warehouse_Sales_Org_Map_ErpSalesOrgId_Erp_Sales_Org_Id]
GO

-- 36.
---------------------- Create [ErpUserRegistrationInfo] Table ----------------------

CREATE TABLE [dbo].[Erp_User_Registration_Info](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NopCustomerId] [int] NOT NULL,
	[ErpSalesOrgId] [int] NULL,
	[ErpUserId] [int] NULL,
	[ErpUserTypeId] [int] NULL,
	[NearestWareHouseId] [int] NULL,
	[AddressId] [int] NULL,
	[DeliveryOptionId] [int] NULL,
	[DistanceToNearestWarehouse] [float] NULL,
	[Longitude] [float] NULL,
	[Latitude] [float] NULL,
	[HouseNumber] [nvarchar](max) NULL,
	[Street] [nvarchar](max) NULL,
	[Suburb] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[PostalCode] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[SpecialInstructions] [nvarchar](max) NULL,
	[QueuedEmailInfo] [nvarchar](max) NULL,
	[AuthorisationFullName] [nvarchar](max) NULL,
	[AuthorisationContactNumber] [nvarchar](max) NULL,
	[AuthorisationAlternateContactNumber] [nvarchar](max) NULL,
	[PersonalAlternateContactNumber] [nvarchar](max) NULL,
	[AuthorisationJobTitle] [nvarchar](max) NULL,
	[AuthorisationAdditionalComment] [nvarchar](max) NULL,
    [ErpAccountIdForB2C] [int] NULL,
    [ErpSalesOrganisationIds] [nvarchar](max) NULL,
    [ErpAccountNumber] [nvarchar](max) NULL
 CONSTRAINT [PK_Erp_User_Registration_Info] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- 37.
---------------------- B2BCategoryImageShow -> Erp_Category_Image_Show ---------------------- No complexity
-- Rename tables
EXEC sp_rename 'B2BCategoryImageShow', 'Erp_Category_Image_Show';
GO


-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------






-------------------------------------------- Merging multiple entity data -----------------------------------------
-------------------------------------------------------------------------------------------------------------------




---------------------- B2BShipToAddress + B2CShipToAddress -> Erp_ShipToAddress ----------------------

-- Insert data from B2CShipToAddress into Erp_ShipToAddress
INSERT INTO Erp_ShipToAddress (
	IsActive,
	CreatedOnUtc,
	CreatedById,
	UpdatedOnUtc,
	UpdatedById,
	IsDeleted,
	ShipToCode,
	ShipToName,
	Address_Id,
	Suburb,
	DeliveryNotes,
	EmailAddresses,
	B2BAccountId,
	B2BSalesOrganisationId,
	ShipToAddressCreatedByTypeId,
	OrderId,
	ProvinceCode,
	RepNumber,
	RepFullName,
	RepPhoneNumber,
	RepEmail,
	Comment,
	LastShipToAddressSyncDate,
    DeliveryOptionId,
	Latitude,
	Longitude,
	DistanceToNearestWareHouse,
	NearestWareHouseId,
	B2CShipToAddressId
)
SELECT 
	CASE 
        WHEN IsActive IS NOT NULL 
            THEN IsActive
        ELSE 
			1
	END AS IsActive,
	CASE 
        WHEN CreatedOnUtc IS NOT NULL 
            THEN CreatedOnUtc
        ELSE 
			GETUTCDATE()
	END AS CreatedOnUtc,
	CASE 
        WHEN CreatedById IS NOT NULL 
            THEN CreatedById
        ELSE 
			1
	END AS CreatedById,
	CASE 
        WHEN UpdatedOnUtc IS NOT NULL 
            THEN UpdatedOnUtc
        ELSE 
			GETUTCDATE()
	END AS UpdatedOnUtc,
	CASE 
        WHEN UpdatedById IS NOT NULL 
            THEN UpdatedById
        ELSE 
			1
	END AS UpdatedById,
	CASE 
        WHEN IsDeleted IS NOT NULL 
            THEN IsDeleted
        ELSE 
			0
	END AS IsDeleted,
	ShipToCode,
	ShipToName,
	AddressId,
	Suburb,
	DeliveryNotes,
	EmailAddresses,
	B2BAccountIdForB2C,
	B2BSalesOrganisationId,
	ShipToAddressCreatedByTypeId,
	OrderId,
	ProvinceCode,
	RepNumber,
	RepFullName,
	RepPhoneNumber,
	RepEmail,
    NULL AS Comment,
	NULL AS LastShipToAddressSyncDate,
    DeliveryOptionId AS DeliveryOptionId,
	Latitute AS Latitude,
	Longitute AS Longitude,
	DistanceToNearestWareHouse AS DistanceToNearestWareHouse,
	NearestWareHouseId AS NearestWareHouseId,
    Id AS B2CShipToAddressId
FROM B2CShipToAddress;
GO

-- Insert mapping for Erp_ShipToAddress
INSERT INTO Erp_ShiptoAddress_Erp_Account_Map (ErpAccountId, ErpShiptoAddress_Id)
SELECT 
    B2BAccountId AS ErpAccountId,
    Id AS ErpShiptoAddress_Id
FROM 
    Erp_ShipToAddress


Update Erp_ShiptoAddress_Erp_Account_Map 
SET ErpShipToAddressCreatedByTypeId = 20 
FROM Erp_ShipToAddress shipTo Join Erp_ShiptoAddress_Erp_Account_Map accShipToMap 
On accShipToMap.[ErpShiptoAddress_Id] = shipTo.[Id]
Where shipTo.[ShipToAddressCreatedByTypeId] = 10 or shipTo.[ShipToAddressCreatedByTypeId] = 15
GO

Update Erp_ShiptoAddress_Erp_Account_Map 
SET ErpShipToAddressCreatedByTypeId = 10 
FROM Erp_ShipToAddress shipTo Join Erp_ShiptoAddress_Erp_Account_Map accShipToMap 
On accShipToMap.[ErpShiptoAddress_Id] = shipTo.[Id]
Where shipTo.[ShipToAddressCreatedByTypeId] = 5
GO



---------------------- B2BOrderPerAccount + B2COrderPerUser -> Erp_Order_Additional_Data ----------------------


-- Update B2COrderPerUser table for replacing ShipToAddressId with new ones on ErpShipToAddress table
--UPDATE B2COrderPerUser
--SET B2COrderPerUser.[B2CShiptoAddressId] = addr.Id
--FROM B2COrderPerUser INNER JOIN Erp_ShipToAddress addr ON B2COrderPerUser.[B2CShiptoAddressId] = addr.B2CShipToAddressId 
--GO


-- if no need, please remove
--UPDATE B2COrderPerUser
--SET B2COrderPerUser.[B2CShiptoAddressId] = erpShipTo.[Id] 
--FROM B2COrderPerUser 
--Inner Join Erp_ShiptoAddress_Erp_Account_Map shipToMap On B2COrderPerUser.[B2BAccountId] = shipToMap.[ErpAccountId] 
--Inner Join Erp_ShipToAddress erpShipTo On erpShipTo.[Id] = shipToMap.[ErpShiptoAddress_Id] 
--GO

-- Insert data from B2COrderPerUser into Erp_Order_Additional_Data
INSERT INTO Erp_Order_Additional_Data (
    NopOrder_Id,
    OrderNumber,
    ErpOrderOriginTypeId,
    ErpOrderTypeId,
    QuoteExpiryDate,
    QuoteSalesOrder_Id,
    ErpAccountId,
    ErpSalesOrgId,
    ErpShipToAddress_Id,
    CustomerReference,
    ErpOrderNumber,
    ERPOrderStatus,
    DeliveryDate,
    IntegrationStatusTypeId,
    IntegrationError,
    IntegrationErrorDateTimeUtc,
    LastERPUpdateUtc,
    ChangedOnUtc,
    ChangedById,
    OrderPlacedByNopCustomer_Id,
    IntegrationRetries,
    SpecialInstructions,
    IsShippingAddressModified,
    ErpOrderPlaceByCustomerTypeId,
    IsOrderPlaceNotificationSent,
    PaygateReferenceNumber,
    CashRounding,
    ShippingCost,
    B2COrderPerUserId
)
SELECT 
    o.NopOrderId AS NopOrder_Id,
    o.OrderNumber,
    o.B2BOrderOriginTypeId AS ErpOrderOriginTypeId,
    o.B2BOrderTypeId AS ErpOrderTypeId,
    o.QuoteExpiryDate,
    o.QuoteSalesOrderId AS QuoteSalesOrder_Id,
    o.B2BAccountId AS ErpAccountId,
    o.B2BSalesOrganisationId AS ErpSalesOrgId,
    s.Id AS ErpShipToAddress_Id,   -- mapped from ERP table
    o.CustomerReference,
    o.ERPOrderNumber AS ErpOrderNumber,
    o.ERPOrderStatus,
    o.DeliveryDate,
    o.IntegrationStatusTypeId,
    o.IntegrationError,
    o.IntegrationErrorDateTimeUtc,
    o.LastERPUpdateUtc,
    o.ChangedOnUtc,
    o.ChangedById,
    o.OrderPlacedByNopCustomerId,
    o.IntegrationRetries,
    o.SpecialInstructions,
    o.IsShippingAddressModified,
    o.B2BOrderPlacedByCustomerTypeId AS ErpOrderPlaceByCustomerTypeId,
    o.IsOrderPlacedNotificationSent,
    o.PaygateReferenceNumber,
    o.CashRounding,
    o.ShippingCost,
    o.Id AS B2COrderPerUserId
FROM B2COrderPerUser o
LEFT JOIN Erp_ShipToAddress s
    ON s.B2CShipToAddressId = o.B2CShipToAddressId
Where o.B2BAccountId != 0
--WHERE o.B2CShipToAddressId IS NOT NULL
--  AND o.B2CShipToAddressId <> 0;
GO

--UPDATE eoad
--SET eoad.[ErpShipToAddress_Id] = esa.[Id]
--FROM Erp_Order_Additional_Data eoad
--INNER JOIN Erp_ShipToAddress esa ON eoad.[ErpShipToAddress_Id] = esa.[B2CShipToAddressId]
--GO


---------------------- B2BOrderItem + B2COrderItem -> Erp_Order_Item_Additional_Data ----------------------

--UPDATE B2COrderItem 
--SET B2COrderItem.[B2COrderPerUserId] = eod.Id 
--From B2COrderItem
--INNER JOIN Erp_Order_Additional_Data eod
--    ON B2COrderItem.[B2COrderPerUserId] = eod.[B2COrderPerUserId]
--GO

-- if no need, please remove
--UPDATE B2COrderItem 
--SET B2COrderPerUserId = eod.Id 
--FROM B2COrderItem b2coi
--INNER JOIN OrderItem oi
--    ON b2coi.NopOrderItemId = oi.Id
--INNER JOIN Erp_Order_Additional_Data eod
--    ON oi.OrderId = eod.NopOrder_Id
--GO

INSERT INTO Erp_Order_Item_Additional_Data (
    NopOrderItem_Id,
    ErpOrderLineNumber,
    ErpSalesUoM,
    ErpOrderLineStatus,
    ErpDateRequired,
    ErpDateExpected,
    ErpDeliveryMethod,
    ErpInvoiceNumber,
    ErpOrderLineNotes,
    LastERPUpdateUtc,
    ChangedOnUtc,
    ChangedBy,
    ErpOrder_Id, 
    Warehouse, 
    DiscountPercentage, 
    ListPricePerUnit, 
    TotalListPriceForQuantity, 
    BatchCode, 
    SpecialInstruction, 
    DeliveryDate, 
    NopWarehouseId
)
SELECT 
    b2coi.[NopOrderItemId],
    b2coi.[ERPOrderLineNumber],
    b2coi.[ERPSalesUoM],
    b2coi.[ERPOrderLineStatus],
    b2coi.[ERPDateRequired],
    b2coi.[ERPDateExpected],
    b2coi.[ERPDeliveryMethod],
    b2coi.[ERPInvoiceNumber],
    b2coi.[ERPOrderLineNotes],
    b2coi.[LastERPUpdateUtc],
    b2coi.[ChangedOnUtc],
    b2coi.[ChangedBy],
    eod.[Id] AS ErpOrder_Id, 
    b2coi.[WarehouseCode] AS Warehouse,
    NULL AS DiscountPercentage,
    NULL AS ListPricePerUnit,
    NULL AS TotalListPriceForQuantity,
    NULL AS BatchCode,
    b2coi.[SpecialInstructions] AS SpecialInstruction,
    b2coi.[DeliveryDate],
    b2coi.[NopWarehouseId]
FROM B2COrderItem b2coi
LEFT JOIN Erp_Order_Additional_Data eod
    ON b2coi.[B2COrderPerUserId] = eod.[B2COrderPerUserId]; 
--INNER JOIN OrderItem oi
--    ON b2coi.[NopOrderItemId] = oi.[Id]
--INNER JOIN Erp_Order_Additional_Data eod
--    ON oi.[OrderId] = eod.[NopOrder_Id]
--Where b2coi.[B2COrderPerUserId] = eod.[Id]
--GO


-------------------------------------------
-- Set the ErpOrder_Id values for foreign key 
--UPDATE erpOrderItemData 
--SET erpOrderItemData.[ErpOrder_Id] = eod.[Id] 
--FROM Erp_Order_Item_Additional_Data erpOrderItemData
--INNER JOIN OrderItem oi ON erpOrderItemData.NopOrderItem_Id = oi.Id
--INNER JOIN Erp_Order_Additional_Data eod ON oi.OrderId = eod.NopOrder_Id


-- Add constraints
ALTER TABLE [dbo].[Erp_Order_Item_Additional_Data] WITH CHECK ADD CONSTRAINT [FK_Erp_Order_Item_Additional_Data_Erp_Order_Additional_Data_Erp_Order_Additional_Data_Id]
FOREIGN KEY ([ErpOrder_Id]) REFERENCES [dbo].[Erp_Order_Additional_Data] ([Id]);
GO

ALTER TABLE [dbo].[Erp_Order_Item_Additional_Data] CHECK CONSTRAINT [FK_Erp_Order_Item_Additional_Data_Erp_Order_Additional_Data_Erp_Order_Additional_Data_Id];
GO



---------------------- B2BUserInformation + B2CUser -> Erp_Nop_User ----------------------


--UPDATE b2cUser
--SET b2cUser.[B2CShipToAddressId] = esa.[Id]
--FROM B2CUser b2cUser
--INNER JOIN Erp_ShipToAddress esa ON b2cUser.[B2CShipToAddressId] = esa.[B2CShipToAddressId]
--GO

-- if no need, please remove
--UPDATE b2cUser
--SET b2cUser.[B2CShiptoAddressId] = erpShipTo.[Id] 
--FROM B2CUser b2cUser
--Inner Join Erp_ShiptoAddress_Erp_Account_Map shipToMap On B2CUser.[B2BAccountIdForB2C] = shipToMap.[ErpAccountId] 
--Inner Join Erp_ShipToAddress erpShipTo On erpShipTo.[Id] = shipToMap.[ErpShiptoAddress_Id] 
--GO

-- need to talk
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = 'RegistrationAuthorisedBy'
      AND Object_ID = Object_ID('Erp_Nop_User')
)
BEGIN
    ALTER TABLE Erp_Nop_User
    ADD RegistrationAuthorisedBy NVARCHAR(500) NULL;
END
GO

INSERT INTO Erp_Nop_User (
    IsActive,
    CreatedOnUtc,
    CreatedById,
    UpdatedOnUtc,
    UpdatedById,
    IsDeleted,
    NopCustomer_Id,
    ErpAccountId,
    ErpShipToAddress_Id,
    BillingErpShipToAddress_Id,
    ShippingErpShipToAddress_Id,
    SalesOrgAsCustomerRoleId,
    RegistrationAuthorisedBy,
    TotalSavingsForthisYear,
    TotalSavingsForthisYearUpdatedOnUtc,
    TotalSavingsForAllTime,
    TotalSavingsForAllTimeUpdatedOnUtc,
    LastWarehouseCalculationTimeUtc,
    ErpUserTypeId,    
    B2CUser_Id
)
SELECT 
    CASE 
        WHEN b.IsActive IS NOT NULL 
            THEN b.IsActive
        ELSE 1
    END AS IsActive,
    CASE 
        WHEN b.CreatedOnUtc IS NOT NULL 
            THEN b.CreatedOnUtc
        ELSE GETUTCDATE()
    END AS CreatedOnUtc,
    CASE 
        WHEN b.CreatedById IS NOT NULL 
            THEN b.CreatedById
        ELSE 1
    END AS CreatedById,
    CASE 
        WHEN b.UpdatedOnUtc IS NOT NULL 
            THEN b.UpdatedOnUtc
        ELSE GETUTCDATE()
    END AS UpdatedOnUtc,
    CASE 
        WHEN b.UpdatedById IS NOT NULL 
            THEN b.UpdatedById
        ELSE 1
    END AS UpdatedById,
    CASE 
        WHEN b.IsDeleted IS NOT NULL 
            THEN b.IsDeleted
        ELSE 0
    END AS IsDeleted,
    b.NopCustomerId AS NopCustomer_Id,
    b.B2BAccountIdForB2C AS ErpAccountId,
    esa.Id AS ErpShipToAddress_Id,
    0 AS BillingErpShipToAddress_Id,
    0 AS ShippingErpShipToAddress_Id,
    0 AS SalesOrgAsCustomerRoleId,
    0 AS RegistrationAuthorisedBy,  
    b.TotalSavingsForthisYear,
    b.TotalSavingsForthisYearUpdatedOnUtc,
    b.TotalSavingsForAllTime,
    b.TotalSavingsForAllTimeUpdatedOnUtc,
    b.LastWarehouseCalculationTimeUtc,
    0 AS ErpUserTypeId,
    b.Id AS B2CUser_Id
FROM B2CUser b
LEFT JOIN Erp_ShipToAddress esa
    ON esa.B2CShipToAddressId = b.B2CShipToAddressId
WHERE b.B2CShipToAddressId IS NOT NULL
AND b.B2CShipToAddressId <> 0;
GO


--UPDATE erpNopUser
--SET erpNopUser.[ErpShipToAddress_Id] = erpShipTo.[Id] 
--FROM Erp_Nop_User erpNopUser
--Inner Join Erp_ShiptoAddress_Erp_Account_Map shipToMap On erpNopUser.[ErpAccountId] = shipToMap.[ErpAccountId] 
--Inner Join Erp_ShipToAddress erpShipTo On erpShipTo.[Id] = shipToMap.[ErpShiptoAddress_Id] 
--GO


-- Insert data into Erp_Nop_User_Account_Map
INSERT INTO Erp_Nop_User_Account_Map (
    [ErpAccountId],
    [ErpUser_Id]
)
SELECT 
    erpNopUser.[ErpAccountId],                                
    erpNopUser.[Id] AS ErpUser_Id                         
FROM Erp_Nop_User erpNopUser     
GO


-- Insert the Erp-User-TypeId into the ErpNopUser

-- For B2B Users
 Update [dbo].[Erp_Nop_User] 
 Set [ErpUserTypeId] = 5
 Where [NopCustomer_Id] In
 (Select CCM.[Customer_Id] 
 From [dbo].[Customer_CustomerRole_Mapping] CCM Inner join [dbo].[CustomerRole] CR 
 On CCM.[CustomerRole_Id] = CR.[Id]
 Where CR.[SystemName] = 'B2BCustomer')

 -- For B2C Users
 Update [dbo].[Erp_Nop_User] 
 Set [ErpUserTypeId] = 10
 Where [NopCustomer_Id] In
 (Select CCM.[Customer_Id] 
 From [dbo].[Customer_CustomerRole_Mapping] CCM Inner join [dbo].[CustomerRole] CR 
 On CCM.[CustomerRole_Id] = CR.[Id]
 Where CR.[SystemName] = 'B2CCustomer')




    ---------------------- B2BSalesOrgWarehouse + B2CSalesOrgWarehouse -> [Erp_Warehouse_Sales_Org_Map] ----------------------

-- Insert B2B warehouse data (IsB2CWarehouse = 0)
INSERT INTO [Erp_Warehouse_Sales_Org_Map] 
    ([NopWarehouseId], [ErpSalesOrgId], [WarehouseCode], [LastSyncedOnUtc], [IsB2CWarehouse])
SELECT 
    [NopWarehouseId],
    [B2BSalesOrgId],
    [Code],
    [LastTimeSyncOnUtc],
    0 AS [IsB2CWarehouse]
FROM [B2BSalesOrgWarehouse];

-- Insert B2C warehouse data (IsB2CWarehouse = 1)
INSERT INTO [Erp_Warehouse_Sales_Org_Map] 
    ([NopWarehouseId], [ErpSalesOrgId], [WarehouseCode], [LastSyncedOnUtc], [IsB2CWarehouse])
SELECT 
    [NopWarehouseId],
    [B2BSalesOrganisationId],
    [WarehouseCode],
    [LastSyncedOnUtc],
    1 AS [IsB2CWarehouse]
FROM [B2CSalesOrgWarehouse];


---------------------- B2BUserFavourite + B2CUserFavourite -> Erp_User_Favourite ----------------------


-- Insert data from B2CUserFavourite into Erp_User_Favourite
INSERT INTO Erp_User_Favourite (
	NopCustomer_Id,
	ErpNopUser_Id
)
SELECT 
	NopCustomerId AS NopCustomer_Id,
	enu.[Id] AS ErpNopUser_Id
FROM B2CUserFavourite INNER JOIN Erp_Nop_User enu On B2CUserFavourite.[B2CUserId] = enu.[B2CUser_Id];
GO

---------------------- [B2BUserRegistrationInfo] + [B2CRegistrationInfo] -> [Erp_User_Registration_Info] ----------------------


INSERT INTO [dbo].[Erp_User_Registration_Info] (
    [NopCustomerId],
    [ErpSalesOrganisationIds],
    [ErpAccountNumber],    
    [SpecialInstructions],
    [QueuedEmailInfo],
    [AuthorisationFullName],
    [AuthorisationContactNumber],
    [AuthorisationAlternateContactNumber],
    [PersonalAlternateContactNumber],
    [AuthorisationJobTitle],
    [AuthorisationAdditionalComment],
    [ErpUserTypeId]
)
SELECT
    b.[NopCustomerId],
    b.B2BSalesOrganisationIds AS ErpSalesOrganisationIds,
    b.AccountNumber AS ErpAccountNumber,
    b.[SpecialInstructions],
    b.[QueuedEmailInfo],
    b.[AuthorisationFullName],
    b.[AuthorisationContactNumber],
    b.[AuthorisationAlternateContactNumber],
    b.[PersonalAlternateContactNumber],
    b.[AuthorisationJobTitle],
    b.[AuthorisationAdditionalComment],
    5 AS ErpUserTypeId
FROM 
    [dbo].[B2BUserRegistrationInfo] b

-- Update B2CRegistrationInfo table for replacing B2CUserId with new ones on Erp_Nop_User table
UPDATE B2CRegistrationInfo
SET B2CRegistrationInfo.B2CUserId = erpNopUser.Id
FROM B2CRegistrationInfo INNER JOIN Erp_Nop_User erpNopUser ON B2CRegistrationInfo.B2CUserId = erpNopUser.B2CUser_Id --this is old id of b2cUser table
GO


-- Migrate data from B2CUserRegistrationInfo
INSERT INTO [dbo].[Erp_User_Registration_Info] (
	[NopCustomerId],
	[ErpSalesOrgId],
	[ErpAccountIdForB2C],
	[ErpUserId],
	[NearestWareHouseId],
	[AddressId],
	[DeliveryOptionId],
	[DistanceToNearestWarehouse],
	[Longitude],
	[Latitude],
	[HouseNumber],
	[Street],
	[Suburb],
	[City],
	[PostalCode],
	[Country],
	[ErrorMessage],
	[ErpUserTypeId]
)
SELECT
	c.[NopCustomerId],
	c.[B2BSalesOrganisationId] AS ErpSalesOrgId,
	c.[B2BAccountIdForB2C] AS ErpAccountIdForB2C,
	c.[B2CUserId] AS ErpUserId,
	c.[NearestWarehouseId],
	c.[AddressId],
	c.[DeliveryOption] AS DeliveryOptionId,
	c.[DistanceToNearestWarehouse],
	c.[Longitude],
	c.[Latitude],
	c.[HouseNumber],
	c.[Street],
	c.[Suburb],
	c.[City],
	c.[PostalCode],
	c.[Country],
	c.[ErrorMessage],
	10 AS ErpUserTypeId
FROM [dbo].[B2CRegistrationInfo] c;
GO


---------------------- B2BSalesRepMultiAccountShipto -> Erp_Sales_Rep_Erp_Account_Map ----------------------

-- for b2b
Insert into [dbo].[Erp_Sales_Rep_Erp_Account_Map] (ErpSalesRep_Id, ErpAccountId)
SELECT salesShipToMap.[B2BSalesRepId] as ErpSalesRepId, accShipToMap.[ErpAccountId] as ErpAccountId
FROM [dbo].[B2BSalesRepMultiAccountShipto] salesShipToMap
  Left Join [dbo].[Erp_Nop_User] erpNopUser On salesShipToMap.[B2BShipToAddressId] = erpNopUser.[ErpShipToAddress_Id]
  Left Join [dbo].[Erp_ShipToAddress] shipTo On erpNopUser.[ErpShipToAddress_Id] = shipTo.[Id]
  Left Join [dbo].[Erp_ShiptoAddress_Erp_Account_Map] accShipToMap On accShipToMap.[ErpShiptoAddress_Id] = shipTo.[Id]
Where shipTo.[B2CShipToAddressId] is null --And shipTo.[IsActive] = 1 
        --And shipTo.[IsDeleted] = 0
GO

---- for b2c
--Insert into [dbo].[Erp_Sales_Rep_Erp_Account_Map] (ErpSalesRep_Id, ErpAccountId)
--SELECT salesShipToMap.[B2BSalesRepId] as ErpSalesRepId, accShipToMap.[ErpAccountId] as ErpAccountId
--FROM [dbo].[B2BSalesRepMultiAccountShipto] salesShipToMap
--  Left Join [dbo].[Erp_Nop_User] erpNopUser On salesShipToMap.[B2BShipToAddressId] = erpNopUser.[ErpShipToAddress_Id]
--  Left Join [dbo].[Erp_ShipToAddress] shipTo On erpNopUser.[ErpShipToAddress_Id] = shipTo.[Id]
--  Left Join [dbo].[Erp_ShiptoAddress_Erp_Account_Map] accShipToMap On accShipToMap.[ErpShiptoAddress_Id] = shipTo.[Id]
--Where shipTo.[B2CShipToAddressId] is not null --And shipTo.[IsActive] = 1 
--        --And shipTo.[IsDeleted] = 0
--GO

-- All ok upto this line


-- 1. Rename and Modify Slider Tables
EXEC sp_rename 'SS_AS_SliderImage', 'SS_AS_Slide';
GO

ALTER TABLE SS_AS_Slide
ADD SlideType int NOT NULL DEFAULT 0,
    SystemName nvarchar(max) NULL,
    Content nvarchar(max) NULL;
GO

-- Rename foreign key constraint
EXEC sp_rename 'FK_SS_AS_SliderImage_SS_AS_AnywhereSlider_SliderId', 'FK_SS_AS_Slide_SliderId_SS_AS_AnywhereSlider_Id';
GO

-- 2. Modify AnywhereSlider
ALTER TABLE SS_AS_AnywhereSlider
ADD PreLoadFirstSlide bit NOT NULL DEFAULT 0,
    Autoplay bit NOT NULL DEFAULT 0,
    AutoplaySpeed int NOT NULL DEFAULT 0,
    Speed int NOT NULL DEFAULT 0,
    PauseOnHover bit NOT NULL DEFAULT 0,
    Fade bit NOT NULL DEFAULT 0,
    Dots bit NOT NULL DEFAULT 0,
    Arrows bit NOT NULL DEFAULT 0,
    MobileBreakpoint int NOT NULL DEFAULT 0,
    CustomClass nvarchar(max) NULL;
GO

-- Remove SliderType column as it's not in 4.7
ALTER TABLE SS_AS_AnywhereSlider
DROP COLUMN SliderType;
GO

-- 3. Create Product Ribbon tables (new in 4.7)
if object_id(N'[dbo].[SS_PR_ProductRibbon]') is null
begin
CREATE TABLE [dbo].[SS_PR_ProductRibbon](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Enabled] [bit] NOT NULL,
    [Name] [nvarchar](max) NULL,
    [StopAddingRibbonsAftherThisOneIsAdded] [bit] NOT NULL,
    [Priority] [int] NOT NULL,
    [FromDate] [datetime2](6) NULL,
    [ToDate] [datetime2](6) NULL,
    [LimitedToStores] [bit] NOT NULL,
    CONSTRAINT [PK_SS_PR_ProductRibbon] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

if object_id(N'[dbo].[SS_PR_CategoryPageRibbon]') is null
begin
CREATE TABLE [dbo].[SS_PR_CategoryPageRibbon](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [ProductRibbonId] [int] NOT NULL,
    [PictureId] [int] NULL,
    [Enabled] [bit] NOT NULL,
    [Text] [nvarchar](max) NULL,
    [Position] [nvarchar](max) NULL,
    [TextStyle] [nvarchar](max) NULL,
    [ImageStyle] [nvarchar](max) NULL,
    [ContainerStyle] [nvarchar](max) NULL,
    CONSTRAINT [PK_SS_PR_CategoryPageRibbon] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

if object_id(N'[dbo].[SS_PR_ProductPageRibbon]') is null
begin
CREATE TABLE [dbo].[SS_PR_ProductPageRibbon](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [ProductRibbonId] [int] NOT NULL,
    [PictureId] [int] NULL,
    [Enabled] [bit] NOT NULL,
    [Text] [nvarchar](max) NULL,
    [Position] [nvarchar](max) NULL,
    [TextStyle] [nvarchar](max) NULL,
    [ImageStyle] [nvarchar](max) NULL,
    [ContainerStyle] [nvarchar](max) NULL,
    CONSTRAINT [PK_SS_PR_ProductPageRibbon] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

if object_id(N'[dbo].[SS_PR_RibbonPicture]') is null
begin
CREATE TABLE [dbo].[SS_PR_RibbonPicture](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [PictureId] [int] NOT NULL,
    CONSTRAINT [PK_SS_PR_RibbonPicture] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

-- 4. Create Smart Product Collections tables (new in 4.7)
if object_id(N'[dbo].[SS_SPC_ProductsGroup]') is null
begin
CREATE TABLE [dbo].[SS_SPC_ProductsGroup](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Published] [bit] NOT NULL,
    [Title] [nvarchar](max) NULL,
    [WidgetZone] [nvarchar](max) NULL,
    [Store] [int] NOT NULL,
    [NumberOfProductsPerItem] [int] NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    CONSTRAINT [PK_SS_SPC_ProductsGroup] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

if object_id(N'[dbo].[SS_SPC_ProductsGroupItem]') is null
begin
CREATE TABLE [dbo].[SS_SPC_ProductsGroupItem](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [GroupId] [int] NOT NULL,
    [SourceType] [int] NOT NULL,
    [SortMethod] [int] NOT NULL,
    [Active] [bit] NOT NULL,
    [Title] [nvarchar](max) NULL,
    [EntityId] [int] NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    CONSTRAINT [PK_SS_SPC_ProductsGroupItem] PRIMARY KEY CLUSTERED ([Id] ASC)
);
end
GO

-- 5. Update Schedule table datetime precision
ALTER TABLE SS_S_Schedule
ALTER COLUMN EntityFromDate datetime2(6);
GO

ALTER TABLE SS_S_Schedule
ALTER COLUMN EntityToDate datetime2(6);
GO

-- 6. Add foreign key constraints for new tables
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_SS_PR_CategoryPageRibbon_ProductRibbonId_SS_PR_ProductRibbon_Id'
      AND parent_object_id = OBJECT_ID('dbo.SS_PR_CategoryPageRibbon')
)
BEGIN
    ALTER TABLE [dbo].[SS_PR_CategoryPageRibbon] 
    ADD CONSTRAINT [FK_SS_PR_CategoryPageRibbon_ProductRibbonId_SS_PR_ProductRibbon_Id] 
    FOREIGN KEY([ProductRibbonId]) REFERENCES [dbo].[SS_PR_ProductRibbon] ([Id]) ON DELETE CASCADE;
END
GO


IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_SS_PR_ProductPageRibbon_ProductRibbonId_SS_PR_ProductRibbon_Id'
      AND parent_object_id = OBJECT_ID('dbo.SS_PR_ProductPageRibbon')
)
BEGIN
    ALTER TABLE [dbo].[SS_PR_ProductPageRibbon] 
    ADD CONSTRAINT [FK_SS_PR_ProductPageRibbon_ProductRibbonId_SS_PR_ProductRibbon_Id] 
    FOREIGN KEY([ProductRibbonId]) REFERENCES [dbo].[SS_PR_ProductRibbon] ([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_SS_SPC_ProductsGroupItem_GroupId_SS_SPC_ProductsGroup_Id'
      AND parent_object_id = OBJECT_ID('dbo.SS_SPC_ProductsGroupItem')
)
BEGIN
    ALTER TABLE [dbo].[SS_SPC_ProductsGroupItem] 
    ADD CONSTRAINT [FK_SS_SPC_ProductsGroupItem_GroupId_SS_SPC_ProductsGroup_Id] 
    FOREIGN KEY([GroupId]) REFERENCES [dbo].[SS_SPC_ProductsGroup] ([Id]) ON DELETE CASCADE;
END
GO

-- 7. Rename foreign key constraints to match 4.7 naming convention
EXEC sp_rename 'FK_SS_C_ConditionGroup_SS_C_Condition_ConditionId', 'FK_SS_C_ConditionGroup_ConditionId_SS_C_Condition_Id';
GO

EXEC sp_rename 'FK_SS_C_ConditionStatement_SS_C_ConditionGroup_ConditionGroupId', 'FK_SS_C_ConditionStatement_ConditionGroupId_SS_C_ConditionGroup_Id';
GO

EXEC sp_rename 'FK_SS_C_CustomerOverride_SS_C_Condition_ConditionId', 'FK_SS_C_CustomerOverride_ConditionId_SS_C_Condition_Id';
GO

EXEC sp_rename 'FK_SS_C_EntityCondition_SS_C_Condition_ConditionId', 'FK_SS_C_EntityCondition_ConditionId_SS_C_Condition_Id';
GO

EXEC sp_rename 'FK_SS_MM_MenuItem_SS_MM_Menu_MenuId', 'FK_SS_MM_MenuItem_MenuId_SS_MM_Menu_Id';
GO

EXEC sp_rename 'FK_SS_C_ProductOverride_SS_C_Condition_ConditionId', 'FK_SS_C_ProductOverride_ConditionId_SS_C_Condition_Id';
GO


-- Adding IsFullLoadRequired column to ErpDeliveryDates table

ALTER TABLE [dbo].[ErpDeliveryDates]
ADD [IsFullLoadRequired] BIT NULL;
    
-- Rename columns

EXEC sp_rename 'ErpDeliveryDates.Del_Date1', 'DelDate1', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date2', 'DelDate2', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date3', 'DelDate3', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date4', 'DelDate4', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date5', 'DelDate5', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date6', 'DelDate6', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date7', 'DelDate7', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date8', 'DelDate8', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date9', 'DelDate9', 'COLUMN';
GO
EXEC sp_rename 'ErpDeliveryDates.Del_Date10', 'DelDate10', 'COLUMN';
GO


---------------------- AdditionalCategoryInfoInfo -> AdditionalCategoryInfo ---------------------- 

-- Rename table
EXEC sp_rename 'AdditionalCategoryInfoInfo', 'AdditionalCategoryInfo';


---------------------- n4y_storelocators -> StoreLocators ---------------------- 

-- Rename table
EXEC sp_rename 'n4y_storelocators', 'StoreLocators';