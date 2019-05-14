#!/bin/sh

echo "** Warning: This Script will scale up your apps and deploy JMETER workers! **"

echo "Runs load test against SimpleCachedApp or SimpleCachedPersistentStoreApp"
read -p "Enter app name to target: " $target_app

PLAN_FILE_PATH="LoadTesting/Plan_LoadTest_SteelToeCachedApp.jmx"

if [ "$target_app" == "SimpleCachedPersistentStoreApp" ]
then
    PLAN_FILE_PATH="LoadTesting/Plan_LoadTest_SteelToePersistentStoreCachedApp.jmx"
fi

# Scale up for load test
CF_APP_NAME="simple_cached_app"
if [ "$target_app" == "SimpleCachedPersistentStoreApp" ]
then
    CF_APP_NAME="simple_cached_persistent_store_app"
fi
    
cf scale $CF_APP_NAME -i 6 -m 2048MB -f

# Deploy jmeter including plans
bosh -d jmeter_storm deploy LoadTesting/jmeter_storm_manifest.yml \
    --var=jmx_plan="$(cat $PLAN_FILE_PATH)" \
    --vars-file="LoadTesting/opsvars/ops_bosh_cloud_config.yml" \
    --non-interactive

# Run errand (the actual load test)
bosh -d jmeter_storm run-errand storm --download-logs