# MicroWebShop

eCommerce applicaiton built using ASP.Net and microservice architecture, containerised using Docker and Kubernetes.
Based on Microsoft's eShopOnContainers application.

Main Components:
- MVC Web App - Client app
- Catalogue Service - Stores product information, CRUD based. (uses SQL Server and EntityFramework core)
- Basket Service - Enables products to be stored in a user's basket, CRUD based. (uses Redis as a cache)
- Identity Service - Security Token Service (STS) (uses Identiy Server 4 and SQL Server) 
- Order Service - Incomplete (work in progress)
