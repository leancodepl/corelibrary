# CQRS

**Command-Query Responsibility Segregation** is a pattern that promotes hard split between parts of code (classes/objects) that read data ([query]) and the ones that modify and write the data ([command]). It also separates the payload (e.g. object id, user name) from the code that executes the query/command.

This library went a step further and used commands and queries as client-facing API. This simplifies the interaction for clients, as they can easily distinguish whether their interaction with the API will change the system's state or just request data.

The strict separation of [command] and [query] has sometimes led to awkwardness for clients consuming the API. So [operation] was introduced to add more flexibility. Operations change the state of the system, but also allow to return some data.

Additionally, CoreLibrary has integrated the use of events within the API which enhances the core functionalities of CQRS and supports the decoupling of services in the microservices architecture, allowing for scalable and efficient event-driven interactions across different components of the system.

## Why CQRS?

- **Separation of concerns:** CQRS enforces a clear separation of concerns between the write and read actions. This leads to a more maintainable and modular codebase, making it easier to understand, test, and develop.
- **Improved security:** Security measures can be applied more granularly. For instance, you might have different authorization mechanisms for read and write operations, providing an additional layer of control.
- **Validation on the write side:** With CQRS, the write side handles commands that change the state of the system. This is where validation of incoming data and business rules can be enforced. By centralizing validation logic on the write side, you ensure that all changes to the system go through a consistent validation process eliminating the need for validation on the read side.
- **Flexibility in storage and models:** CQRS allows to use different data storage mechanisms for the read and write sides (this isn't a requirement, as a single storage mechanism can be used as well). For example, you might use a relational database for the write side and a NoSQL database for the read side. This flexibility enables you to choose the right tool for each job.

!!! tip
    For a deeper understanding of CQRS, explore these resources: Martin Fowler's insights on CQRS at [Martin Fowler's Blog](https://martinfowler.com/bliki/CQRS.html) and the explanation provided by Microsoft at [Documentation - CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs).

!!! tip
    You can find more about how CQRS allows us generate API contracts for clients  [ContractsGenerator documentation](https://github.com/leancodepl/contractsgenerator/blob/main/docs/index.md).

[query]: ./query/index.md
[command]: ./command/index.md
[operation]: ./operation/index.md
