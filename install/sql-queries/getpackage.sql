use FileProcessingDb
go

declare @id uniqueidentifier = '923EF314-1D56-4968-B0C5-750249ABC38C'

SELECT [Id]
      ,[Identifier]
      ,[State]
      ,[Result]
      ,[Details]
      ,[CreatedAt]
      ,[UpdatedAt]
  FROM [FileProcessingDb].[dbo].[MediaPackages] WHERE Id = @id

  SELECT [Id]
      ,[MediaPackageId]
      ,[Filename]
      ,[Result]
      ,[Details]
      ,[CreatedAt]
      ,[UpdatedAt]
      ,[AssetType]
      ,[Etag]
      ,[Size]
      ,[Version]
  FROM [FileProcessingDb].[dbo].[MediaAssets] where MediaPackageId = @id
