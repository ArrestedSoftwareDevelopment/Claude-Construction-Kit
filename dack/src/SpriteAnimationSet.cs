using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record SpriteFrame(Texture2D Texture, Rect2 SourceRegion, Vector2 DisplaySize);
public readonly record struct AnimationFrameRange(int Start, int End);

public sealed class SpriteAnimationSet
{
    private const string StickmanV1Root = "res://assets/third_party/stickman-pack-v0.1";
    private const string StickmanV2Root = "res://assets/third_party/stickman-pack-v0.2";
    private const string DungeonRoot = "res://assets/third_party/8-bit-dungeon";
    private const string SunnyDragonRelativePath = "raw base assets/Legacy Collection/Legacy Collection/Assets/Misc/Characters/sunny-dragon/spritesheets/sunny-dragon-fly.png";
    private const string TgcPlatformerRuntimePath = "res://assets/project/game-creators-pack/platformer-spritesheet.png";
    private const string TgcShooterBossRuntimePath = "res://assets/project/game-creators-pack/shooter-boss-sprite.png";
    private const string TgcShooterRuntimePath = "res://assets/project/game-creators-pack/shooter-spritesheet.png";
    private const string BattleFleetShip01RelativePath = "raw base assets/Legacy Collection/Legacy Collection/Assets/Warped/Characters/top-down-shooter-ship/spritesheets/red/ship-01.png";
    private static readonly AnimationFrameRange DefaultGameCreatorIdle = new(0, 2);
    private static readonly AnimationFrameRange DefaultGameCreatorRun = new(3, 14);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpUp = new(15, 15);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpDown = new(16, 16);
    private static readonly AnimationFrameRange DefaultGameCreatorFall = new(16, 16);
    private static readonly AnimationFrameRange DefaultGameCreatorRunShoot = new(9, 10);
    private static readonly AnimationFrameRange DefaultGameCreatorJumpShoot = new(11, 12);
    private static readonly AnimationFrameRange DefaultGameCreatorDeath = new(16, 17);

    private readonly Dictionary<ActorMotionState, SpriteFrame[]> _frames;
    private delegate bool SpriteFrameLoader(out SpriteFrame[] frames);

    private SpriteAnimationSet(Dictionary<ActorMotionState, SpriteFrame[]> frames)
    {
        _frames = frames;
    }

    public static SpriteAnimationSet? TryLoadStickman()
    {
        if (!TryLoadStickmanFrames(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(6, 14),
            new AnimationFrameRange(15, 15),
            new AnimationFrameRange(16, 16),
            new AnimationFrameRange(16, 16),
            new AnimationFrameRange(6, 14),
            new AnimationFrameRange(15, 15),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadStickmanV2()
    {
        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        AddGridFrames(frames, ActorMotionState.Idle, $"{StickmanV2Root}/thin-idle.png", 64);
        AddGridFrames(frames, ActorMotionState.Run, $"{StickmanV2Root}/run.png", 64);
        AddGridFrames(frames, ActorMotionState.JumpUp, $"{StickmanV2Root}/jump-up.png", 64);
        AddGridFrames(frames, ActorMotionState.JumpDown, $"{StickmanV2Root}/jump-down.png", 64);
        AddGridFrames(frames, ActorMotionState.Fall, $"{StickmanV2Root}/jump-down.png", 64);
        AddGridFrames(frames, ActorMotionState.RunShoot, $"{StickmanV2Root}/punch.png", 64);
        AddGridFrames(frames, ActorMotionState.JumpShoot, $"{StickmanV2Root}/punch.png", 64);
        AddGridFrames(frames, ActorMotionState.Punch, $"{StickmanV2Root}/punch.png", 64);
        AddGridFrames(frames, ActorMotionState.Death, $"{StickmanV2Root}/death.png", 64);

        if (!frames.ContainsKey(ActorMotionState.Idle))
            return null;

        foreach (ActorMotionState state in Enum.GetValues<ActorMotionState>())
        {
            if (!frames.ContainsKey(state))
                frames[state] = frames[ActorMotionState.Idle];
        }

        frames[ActorMotionState.Crawl] = frames.TryGetValue(ActorMotionState.Run, out SpriteFrame[]? run)
            ? run
            : frames[ActorMotionState.Idle];

        return new SpriteAnimationSet(frames);
    }

    public static SpriteAnimationSet? TryLoadDungeonRunner()
    {
        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        AddSingleFileFrames(frames, ActorMotionState.Idle, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-idle.png");
        AddSingleFileFrames(frames, ActorMotionState.Run,
            new FrameImportOptions(RecolorNearWhite: true),
            $"{DungeonRoot}/player-run-01.png",
            $"{DungeonRoot}/player-run-02.png",
            $"{DungeonRoot}/player-run-03.png",
            $"{DungeonRoot}/player-run-04.png"
        );
        AddSingleFileFrames(frames, ActorMotionState.Crawl,
            new FrameImportOptions(RecolorNearWhite: true),
            $"{DungeonRoot}/player-climb-01.png",
            $"{DungeonRoot}/player-climb-02.png"
        );
        AddSingleFileFrames(frames, ActorMotionState.JumpUp, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-fall.png");
        AddSingleFileFrames(frames, ActorMotionState.JumpDown, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-fall.png");
        AddSingleFileFrames(frames, ActorMotionState.Fall, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-fall.png");
        AddSingleFileFrames(frames, ActorMotionState.Punch, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-rope-01.png");
        AddSingleFileFrames(frames, ActorMotionState.RunShoot,
            new FrameImportOptions(RecolorNearWhite: true),
            $"{DungeonRoot}/player-rope-01.png",
            $"{DungeonRoot}/player-rope-02.png",
            $"{DungeonRoot}/player-rope-03.png"
        );
        AddSingleFileFrames(frames, ActorMotionState.JumpShoot, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-rope-02.png");
        AddSingleFileFrames(frames, ActorMotionState.Death, new FrameImportOptions(RecolorNearWhite: true), $"{DungeonRoot}/player-fall.png");

        if (!frames.ContainsKey(ActorMotionState.Idle))
            return null;

        foreach (ActorMotionState state in Enum.GetValues<ActorMotionState>())
        {
            if (!frames.ContainsKey(state))
                frames[state] = frames[ActorMotionState.Idle];
        }

        return new SpriteAnimationSet(frames);
    }

    public static SpriteAnimationSet? TryLoadStickman(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadStickmanFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                idle,
                run,
                jumpUp,
                jumpDown,
                fall,
                runShoot,
                jumpShoot,
                death,
                idlePingPong,
                runPingPong,
                jumpUpPingPong,
                jumpDownPingPong,
                fallPingPong,
                runShootPingPong,
                jumpShootPingPong,
                deathPingPong
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer()
    {
        return TryLoadGameCreatorPlayer(
            DefaultGameCreatorIdle,
            DefaultGameCreatorRun,
            DefaultGameCreatorJumpUp,
            DefaultGameCreatorJumpDown,
            DefaultGameCreatorFall,
            DefaultGameCreatorRunShoot,
            DefaultGameCreatorJumpShoot,
            DefaultGameCreatorDeath,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcPlatformerEnemy(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange crawl
    )
    {
        if (!TryLoadBlobFrames(TgcPlatformerRuntimePath, out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            idle,
            idle,
            idle,
            run,
            idle,
            idle,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        ).WithCrawl(all, crawl);
    }

    public static SpriteAnimationSet? TryLoadTgcOrangeWorker()
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcOrangeWorkerFrames,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(6, 11),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcRedRunner()
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcRedRunnerFrames,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(7, 12),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcBlueGuard()
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcBlueGuardFrames,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            new AnimationFrameRange(0, 4),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcGreenCrawler()
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcGreenCrawlerFrames,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcGreenSnake()
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcGreenSnakeFrames,
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            new AnimationFrameRange(0, 5),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadTgcOrangeWorker(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcOrangeWorkerFrames,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    public static SpriteAnimationSet? TryLoadTgcRedRunner(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcRedRunnerFrames,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    public static SpriteAnimationSet? TryLoadTgcBlueGuard(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcBlueGuardFrames,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    public static SpriteAnimationSet? TryLoadTgcGreenCrawler(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadTgcFixedPlatformerEnemy(
            TryLoadTgcGreenCrawlerFrames,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    private static SpriteAnimationSet? TryLoadTgcFixedPlatformerEnemy(
        SpriteFrameLoader loader,
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        if (!loader(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        ).WithCrawl(all, run);
    }

    public static bool TryLoadTgcOrangeWorkerFrames(out SpriteFrame[] frames)
    {
        Rect2[] rects =
        [
            new Rect2(0, 0, 14, 16),
            new Rect2(15, 0, 14, 16),
            new Rect2(30, 0, 14, 16),
            new Rect2(45, 0, 15, 16),
            new Rect2(61, 0, 13, 16),
            new Rect2(75, 0, 13, 16),
            new Rect2(0, 16, 14, 16),
            new Rect2(15, 16, 16, 16),
            new Rect2(32, 16, 15, 16),
            new Rect2(48, 16, 16, 16),
            new Rect2(65, 16, 13, 16),
            new Rect2(79, 16, 14, 16)
        ];

        return TryLoadFixedSourceFrames(TgcPlatformerRuntimePath, rects, out frames);
    }

    public static bool TryLoadTgcRedRunnerFrames(out SpriteFrame[] frames)
    {
        Rect2[] rects =
        [
            new Rect2(113, 0, 14, 16),
            new Rect2(128, 0, 14, 16),
            new Rect2(143, 0, 14, 16),
            new Rect2(158, 0, 14, 16),
            new Rect2(173, 0, 14, 16),
            new Rect2(188, 0, 14, 16),
            new Rect2(203, 0, 14, 16),
            new Rect2(113, 16, 14, 16),
            new Rect2(128, 16, 13, 16),
            new Rect2(142, 16, 16, 16),
            new Rect2(159, 16, 16, 16),
            new Rect2(176, 16, 16, 16),
            new Rect2(193, 16, 15, 16)
        ];

        return TryLoadFixedSourceFrames(TgcPlatformerRuntimePath, rects, out frames);
    }

    public static bool TryLoadTgcBlueGuardFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobIndexFrames(TgcPlatformerRuntimePath, [28, 29, 30, 31, 32, 33, 34, 40, 41, 42, 43, 44, 51], out frames);
    }

    public static bool TryLoadTgcGreenCrawlerFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobIndexFrames(TgcPlatformerRuntimePath, [54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66], out frames);
    }

    public static bool TryLoadTgcGreenSnakeFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobIndexFrames(TgcPlatformerRuntimePath, [61, 62, 63, 64, 65, 66], out frames);
    }

    public static SpriteAnimationSet? TryLoadTgcPlatformerEnemy(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        AnimationFrameRange crawl,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        if (!TryLoadBlobFrames(TgcPlatformerRuntimePath, out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        ).WithCrawl(all, crawl);
    }

    public static SpriteAnimationSet? TryLoadTgcShooterBoss()
    {
        return TryLoadSingleSprite(TgcShooterBossRuntimePath);
    }

    public static bool TryLoadTgcShooterBossFrames(out SpriteFrame[] frames)
    {
        return TryLoadSingleSpriteFrames(TgcShooterBossRuntimePath, out frames);
    }

    public static SpriteAnimationSet? TryLoadTgcShooterFleet()
    {
        return TryLoadTgcShooterFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, Mathf.Min(5, all.Length - 1)),
                new AnimationFrameRange(0, 0),
                new AnimationFrameRange(0, 0),
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadTgcShooterFleet(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        if (!TryLoadTgcShooterFrames(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    public static SpriteAnimationSet? TryLoadSunnyDragon()
    {
        return TryLoadSunnyDragonFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                new AnimationFrameRange(0, all.Length - 1),
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadSunnyDragon(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        return TryLoadSunnyDragonFrames(out SpriteFrame[] all)
            ? BuildAnimationSetFromFrames(
                all,
                idle,
                run,
                jumpUp,
                jumpDown,
                fall,
                runShoot,
                jumpShoot,
                death,
                idlePingPong,
                runPingPong,
                jumpUpPingPong,
                jumpDownPingPong,
                fallPingPong,
                runShootPingPong,
                jumpShootPingPong,
                deathPingPong
            )
            : null;
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown
    )
    {
        return TryLoadGameCreatorPlayer(
            idle,
            run,
            jumpUp,
            jumpDown,
            DefaultGameCreatorFall,
            DefaultGameCreatorRunShoot,
            DefaultGameCreatorJumpShoot,
            DefaultGameCreatorDeath,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        );
    }

    public static SpriteAnimationSet? TryLoadGameCreatorPlayer(
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        if (!TryLoadGameCreatorPlayerFrames(out SpriteFrame[] all))
            return null;

        return BuildAnimationSetFromFrames(
            all,
            idle,
            run,
            jumpUp,
            jumpDown,
            fall,
            runShoot,
            jumpShoot,
            death,
            idlePingPong,
            runPingPong,
            jumpUpPingPong,
            jumpDownPingPong,
            fallPingPong,
            runShootPingPong,
            jumpShootPingPong,
            deathPingPong
        );
    }

    private static SpriteAnimationSet BuildAnimationSetFromFrames(
        SpriteFrame[] all,
        AnimationFrameRange idle,
        AnimationFrameRange run,
        AnimationFrameRange jumpUp,
        AnimationFrameRange jumpDown,
        AnimationFrameRange fall,
        AnimationFrameRange runShoot,
        AnimationFrameRange jumpShoot,
        AnimationFrameRange death,
        bool idlePingPong,
        bool runPingPong,
        bool jumpUpPingPong,
        bool jumpDownPingPong,
        bool fallPingPong,
        bool runShootPingPong,
        bool jumpShootPingPong,
        bool deathPingPong
    )
    {
        Dictionary<ActorMotionState, SpriteFrame[]> frames = [];
        frames[ActorMotionState.Idle] = SliceFrames(all, idle, idlePingPong);
        frames[ActorMotionState.Run] = SliceFrames(all, run, runPingPong);
        frames[ActorMotionState.Crawl] = frames[ActorMotionState.Run];
        frames[ActorMotionState.JumpUp] = SliceFrames(all, jumpUp, jumpUpPingPong);
        frames[ActorMotionState.JumpDown] = SliceFrames(all, jumpDown, jumpDownPingPong);
        frames[ActorMotionState.Fall] = SliceFrames(all, fall, fallPingPong);
        frames[ActorMotionState.RunShoot] = SliceFrames(all, runShoot, runShootPingPong);
        frames[ActorMotionState.JumpShoot] = SliceFrames(all, jumpShoot, jumpShootPingPong);
        frames[ActorMotionState.Punch] = frames[ActorMotionState.RunShoot];
        frames[ActorMotionState.Death] = SliceFrames(all, death, deathPingPong);

        return new SpriteAnimationSet(frames);
    }

    public static int GetGameCreatorPlayerFrameCount()
    {
        string filePath = GameCreatorPlayerPath();

        if (!File.Exists(filePath))
            return 0;

        Image strip = Image.LoadFromFile(filePath);
        if (strip.IsEmpty())
            return 0;

        strip.Convert(Image.Format.Rgba8);
        return DetectGameCreatorPlayerFrames(strip).Length;
    }

    public static bool TryLoadGameCreatorPlayerFramePreview(out Texture2D? texture, out Rect2[] frames)
    {
        texture = null;
        frames = [];
        string filePath = GameCreatorPlayerPath();

        if (!File.Exists(filePath))
            return false;

        Image strip = Image.LoadFromFile(filePath);
        if (strip.IsEmpty())
            return false;

        strip.Convert(Image.Format.Rgba8);
        frames = DetectGameCreatorPlayerFrames(strip);
        if (frames.Length == 0)
            return false;

        texture = ImageTexture.CreateFromImage(strip);
        return true;
    }

    public static bool TryLoadGameCreatorPlayerFrames(out SpriteFrame[] frames)
    {
        frames = [];
        if (!TryLoadGameCreatorPlayerFramePreview(out Texture2D? texture, out Rect2[] rects) || texture is null)
            return false;

        Vector2 displaySize = GetCommonDisplaySize(rects);
        frames = new SpriteFrame[rects.Length];
        for (int i = 0; i < rects.Length; i++)
            frames[i] = new SpriteFrame(texture, rects[i], displaySize);

        return frames.Length > 0;
    }

    private static Rect2[] DetectGameCreatorPlayerFrames(Image image)
    {
        return DetectBlobFrames(
            image,
            minOpaquePixelsOverride: 180,
            minHeightOverride: 30,
            minWidthOverride: 16
        );
    }

    public static bool TryLoadTgcPlatformerFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobFrames(TgcPlatformerRuntimePath, out frames);
    }

    public static bool TryLoadTgcShooterFrames(out SpriteFrame[] frames)
    {
        return TryLoadBlobFrames(TgcShooterRuntimePath, out frames);
    }

    public static SpriteAnimationSet? TryLoadBattleFleetRedShip01()
    {
        if (!TryLoadHorizontalRawStripFrames(BattleFleetShip01RelativePath, 5, out SpriteFrame[] frames))
            return null;

        Dictionary<ActorMotionState, SpriteFrame[]> states = [];
        foreach (ActorMotionState state in Enum.GetValues<ActorMotionState>())
            states[state] = frames;

        return new SpriteAnimationSet(states);
    }

    public static bool TryLoadBattleFleetRedShip01Frames(out SpriteFrame[] frames)
    {
        return TryLoadHorizontalRawStripFrames(BattleFleetShip01RelativePath, 5, out frames);
    }

    public static bool TryLoadStickmanFrames(out SpriteFrame[] frames)
    {
        List<SpriteFrame> loaded = [];
        AppendFrames(loaded, $"{StickmanV1Root}/thin-idle-sheet.png");
        AppendFrames(loaded, $"{StickmanV1Root}/thin-run-sheet.png");
        AppendFrames(loaded, $"{StickmanV1Root}/thin-jump-up.png");
        AppendFrames(loaded, $"{StickmanV1Root}/thin-jump-down.png");
        frames = loaded.ToArray();
        return frames.Length > 0;
    }

    public static bool TryLoadStickmanV2Frames(out SpriteFrame[] frames)
    {
        List<SpriteFrame> loaded = [];
        AppendGridFrames(loaded, $"{StickmanV2Root}/thin-idle.png", 64);
        AppendGridFrames(loaded, $"{StickmanV2Root}/run.png", 64);
        AppendGridFrames(loaded, $"{StickmanV2Root}/jump-up.png", 64);
        AppendGridFrames(loaded, $"{StickmanV2Root}/jump-down.png", 64);
        AppendGridFrames(loaded, $"{StickmanV2Root}/punch.png", 64);
        AppendGridFrames(loaded, $"{StickmanV2Root}/death.png", 64);
        frames = loaded.ToArray();
        return frames.Length > 0;
    }

    public static bool TryLoadDungeonRunnerFrames(out SpriteFrame[] frames)
    {
        List<SpriteFrame> loaded = [];
        FrameImportOptions dungeonOptions = new(RecolorNearWhite: true);
        AppendSingleFileFrames(loaded, dungeonOptions, $"{DungeonRoot}/player-idle.png");
        AppendSingleFileFrames(loaded,
            dungeonOptions,
            $"{DungeonRoot}/player-run-01.png",
            $"{DungeonRoot}/player-run-02.png",
            $"{DungeonRoot}/player-run-03.png",
            $"{DungeonRoot}/player-run-04.png"
        );
        AppendSingleFileFrames(loaded,
            dungeonOptions,
            $"{DungeonRoot}/player-climb-01.png",
            $"{DungeonRoot}/player-climb-02.png"
        );
        AppendSingleFileFrames(loaded, dungeonOptions, $"{DungeonRoot}/player-fall.png");
        AppendSingleFileFrames(loaded,
            dungeonOptions,
            $"{DungeonRoot}/player-rope-01.png",
            $"{DungeonRoot}/player-rope-02.png",
            $"{DungeonRoot}/player-rope-03.png"
        );
        frames = loaded.ToArray();
        return frames.Length > 0;
    }

    public static bool TryLoadSunnyDragonFrames(out SpriteFrame[] frames)
    {
        frames = [];
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string filePath = Path.GetFullPath(Path.Combine(projectRoot, "..", SunnyDragonRelativePath));
        if (!File.Exists(filePath))
            return false;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return false;

        sheet.Convert(Image.Format.Rgba8);
        int frameCount = 9;
        int frameWidth = sheet.GetWidth() / frameCount;
        int frameHeight = sheet.GetHeight();
        if (frameWidth <= 0 || frameHeight <= 0)
            return false;

        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        frames = new SpriteFrame[frameCount];
        Vector2 displaySize = new(frameWidth, frameHeight);
        for (int i = 0; i < frameCount; i++)
            frames[i] = new SpriteFrame(texture, new Rect2(i * frameWidth, 0, frameWidth, frameHeight), displaySize);

        return true;
    }

    public SpriteFrame GetFrame(ActorMotionState state, double clock)
    {
        SpriteFrame[] frames = _frames.TryGetValue(state, out SpriteFrame[]? stateFrames)
            ? stateFrames
            : _frames[ActorMotionState.Idle];

        float framesPerSecond = state is ActorMotionState.Run or ActorMotionState.Crawl ? 12f : 6f;
        int index = Mathf.FloorToInt((float)clock * framesPerSecond) % frames.Length;
        return frames[index];
    }

    public SpriteFrame GetFrame(int index)
    {
        SpriteFrame[] frames = _frames[ActorMotionState.Idle];
        int safeIndex = Mathf.PosMod(index, frames.Length);
        return frames[safeIndex];
    }

    private SpriteAnimationSet WithCrawl(SpriteFrame[] all, AnimationFrameRange crawl)
    {
        _frames[ActorMotionState.Crawl] = SliceFrames(all, crawl);
        return this;
    }

    private static SpriteAnimationSet? TryLoadSingleSprite(string resourcePath)
    {
        if (!TryLoadSingleSpriteFrames(resourcePath, out SpriteFrame[] frames))
            return null;

        Dictionary<ActorMotionState, SpriteFrame[]> states = [];
        foreach (ActorMotionState state in Enum.GetValues<ActorMotionState>())
            states[state] = frames;

        return new SpriteAnimationSet(states);
    }

    private static bool TryLoadSingleSpriteFrames(string resourcePath, out SpriteFrame[] frames)
    {
        frames = [];
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        ImageTexture texture = ImageTexture.CreateFromImage(image);
        SpriteFrame frame = new(texture, new Rect2(0, 0, image.GetWidth(), image.GetHeight()), new Vector2(image.GetWidth(), image.GetHeight()));
        frames = [frame];
        return true;
    }

    private static bool TryLoadHorizontalRawStripFrames(string relativePath, int frameCount, out SpriteFrame[] frames)
    {
        frames = [];
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string filePath = Path.GetFullPath(Path.Combine(projectRoot, "..", relativePath));
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        frameCount = Mathf.Max(1, frameCount);
        int frameWidth = image.GetWidth() / frameCount;
        int frameHeight = image.GetHeight();
        if (frameWidth <= 0 || frameHeight <= 0)
            return false;

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        Vector2 displaySize = new(frameWidth, frameHeight);
        frames = new SpriteFrame[frameCount];
        for (int i = 0; i < frameCount; i++)
            frames[i] = new SpriteFrame(texture, new Rect2(i * frameWidth, 0, frameWidth, frameHeight), displaySize);

        return true;
    }

    private static bool TryLoadBlobFrames(string resourcePath, out SpriteFrame[] frames)
    {
        frames = [];
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        Rect2[] rects = DetectBlobFrames(image);
        if (rects.Length == 0)
            return false;

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        Vector2 displaySize = GetCommonDisplaySize(rects);
        frames = new SpriteFrame[rects.Length];
        for (int i = 0; i < rects.Length; i++)
            frames[i] = new SpriteFrame(texture, rects[i], displaySize);

        return true;
    }

    private static bool TryLoadBlobIndexFrames(string resourcePath, int[] indices, out SpriteFrame[] frames)
    {
        frames = [];
        if (!TryLoadBlobFrames(resourcePath, out SpriteFrame[] all) || all.Length == 0)
            return false;

        List<SpriteFrame> selected = [];
        foreach (int index in indices)
        {
            if (index >= 0 && index < all.Length)
                selected.Add(all[index]);
        }

        frames = selected.ToArray();
        return frames.Length > 0;
    }

    private static bool TryLoadFixedSourceFrames(string resourcePath, Rect2[] rects, out SpriteFrame[] frames)
    {
        frames = [];
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        Vector2 displaySize = GetCommonDisplaySize(rects);
        const int EdgePadding = 2;
        Vector2 paddedDisplaySize = displaySize + new Vector2(EdgePadding * 2, EdgePadding * 2);
        frames = new SpriteFrame[rects.Length];
        for (int i = 0; i < rects.Length; i++)
        {
            Rect2 rect = rects[i];
            Rect2I source = new(
                Mathf.RoundToInt(rect.Position.X),
                Mathf.RoundToInt(rect.Position.Y),
                Mathf.RoundToInt(rect.Size.X),
                Mathf.RoundToInt(rect.Size.Y)
            );
            int paddedWidth = source.Size.X + EdgePadding * 2;
            int paddedHeight = source.Size.Y + EdgePadding * 2;
            Image frameImage = Image.CreateEmpty(paddedWidth, paddedHeight, false, Image.Format.Rgba8);
            frameImage.Fill(Colors.Transparent);
            Vector2I destination = new(EdgePadding, EdgePadding);
            frameImage.BlitRect(image, source, destination);
            ImageTexture texture = ImageTexture.CreateFromImage(frameImage);
            frames[i] = new SpriteFrame(texture, new Rect2(0, 0, paddedWidth, paddedHeight), paddedDisplaySize);
        }

        return true;
    }

    private static bool TryLoadComponentSourceFrames(string resourcePath, Vector2I[] seeds, out SpriteFrame[] frames)
    {
        frames = [];
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return false;

        Image image = Image.LoadFromFile(filePath);
        if (image.IsEmpty())
            return false;

        image.Convert(Image.Format.Rgba8);
        List<ComponentFrame> components = [];
        foreach (Vector2I seed in seeds)
        {
            if (TryExtractComponent(image, seed, out ComponentFrame component))
                components.Add(component);
        }

        if (components.Count == 0)
            return false;

        int maxWidth = 1;
        int maxHeight = 1;
        foreach (ComponentFrame component in components)
        {
            maxWidth = Mathf.Max(maxWidth, component.Bounds.Size.X);
            maxHeight = Mathf.Max(maxHeight, component.Bounds.Size.Y);
        }

        const int EdgePadding = 2;
        Vector2 displaySize = new(maxWidth + EdgePadding * 2, maxHeight + EdgePadding * 2);
        frames = new SpriteFrame[components.Count];
        for (int i = 0; i < components.Count; i++)
        {
            ComponentFrame component = components[i];
            int paddedWidth = component.Bounds.Size.X + EdgePadding * 2;
            int paddedHeight = component.Bounds.Size.Y + EdgePadding * 2;
            Image frameImage = Image.CreateEmpty(paddedWidth, paddedHeight, false, Image.Format.Rgba8);
            frameImage.Fill(Colors.Transparent);
            foreach (Vector2I point in component.Points)
            {
                Vector2I destination = point - component.Bounds.Position + new Vector2I(EdgePadding, EdgePadding);
                frameImage.SetPixelv(destination, image.GetPixelv(point));
            }

            ImageTexture texture = ImageTexture.CreateFromImage(frameImage);
            frames[i] = new SpriteFrame(texture, new Rect2(0, 0, paddedWidth, paddedHeight), displaySize);
        }

        return true;
    }

    private static bool TryExtractComponent(Image image, Vector2I seed, out ComponentFrame component)
    {
        component = new ComponentFrame(new Rect2I(), []);
        int width = image.GetWidth();
        int height = image.GetHeight();
        if (!TryFindOpaqueSeed(image, seed, out Vector2I start))
            return false;

        bool[,] visited = new bool[width, height];
        Stack<Vector2I> stack = new();
        List<Vector2I> points = [];
        stack.Push(start);
        visited[start.X, start.Y] = true;
        int minX = start.X;
        int maxX = start.X;
        int minY = start.Y;
        int maxY = start.Y;

        while (stack.Count > 0)
        {
            Vector2I point = stack.Pop();
            points.Add(point);
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);

            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    if (ox == 0 && oy == 0)
                        continue;

                    int x = point.X + ox;
                    int y = point.Y + oy;
                    if (x < 0 || y < 0 || x >= width || y >= height || visited[x, y])
                        continue;

                    if (image.GetPixel(x, y).A <= 0.03f)
                        continue;

                    visited[x, y] = true;
                    stack.Push(new Vector2I(x, y));
                }
            }
        }

        component = new ComponentFrame(new Rect2I(minX, minY, maxX - minX + 1, maxY - minY + 1), points);
        return true;
    }

    private static bool TryFindOpaqueSeed(Image image, Vector2I seed, out Vector2I result)
    {
        result = seed;
        int width = image.GetWidth();
        int height = image.GetHeight();
        if (seed.X >= 0 && seed.Y >= 0 && seed.X < width && seed.Y < height && image.GetPixelv(seed).A > 0.03f)
            return true;

        for (int radius = 1; radius <= 8; radius++)
        {
            for (int y = seed.Y - radius; y <= seed.Y + radius; y++)
            {
                for (int x = seed.X - radius; x <= seed.X + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    if (image.GetPixel(x, y).A <= 0.03f)
                        continue;

                    result = new Vector2I(x, y);
                    return true;
                }
            }
        }

        return false;
    }

    private static void AddFrames(
        Dictionary<ActorMotionState, SpriteFrame[]> frames,
        ActorMotionState state,
        string resourcePath
    )
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return;

        sheet.Convert(Image.Format.Rgba8);
        MakeNearWhiteTransparent(sheet);
        ThickenOpaquePixels(sheet, 1);

        int frameSize = sheet.GetHeight();
        int frameCount = Mathf.Max(1, sheet.GetWidth() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        SpriteFrame[] loaded = new SpriteFrame[frameCount];

        for (int i = 0; i < frameCount; i++)
            loaded[i] = new SpriteFrame(texture, new Rect2(i * frameSize, 0, frameSize, frameSize), new Vector2(frameSize, frameSize));

        frames[state] = loaded;
    }

    private static void AddGridFrames(
        Dictionary<ActorMotionState, SpriteFrame[]> frames,
        ActorMotionState state,
        string resourcePath,
        int frameSize
    )
    {
        List<SpriteFrame> loaded = [];
        AppendGridFrames(loaded, resourcePath, frameSize);
        if (loaded.Count > 0)
            frames[state] = loaded.ToArray();
    }

    private static void AddSingleFileFrames(
        Dictionary<ActorMotionState, SpriteFrame[]> frames,
        ActorMotionState state,
        FrameImportOptions options,
        params string[] resourcePaths
    )
    {
        List<SpriteFrame> loaded = [];
        AppendSingleFileFrames(loaded, options, resourcePaths);
        if (loaded.Count > 0)
            frames[state] = loaded.ToArray();
    }

    private static void AppendFrames(List<SpriteFrame> frames, string resourcePath)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return;

        sheet.Convert(Image.Format.Rgba8);
        MakeNearWhiteTransparent(sheet);
        ThickenOpaquePixels(sheet, 1);

        int frameSize = sheet.GetHeight();
        int frameCount = Mathf.Max(1, sheet.GetWidth() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);

        for (int i = 0; i < frameCount; i++)
            frames.Add(new SpriteFrame(texture, new Rect2(i * frameSize, 0, frameSize, frameSize), new Vector2(frameSize, frameSize)));
    }

    private static void AppendSingleFileFrames(List<SpriteFrame> frames, FrameImportOptions options, params string[] resourcePaths)
    {
        foreach (string resourcePath in resourcePaths)
        {
            string filePath = ProjectSettings.GlobalizePath(resourcePath);
            if (!File.Exists(filePath))
                continue;

            Image image = Image.LoadFromFile(filePath);
            if (image.IsEmpty())
                continue;

            image.Convert(Image.Format.Rgba8);
            if (options.RemoveNearWhite)
                MakeNearWhiteTransparent(image);
            if (options.RecolorNearWhite)
                RecolorNearWhite(image, Colors.Black);
            ImageTexture texture = ImageTexture.CreateFromImage(image);
            frames.Add(new SpriteFrame(texture, new Rect2(0, 0, image.GetWidth(), image.GetHeight()), new Vector2(image.GetWidth(), image.GetHeight())));
        }
    }

    private static void AppendGridFrames(List<SpriteFrame> frames, string resourcePath, int frameSize)
    {
        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        if (!File.Exists(filePath))
            return;

        Image sheet = Image.LoadFromFile(filePath);
        if (sheet.IsEmpty())
            return;

        sheet.Convert(Image.Format.Rgba8);
        MakeNearWhiteTransparent(sheet);
        ThickenOpaquePixels(sheet, 1);

        int columns = Mathf.Max(1, sheet.GetWidth() / frameSize);
        int rows = Mathf.Max(1, sheet.GetHeight() / frameSize);
        ImageTexture texture = ImageTexture.CreateFromImage(sheet);
        Vector2 displaySize = new(frameSize, frameSize);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Rect2I source = new(column * frameSize, row * frameSize, frameSize, frameSize);
                if (IsTransparentFrame(sheet, source))
                    continue;

                frames.Add(new SpriteFrame(texture, new Rect2(source.Position, source.Size), displaySize));
            }
        }
    }

    private static bool IsTransparentFrame(Image image, Rect2I source)
    {
        int xMax = Mathf.Min(image.GetWidth(), source.End.X);
        int yMax = Mathf.Min(image.GetHeight(), source.End.Y);
        for (int y = source.Position.Y; y < yMax; y++)
        {
            for (int x = source.Position.X; x < xMax; x++)
            {
                if (image.GetPixel(x, y).A > 0.03f)
                    return false;
            }
        }

        return true;
    }

    private static string GameCreatorPlayerPath()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(
            projectRoot,
            "..",
            "raw base assets",
            "The Game Creator's Pack",
            "The Game Creator's Pack",
            "Graphic Pack",
            "Player_DarkOutline.png"
        ));
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, int start, int count)
    {
        if (frames.Length == 0)
            return [];

        start = Mathf.Clamp(start, 0, frames.Length - 1);
        count = Mathf.Clamp(count, 1, frames.Length - start);
        SpriteFrame[] slice = new SpriteFrame[count];
        Array.Copy(frames, start, slice, 0, count);
        return slice;
    }

    private static Vector2 GetCommonDisplaySize(Rect2[] frames)
    {
        float maxWidth = 1f;
        float maxHeight = 1f;
        foreach (Rect2 frame in frames)
        {
            maxWidth = Mathf.Max(maxWidth, frame.Size.X);
            maxHeight = Mathf.Max(maxHeight, frame.Size.Y);
        }

        return new Vector2(maxWidth, maxHeight);
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, AnimationFrameRange range)
    {
        return SliceFrames(frames, range, false);
    }

    private static SpriteFrame[] SliceFrames(SpriteFrame[] frames, AnimationFrameRange range, bool pingPong)
    {
        int start = Mathf.Min(range.Start, range.End);
        int end = Mathf.Max(range.Start, range.End);
        SpriteFrame[] forward = SliceFrames(frames, start, end - start + 1);
        if (!pingPong || forward.Length <= 1)
            return forward;

        SpriteFrame[] expanded = new SpriteFrame[forward.Length * 2 - 1];
        Array.Copy(forward, expanded, forward.Length);
        for (int i = 1; i < forward.Length; i++)
            expanded[forward.Length + i - 1] = forward[forward.Length - 1 - i];

        return expanded;
    }

    private static Rect2[] DetectBlobFrames(
        Image image,
        int? minOpaquePixelsOverride = null,
        int? minHeightOverride = null,
        int? minWidthOverride = null
    )
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        bool[,] visited = new bool[width, height];
        List<DetectedFrame> frames = [];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (visited[x, y] || image.GetPixel(x, y).A <= 0.03f)
                    continue;

                DetectedFrame frame = FloodFrame(image, visited, x, y);
                int minPixels = minOpaquePixelsOverride ?? (height <= 180 ? 25 : 180);
                int minHeight = minHeightOverride ?? (height <= 180 ? 4 : 24);
                int minWidth = minWidthOverride ?? 4;
                if (frame.OpaquePixels < minPixels || frame.Height < minHeight || frame.Width < minWidth)
                    continue;

                frames.Add(frame.Grow(1, width, height));
            }
        }

        bool likelyHorizontalStrip = width > height * 4;
        frames.Sort((a, b) =>
        {
            if (likelyHorizontalStrip)
            {
                int xCompare = a.X.CompareTo(b.X);
                return xCompare != 0 ? xCompare : a.Y.CompareTo(b.Y);
            }

            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        Rect2[] rects = new Rect2[frames.Count];
        for (int i = 0; i < frames.Count; i++)
            rects[i] = new Rect2(frames[i].X, frames[i].Y, frames[i].Width, frames[i].Height);

        return rects;
    }

    private static DetectedFrame FloodFrame(Image image, bool[,] visited, int startX, int startY)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        Stack<Vector2I> stack = new();
        stack.Push(new Vector2I(startX, startY));
        visited[startX, startY] = true;
        int minX = startX;
        int maxX = startX;
        int minY = startY;
        int maxY = startY;
        int opaquePixels = 0;

        while (stack.Count > 0)
        {
            Vector2I point = stack.Pop();
            opaquePixels++;
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);

            TryPush(image, visited, stack, point.X + 1, point.Y, width, height);
            TryPush(image, visited, stack, point.X - 1, point.Y, width, height);
            TryPush(image, visited, stack, point.X, point.Y + 1, width, height);
            TryPush(image, visited, stack, point.X, point.Y - 1, width, height);
        }

        return new DetectedFrame(minX, minY, maxX - minX + 1, maxY - minY + 1, opaquePixels);
    }

    private static void TryPush(Image image, bool[,] visited, Stack<Vector2I> stack, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x >= width || y >= height || visited[x, y])
            return;

        if (image.GetPixel(x, y).A <= 0.03f)
            return;

        visited[x, y] = true;
        stack.Push(new Vector2I(x, y));
    }

    private static void MakeNearWhiteTransparent(Image image)
    {
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.R > 0.95f && pixel.G > 0.95f && pixel.B > 0.95f)
                    image.SetPixel(x, y, Colors.Transparent);
            }
        }
    }

    private static void RecolorNearWhite(Image image, Color replacement)
    {
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.A > 0.03f && pixel.R > 0.92f && pixel.G > 0.92f && pixel.B > 0.92f)
                    image.SetPixel(x, y, new Color(replacement.R, replacement.G, replacement.B, pixel.A));
            }
        }
    }

    private static void ThickenOpaquePixels(Image image, int radius)
    {
        if (radius <= 0)
            return;

        int width = image.GetWidth();
        int height = image.GetHeight();
        Image source = (Image)image.Duplicate();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = source.GetPixel(x, y);
                if (pixel.A <= 0.03f)
                    continue;

                for (int oy = -radius; oy <= radius; oy++)
                {
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        int targetX = x + ox;
                        int targetY = y + oy;
                        if (targetX < 0 || targetY < 0 || targetX >= width || targetY >= height)
                            continue;

                        Color target = image.GetPixel(targetX, targetY);
                        if (target.A < pixel.A)
                            image.SetPixel(targetX, targetY, pixel);
                    }
                }
            }
        }
    }

    private readonly record struct ComponentFrame(Rect2I Bounds, List<Vector2I> Points);
    private readonly record struct FrameImportOptions(bool RemoveNearWhite = false, bool RecolorNearWhite = false);

    private readonly record struct DetectedFrame(int X, int Y, int Width, int Height, int OpaquePixels)
    {
        public DetectedFrame Grow(int pixels, int maxWidth, int maxHeight)
        {
            int x = Math.Max(0, X - pixels);
            int y = Math.Max(0, Y - pixels);
            int endX = Math.Min(maxWidth, X + Width + pixels);
            int endY = Math.Min(maxHeight, Y + Height + pixels);
            return new DetectedFrame(x, y, endX - x, endY - y, OpaquePixels);
        }
    }
}
