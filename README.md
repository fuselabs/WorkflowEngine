# A Stateful Workflow Execution Engine

This Library allows for creating long running, stateful and versioned workflows for breaking complex workflows into smaller ones that can be run synchronously or asynchronously over short or long periods of time.

The library has support for:
* Maintaining workflow state in a custom data store
* Pausing and restarting workflows
* Complex control flow using C# constructs
* Infrastructure features like logging, configuration and flighting
* Versioning

The accompanying unit tests have multiple examples for how to create and run a workflow for multiple use cases. 