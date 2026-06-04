# Digimarket.Microservices

A production-oriented e-commerce platform built with modern .NET technologies and enterprise architecture patterns.

The primary goal of this project is to simulate real-world business workflows found in large-scale online marketplaces while applying industry-standard architectural principles such as Domain-Driven Design, CQRS, Event-Driven Architecture, and Saga Pattern.

## Key Features

### Order Management Lifecycle

* Create and manage customer orders
* Distributed order processing using Saga State Machine
* Event-driven communication between services
* Reliable message delivery using Outbox/Inbox Pattern
* Order state transitions (Pending, Paid, Cancelled, Completed)

### Identity & Access Management

* Authentication and Authorization powered by OpenIddict
* OAuth2 and OpenID Connect support
* JWT access tokens
* Claim-based authorization policies
* Refresh token support

### Distributed Communication

* Asynchronous messaging with RabbitMQ and MassTransit
* gRPC communication for high-performance service-to-service calls
* Event-driven integration between microservices

### Data Consistency

* Transactional Outbox Pattern
* Inbox Pattern for idempotent message processing
* Saga orchestration for distributed transactions
* Unit of Work implementation

### Testing Strategy

* Unit Tests
* Integration Tests
* End-to-End business workflow validation
* Testcontainers for infrastructure dependencies
* Respawn for database cleanup and test isolation

---

## Architecture

The solution follows a layered architecture inspired by Domain-Driven Design:

* Domain Layer
* Application Layer
* Infrastructure Layer
* API Layer

### Architectural Patterns

* Domain-Driven Design (DDD)
* CQRS
* Mediator Pattern (MediatR)
* Repository Pattern
* Unit of Work
* Saga Pattern
* Event-Driven Architecture
* Outbox / Inbox Pattern

---

## Technology Stack

### Backend

* .NET 9
* ASP.NET Core
* MediatR
* FluentValidation
* MassTransit

### Databases

* PostgreSQL
* SQL Server
* Redis

### Messaging

* RabbitMQ
* MassTransit Saga State Machine

### Communication

* REST API
* gRPC

### Security

* OpenIddict
* OAuth2
* OpenID Connect
* JWT Authentication

### DevOps

* Docker
* Docker Compose

### Testing

* xUnit
* FluentAssertions
* Testcontainers
* Respawn

---

## Project Goals

This repository is intended as a reference implementation for developers interested in:

* Enterprise-grade microservice architecture
* Distributed transaction management
* Event-driven systems
* Saga orchestration
* Reliable messaging patterns
* Authentication and Authorization with OpenIddict
* Automated testing in distributed systems
