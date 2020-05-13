# Simple Cache Use case for performance and scalability

This app has a simple REST api to POST, GET, DELETE a token from storage.  
For simplicity, it uses .net's `IDistributedCache` to quickly get the app up and running with basic functionality.   
`IDistributedCache` is very limited in terms of functionality, and uses `eval` under the hood to ensure things like TTL, expiry and transaction coherence are met.

## Architectural Considerations
Sometimes there is data that is good to access fast (e.g. good user experience with responsive sites, faster transactions), but not necessarily important if it gets lost. An cache that works in-memory - such as Redis - can provide that, by storing your session data, most seen items on your website, deals, or inflight order information to name but a few.

Redis is very fast at recovering key-pair values as all data is stored in memory. You can define a TTL (time-to-live) to ensure data is not getting stale in your cache, but it is important to note that you should not store anything in Redis that you cannot afford to loose.

Because Redis uses RAM as main storage solution - often backing up to disk - scaling vertically may be expensive to achieve as RAM is more expensive than hard drives (even SSD). Bear that in mind when deciding to adopt Redis and what the future of your app may look like.

The following diagram illustrates thise very simple app backed by Redis:  
![SimpleAppArchitectureDiagram](/assets/simple_cached_app/diagrams/ArchitectureDiagram_01.png "Simple App Architecture Diagram")
<br>

This is an example of the interactions that follow when a token is POST through our app:  
![DataInsertionSequenceDiagram](/assets/simple_cached_app/diagrams/DataInsertionSeq_01.png "Data Insertion Sequence Diagram")
<br>

This is an example of the interactions that follow when a token is GET through our app:  
![SuccessfullDataRetrievelSequenceDiagram](/assets/simple_cached_app/diagrams/SuccessfullDataRetrievalSeq_01.png "Successfull Data Retrievel Sequence Diagram")
<br>

This app is a very simple example of how to get started with Redis in .Net Core and Steeltoe, not intended to be used in production.

## Inner workings of the App:
//TODO


## Manually Deploying the App
Follow the requirements in this [README](../../README.md) to setup a CF environment with the required tiles.

__You will also need to meet the pre-requisits listed on the [main Readme](../README.md).__

Once your CloudFoundry environment is setup, follow the next steps:

```
cf login -a api.sys.env-name.cf-app.com -u "$CF_USERNAME" -p "$CF_PASSWORD"
cf target -o "$ORG" -s "$SPACE"
cf marketplace
cf create-service p-redis shared-vm redis_cache_01
cf push
cf bind-service simple_cached_app redis_cache_01
cf restage simple_cached_app
```