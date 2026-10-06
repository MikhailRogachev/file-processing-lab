# File Orchestrator Processes

## Current

<table>
<tr>
    <th>Process</th>
    <th>Stage</th>
    <th>Subjob</th>
    <th>Task</th>
</tr>
<tr>
    <td>Download</td>
    <td>Filetransfer</td>
    <td></td>
    <td></td>
</tr>
<!-- Preprocess -->
<tr>
    <td rowspan='6'>Preprocess</td>
    <td>Filevalidation</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Mediainfo</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Qualitycheck</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>MosDispatcher</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Start-Container</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Mediainfo-Creator</td>
    <td></td>
    <td></td>
</tr>
<!-- Process -->
<tr>
    <td rowspan='12'>Process</td>
    <td rowspan='4'>Transcoding</td>
    <td>Start-Container</td>
    <td></td>
</tr>
<tr>
    <td>Upload</td>
    <td>Filetransfer</td>
</tr>
<tr>
    <td>FFmpeg-Transcoding</td>
    <td></td>
</tr>
<tr>
    <td>CreateChildAsset</td>
    <td></td>
</tr>
<tr>
    <td rowspan='4'>Thumbnail</td>
    <td>Start-Container</td>
    <td></td>
</tr>
<tr>
    <td>Upload</td>
    <td>Filetransfer</td>
</tr>
<tr>
    <td>FFmpeg-Thumbnail</td>
    <td></td>
</tr>
<tr>
    <td>CreateChildAsset</td>
    <td></td>
</tr>
<tr>
    <td rowspan='4'>Checksum</td>
    <td>Start-Container</td>
    <td></td>
</tr>
<tr>
    <td>Upload</td>
    <td>Filetransfer</td>
</tr>
<tr>
    <td>Checksum-Builder</td>
    <td></td>
</tr>
<tr>
    <td>CreateChildAsset</td>
    <td></td>
</tr>
<!-- Postprocess -->
<tr>
    <td rowspan='4'>Postprocess</td>
    <td rowspan='4'>MediainfoSidecar</td>
    <td>Start-Container</td>
    <td></td>
</tr>
<tr>
    <td>Upload</td>
    <td>Filetransfer</td>
</tr>
<tr>
    <td>Xml-Writer</td>
    <td></td>
</tr>
<tr>
    <td>CreateChildAsset</td>
    <td></td>
</tr>
</table>

## New Flow

|Process|Description|
|---|---|
|FileValidation|Check: Is file exist? File extension, file size, file memitypes|
|Mediainfo|Get File mediainfo|