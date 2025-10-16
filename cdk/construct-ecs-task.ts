import { Construct } from "constructs";
import * as ecs from "aws-cdk-lib/aws-ecs";
import * as ecr from "aws-cdk-lib/aws-ecr";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import { PolicyStatement, Effect } from "aws-cdk-lib/aws-iam";

export interface StandardEcsTaskProps {
  cluster?: ecs.Cluster;
  subnets?: ec2.SubnetSelection;
  imageTag?: string;
  taskFamily?: string;
  region?: string;
  ecsCapacityProvider?: string;
  imageRepository?: string;
  environment?: {
    [key: string]: string;
  };
  secrets?: {
    [key: string]: ecs.Secret;
  };
  otelSecrets?: {
    [key: string]: ecs.Secret;
  };
  otelEnvironment?: {
    [key: string]: string;
  };     
}

export class StandardEcsTask extends Construct {
  public readonly container: ecs.ContainerDefinition;
  public readonly taskDefinition: ecs.TaskDefinition;
  public readonly service: ecs.FargateService;
  constructor(scope: Construct, id: string, props: StandardEcsTaskProps = {}) {
    super(scope, id);

    // Create ECS task definition
    this.taskDefinition = new ecs.FargateTaskDefinition(this, "taskdef", {
      family: props.taskFamily!,
      cpu: 2048,
      memoryLimitMiB: 4096,
    });

    const imageRepository = ecr.Repository.fromRepositoryAttributes(
      this,
      "image-repository",
      {
        repositoryName: props.imageRepository!,
        repositoryArn: `arn:aws:ecr:${props.region}:765057520137:repository/${props.imageRepository}`,
      }
    );

    var standardEnvironment = {
      ASPNETCORE_ENVIRONMENT: "Production",
      CONTAINER_TAG: props.imageTag!,
    };

    // Add container to task definition
    this.container = this.taskDefinition.addContainer(
      `${props.taskFamily}-container`,
      {
        image: ecs.ContainerImage.fromEcrRepository(
          imageRepository,
          props.imageTag
        ),
        memoryLimitMiB: 2048,
        logging: ecs.LogDrivers.awsLogs({ streamPrefix: props.taskFamily! }),
        secrets: props.secrets,
        environment: Object.assign({}, standardEnvironment, props.environment),
      }
    );

    const collectorContainer = this.taskDefinition.addContainer('OpenTelemetryCollector', {

        containerName: 'OpenTelemetryCollector',
        image: ecs.ContainerImage.fromRegistry('otel/opentelemetry-collector-contrib:0.122.1'),
        //NOTE: config is coming from environment variable OTEL_CONFIG, which is sourced from Secrets Manager
          command: ['--config=env:OTEL_CONFIG'],
        logging: ecs.LogDrivers.awsLogs({ streamPrefix: 'OpenTelemetryCollector' }),
        memoryLimitMiB: 2048,
        secrets: props.otelSecrets,
        environment: Object.assign({}, standardEnvironment, props.otelEnvironment) ,
        portMappings: [
            { containerPort: 4317, hostPort: 4317, protocol: ecs.Protocol.TCP, appProtocol: ecs.AppProtocol.grpc, name: 'grpc' },
        ],
      });    

    // Map the port to the container
    this.container.addPortMappings({
      containerPort: 8080,
    });

    //makes sure no task running on DR region i.e N.Virginia(us-east-1)
    const count = props.region === "us-east-1" ? 0 : 1;

    // Instantiate an Amazon ECS Service
    this.service = new ecs.FargateService(this, "hedgeaccountingui-svc", {
      cluster: props.cluster!,
      taskDefinition: this.taskDefinition,
      circuitBreaker: { rollback: true },
      assignPublicIp: false,
      vpcSubnets: props.subnets,
      desiredCount: count,
      capacityProviderStrategies: [
        { capacityProvider: props.ecsCapacityProvider!, weight: 1 },
      ],
    });

    // Add inline policy to task definition role
    this.taskDefinition.addToTaskRolePolicy(
      new PolicyStatement({
        effect: Effect.ALLOW,
        actions: [
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchGetImage",
          "ecr:BatchCheckLayerAvailability",
          "ecr:GetAuthorizationToken",
        ],
        resources: [
          `arn:aws:ecr:${props.region}:765057520137:repository/${props.imageRepository}`,
        ],
      })
    );
  }
}
