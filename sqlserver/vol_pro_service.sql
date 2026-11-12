/*
Navicat Premium Data Transfer

Source Server         : mssql2019
Source Server Type    : SQL Server
Source Server Version : 15004430
Source Host           : 172.24.10.28:1433
Source Catalog        : vol_pro_service
Source Schema         : dbo

Target Server Type    : SQL Server
Target Server Version : 15004430
File Encoding         : 65001

Date: 12/04/2026 14:20:32
*/

-- ----------------------------
-- Table structure for TestService
-- ----------------------------
IF EXISTS (
    SELECT *
    FROM sys.all_objects
    WHERE
        object_id = OBJECT_ID(N'[dbo].[TestService]')
        AND
    type IN ('U')
)
DROP TABLE [dbo].[TestService]

CREATE TABLE [dbo].[TestService] (
    [Id] uniqueidentifier NOT NULL,
    [DbName] varchar(100)
    COLLATE Chinese_PRC_CI_AS NOT NULL,
    [DbContent] varchar(100)
    COLLATE Chinese_PRC_CI_AS NULL,
    [CreateDate] datetime NULL,
    [CreateID] int NULL,
    [Creator] varchar(30)
    COLLATE Chinese_PRC_CI_AS NULL,
    [Modifier] varchar(30)
    COLLATE Chinese_PRC_CI_AS NULL,
    [ModifyDate] datetime NULL,
    [ModifyID] int NULL
)

ALTER TABLE [dbo].[TestService] SET ( LOCK_ESCALATION = TABLE )

-- ----------------------------
-- Records of TestService
-- ----------------------------
INSERT INTO
    [dbo].[TestService] (
        [Id],
        [DbName],
        [DbContent],
        [CreateDate],
        [CreateID],
        [Creator],
        [Modifier],
        [ModifyDate],
        [ModifyID]
    )
VALUES (
        N'C0737155-AC39-4F35-87DC-22DD83896CAD',
        N'这是业务数据库',
        N'这是业务数据库。。',
        N'2023-05-19 11:46:34.000',
        N'3362',
        N'测试管理员',
        NULL,
        NULL,
        NULL
    )

-- ----------------------------
-- Primary Key structure for table TestService
-- ----------------------------
ALTER TABLE [dbo].[TestService]
ADD CONSTRAINT [PK__TestServ__3214EC07E508F8B8] PRIMARY KEY CLUSTERED ([Id])
WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        IGNORE_DUP_KEY = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    ) ON [PRIMARY]