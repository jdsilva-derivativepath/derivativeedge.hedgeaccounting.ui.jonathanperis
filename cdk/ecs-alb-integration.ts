// ecs-alb-integration.ts
import * as elbv2 from "aws-cdk-lib/aws-elasticloadbalancingv2";
import * as ecs from "aws-cdk-lib/aws-ecs";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import { Construct } from 'constructs';

interface EcsAlbIntegrationProps {
  vpc: ec2.IVpc;
  ecsService: ecs.FargateService;
  hostHeader: string;
  priority: number;
}

export class EcsAlbIntegration extends Construct {
  constructor(scope: Construct, id: string, props: EcsAlbIntegrationProps) {
    super(scope, id);

    const listener = elbv2.ApplicationListener.fromLookup(this, 'ALBListener', {
      loadBalancerTags: { Name: 'caar'},
      listenerProtocol: elbv2.ApplicationProtocol.HTTPS,
      listenerPort: 443,
    });

    const targetGroup = new elbv2.ApplicationTargetGroup(this, 'EcsTargetGroup', {
      targetGroupName: `${id}`,
      vpc: props.vpc,
      protocol: elbv2.ApplicationProtocol.HTTP,
      port: 8080,
      targetType: elbv2.TargetType.IP,
      healthCheck: { path: "/health" },
    });

    targetGroup.addTarget(props.ecsService);

    new elbv2.ApplicationListenerRule(this, 'ListenerRule', {
      listener: listener,
      priority: props.priority,
      conditions: [
        elbv2.ListenerCondition.hostHeaders([props.hostHeader])
      ],
      action: elbv2.ListenerAction.forward([targetGroup]),
    });
  }
}
