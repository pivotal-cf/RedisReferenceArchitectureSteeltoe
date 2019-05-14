#!/bin/sh

echo "Checking for dependencies"
which mysql
if [ $? -ne 0 ]
then
    echo "you need to install mysql client and have it on PATH"
    echo "eg.: brew install mysql && PATH=PATH:path-to-bin"
    exit 1
fi

wait_for() {
    SERVICE=$1

    echo "Waiting until ${SERVICE} are created"
    while ! cf s | grep ${SERVICE} | grep "create succeeded"; do
        if cf s | grep ${SERVICE} | grep "create failed"; then
            echo "Failed to create ${SERVICE}"
            exit 1
        fi
        echo "${SERVICE} not ready yet, retrying in 20s"
        sleep 20s
    done
}

read -p "Enter environment url, usually api.sys.env-name.cf-app.com: " ENV_API
read -p "Enter environment username: " USERNAME
read -sp "Enter environment password: " PASSWORD

cf login -a $ENV_API -u $USERNAME -p $PASSWORD --skip-ssl-validation
cf create-space system
cf target -o "system" -s "system"
cf create-service p.redis cache-small redis_01
cf create-service p.mysql db-small sql_01

wait_for redis_01
wait_for sql_01

# Run DB Migration to setup up SQL schema
./DBMigrations/perform_migration.sh

cf push
