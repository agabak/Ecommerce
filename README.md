E-Commerce Microservices System
This project is a modular e-commerce backend built with a modern microservices architecture. The solution separates core business domains—products, inventory, and orders—into independent services, each with its own database. The services communicate asynchronously via Kafka, supporting scalability, flexibility, and resilience.

Key features:

Microservice Design: Each business domain (Products, Inventory, Orders) is developed and deployed independently.

Event-Driven Integration: Kafka is used for reliable messaging and eventual consistency across services.

Database Per Service: Every microservice uses its own database and employs globally unique identifiers (GUIDs) for distributed data integrity.

Inventory Management: Supports real-time stock tracking, inventory history, and multi-warehouse management.

Designed for Dapper: Lightweight DTOs and repositories are optimized for fast data access with Dapper in .NET.
