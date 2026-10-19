# IEvent objects description

## ValidateMediaAssetEvent
This event is occured when the `FileChanged` event is received from the AWS storage. 

|Parameter|Type|Default|Comments|
|---|---|---|---|
|Id|Guid||Unique event Id|
|ReferenceId|Guid||MediaPackage unique Id|
|CreatedAt|DateTimeOffset||Date and time created|
|FileName|string||the file name is received in the Aws storage|


### **Pipeline Stage Summary**

1. **Ingestion Layer:**
* S3 triggers an event notification to SQS when a file lands in the bucket.
* `SqsListener` pulls the event message containing the file key and bucket name.


2. **Phase 1: Fast Probing (Milliseconds):**
* Extensions map to registered `IFileProbeValidator` implementations (MP4, AVI, MXF, PNG, SRT, STL).
* **S3 Range Requests** fetch only the required header bytes (e.g., 8–1024 bytes) without downloading the full payload.
* Employs **All-Pass Logic**: every probe mapped to that extension must return `true`.


3. **Phase 2: Technical Quality Assessment (Media Files):**
* Non-media files (SRT, STL, PNG) complete validation immediately after Phase 1.
* Video files generate a temporary 15-minute **S3 Pre-signed URL**.
* `FFprobe` reads the metadata container over HTTP via pre-signed URL to verify width, height, bitrate, and codec without downloading the raw video file.


4. **Event Outbox & Downstream Processing:**
* The validation state is recorded in SQL Server inside the transactional outbox table.
* The polling worker publishes a message to RabbitMQ to kick off downstream transcoding or subtitle processing services.



```mermaid

flowchart TD
    %% 1. Ingestion & Event Layer
    subgraph Ingestion ["1. Event Ingestion Layer"]
        A[File Uploaded to AWS S3] -->|S3 Event Notification| B[AWS SQS Queue]
        B -->|Consume Message| C[SqsListener Service]
    end

    %% 2. Phase 1: Fast Probe Pipeline
    subgraph Phase1 ["2. Phase 1: Fast Header & Format Validation (S3 Range Requests)"]
        C --> D[Extract File Extension]
        D --> E[Resolve Probes via DI Keyed Services]
        
        E --> F[Execute S3 Byte-Range Probes]
        
        subgraph Probes ["Probe Implementations"]
            F -->|Magic Bytes| P1[S3Mp4Validator / S3AviValidator / S3MxfValidator / S3PngValidator]
            F -->|Syntax / Header| P2[S3SrtValidator / S3StlValidator]
        end

        P1 --> G{All-Pass Check}
        P2 --> G
        
        G -->|FAIL Any Probe| FAIL[Mark Event as Failed & Log Warning]
    end

    %% 3. Phase 2: Technical Quality Assessment
    subgraph Phase2 ["3. Phase 2: Technical Quality Assessment (FFprobe)"]
        G -->|PASS All Probes| H{Is Video / Media File?}
        
        H -->|No Subtitle / Image| COMPLETE[Mark File Validated in DB]
        H -->|Yes MP4 / AVI / MXF| I[Generate S3 Pre-Signed URL]
        
        I --> J[Run FFprobe against Pre-Signed URL]
        J --> K[Parse JSON Metrics: Resolution, Bitrate, Codec]
        
        K --> L{Meets Requirements?}
        L -->|No| FAIL
        L -->|Yes| COMPLETE
    end

    %% 4. Downstream Processing
    subgraph Downstream ["4. Event Outbox & Downstream Routing"]
        COMPLETE --> M[Write Commands to SQL Outbox Table]
        M -->|Transactional Outbox Poller| N[Publish Event to RabbitMQ]
        N --> O[Downstream Workers: Transcoding / Subtitle Parsing]
    end

    %% Styling
    style Ingestion fill:#f9f9f9,stroke:#333,stroke-width:1px
    style Phase1 fill:#e1f5fe,stroke:#0288d1,stroke-width:1px
    style Phase2 fill:#fff3e0,stroke:#f57c00,stroke-width:1px
    style Downstream fill:#e8f5e9,stroke:#388e3c,stroke-width:1px
    style FAIL fill:#ffebee,stroke:#d32f2f,color:#d32f2f
    style COMPLETE fill:#e8f5e9,stroke:#2e7d32,color:#2e7d32


```