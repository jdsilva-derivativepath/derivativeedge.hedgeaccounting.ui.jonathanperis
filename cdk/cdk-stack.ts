import * as cdk from "aws-cdk-lib";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import * as ecs from "aws-cdk-lib/aws-ecs";
import * as iam from "aws-cdk-lib/aws-iam";
import * as sm from "aws-cdk-lib/aws-secretsmanager";
import { StandardEcsTask } from "./construct-ecs-task";
import { EcsAlbIntegration } from "./ecs-alb-integration";
import { Construct } from 'constructs';

interface EnvProps {
  vpcName: string;
  albSubnetGroupName: string;
  ecsSubnetGroupName: string;
  hedgeAccountingServiceApi: string;
  identityApiBase: string;
  auth0Domain: string;
  auth0ClientId: string;
  ecsCapacityProvider: string;
  appHeader: string;
  authServiceUrl: string;
  splitioConnectionTimeout: string;
  authUrl: string;
  notificationHubBaseUrl: string;
  swapzillaBaseAddress: string;
  environment: string;
}

export class CdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps, envprops?: EnvProps) {
    super(scope, id, props);

    // Import a reference to the VPC based on name
    const vpc = ec2.Vpc.fromLookup(this, "edge-vpc", { vpcName: envprops?.vpcName, subnetGroupNameTag: "subnet-group-name" });
    const albSubnets = vpc.selectSubnets({ subnetGroupName: envprops?.albSubnetGroupName });
    const ecsSubnets = vpc.selectSubnets({ subnetGroupName: envprops?.ecsSubnetGroupName });

    // Create an ECS cluster
    const cluster = new ecs.Cluster(this, "hedgeaccountingui", {
      vpc,
      clusterName: `edge-hedge-accounting-ui`
    });
  
    // Import secrets. Secret Manager is where we want to keep connection strings and other sensitive environment variables
    const identityApiKey = sm.Secret.fromSecretAttributes(this,"identityApiKey",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/identityapikey`,
    });

    const jwtIssuerSigningKey = sm.Secret.fromSecretAttributes(this,"jwtIssuerSigningKey",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/edgeproxyjwtsecuritykey`,
    });

    const jwtAudience = sm.Secret.fromSecretAttributes(this,"jwtAudience",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/jwtAudience`,
    });

    const jwtIssuer = sm.Secret.fromSecretAttributes(this,"jwtIssuer",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/jwtIssuer`,
    });

    const swapzillaApiKey = sm.Secret.fromSecretAttributes(this, "swapzillaApiKey", { // Add this only
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/swapzillaApiKey`,
    });

    const splitioApiKey = sm.Secret.fromSecretAttributes(this,"splitioApiKey",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/splitapikey`,
    });
  
    const hedgeaccountingUiSecret = sm.Secret.fromSecretAttributes(this,"hedgeAccountingApiKey",{
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/hedgeaccountingui/secrets`,
    })

    const otelConfig = sm.Secret.fromSecretAttributes(this, "corelogixotelConfig", {
      secretPartialArn: `arn:aws:secretsmanager:${this.region}:${this.account}:secret:edge/corelogix/otelConfig`,
    });

  // Get the tag from the arguments
    const imageTag = new cdk.CfnParameter(this, "imageTag", {
      type: "String",
      description: "The docker image tag to deploy (DE-4728-20)"
    })
    const tag = imageTag.valueAsString;
  
    
    const task = new StandardEcsTask(this, `hedge-accounting-app-${this.region}`, {
      cluster: cluster,
      subnets: ecsSubnets,
      imageTag: tag,
      taskFamily: "hedgeaccounting-ui",
      region: this.region,
      ecsCapacityProvider: envprops?.ecsCapacityProvider!,
      imageRepository: "derivativeedge/hedgeaccountingui/app",
      otelSecrets:
      {
          OTEL_CONFIG: ecs.Secret.fromSecretsManager(otelConfig),
      },
      otelEnvironment:
      {
          CORALOGIX_SERVICE_NAME: envprops?.environment! + "-hedgeaccounting-ui",
          CORALOGIX_ENVIRONMENT: envprops?.environment!,
      },      
      secrets: {
          IDENTITY_API_KEY: ecs.Secret.fromSecretsManager(identityApiKey),
          JWT_ISSUERSIGNINGKEY: ecs.Secret.fromSecretsManager(jwtIssuerSigningKey),
          JWT_AUDIENCE: ecs.Secret.fromSecretsManager(jwtAudience),
          JWT_ISSUER: ecs.Secret.fromSecretsManager(jwtIssuer),
          SPLITIO_API_KEY: ecs.Secret.fromSecretsManager(splitioApiKey),
          HEDGE_ACCOUNTING_SERVICE_API_KEY: ecs.Secret.fromSecretsManager(hedgeaccountingUiSecret,"hedgeAccountingApiKey"),
          HEDGE_ACCOUNTING_SERVICE_CLIENTID: ecs.Secret.fromSecretsManager(hedgeaccountingUiSecret,"hedgeAccountingClientId"),
          HEDGE_ACCOUNTING_SERVICE_CLIENTSECRET: ecs.Secret.fromSecretsManager(hedgeaccountingUiSecret,"hedgeAccountingClientSecret"),
          AUTH_CLIENTID_API: ecs.Secret.fromSecretsManager(hedgeaccountingUiSecret,"authClientId"),
          AUTH_CLIENTSECRET_API: ecs.Secret.fromSecretsManager(hedgeaccountingUiSecret,"authClientSecret"),
          SWAPZILLA_APIKEY_USER: ecs.Secret.fromSecretsManager(swapzillaApiKey,"user"),
          SWAPZILLA_APIKEY_PASSWORD: ecs.Secret.fromSecretsManager(swapzillaApiKey,"password")  
      },
      environment: {
        AUTH0_DOMAIN: envprops?.auth0Domain!,
        AUTH0_CLIENTID: envprops?.auth0ClientId!,
        IDENTITY_API_BASE: envprops?.identityApiBase!,
        HEDGE_ACCOUNTING_SERVICE_URL: envprops?.hedgeAccountingServiceApi!,
        AUTH_SERVICE_URL: envprops?.authServiceUrl!,
        SPLITIO_CONNECTION_TIMEOUT: envprops?.splitioConnectionTimeout!,
        AUTH_URL: envprops?.authUrl!,
        NOTIFICATION_HUB_BASE_URL: envprops?.notificationHubBaseUrl!,
        SWAPZILLA_BASE_ADDRESS: envprops?.swapzillaBaseAddress!,
        OTEL_SERVICE_NAME: envprops?.environment! + "-hedgeaccounting-ui",
        OTEL_ENDPOINT: "http://localhost:4317",           
      },
    });
    
    // Create Application Load Balancer Target Group 
    const appTG = new EcsAlbIntegration(this, "hedgeaccounting-ui-tg", {
      vpc: vpc,
      ecsService: task.service,
      hostHeader: envprops?.appHeader!,
      priority: 2,//random unique number (ref:ALB)
    });
  }
}