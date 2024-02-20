#!/bin/bash

cd "$(dirname "$0")"
cd ..

models=$(ls Models/*/ -d | grep -Po '(?<=/)[A-Za-z0-9_]*' | jq -R -s -c 'split("\n")[:-1]')
model_changes=$(git diff --name-only HEAD HEAD~1 | grep -Po '(?<=Models\/)[A-Za-z0-9_]*(?=\/)' | sort --unique | jq -R -s -c 'split("\n")[:-1]')

if [ $model_changes == "[]" ];
then
	result=$models
else
	result=$(jq --null-input "$models-($models-$model_changes)")
fi

echo $result
