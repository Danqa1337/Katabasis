using Unity.Burst;
using Unity.Entities;
public partial struct EvaluateBehaviourJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int index, Entity entity, in EvaluationInputData data)
    {
        var bestScore = 0f;

        var currentScore = 0f;
        var currentBehaviour = AiBehaviourName.Wait;

        var bestBehaviour = AiBehaviourName.Wait;

        currentScore = new Behaviours.ChaseBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Chase;
        Compare();
        currentScore = new Behaviours.AttackBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Atack;
        Compare();
        currentScore = new Behaviours.FleeBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Flee;
        Compare();
        currentScore = new Behaviours.RoamBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Roam;
        Compare();
        currentScore = new Behaviours.StepBackBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.StepBack;
        Compare();
        currentScore = new Behaviours.StepAsideBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.StepAside;
        Compare();
        currentScore = new Behaviours.ShootBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Shoot;
        Compare();
        currentScore = new Behaviours.ReloadBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Reload;
        Compare();
        currentScore = new Behaviours.ThrowBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Throw;
        Compare();
        currentScore = new Behaviours.PicupItemBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Picup;
        Compare();
        currentScore = new Behaviours.FollowLeaderBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.Follow;
        Compare();
        currentScore = new Behaviours.ReturnToHomePointBehaviourStruct().Evaluate(data);
        currentBehaviour = AiBehaviourName.ReturnHome;
        Compare();
        currentScore = new Behaviours.GoToOrder().Evaluate(data);
        currentBehaviour = AiBehaviourName.GoToOrder;
        Compare();

        ecb.AddComponent(index, entity, new EvaluationResult(bestBehaviour));

        void Compare()
        {
            if (currentScore > bestScore)
            {
                bestBehaviour = currentBehaviour;
                bestScore = currentScore;
            }
        }
    }
}

public struct EvaluationResult : IComponentData
{
    public AiBehaviourName aiBehvaiour;

    public EvaluationResult(AiBehaviourName aiBehvaiour)
    {
        this.aiBehvaiour = aiBehvaiour;
    }
}