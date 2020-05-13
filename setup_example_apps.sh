#!/usr/bin/env bash

echo "Checking for dependencies"
if ! command -v -- mysql
then
    echo "you need to install mysql client and have it on PATH"
    echo "eg.: brew install mysql && PATH=PATH:path-to-bin"
    exit 1
fi

wait_for() {
    SERVICE=$1

    echo "Waiting until ${SERVICE} are created"
    while ! cf s | grep "${SERVICE}" | grep "create succeeded"; do
        if cf s | grep "${SERVICE}" | grep "create failed"; then
            echo "Failed to create ${SERVICE}"
            exit 1
        fi
        echo "${SERVICE} not ready yet, retrying in 20s"
        sleep 20s
    done
}

read -rp "Enter environment url, usually api.sys.env-name.cf-app.com: " CF_API
read -rp "Enter environment username: " CF_USERNAME
read -rsp "Enter environment password: " CF_PASSWORD
read -rp "Enter org name: " ORG_NAME
read -rp "Enter space name: " SPACE_NAME

cf login -a "$CF_API" -u "$CF_USERNAME" -p "$CF_PASSWORD"
cf target -o "$ORG_NAME" -s "$SPACE_NAME"
cf create-service p.redis "$(cf marketplace -s p.redis | grep Redis | awk '{print $1}')" redis_01
cf create-service p.mysql "$(cf marketplace -s p.mysql | grep MySQL | awk '{print $1}')" sql_01

wait_for redis_01
wait_for sql_01

# Run DB Migration to setup up SQL schema
./DBMigrations/perform_migration.sh

cf push
