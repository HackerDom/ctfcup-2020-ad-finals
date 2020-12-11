#!/bin/sh

until nc -z -v -w30 mysql 3306
do
  echo "Waiting for database connection..."
  sleep 1
done

su barker
python3 manage.py migrate
python3 manage.py runserver 0.0.0.0:1337 --insecure
