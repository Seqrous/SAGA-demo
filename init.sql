CREATE SCHEMA IF NOT EXISTS orchestrator;

CREATE TABLE IF NOT EXISTS orchestrator.SagaLog (
    saga_id uuid PRIMARY KEY,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(32) NOT NULL,
    current_step VARCHAR(32)
);

CREATE TABLE IF NOT EXISTS orchestrator.SagaStep (
    saga_id UUID NOT NULL,
    step_name VARCHAR(32) NOT NULL,
    status VARCHAR(32) NOT NULL,
    idempotency_key UUID NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (saga_id, step_name)
);
