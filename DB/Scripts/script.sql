/****** Object:  Table [dbo].[Practice]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_Practice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[SYS_Leaflet]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[SYS_Laboratory]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[SYS_ActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[Secretary]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Secretary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Secretary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Person]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [varchar](200) NOT NULL,
	[UrlIdentifier] [varchar](200) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[DateOfBirth] [datetime] NOT NULL,
	[Gender] [smallint] NOT NULL,
	[MaritalStatus] [smallint] NULL,
	[BirthPlace] [varchar](100) NULL,
	[CPF] [varchar](12) NULL,
	[CPFOwner] [smallint] NULL,
	[Observations] [text] NULL,
	[Profession] [varchar](100) NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Receipt]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receipt](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Receipt] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[MedicalSpecialty]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalSpecialty](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_MedicalSpecialty] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[MedicalEntity]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalEntity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_MedicalEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Leaflet]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Leaflet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](200) NULL,
	[Url] [varchar](200) NOT NULL,
 CONSTRAINT [PK_MedicineLeaflet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Coverage]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coverage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Coverage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Email]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Email](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Address] [varchar](200) NOT NULL,
	[PersonId] [int] NOT NULL,
	[GravatarEmailHash] [varchar](200) NULL,
 CONSTRAINT [PK_Email] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Doctor]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Doctor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CRM] [varchar](50) NOT NULL,
	[MedicalEntityId] [int] NOT NULL,
	[MedicalSpecialtyId] [int] NOT NULL,
 CONSTRAINT [PK_Doctor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[User]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Password] [varchar](50) NOT NULL,
	[PasswordSalt] [varchar](50) NOT NULL,
	[LastActiveOn] [datetime] NULL,
	[PersonId] [int] NOT NULL,
	[Email] [varchar](200) NOT NULL,
	[GravatarEmailHash] [varchar](100) NULL,
	[DoctorId] [int] NULL,
	[SecretaryId] [int] NULL,
	[PracticeId] [int] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Appointment]    Script Date: 06/19/2012 20:50:22 ******/
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
	[PatientId] [int] NOT NULL,
 CONSTRAINT [PK_Appointment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Address]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[CEP] [varchar](20) NULL,
	[City] [varchar](100) NULL,
	[StateProvince] [varchar](100) NULL,
	[Neighborhood] [varchar](100) NULL,
	[Complement] [varchar](50) NULL,
	[Street] [varchar](100) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[SYS_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[Phone]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phone](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[Number] [varchar](20) NOT NULL,
 CONSTRAINT [PK_PersonPhone] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Patient]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CoverageId] [int] NULL,
	[Registration] [varchar](100) NULL,
	[DoctorId] [int] NOT NULL,
	[PersonId] [int] NOT NULL,
 CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[SYS_MedicineLeaflet]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[SYS_MedicineActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[Laboratory]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Laboratory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[DoctorId] [int] NOT NULL,
 CONSTRAINT [PK_MedicineLaboratory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[ModelMedicalCertificate]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModelMedicalCertificate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Text] [text] NOT NULL,
	[DoctorId] [int] NOT NULL,
 CONSTRAINT [PK_ModelMedicalCertificate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[CFG_Schedule]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_CFG_Schedule_1] PRIMARY KEY CLUSTERED 
(
	[DoctorId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[CFG_Documents]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_CFG_Documents] PRIMARY KEY CLUSTERED 
(
	[DoctorId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[ActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActiveIngredient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[DoctorId] [int] NOT NULL,
 CONSTRAINT [PK_ActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Anamnese]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Anamnese](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Text] [text] NULL,
 CONSTRAINT [PK_Anamnese] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Medicine]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_Medicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[MedicalCertificate]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_MedicalCertificate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[ModelMedicalCertificateField]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModelMedicalCertificateField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[ModelMedicalCertificateId] [int] NOT NULL,
 CONSTRAINT [PK_ModelMedicalCertificateField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[ReceiptMedicine]    Script Date: 06/19/2012 20:50:22 ******/
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
 CONSTRAINT [PK_ReceiptMedicine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[MedicineLeaflet]    Script Date: 06/19/2012 20:50:22 ******/
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
/****** Object:  Table [dbo].[MedicineActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicineActiveIngredient](
	[MedicineId] [int] NOT NULL,
	[ActiveIngredientId] [int] NOT NULL,
 CONSTRAINT [PK_MedicineActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[MedicineId] ASC,
	[ActiveIngredientId] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[MedicalCertificateField]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalCertificateField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MedicalCertificateId] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Value] [varchar](50) NULL,
 CONSTRAINT [PK_MedicalCertificateField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Diagnosis]    Script Date: 06/19/2012 20:50:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Diagnosis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AnamneseId] [int] NOT NULL,
	[Observations] [varchar](200) NULL,
	[Cid10Code] [varchar](10) NOT NULL,
	[Cid10Name] [varchar](100) NULL,
 CONSTRAINT [PK_Diagnosis] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  ForeignKey [FK_ActiveIngredient_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[ActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_ActiveIngredient_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[ActiveIngredient] CHECK CONSTRAINT [FK_ActiveIngredient_Doctor]
GO
/****** Object:  ForeignKey [FK_Address_Person]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Address]  WITH NOCHECK ADD  CONSTRAINT [FK_Address_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Address] CHECK CONSTRAINT [FK_Address_Person]
GO
/****** Object:  ForeignKey [FK_Anamnese_Patient]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Anamnese]  WITH NOCHECK ADD  CONSTRAINT [FK_Anamnese_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[Anamnese] CHECK CONSTRAINT [FK_Anamnese_Patient]
GO
/****** Object:  ForeignKey [FK_Appointment_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Appointment]  WITH NOCHECK ADD  CONSTRAINT [FK_Appointment_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Doctor]
GO
/****** Object:  ForeignKey [FK_Appointment_User]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Appointment]  WITH NOCHECK ADD  CONSTRAINT [FK_Appointment_User] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_User]
GO
/****** Object:  ForeignKey [FK_CFG_Documents_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[CFG_Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_CFG_Documents_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[CFG_Documents] CHECK CONSTRAINT [FK_CFG_Documents_Doctor]
GO
/****** Object:  ForeignKey [FK_CFG_Schedule_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[CFG_Schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_CFG_Schedule_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[CFG_Schedule] CHECK CONSTRAINT [FK_CFG_Schedule_Doctor]
GO
/****** Object:  ForeignKey [FK_Diagnosis_Anamnese]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Diagnosis]  WITH NOCHECK ADD  CONSTRAINT [FK_Diagnosis_Anamnese] FOREIGN KEY([AnamneseId])
REFERENCES [dbo].[Anamnese] ([Id])
GO
ALTER TABLE [dbo].[Diagnosis] CHECK CONSTRAINT [FK_Diagnosis_Anamnese]
GO
/****** Object:  ForeignKey [FK_Doctor_MedicalEntity]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Doctor]  WITH NOCHECK ADD  CONSTRAINT [FK_Doctor_MedicalEntity] FOREIGN KEY([MedicalEntityId])
REFERENCES [dbo].[MedicalEntity] ([Id])
GO
ALTER TABLE [dbo].[Doctor] CHECK CONSTRAINT [FK_Doctor_MedicalEntity]
GO
/****** Object:  ForeignKey [FK_Doctor_MedicalSpecialty]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Doctor]  WITH NOCHECK ADD  CONSTRAINT [FK_Doctor_MedicalSpecialty] FOREIGN KEY([MedicalSpecialtyId])
REFERENCES [dbo].[MedicalSpecialty] ([Id])
GO
ALTER TABLE [dbo].[Doctor] CHECK CONSTRAINT [FK_Doctor_MedicalSpecialty]
GO
/****** Object:  ForeignKey [FK_Email_Person]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Email]  WITH NOCHECK ADD  CONSTRAINT [FK_Email_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Email] CHECK CONSTRAINT [FK_Email_Person]
GO
/****** Object:  ForeignKey [FK_Laboratory_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Laboratory]  WITH NOCHECK ADD  CONSTRAINT [FK_Laboratory_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Laboratory] CHECK CONSTRAINT [FK_Laboratory_Doctor]
GO
/****** Object:  ForeignKey [FK_MedicalCertificate_ModelMedicalCertificate]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificate_ModelMedicalCertificate] FOREIGN KEY([ModelMedicalCertificateId])
REFERENCES [dbo].[ModelMedicalCertificate] ([Id])
GO
ALTER TABLE [dbo].[MedicalCertificate] CHECK CONSTRAINT [FK_MedicalCertificate_ModelMedicalCertificate]
GO
/****** Object:  ForeignKey [FK_MedicalCertificate_Patient]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificate_Patient] FOREIGN KEY([PatientId])
REFERENCES [dbo].[Patient] ([Id])
GO
ALTER TABLE [dbo].[MedicalCertificate] CHECK CONSTRAINT [FK_MedicalCertificate_Patient]
GO
/****** Object:  ForeignKey [FK_MedicalCertificateField_MedicalCertificate]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicalCertificateField]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicalCertificateField_MedicalCertificate] FOREIGN KEY([MedicalCertificateId])
REFERENCES [dbo].[MedicalCertificate] ([Id])
GO
ALTER TABLE [dbo].[MedicalCertificateField] CHECK CONSTRAINT [FK_MedicalCertificateField_MedicalCertificate]
GO
/****** Object:  ForeignKey [FK_Medicine_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_Medicine_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Medicine] CHECK CONSTRAINT [FK_Medicine_Doctor]
GO
/****** Object:  ForeignKey [FK_Medicine_MedicineLaboratory]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_Medicine_MedicineLaboratory] FOREIGN KEY([LaboratoryId])
REFERENCES [dbo].[Laboratory] ([Id])
GO
ALTER TABLE [dbo].[Medicine] CHECK CONSTRAINT [FK_Medicine_MedicineLaboratory]
GO
/****** Object:  ForeignKey [FK_MedicineActiveIngredient_ActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineActiveIngredient_ActiveIngredient] FOREIGN KEY([ActiveIngredientId])
REFERENCES [dbo].[ActiveIngredient] ([Id])
GO
ALTER TABLE [dbo].[MedicineActiveIngredient] CHECK CONSTRAINT [FK_MedicineActiveIngredient_ActiveIngredient]
GO
/****** Object:  ForeignKey [FK_MedicineActiveIngredient_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineActiveIngredient_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[MedicineActiveIngredient] CHECK CONSTRAINT [FK_MedicineActiveIngredient_Medicine]
GO
/****** Object:  ForeignKey [FK_MedicineLeaflet_Leaflet]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineLeaflet_Leaflet] FOREIGN KEY([LeaftletId])
REFERENCES [dbo].[Leaflet] ([Id])
GO
ALTER TABLE [dbo].[MedicineLeaflet] CHECK CONSTRAINT [FK_MedicineLeaflet_Leaflet]
GO
/****** Object:  ForeignKey [FK_MedicineLeaflet_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_MedicineLeaflet_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[MedicineLeaflet] CHECK CONSTRAINT [FK_MedicineLeaflet_Medicine]
GO
/****** Object:  ForeignKey [FK_ModelMedicalCertificate_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[ModelMedicalCertificate]  WITH NOCHECK ADD  CONSTRAINT [FK_ModelMedicalCertificate_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[ModelMedicalCertificate] CHECK CONSTRAINT [FK_ModelMedicalCertificate_Doctor]
GO
/****** Object:  ForeignKey [FK_ModelMedicalCertificateField_ModelMedicalCertificate]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[ModelMedicalCertificateField]  WITH NOCHECK ADD  CONSTRAINT [FK_ModelMedicalCertificateField_ModelMedicalCertificate] FOREIGN KEY([ModelMedicalCertificateId])
REFERENCES [dbo].[ModelMedicalCertificate] ([Id])
GO
ALTER TABLE [dbo].[ModelMedicalCertificateField] CHECK CONSTRAINT [FK_ModelMedicalCertificateField_ModelMedicalCertificate]
GO
/****** Object:  ForeignKey [FK_Patient_Coverage]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_Coverage] FOREIGN KEY([CoverageId])
REFERENCES [dbo].[Coverage] ([Id])
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Coverage]
GO
/****** Object:  ForeignKey [FK_Patient_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Doctor]
GO
/****** Object:  ForeignKey [FK_Patient_Person]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Patient]  WITH NOCHECK ADD  CONSTRAINT [FK_Patient_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patient] CHECK CONSTRAINT [FK_Patient_Person]
GO
/****** Object:  ForeignKey [FK_Phone_Person]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Phone]  WITH NOCHECK ADD  CONSTRAINT [FK_Phone_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Phone] CHECK CONSTRAINT [FK_Phone_Person]
GO
/****** Object:  ForeignKey [FK_Practice_User]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[Practice]  WITH NOCHECK ADD  CONSTRAINT [FK_Practice_User] FOREIGN KEY([OwnerId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Practice] CHECK CONSTRAINT [FK_Practice_User]
GO
/****** Object:  ForeignKey [FK_ReceiptMedicine_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[ReceiptMedicine]  WITH NOCHECK ADD  CONSTRAINT [FK_ReceiptMedicine_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([Id])
GO
ALTER TABLE [dbo].[ReceiptMedicine] CHECK CONSTRAINT [FK_ReceiptMedicine_Medicine]
GO
/****** Object:  ForeignKey [FK_ReceiptMedicine_Receipt]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[ReceiptMedicine]  WITH NOCHECK ADD  CONSTRAINT [FK_ReceiptMedicine_Receipt] FOREIGN KEY([ReceiptId])
REFERENCES [dbo].[Receipt] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReceiptMedicine] CHECK CONSTRAINT [FK_ReceiptMedicine_Receipt]
GO
/****** Object:  ForeignKey [FK_SYS_Medicine_SYS_Laboratory]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[SYS_Medicine]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_Medicine_SYS_Laboratory] FOREIGN KEY([LaboratoryId])
REFERENCES [dbo].[SYS_Laboratory] ([Id])
GO
ALTER TABLE [dbo].[SYS_Medicine] CHECK CONSTRAINT [FK_SYS_Medicine_SYS_Laboratory]
GO
/****** Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient] FOREIGN KEY([ActiveIngredientId])
REFERENCES [dbo].[SYS_ActiveIngredient] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient] CHECK CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_ActiveIngredient]
GO
/****** Object:  ForeignKey [FK_SYS_MedicineActiveIngredient_SYS_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[SYS_Medicine] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineActiveIngredient] CHECK CONSTRAINT [FK_SYS_MedicineActiveIngredient_SYS_Medicine]
GO
/****** Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Leaflet]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[SYS_MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Leaflet] FOREIGN KEY([SYS_LeafletId])
REFERENCES [dbo].[SYS_Leaflet] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineLeaflet] CHECK CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Leaflet]
GO
/****** Object:  ForeignKey [FK_SYS_MedicineLeaflet_SYS_Medicine]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[SYS_MedicineLeaflet]  WITH NOCHECK ADD  CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Medicine] FOREIGN KEY([SYS_MedicineId])
REFERENCES [dbo].[SYS_Medicine] ([Id])
GO
ALTER TABLE [dbo].[SYS_MedicineLeaflet] CHECK CONSTRAINT [FK_SYS_MedicineLeaflet_SYS_Medicine]
GO
/****** Object:  ForeignKey [FK_User_Doctor]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [dbo].[Doctor] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Doctor]
GO
/****** Object:  ForeignKey [FK_User_Person]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Person]
GO
/****** Object:  ForeignKey [FK_User_Practice]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Practice] FOREIGN KEY([PracticeId])
REFERENCES [dbo].[Practice] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Practice]
GO
/****** Object:  ForeignKey [FK_User_Secretary]    Script Date: 06/19/2012 20:50:22 ******/
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Secretary] FOREIGN KEY([SecretaryId])
REFERENCES [dbo].[Secretary] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Secretary]
GO
