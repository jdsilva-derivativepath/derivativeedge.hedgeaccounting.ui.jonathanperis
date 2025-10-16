#!/bin/bash
export AWS_DEFAULT_REGION=us-west-2
export AWS_ROLE_ARN=$AWS_ROLE_STAGE
export AWS_WEB_IDENTITY_TOKEN_FILE=$(pwd)/web-identity-token
echo $BITBUCKET_STEP_OIDC_TOKEN > $(pwd)/web-identity-token
export FEATURE=${BITBUCKET_BRANCH////-}
export TAG=${FEATURE}-${BITBUCKET_BUILD_NUMBER}
# Add the AWS CodeArtifact NuGet repo
aws codeartifact login --tool dotnet --repository nuget --domain derivativepath --domain-owner 777791839250 --region us-west-2