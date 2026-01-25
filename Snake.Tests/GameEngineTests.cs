using Snake.Core;
using Snake.Models;

namespace Snake.Tests;

/// <summary>
/// Tests unitaires du <see cref="GameEngine"/>.
/// On vérifie la logique de jeu (Initialize, Move, collisions) sans WPF ni UI.
/// </summary>
public class GameEngineTests
{
    private static GameEngine CreateEngine() => new GameEngine();

    // --- Initialize ---

    [Fact]
    public void Initialize_QuandAppele_StateEstPlaying()
    {
        var engine = CreateEngine();
        engine.Initialize(100, 100, 20, 5);
        Assert.Equal(GameState.Playing, engine.State);
    }

    [Fact]
    public void Initialize_QuandAppele_ScoreEstZero()
    {
        var engine = CreateEngine();
        engine.Initialize(100, 100, 20, 5);
        Assert.Equal(0, engine.Score);
    }

    [Fact]
    public void Initialize_QuandAppele_SerpentALongueurDemandee()
    {
        var engine = CreateEngine();
        engine.Initialize(100, 100, 20, 7);
        Assert.Equal(7, engine.SnakeParts.Count);
    }

    [Fact]
    public void Initialize_QuandAppele_SquareSizeEstEnregistre()
    {
        var engine = CreateEngine();
        engine.Initialize(100, 100, 20, 5);
        Assert.Equal(20, engine.SquareSize);
    }

    [Fact]
    public void Initialize_QuandAppele_TeteAuBoutDroite()
    {
        var engine = CreateEngine();
        engine.Initialize(100, 100, 20, 4);
        var head = engine.SnakeParts[^1];
        Assert.Equal(60, head.X); // 3 * 20 (indices 0,1,2,3)
        Assert.Equal(40, head.Y); // (5/2)*20 = 40, milieu vertical
    }

    [Fact]
    public void Initialize_QuandGrilleAssezGrande_UnFruitEstPlace()
    {
        var engine = CreateEngine();
        engine.Initialize(200, 200, 20, 5); // 10x10 = 100 cellules, 5 serpent → 95 libres
        Assert.NotNull(engine.FoodPosition);
        var (fx, fy) = engine.FoodPosition!.Value;
        Assert.True(fx >= 0 && fx < 200);
        Assert.True(fy >= 0 && fy < 200);
    }

    // --- Move : déplacement normal ---

    [Fact]
    public void Move_DirectionDroite_TeteSeDeplaceADroite()
    {
        var engine = CreateEngine();
        engine.Initialize(200, 100, 20, 3); // 3 segments : tête (40,40), pas au bord
        double xAvant = engine.SnakeParts[^1].X;
        double yAvant = engine.SnakeParts[^1].Y;
        engine.Move(Direction.Right);
        var headApres = engine.SnakeParts[^1];
        Assert.Equal(xAvant + engine.SquareSize, headApres.X);
        Assert.Equal(yAvant, headApres.Y);
    }

    [Fact]
    public void Move_DirectionGauche_TeteSeDeplaceAGauche()
    {
        var engine = CreateEngine();
        engine.Initialize(200, 100, 20, 3); // 3 segments : après Up, tête (40,20)
        engine.Move(Direction.Up);   // (40,40) -> (40,20), direction = Up
        double xAvant = engine.SnakeParts[^1].X;
        engine.Move(Direction.Left); // (40,20) -> (20,20)
        var headApres = engine.SnakeParts[^1];
        Assert.Equal(xAvant - engine.SquareSize, headApres.X);
    }

    // --- Move : demi-tour ignoré ---

    [Fact]
    public void Move_DemiTourGaucheQuandDroite_ConserveDirectionDroite()
    {
        var engine = CreateEngine();
        engine.Initialize(200, 100, 20, 3); // direction initiale = Right, tête (40,40)
        double xAvant = engine.SnakeParts[^1].X;
        engine.Move(Direction.Left); // ignoré (demi-tour), on continue à droite
        var headApres = engine.SnakeParts[^1];
        Assert.Equal(xAvant + engine.SquareSize, headApres.X); // a avancé à droite
    }

    // --- Move : collision mur ---

    [Fact]
    public void Move_TeteEntreDansMur_StatePasseAGameOver()
    {
        var engine = CreateEngine();
        engine.Initialize(60, 60, 20, 3); // grille 3x3, tête à (40,20)
        engine.Move(Direction.Right);     // (60,20) → 60 >= 60, mur
        Assert.Equal(GameState.GameOver, engine.State);
    }

    // --- Move : collision corps ---

    [Fact]
    public void Move_TeteEntreDansSonCorps_StatePasseAGameOver()
    {
        var engine = CreateEngine();
        engine.Initialize(60, 40, 20, 2); // 3x2, serpent [(0,20),(20,20)]
        engine.Move(Direction.Up);         // tête (20,0)
        engine.Move(Direction.Down);       // tête (20,20) = corps
        Assert.Equal(GameState.GameOver, engine.State);
    }

    // --- Move : après Game Over = no-op ---

    [Fact]
    public void Move_QuandDejaGameOver_NeChangePlusRien()
    {
        var engine = CreateEngine();
        engine.Initialize(60, 60, 20, 3);
        engine.Move(Direction.Right); // Game Over (mur)
        Assert.Equal(GameState.GameOver, engine.State);
        engine.Move(Direction.Left);
        engine.Move(Direction.Up);
        Assert.Equal(GameState.GameOver, engine.State);
    }
}
