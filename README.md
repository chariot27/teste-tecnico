Esta é uma excelente base técnica. Para elevar o nível desta documentação e torná-la um padrão de mercado (focada em **DX - Developer Experience**), apliquei melhorias na hierarquia visual, utilizei diagramas conceituais e organizei os fluxos lógicos para facilitar a leitura rápida.

---

# 🏦 BankMore - Documentação Técnica

A plataforma **BankMore** é uma solução de core banking distribuída, projetada para alta disponibilidade e consistência eventual, utilizando padrões modernos de arquitetura em ecossistema .NET.

## 🏗️ 1. Arquitetura de Sistema

O sistema adota o **Domain-Driven Design (DDD)** para segregação de contextos e o padrão **CQRS** para otimizar a performance de leitura e escrita.

### Stack Tecnológica

* **Runtime:** `.NET 8` (LTS)
* **Persistência:** `Dapper` (Performance-first) & `SQLite` (Dev/Test)
* **Mensageria:** `Kafka` (via KafkaFlow)
* **Padrões:** Mediator (MediatR), Repository Pattern, JWT Auth
* **Resiliência:** Estratégia de Idempotência por `IdRequisicao`

---

## 🔐 2. API de Conta Corrente

Gerencia o ciclo de vida do cliente e a integridade do saldo.

### 📍 Endpoints de Operação

> **Base Path:** `/api/contacorrente`

| Método | Endpoint | Proteção | Descrição |
| --- | --- | --- | --- |
| `POST` | `/cadastro` | Aberto | Cadastro de novos usuários (Validação de CPF). |
| `POST` | `/login` | Aberto | Autenticação e emissão de **Token JWT**. |
| `PATCH` | `/inativar` | Bearer | Soft delete da conta (`ATIVO = 0`). |
| `POST` | `/movimentacao` | Bearer | Registro de créditos/débitos com **Idempotência**. |
| `GET` | `/saldo` | Bearer | Cálculo de saldo: $\sum \text{Créditos} - \sum \text{Débitos}$. |

---

## 💸 3. Fluxo de Transferência e Tarifas

Abaixo, o fluxo detalhado da orquestração entre a **API de Transferência** e o **Serviço de Tarifas**.

### Processo Assíncrono

1. **Transferência:** A API valida saldo e executa débito/crédito via chamadas REST.
2. **Evento:** Publica no tópico `transferencias-realizadas`.
3. **Tarifação:** O Worker de Tarifas consome o evento e calcula a taxa (via `appsettings.json`).
4. **Conciliação:** O evento `tarifacoes-realizadas` é disparado e a API de Conta Corrente debita o valor final.

---

## 🛡️ 4. Qualidade e Resiliência

### ⚡ Idempotência

Para prevenir duplicidade em cenários de instabilidade de rede (Retry do cliente), implementamos uma camada de interceptação:

* **Check:** Antes do processamento, consulta-se a tabela `idempotencia`.
* **Cache/Store:** Se o `IdRequisicao` existir, retorna o `Status 200` com o payload armazenado.

### 🔐 Segurança & Dados

* **JWT:** Tokens com tempo de vida curto e assinatura `HMAC SHA256`.
* **Status da Conta:** Bloqueio imediato de transações para contas com flag `INACTIVE_ACCOUNT`.

---


---

