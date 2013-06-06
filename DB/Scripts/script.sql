

/****** Begin Object:  User [IIS APPPOOL\ASP.NET V4.0 Integrated] ******/
CREATE USER [IIS APPPOOL\ASP.NET V4.0 Integrated] FOR LOGIN [IIS APPPOOL\ASP.NET V4.0 Integrated]
GO
/****** End Object:  User [IIS APPPOOL\ASP.NET V4.0 Integrated] ******/


/****** Begin Object:  User [NT AUTHORITY\NETWORK SERVICE] ******/
CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** End Object:  User [NT AUTHORITY\NETWORK SERVICE] ******/


/****** Begin Object:  Table [dbo].[AccountContract] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountContract](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractTypeId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[IssuanceDate] [date] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
	[BillingAmount] [numeric](18, 2) NULL,
	[BillingPeriodType] [varchar](10) NULL,
	[BillingPeriodSize] [int] NULL,
	[BillingPeriodCount] [int] NULL,
	[BillingDueDay] [int] NULL,
	[DoctorsLimit] [int] NULL,
	[PatientsLimit] [int] NULL,
	[IsTrial] [bit] NOT NULL,
	[CustomText] [nvarchar](max) NULL,
	[BillingPaymentMethod] [varchar](20) NULL,
	[BillingDiscountAmount] [numeric](18, 2) NULL,
	[EndReason] [varchar](10) NULL,
	[EndReturnAmount] [numeric](18, 2) NULL,
	[EndReturnTax] [numeric](4, 2) NULL,
	[BillingPeriodIndex] [int] NULL,
 CONSTRAINT [PK_AccountContract] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[AccountContract] ******/


/****** Begin Object:  Table [dbo].[Address] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CEP] [varchar](20) NULL,
	[City] [varchar](100) NULL,
	[StateProvince] [varchar](100) NULL,
	[Neighborhood] [varchar](100) NULL,
	[Complement] [varchar](50) NULL,
	[Street] [varchar](100) NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Address] ******/


/****** Begin Object:  Table [dbo].[Administrator] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Administrator](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_Administrator] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Administrator] ******/


/****** Begin Object:  Table [dbo].[Anamnese] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Anamnese](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[ChiefComplaint] [varchar](max) NOT NULL,
	[HistoryOfThePresentIllness] [varchar](max) NULL,
	[PastMedicalHistory] [varchar](max) NULL,
	[ReviewOfSystems] [varchar](max) NULL,
	[FamilyDiseases] [varchar](max) NULL,
	[SocialDiseases] [varchar](max) NULL,
	[RegularAndAcuteMedications] [varchar](max) NULL,
	[Allergies] [varchar](max) NULL,
	[SexualHistory] [varchar](max) NULL,
	[Conclusion] [varchar](max) NULL,
	[MedicalRecordDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Anamnese] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Anamnese] ******/


/****** Begin Object:  Table [dbo].[Appointment] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[Start] [datetime] NOT NULL,
	[End] [datetime] NOT NULL,
	[DoctorId] [int] NOT NULL,
	[PatientId] [int] NULL,
	[Type] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[PracticeId] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[Notified] [bit] NOT NULL,
	[HealthInsuranceId] [int] NULL,
	[ReminderEmailSent] [bit] NOT NULL,
 CONSTRAINT [PK_Appointment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Appointment] ******/


/****** Begin Object:  Table [dbo].[Billing] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Billing](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[IssuanceDate] [date] NOT NULL,
	[MainAmount] [numeric](18, 2) NOT NULL,
	[MainDiscount] [numeric](18, 2) NOT NULL,
	[MainAccountContractId] [int] NOT NULL,
	[DueDate] [date] NOT NULL,
	[AfterDueTax] [numeric](4, 2) NOT NULL,
	[AfterDueMonthlyTax] [numeric](4, 2) NOT NULL,
	[IdentitySetName] [varchar](50) NOT NULL,
	[IdentitySetNumber] [int] NOT NULL,
	[ReferenceDate] [date] NOT NULL,
	[ReferenceDateEnd] [date] NULL,
	[IsPayd] [bit] NOT NULL,
	[PaydAmount] [numeric](18, 2) NULL,
	[PaymentDate] [date] NULL,
 CONSTRAINT [PK_Billing] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Billing] ******/


/****** Begin Object:  Table [dbo].[BillingItem] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BillingItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[BillingId] [int] NOT NULL,
	[ItemAccountContractId] [int] NOT NULL,
	[ItemName] [varchar](50) NOT NULL,
	[ItemAmount] [numeric](18, 2) NOT NULL,
	[ItemDiscount] [numeric](18, 2) NOT NULL,
 CONSTRAINT [PK_BillingItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[BillingItem] ******/


/****** Begin Object:  Table [dbo].[CFG_DayOff] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CFG_DayOff](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [date] NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
	[DoctorId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_DayOff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[CFG_DayOff] ******/


/****** Begin Object:  Table [dbo].[CFG_Documents] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CFG_Documents](
	[DoctorId] [int] NOT NULL,
	[Header1] [varchar](200) NOT NULL,
	[Header2] [varchar](200) NOT NULL,
	[FooterLeft1] [varchar](200) NOT NULL,
	[FooterLeft2] [varchar](200) NULL,
	[FooterRight1] [varchar](200) NOT NULL,
	[FooterRight2] [varchar](200) NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_CFG_Documents] PRIMARY KEY CLUSTERED 
(
	[DoctorId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[CFG_Documents] ******/


/****** Begin Object:  Table [dbo].[CFG_Schedule] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CFG_Schedule](
	[DoctorId] [int] NOT NULL,
	[AppointmentTime] [int] NOT NULL,
	[SundayWorkdayStartTime] [varchar](5) NULL,
	[SundayWorkdayEndTime] [varchar](5) NULL,
	[SundayLunchStartTime] [varchar](5) NULL,
	[SundayLunchEndTime] [varchar](5) NULL,
	[MondayWorkdayStartTime] [varchar](5) NULL,
	[MondayWorkdayEndTime] [varchar](5) NULL,
	[MondayLunchStartTime] [varchar](5) NULL,
	[MondayLunchEndTime] [varchar](5) NULL,
	[TuesdayWorkdayStartTime] [varchar](5) NULL,
	[TuesdayWorkdayEndTime] [varchar](5) NULL,
	[TuesdayLunchStartTime] [varchar](5) NULL,
	[TuesdayLunchEndTime] [varchar](5) NULL,
	[WednesdayWorkdayStartTime] [varchar](5) NULL,
	[WednesdayWorkdayEndTime] [varchar](5) NULL,
	[WednesdayLunchStartTime] [varchar](5) NULL,
	[WednesdayLunchEndTime] [varchar](5) NULL,
	[ThursdayWorkdayStartTime] [varchar](5) NULL,
	[ThursdayWorkdayEndTime] [varchar](5) NULL,
	[ThursdayLunchStartTime] [varchar](5) NULL,
	[ThursdayLunchEndTime] [varchar](5) NULL,
	[FridayWorkdayStartTime] [varchar](5) NULL,
	[FridayWorkdayEndTime] [varchar](5) NULL,
	[FridayLunchStartTime] [varchar](5) NULL,
	[FridayLunchEndTime] [varchar](5) NULL,
	[SaturdayWorkdayStartTime] [varchar](5) NULL,
	[SaturdayWorkdayEndTime] [varchar](5) NULL,
	[SaturdayLunchStartTime] [varchar](5) NULL,
	[SaturdayLunchEndTime] [varchar](5) NULL,
	[Sunday] [bit] NOT NULL,
	[Monday] [bit] NOT NULL,
	[Tuesday] [bit] NOT NULL,
	[Wednesday] [bit] NOT NULL,
	[Thursday] [bit] NOT NULL,
	[Friday] [bit] NOT NULL,
	[Saturday] [bit] NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_CFG_Schedule_1] PRIMARY KEY CLUSTERED 
(
	[DoctorId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[CFG_Schedule] ******/


/****** Begin Object:  Table [dbo].[ChatMessage] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatMessage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserFromId] [int] NOT NULL,
	[UserToId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Message] [varchar](max) NOT NULL,
 CONSTRAINT [PK_ChatMessage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ChatMessage] ******/


/****** Begin Object:  Table [dbo].[Diagnosis] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Diagnosis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Observations] [varchar](max) NULL,
	[Cid10Code] [varchar](10) NULL,
	[Cid10Name] [varchar](100) NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[MedicalRecordDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Diagnosis2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Diagnosis] ******/


/****** Begin Object:  Table [dbo].[DiagnosticHypothesis] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiagnosticHypothesis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Observations] [text] NULL,
	[Cid10Code] [varchar](10) NULL,
	[Cid10Name] [varchar](500) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PatientId] [int] NOT NULL,
	[MedicalRecordDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Diagnosis] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[DiagnosticHypothesis] ******/


/****** Begin Object:  Table [dbo].[Doctor] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Doctor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CRM] [varchar](50) NOT NULL,
	[MedicalEntityJurisdiction] [nvarchar](50) NULL,
	[UrlIdentifier] [varchar](200) NOT NULL,
	[MedicalEntityCode] [nvarchar](7) NOT NULL,
	[MedicalEntityName] [nvarchar](55) NOT NULL,
	[MedicalSpecialtyCode] [nvarchar](7) NOT NULL,
	[MedicalSpecialtyName] [nvarchar](71) NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_Doctor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Doctor] ******/


/****** Begin Object:  Table [dbo].[ExaminationRequest] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExaminationRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[Text] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[MedicalProcedureCode] [nchar](12) NULL,
	[MedicalProcedureName] [nvarchar](310) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[RequestDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ExaminationRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ExaminationRequest] ******/


/****** Begin Object:  Table [dbo].[ExaminationResult] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExaminationResult](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[MedicalProcedureCode] [nchar](12) NULL,
	[MedicalProcedureName] [nvarchar](310) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[ExaminationDate] [datetime] NOT NULL,
	[ReceiveDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ExaminationResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ExaminationResult] ******/


/****** Begin Object:  Table [dbo].[FileMetadata] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileMetadata](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContainerName] [varchar](250) NOT NULL,
	[BlobName] [nvarchar](500) NOT NULL,
	[Description] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[RelatedFileMetadataId] [int] NULL,
	[RelationType] [nvarchar](50) NULL,
	[ExpirationDate] [datetime] NULL,
	[SourceFileName] [nvarchar](250) NOT NULL,
	[Tag] [nvarchar](64) NULL,
	[OwnerUserId] [int] NULL,
 CONSTRAINT [PK_FileMetadata] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[FileMetadata] ******/


/****** Begin Object:  Table [dbo].[GLB_Token] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLB_Token](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Value] [char](32) NOT NULL,
	[ExpirationDate] [datetime] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Token] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[GLB_Token] ******/


/****** Begin Object:  Table [dbo].[GoogleDrivePatientInfo] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GoogleDrivePatientInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[PatientBackupFileId] [varchar](200) NOT NULL,
 CONSTRAINT [PK_GoogleDrivePatientInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[GoogleDrivePatientInfo] ******/


/****** Begin Object:  Table [dbo].[GoogleDrivePracticeInfo] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GoogleDrivePracticeInfo](
	[int] [int] IDENTITY(1,1) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[CerebelloFolderId] [varchar](200) NOT NULL,
 CONSTRAINT [PK_GoogleDrivePracticeInfo] PRIMARY KEY CLUSTERED 
(
	[int] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[GoogleDrivePracticeInfo] ******/


/****** Begin Object:  Table [dbo].[GoogleUserAccoutInfo] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GoogleUserAccoutInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AuthenticationCode] [varchar](80) NOT NULL,
	[RefreshToken] [varchar](80) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[Email] [varchar](250) NOT NULL,
	[PersonId] [int] NOT NULL,
 CONSTRAINT [PK_GoogleAccoutInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[GoogleUserAccoutInfo] ******/


/****** Begin Object:  Table [dbo].[HealthInsurance] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HealthInsurance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[NewAppointmentValue] [numeric](6, 2) NULL,
	[ReturnAppointmentValue] [numeric](6, 2) NULL,
	[DoctorId] [int] NOT NULL,
	[ReturnTimeInterval] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[IsParticular] [bit] NOT NULL,
 CONSTRAINT [PK_HealthEnsurance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
 CONSTRAINT [IX_HealthInsurance] UNIQUE NONCLUSTERED 
(
	[DoctorId] ASC,
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[HealthInsurance] ******/


/****** Begin Object:  Table [dbo].[Holiday] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Holiday](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Date] [date] NOT NULL,
 CONSTRAINT [PK_Holliday] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Holiday] ******/


/****** Begin Object:  Table [dbo].[Laboratory] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Laboratory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[DoctorId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[Observations] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_MedicineLaboratory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Laboratory] ******/


/****** Begin Object:  Table [dbo].[Leaflet] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Leaflet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](200) NULL,
	[Url] [varchar](200) NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_MedicineLeaflet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Leaflet] ******/


/****** Begin Object:  Table [dbo].[MedicalCertificate] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalCertificate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModelMedicalCertificateId] [int] NULL,
	[PatientId] [int] NOT NULL,
	[Text] [text] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[IssuanceDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MedicalCertificate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[MedicalCertificate] ******/


/****** Begin Object:  Table [dbo].[MedicalCertificateField] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalCertificateField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MedicalCertificateId] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Value] [varchar](50) NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_MedicalCertificateField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[MedicalCertificateField] ******/


/****** Begin Object:  Table [dbo].[Medicine] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medicine](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[DoctorId] [int] NOT NULL,
	[LaboratoryId] [int] NULL,
	[Usage] [smallint] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[Observations] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Medicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Medicine] ******/


/****** Begin Object:  Table [dbo].[MedicineActiveIngredient] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicineActiveIngredient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[PracticeId] [int] NOT NULL,
	[MedicineId] [int] NOT NULL,
 CONSTRAINT [PK_ActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[MedicineActiveIngredient] ******/


/****** Begin Object:  Table [dbo].[MedicineLeaflet] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicineLeaflet](
	[MedicineId] [int] NOT NULL,
	[LeaftletId] [int] NOT NULL,
 CONSTRAINT [PK_MedicineLeaflet2] PRIMARY KEY CLUSTERED 
(
	[MedicineId] ASC,
	[LeaftletId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[MedicineLeaflet] ******/


/****** Begin Object:  Table [dbo].[ModelMedicalCertificate] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModelMedicalCertificate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Text] [text] NOT NULL,
	[DoctorId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_ModelMedicalCertificate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ModelMedicalCertificate] ******/


/****** Begin Object:  Table [dbo].[ModelMedicalCertificateField] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModelMedicalCertificateField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[ModelMedicalCertificateId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_ModelMedicalCertificateField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ModelMedicalCertificateField] ******/


/****** Begin Object:  Table [dbo].[Notification] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[IsClosed] [bit] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[Data] [varchar](max) NULL,
	[UserToId] [int] NOT NULL,
 CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Notification] ******/


/****** Begin Object:  Table [dbo].[Patient] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Registration] [varchar](100) NULL,
	[DoctorId] [int] NOT NULL,
	[PersonId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[LastUsedHealthInsuranceId] [int] NULL,
	[IsBackedUp] [bit] NOT NULL,
 CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Patient] ******/


/****** Begin Object:  Table [dbo].[PatientFile] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PatientFile](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[FileMetadataId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[Title] [nvarchar](100) NULL,
	[FileGroupId] [int] NOT NULL,
 CONSTRAINT [PK_PatientFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[PatientFile] ******/


/****** Begin Object:  Table [dbo].[PatientFileGroup] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PatientFileGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[GroupTitle] [nvarchar](100) NOT NULL,
	[GroupNotes] [nvarchar](max) NULL,
	[FileGroupDate] [datetime] NOT NULL,
	[ReceiveDate] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_PatientFileGroup_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[PatientFileGroup] ******/


/****** Begin Object:  Table [dbo].[Person] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [varchar](200) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[DateOfBirth] [datetime] NOT NULL,
	[Gender] [smallint] NOT NULL,
	[MaritalStatus] [smallint] NULL,
	[BirthPlace] [varchar](100) NULL,
	[CPF] [varchar](12) NULL,
	[CPFOwner] [smallint] NULL,
	[Observations] [text] NULL,
	[Profession] [varchar](100) NULL,
	[PhoneLand] [varchar](20) NULL,
	[PhoneCell] [varchar](20) NULL,
	[Email] [varchar](200) NULL,
	[EmailGravatarHash] [varchar](200) NULL,
	[PracticeId] [int] NOT NULL,
	[PictureBlobName] [varchar](200) NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Person] ******/


/****** Begin Object:  Table [dbo].[PersonAddress] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAddress](
	[PersonId] [int] NOT NULL,
	[AddressId] [int] NOT NULL,
 CONSTRAINT [PK_PersonAddress] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[AddressId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[PersonAddress] ******/


/****** Begin Object:  Table [dbo].[PhysicalExamination] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhysicalExamination](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Notes] [text] NOT NULL,
	[MedicalRecordDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PhysicalExamination] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[PhysicalExamination] ******/


/****** Begin Object:  Table [dbo].[Practice] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Practice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[UrlIdentifier] [varchar](200) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[OwnerId] [int] NULL,
	[WindowsTimeZoneId] [varchar](31) NOT NULL,
	[VerificationDate] [datetime] NULL,
	[ActiveAccountContractId] [int] NULL,
	[PhoneMain] [varchar](20) NULL,
	[PhoneAlt] [varchar](20) NULL,
	[PABX] [varchar](20) NULL,
	[Email] [varchar](200) NULL,
	[SiteUrl] [nvarchar](max) NULL,
	[AddressId] [int] NULL,
	[AccountDisabled] [bit] NOT NULL,
	[AccountCancelRequest] [bit] NOT NULL,
	[Province] [int] NULL,
 CONSTRAINT [PK_Practice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Practice] ******/


/****** Begin Object:  Table [dbo].[Receipt] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receipt](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[PracticeId] [int] NOT NULL,
	[IssuanceDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Receipt] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Receipt] ******/


/****** Begin Object:  Table [dbo].[ReceiptMedicine] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReceiptMedicine](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Quantity] [varchar](100) NOT NULL,
	[Prescription] [varchar](150) NOT NULL,
	[Observations] [text] NULL,
	[ReceiptId] [int] NOT NULL,
	[MedicineId] [int] NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_ReceiptMedicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[ReceiptMedicine] ******/


/****** Begin Object:  Table [dbo].[Secretary] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Secretary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_Secretary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[Secretary] ******/


/****** Begin Object:  Table [dbo].[SPECIAL_Test] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SPECIAL_Test](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Value] [nvarchar](1000) NULL,
 CONSTRAINT [PK_SPECIAL_Test_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SPECIAL_Test] ******/


/****** Begin Object:  Table [dbo].[SYS_ActiveIngredient] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_ActiveIngredient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
 CONSTRAINT [PK_SYS_ActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_ActiveIngredient] ******/


/****** Begin Object:  Table [dbo].[SYS_Cid10] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_Cid10](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](300) NULL,
	[Cat] [varchar](200) NULL,
	[SubCat] [varchar](200) NULL,
 CONSTRAINT [PK_SYS_Cid10] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_Cid10] ******/


/****** Begin Object:  Table [dbo].[SYS_ContractType] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_ContractType](
	[Id] [int] NOT NULL,
	[Name] [varchar](150) NOT NULL,
	[CreatedOn] [date] NOT NULL,
	[IsTrial] [bit] NOT NULL,
	[UrlIdentifier] [varchar](50) NOT NULL,
	[CustomTemplateText] [nvarchar](max) NULL,
 CONSTRAINT [PK_SYS_Contract] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_ContractType] ******/


/****** Begin Object:  Table [dbo].[SYS_Holiday] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_Holiday](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[MonthAndDay] [int] NOT NULL,
 CONSTRAINT [PK_SYS_Holliday] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_Holiday] ******/


/****** Begin Object:  Table [dbo].[SYS_Laboratory] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_Laboratory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
 CONSTRAINT [PK_SYS_Laboratory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_Laboratory] ******/


/****** Begin Object:  Table [dbo].[SYS_Leaflet] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_Leaflet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](200) NULL,
	[Url] [varchar](300) NOT NULL,
 CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_Leaflet] ******/


/****** Begin Object:  Table [dbo].[SYS_MedicalEntity] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_MedicalEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Code] [nvarchar](50) NULL,
 CONSTRAINT [PK_MedicalEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_MedicalEntity] ******/


/****** Begin Object:  Table [dbo].[SYS_MedicalProcedure] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_MedicalProcedure](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nchar](12) NOT NULL,
	[Name] [nvarchar](310) NOT NULL,
 CONSTRAINT [PK_SYS_MedicalProcedures] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_MedicalProcedure] ******/


/****** Begin Object:  Table [dbo].[SYS_MedicalSpecialty] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_MedicalSpecialty](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Code] [nvarchar](10) NULL,
 CONSTRAINT [PK_MedicalSpecialty] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_MedicalSpecialty] ******/


/****** Begin Object:  Table [dbo].[SYS_Medicine] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_Medicine](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LaboratoryId] [int] NULL,
	[Name] [varchar](300) NOT NULL,
	[Decription] [text] NULL,
 CONSTRAINT [PK_SYS_Medicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_Medicine] ******/


/****** Begin Object:  Table [dbo].[SYS_MedicineActiveIngredient] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_MedicineActiveIngredient](
	[MedicineId] [int] NOT NULL,
	[ActiveIngredientId] [int] NOT NULL,
 CONSTRAINT [PK_SYS_MedicineActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[MedicineId] ASC,
	[ActiveIngredientId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_MedicineActiveIngredient] ******/


/****** Begin Object:  Table [dbo].[SYS_MedicineLeaflet] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYS_MedicineLeaflet](
	[SYS_MedicineId] [int] NOT NULL,
	[SYS_LeafletId] [int] NOT NULL,
 CONSTRAINT [PK_SYS_MedicineLeaflet] PRIMARY KEY CLUSTERED 
(
	[SYS_MedicineId] ASC,
	[SYS_LeafletId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[SYS_MedicineLeaflet] ******/


/****** Begin Object:  Table [dbo].[User] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Password] [varchar](50) NOT NULL,
	[PasswordSalt] [varchar](50) NOT NULL,
	[LastActiveOn] [datetime] NULL,
	[SecretaryId] [int] NULL,
	[PersonId] [int] NOT NULL,
	[DoctorId] [int] NULL,
	[PracticeId] [int] NOT NULL,
	[AdministratorId] [int] NULL,
	[UserName] [varchar](50) NOT NULL,
	[UserNameNormalized] [varchar](50) NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[SYS_PasswordAlt] [nvarchar](50) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** End Object:  Table [dbo].[User] ******/


/****** Begin Object:  Index [IX_DayOff_Date] ******/
CREATE NONCLUSTERED INDEX [IX_DayOff_Date] ON [dbo].[CFG_DayOff] 
(
	[Date] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_DayOff_Date] ******/


/****** Begin Object:  Index [IX_FileMetadata_ContainerName_Tag] ******/
CREATE NONCLUSTERED INDEX [IX_FileMetadata_ContainerName_Tag] ON [dbo].[FileMetadata] 
(
	[ContainerName] ASC,
	[Tag] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_FileMetadata_ContainerName_Tag] ******/


/****** Begin Object:  Index [IX_FileMetadata_ExpirationDate] ******/
CREATE NONCLUSTERED INDEX [IX_FileMetadata_ExpirationDate] ON [dbo].[FileMetadata] 
(
	[ExpirationDate] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_FileMetadata_ExpirationDate] ******/


/****** Begin Object:  Index [IX_SYS_MedicalEntity_Code] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SYS_MedicalEntity_Code] ON [dbo].[SYS_MedicalEntity] 
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalEntity_Code] ******/


/****** Begin Object:  Index [IX_SYS_MedicalEntity_Name] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SYS_MedicalEntity_Name] ON [dbo].[SYS_MedicalEntity] 
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalEntity_Name] ******/


/****** Begin Object:  Index [IX_SYS_MedicalProcedure_Name] ******/
CREATE NONCLUSTERED INDEX [IX_SYS_MedicalProcedure_Name] ON [dbo].[SYS_MedicalProcedure] 
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalProcedure_Name] ******/


/****** Begin Object:  Index [IX_SYS_MedicalProcedures_Code] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SYS_MedicalProcedures_Code] ON [dbo].[SYS_MedicalProcedure] 
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalProcedures_Code] ******/


/****** Begin Object:  Index [IX_SYS_MedicalSpecialty_Code] ******/
CREATE NONCLUSTERED INDEX [IX_SYS_MedicalSpecialty_Code] ON [dbo].[SYS_MedicalSpecialty] 
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalSpecialty_Code] ******/


/****** Begin Object:  Index [IX_SYS_MedicalSpecialty_Name] ******/
CREATE NONCLUSTERED INDEX [IX_SYS_MedicalSpecialty_Name] ON [dbo].[SYS_MedicalSpecialty] 
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_SYS_MedicalSpecialty_Name] ******/


/****** Begin Object:  Index [IX_User_AdministratorId] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_AdministratorId] ON [dbo].[User] 
(
	[AdministratorId] ASC
)
WHERE ([AdministratorId] IS NOT NULL)
WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_User_AdministratorId] ******/


/****** Begin Object:  Index [IX_User_DoctorId] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_DoctorId] ON [dbo].[User] 
(
	[DoctorId] ASC
)
WHERE ([DoctorId] IS NOT NULL)
WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_User_DoctorId] ******/


/****** Begin Object:  Index [IX_User_PersonId] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_PersonId] ON [dbo].[User] 
(
	[PersonId] ASC
)
WHERE ([PersonId] IS NOT NULL)
WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_User_PersonId] ******/


/****** Begin Object:  Index [IX_User_PracticeId] ******/
CREATE NONCLUSTERED INDEX [IX_User_PracticeId] ON [dbo].[User] 
(
	[PracticeId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_User_PracticeId] ******/


/****** Begin Object:  Index [IX_User_SecretaryId] ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_SecretaryId] ON [dbo].[User] 
(
	[SecretaryId] ASC
)
WHERE ([SecretaryId] IS NOT NULL)
WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
/****** End Object:  Index [IX_User_SecretaryId] ******/


/****** Begin Object:  ForeignKey [FK_AccountContract_Practice] ******/
ALTER TABLE [dbo].[AccountContract]  WITH NOCHECK ADD  CONSTRAINT [FK_AccountContract_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[AccountContract] CHECK CONSTRAINT [FK_AccountContract_Practice]
GO
/****** End Object:  ForeignKey [FK_AccountContract_Practice] ******/


/****** Begin Object:  ForeignKey [FK_AccountContract_SYS_ContractType] ******/
ALTER TABLE [dbo].[AccountContract]  WITH NOCHECK ADD  CONSTRAINT [FK_AccountContract_SYS_ContractType] FOREIGN KEY([ContractTypeId])
REFERENCES [dbo].[SYS_ContractType] ([Id])
GO
ALTER TABLE [dbo].[AccountContract] CHECK CONSTRAINT [FK_AccountContract_SYS_ContractType]
GO
/****** End Object:  ForeignKey [FK_AccountContract_SYS_ContractType] ******/


/****** Begin Object:  ForeignKey [FK_ActiveIngredient_Medicine] ******/
ALTER TABLE [dbo].[MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_ActiveIngredient_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[MedicineActiveIngredient] CHECK CONSTRAINT [FK_ActiveIngredient_Medicine]
GO
/****** End Object:  ForeignKey [FK_ActiveIngredient_Medicine] ******/


/****** Begin Object:  ForeignKey [FK_Anamnese_Patient] ******/
ALTER TABLE [dbo].[Anamnese]  WITH NOCHECK ADD  CONSTRAINT [FK_Anamnese_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[Anamnese] CHECK CONSTRAINT [FK_Anamnese_Patient]
GO
/****** End Object:  ForeignKey [FK_Anamnese_Patient] ******/


/****** Begin Object:  ForeignKey [FK_Appointment_Doctor] ******/
ALTER TABLE [dbo].[Appointment]  WITH NOCHECK ADD  CONSTRAINT [FK_Appointment_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Doctor]
GO
/****** End Object:  ForeignKey [FK_Appointment_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_Appointment_HealthInsurance] ******/
ALTER TABLE [dbo].[Appointment]  WITH NOCHECK ADD  CONSTRAINT [FK_Appointment_HealthInsurance] FOREIGN KEY([HealthInsuranceId])
REFERENCES [dbo].[HealthInsurance] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_HealthInsurance]
GO
/****** End Object:  ForeignKey [FK_Appointment_HealthInsurance] ******/


/****** Begin Object:  ForeignKey [FK_Appointment_Practice] ******/
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Practice]
GO
/****** End Object:  ForeignKey [FK_Appointment_Practice] ******/


/****** Begin Object:  ForeignKey [FK_Appointment_User] ******/
ALTER TABLE [dbo].[Appointment]  WITH NOCHECK ADD  CONSTRAINT [FK_Appointment_User] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_User]
GO
/****** End Object:  ForeignKey [FK_Appointment_User] ******/


/****** Begin Object:  ForeignKey [FK_Billing_AccountContract] ******/
ALTER TABLE [dbo].[Billing]  WITH CHECK ADD  CONSTRAINT [FK_Billing_AccountContract] FOREIGN KEY([MainAccountContractId])
REFERENCES [dbo].[AccountContract] ([Id])
GO
ALTER TABLE [dbo].[Billing] CHECK CONSTRAINT [FK_Billing_AccountContract]
GO
/****** End Object:  ForeignKey [FK_Billing_AccountContract] ******/


/****** Begin Object:  ForeignKey [FK_BillingItem_AccountContract] ******/
ALTER TABLE [dbo].[BillingItem]  WITH CHECK ADD  CONSTRAINT [FK_BillingItem_AccountContract] FOREIGN KEY([ItemAccountContractId])
REFERENCES [dbo].[AccountContract] ([Id])
GO
ALTER TABLE [dbo].[BillingItem] CHECK CONSTRAINT [FK_BillingItem_AccountContract]
GO
/****** End Object:  ForeignKey [FK_BillingItem_AccountContract] ******/


/****** Begin Object:  ForeignKey [FK_BillingItem_Billing] ******/
ALTER TABLE [dbo].[BillingItem]  WITH CHECK ADD  CONSTRAINT [FK_BillingItem_Billing] FOREIGN KEY([BillingId])
REFERENCES [dbo].[Billing] ([Id])
GO
ALTER TABLE [dbo].[BillingItem] CHECK CONSTRAINT [FK_BillingItem_Billing]
GO
/****** End Object:  ForeignKey [FK_BillingItem_Billing] ******/


/****** Begin Object:  ForeignKey [FK_CFG_DayOff_Doctor] ******/
ALTER TABLE [dbo].[CFG_DayOff]  WITH NOCHECK ADD  CONSTRAINT [FK_CFG_DayOff_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[CFG_DayOff] CHECK CONSTRAINT [FK_CFG_DayOff_Doctor]
GO
/****** End Object:  ForeignKey [FK_CFG_DayOff_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_CFG_Documents_Doctor] ******/
ALTER TABLE [dbo].[CFG_Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_CFG_Documents_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[CFG_Documents] CHECK CONSTRAINT [FK_CFG_Documents_Doctor]
GO
/****** End Object:  ForeignKey [FK_CFG_Documents_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_CFG_Schedule_Doctor] ******/
ALTER TABLE [dbo].[CFG_Schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_CFG_Schedule_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[CFG_Schedule] CHECK CONSTRAINT [FK_CFG_Schedule_Doctor]
GO
/****** End Object:  ForeignKey [FK_CFG_Schedule_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_ChatMessage_FromUser_User] ******/
ALTER TABLE [dbo].[ChatMessage]  WITH NOCHECK ADD  CONSTRAINT [FK_ChatMessage_FromUser_User] FOREIGN KEY([UserFromId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[ChatMessage] CHECK CONSTRAINT [FK_ChatMessage_FromUser_User]
GO
/****** End Object:  ForeignKey [FK_ChatMessage_FromUser_User] ******/


/****** Begin Object:  ForeignKey [FK_ChatMessage_ToUser_User] ******/
ALTER TABLE [dbo].[ChatMessage]  WITH NOCHECK ADD  CONSTRAINT [FK_ChatMessage_ToUser_User] FOREIGN KEY([UserToId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[ChatMessage] CHECK CONSTRAINT [FK_ChatMessage_ToUser_User]
GO
/****** End Object:  ForeignKey [FK_ChatMessage_ToUser_User] ******/


/****** Begin Object:  ForeignKey [FK_Diagnosis_Patient] ******/
ALTER TABLE [dbo].[Diagnosis]  WITH NOCHECK ADD  CONSTRAINT [FK_Diagnosis_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[Diagnosis] CHECK CONSTRAINT [FK_Diagnosis_Patient]
GO
/****** End Object:  ForeignKey [FK_Diagnosis_Patient] ******/


/****** Begin Object:  ForeignKey [FK_DiagnosticHypothesis_Patient] ******/
ALTER TABLE [dbo].[DiagnosticHypothesis]  WITH CHECK ADD  CONSTRAINT [FK_DiagnosticHypothesis_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[DiagnosticHypothesis] CHECK CONSTRAINT [FK_DiagnosticHypothesis_Patient]
GO
/****** End Object:  ForeignKey [FK_DiagnosticHypothesis_Patient] ******/


/****** Begin Object:  ForeignKey [FK_Doctor_Practice] ******/
ALTER TABLE [dbo].[Doctor]  WITH CHECK ADD  CONSTRAINT [FK_Doctor_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[Doctor] CHECK CONSTRAINT [FK_Doctor_Practice]
GO
/****** End Object:  ForeignKey [FK_Doctor_Practice] ******/


/****** Begin Object:  ForeignKey [FK_ExaminationRequest_Patient] ******/
ALTER TABLE [dbo].[ExaminationRequest]  WITH NOCHECK ADD  CONSTRAINT [FK_ExaminationRequest_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[ExaminationRequest] CHECK CONSTRAINT [FK_ExaminationRequest_Patient]
GO
/****** End Object:  ForeignKey [FK_ExaminationRequest_Patient] ******/


/****** Begin Object:  ForeignKey [FK_ExaminationResult_Patient] ******/
ALTER TABLE [dbo].[ExaminationResult]  WITH NOCHECK ADD  CONSTRAINT [FK_ExaminationResult_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[ExaminationResult] CHECK CONSTRAINT [FK_ExaminationResult_Patient]
GO
/****** End Object:  ForeignKey [FK_ExaminationResult_Patient] ******/


/****** Begin Object:  ForeignKey [FK_FileMetadata_FileMetadata] ******/
ALTER TABLE [dbo].[FileMetadata]  WITH CHECK ADD  CONSTRAINT [FK_FileMetadata_FileMetadata] FOREIGN KEY([RelatedFileMetadataId])
REFERENCES [dbo].[FileMetadata] ([Id])
GO
ALTER TABLE [dbo].[FileMetadata] CHECK CONSTRAINT [FK_FileMetadata_FileMetadata]
GO
/****** End Object:  ForeignKey [FK_FileMetadata_FileMetadata] ******/


/****** Begin Object:  ForeignKey [FK_FileMetadata_OwnerUser] ******/
ALTER TABLE [dbo].[FileMetadata]  WITH CHECK ADD  CONSTRAINT [FK_FileMetadata_OwnerUser] FOREIGN KEY([OwnerUserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[FileMetadata] CHECK CONSTRAINT [FK_FileMetadata_OwnerUser]
GO
/****** End Object:  ForeignKey [FK_FileMetadata_OwnerUser] ******/


/****** Begin Object:  ForeignKey [FK_GoogleAccoutInfo_Person] ******/
ALTER TABLE [dbo].[GoogleUserAccoutInfo]  WITH CHECK ADD  CONSTRAINT [FK_GoogleAccoutInfo_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GoogleUserAccoutInfo] CHECK CONSTRAINT [FK_GoogleAccoutInfo_Person]
GO
/****** End Object:  ForeignKey [FK_GoogleAccoutInfo_Person] ******/


/****** Begin Object:  ForeignKey [FK_GoogleAccoutInfo_Practice] ******/
ALTER TABLE [dbo].[GoogleUserAccoutInfo]  WITH CHECK ADD  CONSTRAINT [FK_GoogleAccoutInfo_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[GoogleUserAccoutInfo] CHECK CONSTRAINT [FK_GoogleAccoutInfo_Practice]
GO
/****** End Object:  ForeignKey [FK_GoogleAccoutInfo_Practice] ******/


/****** Begin Object:  ForeignKey [FK_GoogleDrivePatientInfo_Patient] ******/
ALTER TABLE [dbo].[GoogleDrivePatientInfo]  WITH CHECK ADD  CONSTRAINT [FK_GoogleDrivePatientInfo_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[GoogleDrivePatientInfo] CHECK CONSTRAINT [FK_GoogleDrivePatientInfo_Patient]
GO
/****** End Object:  ForeignKey [FK_GoogleDrivePatientInfo_Patient] ******/


/****** Begin Object:  ForeignKey [FK_GoogleDrivePatientInfo_Practice] ******/
ALTER TABLE [dbo].[GoogleDrivePatientInfo]  WITH CHECK ADD  CONSTRAINT [FK_GoogleDrivePatientInfo_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[GoogleDrivePatientInfo] CHECK CONSTRAINT [FK_GoogleDrivePatientInfo_Practice]
GO
/****** End Object:  ForeignKey [FK_GoogleDrivePatientInfo_Practice] ******/


/****** Begin Object:  ForeignKey [FK_GoogleDrivePracticeInfo_Practice] ******/
ALTER TABLE [dbo].[GoogleDrivePracticeInfo]  WITH CHECK ADD  CONSTRAINT [FK_GoogleDrivePracticeInfo_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[GoogleDrivePracticeInfo] CHECK CONSTRAINT [FK_GoogleDrivePracticeInfo_Practice]
GO
/****** End Object:  ForeignKey [FK_GoogleDrivePracticeInfo_Practice] ******/


/****** Begin Object:  ForeignKey [FK_HealthInsurance_Doctor] ******/
ALTER TABLE [dbo].[HealthInsurance]  WITH NOCHECK ADD  CONSTRAINT [FK_HealthInsurance_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[HealthInsurance] CHECK CONSTRAINT [FK_HealthInsurance_Doctor]
GO
/****** End Object:  ForeignKey [FK_HealthInsurance_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_Laboratory_Doctor] ******/
ALTER TABLE [dbo].[Laboratory]  WITH NOCHECK ADD  CONSTRAINT [FK_Laboratory_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Laboratory] CHECK CONSTRAINT [FK_Laboratory_Doctor]
GO
/****** End Object:  ForeignKey [FK_Laboratory_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_MedicalCertificate_ModelMedicalCertificate] ******/
ALTER TABLE [dbo].[MedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificate_ModelMedicalCertificate] FOREIGN KEY([ModelMedicalCertificateId])
REFERENCES [dbo].[ModelMedicalCertificate] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[MedicalCertificate] CHECK CONSTRAINT [FK_MedicalCertificate_ModelMedicalCertificate]
GO
/****** End Object:  ForeignKey [FK_MedicalCertificate_ModelMedicalCertificate] ******/


/****** Begin Object:  ForeignKey [FK_MedicalCertificate_Patient] ******/
ALTER TABLE [dbo].[MedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificate_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[MedicalCertificate] CHECK CONSTRAINT [FK_MedicalCertificate_Patient]
GO
/****** End Object:  ForeignKey [FK_MedicalCertificate_Patient] ******/


/****** Begin Object:  ForeignKey [FK_MedicalCertificateField_MedicalCertificate] ******/
ALTER TABLE [dbo].[MedicalCertificateField]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificateField_MedicalCertificate] FOREIGN KEY([MedicalCertificateId])
REFERENCES [dbo].[MedicalCertificate] ([Id])
GO
ALTER TABLE [dbo].[MedicalCertificateField] CHECK CONSTRAINT [FK_MedicalCertificateField_MedicalCertificate]
GO
/****** End Object:  ForeignKey [FK_MedicalCertificateField_MedicalCertificate] ******/


/****** Begin Object:  ForeignKey [FK_Medicine_Doctor] ******/
ALTER TABLE [dbo].[Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_Medicine_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Medicine] CHECK CONSTRAINT [FK_Medicine_Doctor]
GO
/****** End Object:  ForeignKey [FK_Medicine_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_Medicine_MedicineLaboratory] ******/
ALTER TABLE [dbo].[Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_Medicine_MedicineLaboratory] FOREIGN KEY([LaboratoryId])
REFERENCES [dbo].[Laboratory] ([Id])
GO
ALTER TABLE [dbo].[Medicine] CHECK CONSTRAINT [FK_Medicine_MedicineLaboratory]
GO
/****** End Object:  ForeignKey [FK_Medicine_MedicineLaboratory] ******/


/****** Begin Object:  ForeignKey [FK_MedicineLeaflet_Leaflet] ******/
ALTER TABLE [dbo].[MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineLeaflet_Leaflet] FOREIGN KEY([LeaftletId])
REFERENCES [dbo].[Leaflet] ([Id])
GO
ALTER TABLE [dbo].[MedicineLeaflet] CHECK CONSTRAINT [FK_MedicineLeaflet_Leaflet]
GO
/****** End Object:  ForeignKey [FK_MedicineLeaflet_Leaflet] ******/


/****** Begin Object:  ForeignKey [FK_MedicineLeaflet_Medicine] ******/
ALTER TABLE [dbo].[MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineLeaflet_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[MedicineLeaflet] CHECK CONSTRAINT [FK_MedicineLeaflet_Medicine]
GO
/****** End Object:  ForeignKey [FK_MedicineLeaflet_Medicine] ******/


/****** Begin Object:  ForeignKey [FK_ModelMedicalCertificate_Doctor] ******/
ALTER TABLE [dbo].[ModelMedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_ModelMedicalCertificate_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[ModelMedicalCertificate] CHECK CONSTRAINT [FK_ModelMedicalCertificate_Doctor]
GO
/****** End Object:  ForeignKey [FK_ModelMedicalCertificate_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_ModelMedicalCertificateField_ModelMedicalCertificate] ******/
ALTER TABLE [dbo].[ModelMedicalCertificateField]  WITH NOCHECK ADD  CONSTRAINT [FK_ModelMedicalCertificateField_ModelMedicalCertificate] FOREIGN KEY([ModelMedicalCertificateId])
REFERENCES [dbo].[ModelMedicalCertificate] ([Id])
GO
ALTER TABLE [dbo].[ModelMedicalCertificateField] CHECK CONSTRAINT [FK_ModelMedicalCertificateField_ModelMedicalCertificate]
GO
/****** End Object:  ForeignKey [FK_ModelMedicalCertificateField_ModelMedicalCertificate] ******/


/****** Begin Object:  ForeignKey [FK_Notification_Practice] ******/
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_Practice]
GO
/****** End Object:  ForeignKey [FK_Notification_Practice] ******/


/****** Begin Object:  ForeignKey [FK_Notification_User] ******/
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_User] FOREIGN KEY([UserToId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_User]
GO
/****** End Object:  ForeignKey [FK_Notification_User] ******/


/****** Begin Object:  ForeignKey [FK_Patient_Doctor] ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Doctor]
GO
/****** End Object:  ForeignKey [FK_Patient_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_Patient_HealthInsurance] ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_HealthInsurance] FOREIGN KEY([LastUsedHealthInsuranceId])
REFERENCES [dbo].[HealthInsurance] ([Id])
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_HealthInsurance]
GO
/****** End Object:  ForeignKey [FK_Patient_HealthInsurance] ******/


/****** Begin Object:  ForeignKey [FK_Patient_Person] ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Person]
GO
/****** End Object:  ForeignKey [FK_Patient_Person] ******/


/****** Begin Object:  ForeignKey [FK_Patient_Practice] ******/
ALTER TABLE [dbo].[Patient]  WITH CHECK ADD  CONSTRAINT [FK_Patient_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Practice]
GO
/****** End Object:  ForeignKey [FK_Patient_Practice] ******/


/****** Begin Object:  ForeignKey [FK_PatientFile_FileMetadata] ******/
ALTER TABLE [dbo].[PatientFile]  WITH NOCHECK ADD  CONSTRAINT [FK_PatientFile_FileMetadata] FOREIGN KEY([FileMetadataId])
REFERENCES [dbo].[FileMetadata] ([Id])
GO
ALTER TABLE [dbo].[PatientFile] CHECK CONSTRAINT [FK_PatientFile_FileMetadata]
GO
/****** End Object:  ForeignKey [FK_PatientFile_FileMetadata] ******/


/****** Begin Object:  ForeignKey [FK_PatientFile_Patient] ******/
ALTER TABLE [dbo].[PatientFile]  WITH NOCHECK ADD  CONSTRAINT [FK_PatientFile_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[PatientFile] CHECK CONSTRAINT [FK_PatientFile_Patient]
GO
/****** End Object:  ForeignKey [FK_PatientFile_Patient] ******/


/****** Begin Object:  ForeignKey [FK_PatientFile_PatientFileGroup] ******/
ALTER TABLE [dbo].[PatientFile]  WITH CHECK ADD  CONSTRAINT [FK_PatientFile_PatientFileGroup] FOREIGN KEY([FileGroupId])
REFERENCES [dbo].[PatientFileGroup] ([Id])
GO
ALTER TABLE [dbo].[PatientFile] CHECK CONSTRAINT [FK_PatientFile_PatientFileGroup]
GO
/****** End Object:  ForeignKey [FK_PatientFile_PatientFileGroup] ******/


/****** Begin Object:  ForeignKey [FK_PatientFileGroup_Patient] ******/
ALTER TABLE [dbo].[PatientFileGroup]  WITH CHECK ADD  CONSTRAINT [FK_PatientFileGroup_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[PatientFileGroup] CHECK CONSTRAINT [FK_PatientFileGroup_Patient]
GO
/****** End Object:  ForeignKey [FK_PatientFileGroup_Patient] ******/


/****** Begin Object:  ForeignKey [FK_PersonAddress_Address] ******/
ALTER TABLE [dbo].[PersonAddress]  WITH NOCHECK ADD  CONSTRAINT [FK_PersonAddress_Address] FOREIGN KEY([AddressId])
REFERENCES [dbo].[Address] ([Id])
GO
ALTER TABLE [dbo].[PersonAddress] CHECK CONSTRAINT [FK_PersonAddress_Address]
GO
/****** End Object:  ForeignKey [FK_PersonAddress_Address] ******/


/****** Begin Object:  ForeignKey [FK_PersonAddress_Person] ******/
ALTER TABLE [dbo].[PersonAddress]  WITH NOCHECK ADD  CONSTRAINT [FK_PersonAddress_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAddress] CHECK CONSTRAINT [FK_PersonAddress_Person]
GO
/****** End Object:  ForeignKey [FK_PersonAddress_Person] ******/


/****** Begin Object:  ForeignKey [FK_PhysicalExamination_Patient] ******/
ALTER TABLE [dbo].[PhysicalExamination]  WITH NOCHECK ADD  CONSTRAINT [FK_PhysicalExamination_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[PhysicalExamination] CHECK CONSTRAINT [FK_PhysicalExamination_Patient]
GO
/****** End Object:  ForeignKey [FK_PhysicalExamination_Patient] ******/


/****** Begin Object:  ForeignKey [FK_Practice_AccountContract] ******/
ALTER TABLE [dbo].[Practice]  WITH NOCHECK ADD  CONSTRAINT [FK_Practice_AccountContract] FOREIGN KEY([ActiveAccountContractId])
REFERENCES [dbo].[AccountContract] ([Id])
GO
ALTER TABLE [dbo].[Practice] CHECK CONSTRAINT [FK_Practice_AccountContract]
GO
/****** End Object:  ForeignKey [FK_Practice_AccountContract] ******/


/****** Begin Object:  ForeignKey [FK_Practice_Owner_User] ******/
ALTER TABLE [dbo].[Practice]  WITH NOCHECK ADD  CONSTRAINT [FK_Practice_Owner_User] FOREIGN KEY([OwnerId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Practice] CHECK CONSTRAINT [FK_Practice_Owner_User]
GO
/****** End Object:  ForeignKey [FK_Practice_Owner_User] ******/


/****** Begin Object:  ForeignKey [FK_ReceiptMedicine_Medicine] ******/
ALTER TABLE [dbo].[ReceiptMedicine]  WITH NOCHECK ADD  CONSTRAINT [FK_ReceiptMedicine_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[ReceiptMedicine] CHECK CONSTRAINT [FK_ReceiptMedicine_Medicine]
GO
/****** End Object:  ForeignKey [FK_ReceiptMedicine_Medicine] ******/


/****** Begin Object:  ForeignKey [FK_ReceiptMedicine_Receipt] ******/
ALTER TABLE [dbo].[ReceiptMedicine]  WITH NOCHECK ADD  CONSTRAINT [FK_ReceiptMedicine_Receipt] FOREIGN KEY([ReceiptId])
REFERENCES [dbo].[Receipt] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReceiptMedicine] CHECK CONSTRAINT [FK_ReceiptMedicine_Receipt]
GO
/****** End Object:  ForeignKey [FK_ReceiptMedicine_Receipt] ******/


/****** Begin Object:  ForeignKey [FK_SYS_Medicine_SYS_Laboratory] ******/
ALTER TABLE [dbo].[SYS_Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_Medicine_SYS_Laboratory] FOREIGN KEY([LaboratoryId])
REFERENCES [dbo].[SYS_Laboratory] ([Id])
GO
ALTER TABLE [dbo].[SYS_Medicine] CHECK CONSTRAINT [FK_SYS_Medicine_SYS_Laboratory]
GO
/****** End Object:  ForeignKey [FK_SYS_Medicine_SYS_Laboratory] ******/


/****** Begin Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient] ******/
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient] FOREIGN KEY([ActiveIngredientId])
REFERENCES [dbo].[SYS_ActiveIngredient] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient] CHECK CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient]
GO
/****** End Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient] ******/


/****** Begin Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_Medicine] ******/
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[SYS_Medicine] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient] CHECK CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_Medicine]
GO
/****** End Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_Medicine] ******/


/****** Begin Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Leaflet] ******/
ALTER TABLE [dbo].[SYS_MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Leaflet] FOREIGN KEY([SYS_LeafletId])
REFERENCES [dbo].[SYS_Leaflet] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineLeaflet] CHECK CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Leaflet]
GO
/****** End Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Leaflet] ******/


/****** Begin Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Medicine] ******/
ALTER TABLE [dbo].[SYS_MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Medicine] FOREIGN KEY([SYS_MedicineId])
REFERENCES [dbo].[SYS_Medicine] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineLeaflet] CHECK CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Medicine]
GO
/****** End Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Medicine] ******/


/****** Begin Object:  ForeignKey [FK_User_Administrator] ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Administrator] FOREIGN KEY([AdministratorId])
REFERENCES [dbo].[Administrator] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Administrator]
GO
/****** End Object:  ForeignKey [FK_User_Administrator] ******/


/****** Begin Object:  ForeignKey [FK_User_Doctor] ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Doctor]
GO
/****** End Object:  ForeignKey [FK_User_Doctor] ******/


/****** Begin Object:  ForeignKey [FK_User_Person] ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Person]
GO
/****** End Object:  ForeignKey [FK_User_Person] ******/


/****** Begin Object:  ForeignKey [FK_User_Practice] ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Practice]
GO
/****** End Object:  ForeignKey [FK_User_Practice] ******/


/****** Begin Object:  ForeignKey [FK_User_Secretary] ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Secretary] FOREIGN KEY([SecretaryId])
REFERENCES [dbo].[Secretary] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Secretary]
GO
/****** End Object:  ForeignKey [FK_User_Secretary] ******/


/****** Begin Object:  Default [DF_Appointment_IsPolled] ******/
ALTER TABLE [dbo].[Appointment] ADD  CONSTRAINT [DF_Appointment_IsPolled]  DEFAULT ((0)) FOR [Status]
GO
/****** End Object:  Default [DF_Appointment_IsPolled] ******/


/****** Begin Object:  Default [DF_Appointment_IsPolled_1] ******/
ALTER TABLE [dbo].[Appointment] ADD  CONSTRAINT [DF_Appointment_IsPolled_1]  DEFAULT ((0)) FOR [Notified]
GO
/****** End Object:  Default [DF_Appointment_IsPolled_1] ******/


/****** Begin Object:  Default [DF_Notification_IsPolled] ******/
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_IsPolled]  DEFAULT ((0)) FOR [IsClosed]
GO
/****** End Object:  Default [DF_Notification_IsPolled] ******/


/****** Begin Object:  Default [DF_Notification_Type] ******/
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_Type]  DEFAULT ('GENERIC') FOR [Type]
GO
/****** End Object:  Default [DF_Notification_Type] ******/


/****** Begin Object:  Default [DF_Patient_IsBackedUp] ******/
ALTER TABLE [dbo].[Patient] ADD  CONSTRAINT [DF_Patient_IsBackedUp]  DEFAULT ((0)) FOR [IsBackedUp]
GO
/****** End Object:  Default [DF_Patient_IsBackedUp] ******/

