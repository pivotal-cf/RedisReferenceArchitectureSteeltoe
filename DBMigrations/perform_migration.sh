#!/bin/bash

LOCAL_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )"  && pwd )"

# Install CF CLI Plugins needed for the script to complete
cf install-plugin -f -r "CF-Community" mysql-plugin

cat "${LOCAL_DIR}"/01_create-token-table.sql | cf mysql sql_01 
