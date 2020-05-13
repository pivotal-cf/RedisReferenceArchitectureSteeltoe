#!/usr/bin/env bash

echo "** Warning: This Script will scale up your apps! **"

echo "Runs load test against SimpleCachedApp or SimpleCachedPersistentStoreApp"
read -rp "Enter app name to target: " target_app

PLAN_FILE_PATH="LoadTesting/Plan_LoadTest_SteelToeCachedApp.jmx"
JMETER_LOG_FILE="./LoadTesting/results/jmeter_log_file.log"
PLAN_EXECUTION_RESULT_FILE="./LoadTesting/results/result_log.jtl"
RESULTS_DASHBOARD_PATH="./LoadTesting/results/results_dashboard"

if [ "$target_app" = "SimpleCachedPersistentStoreApp" ]
then
    PLAN_FILE_PATH="LoadTesting/Plan_LoadTest_SteelToePersistentStoreCachedApp.jmx"
fi

# Scale up for load test
CF_APP_NAME="simple_cached_app"
if [ "$target_app" = "SimpleCachedPersistentStoreApp" ]
then
    CF_APP_NAME="simple_cached_persistent_store_app"
fi
    
cf scale $CF_APP_NAME -i 6 -m 2048MB -f

jmeter --nongui \
    --testfile $PLAN_FILE_PATH \
    --jmeterlogfile $JMETER_LOG_FILE \
    --logfile $PLAN_EXECUTION_RESULT_FILE \
    --reportatendofloadtests \
    --reportoutputfolder $RESULTS_DASHBOARD_PATH