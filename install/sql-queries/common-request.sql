use FileProcessingDb
go

SELECT
P.id
, P.Identifier
, P.[State]
, A.[Filename]
, A.AssetType
, A.Etag
, A.Size
FROM MediaPackages AS P INNER JOIN MediaAssets AS A ON P.Id = A.MediaPackageId
