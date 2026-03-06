
---

# 🏦 BankMore - Documentação Técnica 

## 1. Visão Geral da Arquitetura

O sistema é baseado em microsserviços distribuídos, seguindo os princípios de **Domain-Driven Design (DDD)** e o padrão **CQRS** (Command Query Responsibility Segregation). A comunicação entre os serviços ocorre de forma síncrona (via HTTPS/REST) para operações críticas e assíncrona (via Kafka) para efeitos colaterais e processamento de background.

### Stack Tecnológica

* 
**Linguagem/Runtime:** .NET 8.


* 
**Acesso a Dados:** Dapper (conforme padrão da instituição) e Repositories.


* 
**Mensageria:** Kafka utilizando a biblioteca **KafkaFlow**.


* 
**Banco de Dados:** SQLite (para desenvolvimento/teste).


* 
**Padrões:** Mediator (MediatR), Injeção de Dependência, Idempotência e Autenticação JWT.



---

## 2. API de Conta Corrente

Responsável pelo gerenciamento do ciclo de vida das contas, autenticação e histórico de movimentações.

### Endpoints Principais

| Método | Endpoint | Descrição |
| --- | --- | --- |
| **POST** | `/api/contacorrente/cadastro` | Realiza o cadastro de novos usuários validando o CPF.

 |
| **POST** | `/api/contacorrente/login` | Autentica o usuário e retorna um **Token JWT** com a identificação da conta.

 |
| **PATCH** | `/api/contacorrente/inativar` | Inativa a conta do usuário autenticado (campo `ATIVO` = 0).

 |
| **POST** | `/api/contacorrente/movimentacao` | Registra depósitos ou saques. Suporta **Idempotência** via `IdRequisicao`.

 |
| **GET** | `/api/contacorrente/saldo` | Consulta o saldo atual (Soma dos créditos - soma dos débitos).

 |

**Regras de Segurança:**

* Todos os endpoints (exceto cadastro e login) exigem o header `Authorization: Bearer {token}`.


* O saldo e a movimentação validam se a conta está ativa (`INACTIVE_ACCOUNT`).



---

## 3. API de Transferência

Orquestra o envio de valores entre contas da mesma instituição, garantindo a integridade financeira.

### Endpoint Principal

#### `POST /api/transferencia`

* 
**Funcionalidade:** Recebe o destino e o valor, executando chamadas internas à API de Conta Corrente para realizar o débito na origem e o crédito no destino.


* 
**Resiliência:** Em caso de falha em uma das etapas, o sistema deve ser capaz de realizar o estorno ou garantir a consistência.


* 
**Assincronismo:** Após o sucesso, produz uma mensagem no tópico Kafka `transferencias-realizadas` para que o serviço de tarifas processe a cobrança.



---

## 4. Aplicação de Tarifas (Serviço Assíncrono)

Implementado como um serviço de background para processar taxas administrativas.

### Fluxo de Eventos

1. 
**Consumo:** Escuta o tópico `transferencias-realizadas`.


2. 
**Processamento:** Aplica o valor da tarifa parametrizado no `appsettings.json`.


3. 
**Persistência:** Registra a tarifação na tabela `tarifas` com data e valor.


4. 
**Notificação:** Envia uma mensagem para o tópico `tarifacoes-realizadas`.


5. 
**Conciliação:** A API de Conta Corrente consome este tópico final para debitar automaticamente o valor da tarifa do saldo do usuário.



---

## 5. Padrões de Qualidade e Infraestrutura

### Idempotência

Para atender ao requisito de resiliência a falhas de conexão, as APIs utilizam uma tabela de `idempotencia`. Antes de processar uma `idrequisicao`, o sistema verifica se ela já foi concluída, retornando o resultado armazenado em vez de processar novamente.

### Segurança da Informação

* 
**Proteção de Dados:** CPF e dados sensíveis são restritos ao contexto necessário.


* 
**JWT:** Tokens possuem validação de tempo de vida e assinatura rigorosa.



### Execução e Testes

* 
**Docker:** O projeto inclui um arquivo `docker-compose.yaml` para subir as APIs, o broker Kafka e o banco de dados de forma orquestrada.


* 
**Testes:** Cobertura através de projetos de testes automatizados (Unitários e de Integração) conforme exigência do time de qualidade.
