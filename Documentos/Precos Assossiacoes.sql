/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP 1000 [PREC_CD_CODIGO]
      ,[PREC_DT_VIGENCIA_INICIO]
      ,[PREC_DT_VIGENCIA_FIM]
      ,[PREC_DS_ASSOCIACAO]
      ,[ASSO_CD_CODIGO]
  FROM [SINCRONIZACAO].[dbo].[SINCTPREC]


  USE [SINCRONIZACAO]
GO

/****** Object:  Table [dbo].[SINCTPREC]    Script Date: 01/11/2012 20:57:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SINCTPREC](
	[PREC_CD_CODIGO] [int] IDENTITY(1,1) NOT NULL,
	[PREC_DT_VIGENCIA_INICIO] [datetime] NOT NULL,
	[PREC_DT_VIGENCIA_FIM] [datetime] NOT NULL,
	[PREC_DS_ASSOCIACAO] [varchar](100) NOT NULL,
	[ASSO_CD_CODIGO] [int] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


PREC_CD_CODIGO	PREC_DT_VIGENCIA_INICIO	PREC_DT_VIGENCIA_FIM	PREC_DS_ASSOCIACAO	ASSO_CD_CODIGO
1	2009-01-01 00:00:00.000	2009-12-31 00:00:00.000	ABER	3
2	2009-01-01 00:00:00.000	2009-12-31 00:00:00.000	ABEM	4
3	2010-01-01 00:00:00.000	2010-12-31 00:00:00.000	ABER	3
4	2010-01-01 00:00:00.000	2010-12-31 00:00:00.000	ABEM	4
5	2011-01-01 00:00:00.000	2011-12-31 00:00:00.000	UBEM	5
6	2011-01-01 00:00:00.000	2011-12-31 00:00:00.000	ABEM	4
7	2012-01-01 00:00:00.000	2012-12-31 00:00:00.000	UBEM	5