#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import { CdkStack } from "./cdk-stack";
import { EcrStack } from "./ecr-stack";

const app = new cdk.App();

const dev = new CdkStack(app, "dev-hedge-accounting-ui", {
  env: {
    account: "812512025548",
    region: "us-west-2"
  },
  },
  {
    vpcName: "DEV DPI",
    albSubnetGroupName: "alb-group",
    ecsCapacityProvider: "FARGATE_SPOT",
    appHeader: "dev-hedgeaccountingui.derivativepath.com",
    auth0Domain: "dev-derivativepath.us.auth0.com",
    auth0ClientId: "Sp5rJy4eUqPJtbzV3sNUzKjzWbzV5bLl",
    ecsSubnetGroupName: "ecs-group",
    hedgeAccountingServiceApi: "https://dev-ha.derivativepath.com",
    identityApiBase: "https://dev-identity.derivativepath.com",
    authServiceUrl: "https://dev-apiauth.derivativepath.com/",
    splitioConnectionTimeout: "10000",
    authUrl: "https://dev-apiauth.derivativepath.com/authorize",
    notificationHubBaseUrl: "https://dev-notification.derivativepath.com/",
    swapzillaBaseAddress: "https://devportal.derivativepath.com/",
    environment: "dev",
  }
);

const stg = new CdkStack(app, "stg-hedge-accounting-ui", {
  env: {
    account: "777791839250",
    region: "us-west-2"
  },
  },
  {
    vpcName: "STG DPI",
    ecsCapacityProvider: "FARGATE_SPOT",
    albSubnetGroupName: "alb-group",
    appHeader: "stg-hedgeaccountingui.derivativepath.com",
    auth0Domain: "stg-derivativepath.us.auth0.com",
    auth0ClientId: "hUgTXYoOOofHQjbHYanOa6FcUfVxJ1zk",
    ecsSubnetGroupName: "ecs-group",
    hedgeAccountingServiceApi: "https://stg-hedgeaccounting.derivativepath.com",
    identityApiBase: "https://stg-identity.derivativepath.com",
    authServiceUrl: "https://stg-apiauth.derivativepath.com/",
    splitioConnectionTimeout: "10000",
    authUrl: "https://stg-apiauth.derivativepath.com/authorize",
    notificationHubBaseUrl: "https://stg-notification.derivativepath.com/",
    swapzillaBaseAddress: "https://stagingportal.derivativepath.com/",
    environment: "stg",    
  }
);

const prod = new CdkStack(app, 'prod-hedge-accounting-ui', {
  env: {
    account: "340631980917",
    region: "us-west-2"
  },
  },
  {
    vpcName: "DPI PROD",
    albSubnetGroupName: "alb-group",
    ecsCapacityProvider: "FARGATE",
    appHeader: "hedgeaccountingui.derivativepath.com",
    auth0Domain: "derivativepath.us.auth0.com",
    auth0ClientId: "zGcZCctd8IdaIXsT4suKat8FrFLe1v4E",
    ecsSubnetGroupName: "ecs-group",
    hedgeAccountingServiceApi: "https://hedgeaccounting.derivativepath.com",
    identityApiBase: "https://identity.derivativepath.com",
    authServiceUrl: "https://apiauth.derivativepath.com/",
    splitioConnectionTimeout: "10000",
    authUrl: "https://apiauth.derivativepath.com/authorize",
    notificationHubBaseUrl: "https://notification.derivativepath.com/",
    swapzillaBaseAddress: "https://portal.derivativepath.com/",
    environment: "prd",    
  }
);

const dr_prod = new CdkStack(app, 'dr-prod-hedge-accounting-ui', {
  env: {
    account: "340631980917",
    region: "us-east-1"
  },
  },
  {
    vpcName: "DR DPI VPC",
    albSubnetGroupName: "alb-group",
    ecsCapacityProvider: "FARGATE",
    appHeader: "dr-hedgeaccountingui.derivativepath.com",
    auth0Domain: "derivativepath.us.auth0.com",
    auth0ClientId: "zGcZCctd8IdaIXsT4suKat8FrFLe1v4E",
    ecsSubnetGroupName: "ecs-group",
    hedgeAccountingServiceApi: "https://dr-hedgeaccounting.derivativepath.com",
    identityApiBase: "https://dr-identity.derivativepath.com",
    authServiceUrl: "https://dr-apiauth.derivativepath.com/",
    splitioConnectionTimeout: "10000",
    authUrl: "https://dr-apiauth.derivativepath.com/authorize",
    notificationHubBaseUrl: "https://dr-notification.derivativepath.com/",
    swapzillaBaseAddress: "https://drportal.derivativepath.com/",
    environment: "dr",    
  }
);

const ecr = new EcrStack(app, "hedge-accounting-ui-ecr", {
  env: {
    region: "us-west-2",
    account: "765057520137",
  },
});

const ecrprod = new EcrStack(app, "prd-hedge-accounting-ui-ecr", {
  env: {
    region: "us-west-2",
    account: "340631980917",
  }
})

//AWS Tagging
cdk.Tags.of(app).add("application", "hedge-accounting-ui");
cdk.Tags.of(app).add("team", "caar");
cdk.Tags.of(dev).add("environment", "development");
cdk.Tags.of(stg).add("environment", "staging");
cdk.Tags.of(prod).add("environment", "production");
cdk.Tags.of(dr_prod).add("environment", "production-dr");

//AWS Unilink
cdk.Tags.of(app).add("hedge-accounting-ui", "unilink");
cdk.Tags.of(app).add("hedge-accounting", "unilink");