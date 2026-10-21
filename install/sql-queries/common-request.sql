use FileProcessingDb
go

SELECT
P.id
, P.Identifier
, A.[Filename]
, A.AssetType
, A.Etag
, A.Size
, A.[Priority]
, A.[Version]
FROM MediaPackages AS P INNER JOIN MediaAssets AS A ON P.Id = A.MediaPackageId
