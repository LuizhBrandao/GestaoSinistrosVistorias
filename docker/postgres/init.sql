-- Script de inicialização do PostgreSQL para a Plataforma de Sinistros e Vistorias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Schemas separados por Contexto Delimitado (DDD)
CREATE SCHEMA IF NOT EXISTS sinistros;
CREATE SCHEMA IF NOT EXISTS vistorias;
CREATE SCHEMA IF NOT EXISTS auditoria;

COMMENT ON SCHEMA sinistros IS 'Contexto Delimitado de Sinistros, Coberturas e Regulação de Alçadas (.NET Core)';
COMMENT ON SCHEMA vistorias IS 'Contexto Delimitado de Vistorias, Checklist de Avarias e Evidências Fotográficas (Node.js/Angular)';
COMMENT ON SCHEMA auditoria IS 'Trilha de auditoria imutável de transações financeiras e aprovações regulatórias';
