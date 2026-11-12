/*
Navicat Premium Data Transfer

Source Server         : mssql2019
Source Server Type    : SQL Server
Source Server Version : 15004430
Source Host           : 172.24.10.28:1433
Source Catalog        : vol_pro_test
Source Schema         : dbo

Target Server Type    : SQL Server
Target Server Version : 15004430
File Encoding         : 65001

Date: 12/04/2026 14:20:48
*/

-- ----------------------------
-- Table structure for TestDb
-- ----------------------------
IF EXISTS (
    SELECT *
    FROM sys.all_objects
    WHERE
        object_id = OBJECT_ID(N'[dbo].[TestDb]')
        AND
    type IN ('U')
)
DROP TABLE [dbo].[TestDb]

CREATE TABLE [dbo].[TestDb] (
    [Id] uniqueidentifier NOT NULL,
    [TestDbName] varchar(100)
    COLLATE Chinese_PRC_CI_AS NOT NULL,
    [TestDbContent] varchar(100)
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

ALTER TABLE [dbo].[TestDb] SET ( LOCK_ESCALATION = TABLE )

-- ----------------------------
-- Records of TestDb
-- ----------------------------
INSERT INTO
    [dbo].[TestDb] (
        [Id],
        [TestDbName],
        [TestDbContent],
        [CreateDate],
        [CreateID],
        [Creator],
        [Modifier],
        [ModifyDate],
        [ModifyID]
    )
VALUES (
        N'6E4D6246-8A6C-4687-B11E-F35523B149E0',
        N'这是测试数据库',
        N'这是测试数据库。。。',
        N'2023-05-19 11:46:47.000',
        N'3362',
        N'测试管理员',
        NULL,
        NULL,
        NULL
    )

-- ----------------------------
-- Primary Key structure for table TestDb
-- ----------------------------
ALTER TABLE [dbo].[TestDb]
ADD CONSTRAINT [PK__TestDb__3214EC07A6DC3113] PRIMARY KEY CLUSTERED ([Id])
WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        IGNORE_DUP_KEY = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    ) ON [PRIMARY]