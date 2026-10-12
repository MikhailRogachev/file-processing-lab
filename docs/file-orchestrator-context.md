# File Orchectrator Application Context


## Context Diagram

![img](/docs/img/file-processing-context.svg)

|Component|Functions|Comments|
|---|---|---|
|S3 Backet|* Store media files for future use;<br/>* Publish __"FileChanged"__ and __"FileDeleted"__ events to the Sqs queue.|__S3 Bucket__ is a public cloud storage container in AWS used to store unstructured data as "objects" (files).|
|Sqs Queue|Consume S3 event messages and dispatch events to the File Orchestrator.|__Amazon SQS (Simple Queue Service)__ is a managed message-queue service offered by AWS.<br/>It allows different software systems or microservices to communicate asynchronously without being directly connected.|
|Sqs Listener|This service consumes messages from an AWS SQS queue, determines the event type (__FileChanged__ or __FileDeleted__), and publishes the event to the File Orchestrator.|Background service|
|EventPublisher|||
|RabbitMQ|||
|EventBus|||
|EventHandler|The __EventHandler__ performs actions based on the received event and saves any modified entities (__assets, jobs, tasks, events__) to the database.|The __File Orchestrator__ contains a set of __EventHandlers__. Each event handler is responsible for processing a __specific event__. The events collection is described below.|
|Database|Store operational data (packages, assets, jobs, tasks), commands|The database diagram is presented below.|
|Outbox Service|Periodically retries commands from the database and publishes corresponding events to the __EventPublisher__.|Background service|



## Database diagram

The current database diagram

![img](/docs/img/db-diagram.png)


## Events collection

|Event|Command|Services|Description|
|---|---|---|---|
|FileChanged|Validate|||
|FileDeleted||||
|AssetValidated|CheckQuality|||
|QualitityChecked|AddAsset|||
|AssetAdded|DownloadFile|||
|FileDownloaded|StartContainer||Downloaded|
|ContainerStarted|||For each Agent, connected to shared file|
|ContainerTaskCompleted|SentInfoToMosDispatcher|||
|||||
|||||
|||||







## Architecture explanation

The Outbox pattern and the RabbitMQ event publisher serve two distinct, complementary roles in this architecture: the Outbox guarantees **atomic reliability**, while RabbitMQ handles **scalable distribution**.

They work together to solve what is known in distributed systems as the "dual-write problem." Here is why both are necessary for the application:

* **Solving the Dual-Write Problem:** If the application saves a new `MediaPackage` to the SQL Server database and then directly tries to publish an event to RabbitMQ, a network error or app crash between those two steps would leave the system in an inconsistent state (the data is saved, but downstream services are never notified). The Transactional Outbox pattern ensures the business data and the pending event (in the `Commands` table) are saved together in a single, atomic database transaction. If one fails, both fail.
* **SQL is Not a Message Broker:** The Outbox table safely stores the pending event, but relational databases are notoriously inefficient at acting as high-throughput message queues. Polling a SQL table continuously creates database contention (which is why the document mentions using specific locking hints like `UPDLOCK` and `READPAST`), and SQL lacks native pub/sub routing capabilities.
* **RabbitMQ for Advanced Routing and Delivery:** Once a background relay worker reads the unprocessed messages from the Outbox table, it uses the RabbitMQ publisher to push them into the message broker. RabbitMQ takes over the complex task of asynchronous distribution. It uses routing keys to deliver messages to specific queues, handles fan-out (e.g., notifying a logging service, a video transcoder, and a notification service simultaneously), and manages dead-letter routing for failed jobs.
* **Decoupled Worker Scaling:** By handing the messages off to RabbitMQ, the system decouples the fast ingestion process (saving the S3 notification to the database) from the slow processing tasks (analyzing and converting the media file). RabbitMQ queues buffer the work, allowing downstream consumer services to pull messages at their own pace without overwhelming the database or the workers.

In short, the database Outbox acts as an infallible safety net so no events are ever lost during system crashes, and the RabbitMQ publisher acts as the logistical engine to efficiently route those saved events to the workers that need them.