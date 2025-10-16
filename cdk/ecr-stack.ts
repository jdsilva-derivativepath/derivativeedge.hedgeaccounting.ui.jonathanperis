import * as cdk from "aws-cdk-lib";
import * as iam from "aws-cdk-lib/aws-iam";
import * as ecr from "aws-cdk-lib/aws-ecr";
import { Construct } from 'constructs';

interface EnvProps {
  vpcName: string;
  albSubnetGroupName: string;
  ecsSubnetGroupName: string;
}

export class EcrStack extends cdk.Stack {
    readonly ecrRepo: ecr.Repository
    constructor(scope: Construct, id: string, props?: cdk.StackProps, envprops?: EnvProps) {
      super(scope, id, props);
        
  
    const repoNames = [
      'derivativeedge/hedgeaccountingui/app'
      // Add more repo names as needed
      //comment the old repo if you are creating new one
    ];

    const accountPrincipals = [
      'arn:aws:iam::340631980917:root',
      'arn:aws:iam::812512025548:root',
      'arn:aws:iam::777791839250:root',
    ];

    const ecrPolicy = new iam.PolicyStatement({
      sid: 'AllowPushPull',
      effect: iam.Effect.ALLOW,
      principals: accountPrincipals.map(arn => new iam.ArnPrincipal(arn)),
      actions: [
        'ecr:BatchCheckLayerAvailability',
        'ecr:BatchGetImage',
        'ecr:CompleteLayerUpload',
        'ecr:GetDownloadUrlForLayer',
        'ecr:InitiateLayerUpload',
        'ecr:PutImage',
        'ecr:UploadLayerPart',
      ],
    });

    repoNames.forEach((repoName, index) => {
      const repo = new ecr.Repository(this, `Repo${index}`, {
        repositoryName: repoName,
        imageTagMutability: ecr.TagMutability.MUTABLE,
        imageScanOnPush: true,
      });

      repo.addToResourcePolicy(ecrPolicy);
    });

    }
}