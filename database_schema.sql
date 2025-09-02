-- Таблица для управления экземплярами webhook приложений и выбора лидера
CREATE TABLE webhook_instances (
    id VARCHAR(50) PRIMARY KEY,                    -- Уникальный ID экземпляра (GUID)
    app_name VARCHAR(100) NOT NULL,               -- Имя приложения (WebhookApp1, WebhookApp2)
    is_leader BOOLEAN DEFAULT FALSE,              -- Является ли экземпляр лидером
    heartbeat_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- Последний heartbeat
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP    -- Время создания записи
);

-- Индекс для быстрого поиска лидера по приложению
CREATE INDEX idx_webhook_instances_app_leader ON webhook_instances(app_name, is_leader);

-- Индекс для очистки устаревших записей
CREATE INDEX idx_webhook_instances_heartbeat ON webhook_instances(heartbeat_at);