﻿namespace HungerGamesSimulator.Data
{

  public class Combat
  {
    private bool AreAllTributesDead( List<IActor> tributes )
    {
      int deadTributes = 0;
      foreach ( var tribute in tributes )
      {
        if ( tribute.IsDead() )
        {
          deadTributes += 1;
        }
      }
      return deadTributes == tributes.Count;
    }

    private bool AreAnyTributesDead( List<IActor> tributes )
    {
      foreach ( var tribute in tributes )
      {
        if ( tribute.IsDead() )
        {
          return true;
        }
      }
      return false;
    }

    private void HandleAttack( IActor fighter, IActor defender )
    {
      if ( fighter.SimulateHit( defender ) )
      {
        defender.TakeDamage( Weapon.RollWeaponDamage( fighter.Weapon ) );
      }
    }

    private bool DefenderEscaped( IActor fighter, IActor defender )
    {
      return fighter.SimulateEscape( defender );
    }

    public CombatResponse Simulate( CombatRequest request )
    {
      // retrieve data needed for combat
      int fightersCount = request.Fighters.Count;
      int defendersCount = request.Defenders.Count;
      int maxActors = fightersCount < defendersCount ? defendersCount : fightersCount;

      // begin combat loop
      bool escaped = false;
      while ( !escaped && !AreAllTributesDead( request.fighters ) && !AreAllTributesDead( request.defenders ) )
      {
        // attack tributes based on 1 -> 1 combat
        for ( int i = 0; i < maxActors; i++ )
        {
          // combat could keep going, but defenders all escaped
          if ( request.Defenders.Count <= 0 )
          {
            break;
          }

          var fighter = request.Fighters.Pop();
          var defender = request.Defenders.Pop();

          HandleAttack( fighter, defender );

          if ( !DefenderEscaped( fighter, defender ) )
          {
            request.Defenders.Push( defender );
          }
          request.Fighters.Push( fighter );
        }


        // reshuffle combat line up for added variety
        CombatUtils.Shuffle( request.Fighters );
        CombatUtils.Shuffle( request.Defenders );

        // check if all the defenders escaped
        escaped = request.Defenders.Count <= 0;
      }

      return new CombatResponse( AreAnyTributesDead( request.fighters ), AreAnyTributesDead( request.defenders ), escaped );
    }
  }

  public record CombatResponse( bool fightersDied, bool defendersDied, bool escaped )
  {
    bool FightersDied { get; } = fightersDied;
    bool DefendersDied { get; } = defendersDied;
    bool Escaped { get; } = escaped;
  }

  public record CombatRequest( List<IActor> fighters, List<IActor> defenders )
  {
    public Stack<IActor> Fighters { get; } = new Stack<IActor>( fighters );
    public Stack<IActor> Defenders { get; } = new Stack<IActor>( defenders );
  }
}
