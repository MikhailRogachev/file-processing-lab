# AppDbContext
## Index







## Command (outbox) table


ColumnData TypePurposeidUUID / BIGINTUnique primary key identifying the outbox message.aggregate_typeVARCHARThe domain entity producing the event (e.g., Order, User, Payment).aggregate_idVARCHARIdentifier of the specific domain object (e.g., order_12345).typeVARCHARThe event/message type (e.g., OrderCreated, PaymentFailed).payloadJSONB / TEXTThe event payload serialized as JSON or Avro.created_atTIMESTAMPRecord creation timestamp.processed_atTIMESTAMPNULL until successfully published to the message broker (RabbitMQ, Kafka, etc.).error_messageTEXTCaptures failure details if publishing encounters retriable errors.