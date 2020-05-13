# Simple Cached Backing Store App Use case for performance and scalability

This app has a simple REST api to POST, GET, DELETE a token from storage.  
For simplicity, it uses .net's `IDistributedCache` to quickly get the app up and running with basic functionality.

It also contains an endpoint to allow an admin to check configurations from the redis cache. This endpoint is shown here for demonstrations purpose only and not recommended for production environments.

## Architectural Considerations
This typical pattern of deploying redis on top of, say, a relational database is meant to help out your database backing - you might have queries that take a long time, or many writes happening which could lead to locks, or even data that requires disk scans.  
It is not unusual to find that a lot of the resources requested have patterns. Some resources are requested more than others - e.g. users in a large city are more likely to ask for services near them, and this traffic could be more significant than users in remote villages.

Redis is very fast at recovering key-pair values as all data is stored in memory. You can define a TTL (time-to-live) to ensure data is not getting stale in your cache, but it's a good idea for the mechanisms that write to your persistent store to also update your cache (or at least raise an event for a worker to update your cache over time).

The following diagram illustrates a very simple app backed by Redis and MySql:  
![SimplePersistentStoreAppArchitectureDiagram](/assets/simple_cached_persistent_store_app/diagrams/ArchitectureDiagram_01.png "Simple Persistent Store App Architecture Diagram")


A user that attempts to store a resource (in our case a token) will result in the token being stored in SQL first, and then in the cache (if it succeeds to store in MySQL):  
![DataInsertionSequenceDiagram](/assets/simple_cached_persistent_store_app/diagrams/DataInsertionSeq_01.png "Data Insertion Sequence Diagram")

Whenever an user attempts to retrieve a token, the app will first attempt to capture from the cache, and only then it will try to find the token in the persistent store.  
The first operation is really cheap and fast, whilst the SQL operation can be slow and expensive.  
![SuccessfullDataRetrievelSequenceDiagram](/assets/simple_cached_persistent_store_app/diagrams/SuccessfullDataRetrievalSeq_01.png "Successfull Data Retrievel Sequence Diagram")
<br>

If the app has to resource to the backing store, and a value for the resource is indeed found, the app will refresh the cache with that token.  
![NoHitDataRetrievalSequenceDiagram](/assets/simple_cached_persistent_store_app/diagrams/NoHitDataRetrievalSeq_01.png "No Hit Data Retrieval Sequence Diagram")


## Inner workings of the App:
// TODO


## Manually Deploying the App
Follow the requirements in this [README](../../README.md) to setup a CF environment with the required tiles.

__You will also need to meet the pre-requisits listed on the [main Readme](../README.md).__

Once your CloudFoundry environment is setup, follow the next steps:

```
cf api api.sys.env-name.cf-app.com
cf login -u "$CF_USERNAME" -p "$CF_PASSWORD"
cf target -o "$ORG" -s "$SPACE"
cf marketplace
cf create-service p.redis on-demand-cache redis_cache_01
cf create-service p.mysql db-small mysql_01
../../DBMigrations/perform_migration.sh
cf push
cf bind-service simple_cached_persistent_store_app redis_cache_01
cf bind-service simple_cached_persistent_store_app mysql_01
cf restage simple_cached_persistent_store_app
```