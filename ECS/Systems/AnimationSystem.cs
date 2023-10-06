using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum AnimationType
{
    PositionChange,
    Step,
    Flip,
    Butt,
    Flight,
}

[DisableAutoCreation]
public partial class AnimationSystem : MySystemBase
{
    private bool _frameDrawScheduled;

    public async Task MinorUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        var render = false;
        Entities.ForEach((Entity entity, Transform transform, ref DynamicBuffer<MinorAnimationElement> animationBuffer) =>
        {
            render = true;
            for (int i = 0; i < animationBuffer.Length; i++)
            {
                var currentAnimation = animationBuffer[i];
                transform.position = currentAnimation.nextTile.position.ToRealPosition();
            }
            ecb.RemoveComponent<MinorAnimationElement>(entity);
        }).WithoutBurst().Run();
        UpdateECB();
        if (render)
        {
            await Task.Delay(LowLevelSettings.instance.minorFrameDrawInterval);
            MainCameraHandler.MainCamera.Render();
        }
    }

    public async Task MajorUpdate()
    {
        int framesPerAnimation = LowLevelSettings.instance.framesPerAnimation;
        bool atLeastOneAnimationIsPlaying = false;

        Entities.ForEach((Entity entity, Transform transform, in DynamicBuffer<MajorAnimationElement> animationBuffer) =>
        {
            if (animationBuffer.Length > 1)
            {
                for (int i = 0; i < animationBuffer.Length - 1; i++)
                {
                    var currentAnimation = animationBuffer[i];

                    if (currentAnimation.animationType != AnimationType.Butt)
                    {
                        transform.position = currentAnimation.nextTileID.ToMapPosition().ToRealPosition(entity);
                    }
                }
            }
        }).WithoutBurst().Run(); ;

        var ecb = CreateEntityCommandBuffer();
        //draw last animation
        for (float currentFrameNum = 0; currentFrameNum < framesPerAnimation; currentFrameNum++)
        {
            Entities.ForEach((Entity entity, EntityAuthoring authoring, RendererComponent rendererComponent, ref DynamicBuffer<MajorAnimationElement> animationBuffer, in CurrentTileComponent currentTileComponent) =>
            {
                _frameDrawScheduled = true;
                var animationComponent = animationBuffer[animationBuffer.Length - 1];
                var transform = authoring.transform;
                var prevTile = animationComponent.prevTileID.ToTileData();
                var nextTile = animationComponent.nextTileID.ToTileData();

                if (prevTile == TileData.Null)
                {
                    nextTile = currentTileComponent.currentTileId.ToTileData();
                    prevTile = nextTile;
                }

                if (LowLevelSettings.instance.playAnimations && animationComponent.animationType != AnimationType.PositionChange
                    && (prevTile.visible || nextTile.visible))
                {
                    //if animation needed

                    var rendererTransform = rendererComponent.transform;
                    float2 vector = nextTile.position - prevTile.position;
                    float2 direction = vector.Normalize();
                    float2 step = vector * new float2(1f / framesPerAnimation, 1f / framesPerAnimation);
                    float2 newObjectPosition = transform.position.ToFloat2();
                    float2 newRendererPosition = rendererTransform.position.ToFloat2();
                    Vector3 newRotation = Vector3.zero;
                    Vector3 newRendererScale = rendererTransform.localScale;
                    float animationProgress = currentFrameNum / framesPerAnimation;

                    switch (animationComponent.animationType)
                    {
                        case AnimationType.Step:
                            newObjectPosition = transform.position.ToFloat2() + step;
                            newRendererPosition = newObjectPosition + new float2(0, LowLevelSettings.instance.MovementAnimation.Evaluate(animationProgress));
                            break;

                        case AnimationType.Butt:

                            newRendererPosition = transform.position.ToFloat2() + direction * LowLevelSettings.instance.ButtAnimation.Evaluate(animationProgress);

                            break;

                        case AnimationType.Flip:
                            newObjectPosition = transform.position.ToFloat2() + step;
                            //newRendererPosition = rendererTransform.position.ToFloat2() + step;
                            //newRotation = new Vector3(0f, 0f, (360 / framesPerAnimation));

                            break;

                        case AnimationType.Flight:
                            newObjectPosition = transform.position.ToFloat2() + step;

                            break;
                    }

                    transform.position = newObjectPosition.ToRealPosition();
                    rendererTransform.position = newRendererPosition.ToRealPosition();
                    rendererTransform.Rotate(newRotation);
                    rendererTransform.localScale = newRendererScale;

                    if (currentFrameNum + 1 == framesPerAnimation) //if this is last frame
                    {
                        rendererTransform.position = transform.position;

                        transform.position = entity.CurrentTile().position.ToRealPosition(entity);
                        ecb.RemoveComponent<MajorAnimationElement>(entity);
                    }
                    else
                    {
                        atLeastOneAnimationIsPlaying = true;
                    }
                }
                else
                {
                    transform.position = entity.CurrentTile().position.ToRealPosition(entity);
                }
            }).WithoutBurst().Run();

            if (atLeastOneAnimationIsPlaying)
            {
                atLeastOneAnimationIsPlaying = false;
                await Task.Delay(LowLevelSettings.instance.majorFrameDrawInterval);
            }
        }

        UpdateECB();
    }

    protected override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}

public struct MajorAnimationElement : IBufferElementData
{
    //public Entity entity;
    public int prevTileID;

    public int nextTileID;
    public AnimationType animationType;

    public MajorAnimationElement(TileData prevTile, TileData nextTile, AnimationType animationType)
    {
        // this.entity = entity;
        this.prevTileID = prevTile.index;
        this.nextTileID = nextTile.index;
        this.animationType = animationType;
    }
}

public struct MinorAnimationElement : IBufferElementData
{
    public TileData prevTile;
    public TileData nextTile;

    public MinorAnimationElement(TileData prevTile, TileData nextTile)
    {
        this.prevTile = prevTile;
        this.nextTile = nextTile;
    }
}