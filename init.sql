CREATE SCHEMA IF NOT EXISTS orchestrator;

CREATE TABLE IF NOT EXISTS orchestrator.SagaLog (
    saga_id uuid PRIMARY KEY,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    status VARCHAR(32) NOT NULL,
    current_step VARCHAR(32),
    payload JSON NOT NULL
);

CREATE TABLE IF NOT EXISTS orchestrator.SagaStep (
    saga_id UUID NOT NULL,
    step_name VARCHAR(32) NOT NULL,
    status VARCHAR(32) NOT NULL,
    idempotency_key UUID NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    PRIMARY KEY (saga_id, step_name),
    FOREIGN KEY (saga_id) REFERENCES orchestrator.SagaLog(saga_id)
);

CREATE TABLE IF NOT EXISTS orchestrator.SagaOutbox (
    saga_id UUID NOT NULL,
    step_name VARCHAR(32) NOT NULL,
    payload JSON NOT NULL,
    sent boolean NOT NULL,
    created_at TIMESTAMP NOT NULL,
    PRIMARY KEY (saga_id, step_name),
    FOREIGN KEY (saga_id, step_name) REFERENCES orchestrator.SagaStep(saga_id, step_name)
)