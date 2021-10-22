# :warning: [DEPRECATED]
Please consider referencing the [SteeltoeOSS](https://github.com/SteeltoeOSS) repository and its [Redis sample project](https://github.com/SteeltoeOSS/Samples/tree/2.x/Connectors/src/AspDotNetCore/Redis) instead.

# Redis Steeltoe Architecture Reference
A serie of Example apps to showcase how to leverage PCF Redis with .NET Steeltoe Apps running on CloudFoundry.  
Includes JMeter plans for load and perf testing.

## Pre-Requisites
- You will need a CloudFoundry environment PAS 2.5 or above.
- You will need one or more plans available for PCF Redis 2.1 or higher **with LUA enabled.** Please ask your platform operator for assistance. 
- You will need one or more plans available for PCF MySql v2.  
- You will need this repository cloned to your local machine.  
- When ready, run the `setup_example_apps.sh` script, which will create one instance for each and all the apps in this repo, as well as the required SI and bindings. It will also setup the DB schema for the app to use.  

## Pre-Requisites for Load Testing
- Read more about how to do load testing on these apps [here](./assets/common/docs/load_testing.md)
  
## Included Examples:
- [Simple Cached App](src/SimpleCachedApp/README.md) - a simple app using Redis as a **volatile** key value store.
- [Simple Cached Persistent Store App](src/SimpleCachedPersistentStoreApp/README.md) - example on how to use Redis on top of a MySQL store for performance and scalability.

### TODO:
- Create app that uses Dapper instead of MySQLConnector directly.
- Create app that uses Polly to correctly do failover logic (retry/circuitbreak).
- Add circuit breaking by leveraging Steeltoe, Spring Cloud Services and Hystrix
- Create a Pipeline for use for the team. 
- Create a diagram of the responsibilities of the app, and abstraction layers
