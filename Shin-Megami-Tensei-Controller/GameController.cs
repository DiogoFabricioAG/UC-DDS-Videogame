using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";
    private const string ERROR_MESSAGE = "Archivo de equipos inválido";
    private int _inputFromUser;
    private bool _executionRunning;
    private readonly TeamController _teamController;

    private int InputFromUser
    {
        get => _inputFromUser;
        set => _inputFromUser = value;
    }

    public GameController(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _teamController = new TeamController(_view);

    }
    
    private string TeamCreation(string[] lines, Team team1, Team team2)
    {
        var position1 = 0;
        int position2 = 1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Player 2"))
            {
                position2 = i;
                break;
            }
        }
        string[] alineacionE1 = lines.Skip(1).Take(position2-1).ToArray();
        string[] alineacionE2 = lines.Skip(position2+1).Take(lines.Length - position2 - 1).ToArray();

        var result = _teamController.EnterUnits(alineacionE1, team1);
        team1.Identifier = "1";
        team1.State = TeamState.WithTurn;
        if (result)
        {
            return ERROR_MESSAGE;
        }
        team1.SelectStarterTeam();
        team1.Samurai.ShowAbility();
        var result2 = _teamController.EnterUnits(alineacionE2, team2);
        
        team2.Identifier = "2";
        team2.State = TeamState.WithoutTurn;
        team2.Samurai.ShowAbility();

        if (result2)
        {
            return ERROR_MESSAGE;
        }
        team2.SelectStarterTeam();
        return SEPARATOR;
    }
    
    private void InputText(string text)
    {
        InputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }
  private void HandleChangeTurn(Game game)
    {
        
        game.CurrentTeam.RealoadTurns();
        game.OtherTeam.RealoadTurns();
        // Esto esta mal, se debe mejorar
        game.CurrentTeam.OrderAttack = 0;
        _view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && game.HandleGameFinished() == null)
        {
            _view.DisplayTeamsUnitsCurrentStatus(game);
            _view.DisplayCurrentTurnsbyType(game.CurrentTeam);
            _view.DisplayCurrentTurnOrder(game.CurrentTeam);

            _executionRunning = true;

            while (_executionRunning)
            {
                var availableActions = game.CurrentTeam.GetUnitInTurn().GetAvailableActions();
                _view.DisplayUnitActions(game.CurrentTeam.GetUnitInTurn().Name, availableActions);

                InputText(_view.ReadLine());
                if (game.CurrentTeam.GetUnitInTurn() is Monster)
                {
                    HandleActionUnit(game);
                }
                else
                {
                    HandleAction(game);
                }
                if (InputFromUser == 6)
                {
                    break;
                }

            }
            game.OtherTeam.AnyUnitDestroyed();
        }
        game.ChangeCurrentTeam();
    }
  
    private String LoadGame(Game game)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        if (Directory.Exists(_teamsFolder))
        {
            try
            {
                string[] archivos = Directory.GetFiles(_teamsFolder);
                int contador = 0;
                foreach (string archivo in archivos)
                {
                    _view.WriteLine($"{contador}: {Path.GetFileName(archivo)}");
                    contador++;
                }
                
            }
            catch (Exception ex)
            {
                _view.WriteLine($"Ocurrió un error al leer la carpeta: {ex.Message}");
            }
        }
        else
        {
            _view.WriteLine("La carpeta de equipos no existe.");
        }
        var input = _view.ReadLine();
        var selection = Convert.ToInt32(input);
        var selectedFile= Directory.GetFiles(_teamsFolder)[selection];
        var lines = File.ReadAllLines(selectedFile);
        var result = TeamCreation(lines, game.CurrentTeam, game.OtherTeam);
        return result;
    }

    
    public void Play()
    {
        var game = new Game();
        var resultLoad = LoadGame(game);
        _view.WriteLine(resultLoad);
        if (resultLoad == ERROR_MESSAGE)
        {
            return; 
        }
        RunGameLoop(game);
        _view.WriteLine($"Ganador: {game.HandleGameFinished().Name()}");
    }

    private void RunGameLoop(Game game)
    {
        while (game.HandleGameFinished() == null)
        {
            HandleChangeTurn(game);
        }
    }

    private void HandleAction(Game game)
    {
        _executionRunning = false;
        switch (InputFromUser)
        {
            case 1:
                HandleAttackUse(game, ElementType.Physics);
                break;

            case 2:
                HandleAttackUse(game, ElementType.Gun);
                break;
            case 3:
                HandleAbilityUse(game);
                break;
            
            case 5:
                HandlePassTurn(game);
                break;
            case 6:
                game.HandleSurrender();
                _view.SurrenderTeamDisplay(game.CurrentTeam);
                break;
        }
    }

    private void HandleActionUnit(Game game)
    {
        _executionRunning = false;
        switch (InputFromUser)
        {
            case 1:
                HandleAttackUse(game, ElementType.Physics);
                break;
            case 2:
                HandleAbilityUse(game);
                break;
            case 3:
                break;
            case 4:
                HandlePassTurn(game);
                break;
        }
    }
    
    private void HandleAttackUse(Game game, ElementType elementType)
    {
        _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, TargetType.Single);
        InputText(_view.ReadLine());
    
        if (InputFromUser == game.OtherTeam.CancelOptionInSelectableTeam())
        {
            _executionRunning = true;
            return;
        }
        
        var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser, game.OtherTeam);

        var (damageDone, affinityType) = AttackController.ExecuteAttack(attacker, attacked, elementType);

        
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, abilityType, 1);

        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(abilityType, attacked, game.CurrentTeam);
        for (int i = 0; i < blinkingTurnLoss; i++)
        {
            game.CurrentTeam.DestroyTurn(TurnType.Blinking);
        }
        for (int i = 0; i < fullTurnLoss; i++)
        {
            game.CurrentTeam.DestroyTurn(TurnType.Full);
        }

        for (int i = 0; i < blinkingTurnWon; i++)
        {
            game.CurrentTeam.AddTurn(TurnType.Blinking);
        }
        
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.TurnRemains();
        game.OtherTeam.AnyUnitDestroyed();
        game.OtherTeam.WasDefeated();

        _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
    }

    private void HandlePassTurn(Game game)
    {
        var type =  game.PassTurn();
        if (type == TurnType.Full)
        {
            _view.TurnUsedDisplayWithParameters(0,1,1);
        }
        else
        {
            _view.TurnUsedDisplayWithParameters(1,0,0);
        }
    }


    private void HandleAbilityUse(Game game)
    {
        _view.WriteLine($"Seleccione una habilidad para que {game.CurrentTeam.GetUnitInTurn().Name} use");
        _view.DisplayShowSelectableAbilities(game.CurrentTeam.GetUnitInTurn());
        InputText(_view.ReadLine());
        
        if (InputFromUser == game.CurrentTeam.GetCancelOptionAbilities())
        {
            _executionRunning = true;
            return;
        }
        
        var ability = game.CurrentTeam.GetUnitInTurn().Abilities[InputFromUser-1];

        


        if (ability.Type == AbilityType.Special)
        {
            HandleInvokeUse(game, ability);

        }
        else
        {
            
            // Alguna Forma de Validar esto de aca, digo que esta terriblemente feo.
            var reviveAbility = ability.Effect.Contains("Revive");
            
            _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, ability.Target, reviveAbility);
            InputText(_view.ReadLine());
        
            var teamSelected = ability.Target == TargetType.Ally ? game.CurrentTeam : game.OtherTeam;
        
            var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser, teamSelected, reviveAbility);

            if (ability.Target != TargetType.Ally)
            {
                var (damageDone, affinityType, numberHits) = AbilityController.UseDamageAbility(attacker, attacked, ability, game.CurrentTeam);
                _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, ability.Type, numberHits);
            }
            else
            {
                var healRealized = AbilityController.UseHealAbility(attacker, attacked, ability);
                _view.DisplayAbilityLogs(healRealized, attacker, attacked, AffinityType.Neutral, ability.Type, 1, reviveAbility);
            }
        
            var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(ability.Type, attacked, game.CurrentTeam);

        
            _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
            game.CurrentTeam.ChangeOrder();
        
        
            // MEJORAR MUCHO POR FFAVOR
            for (int i = 0; i < blinkingTurnLoss; i++)
            {
                game.CurrentTeam.DestroyTurn(TurnType.Blinking);
            }
            for (int i = 0; i < fullTurnLoss; i++)
            {
                game.CurrentTeam.DestroyTurn(TurnType.Full);
            }

            for (int i = 0; i < blinkingTurnWon; i++)
            {
                game.CurrentTeam.AddTurn(TurnType.Blinking);
            }
        
        
            game.CurrentTeam.TurnRemains();
            game.OtherTeam.AnyUnitDestroyed();
            game.OtherTeam.WasDefeated();
        }
    }

    private void HandleInvokeUse(Game game, Ability ability = null)
    {

        _view.ShowInvocableMonsters(game.CurrentTeam);
        InputText(_view.ReadLine());
        if (InputFromUser == game.CurrentTeam.GetCancelOptionInvoke())
        {
            _executionRunning = true;
            return;
        }

        var indexBackupUnit = InputFromUser;
        _view.ShowReplaceableUnits(game.CurrentTeam);
        InputText(_view.ReadLine());
        
        var indexStarterUnit = InputFromUser;
        
        var unitReplaced = game.CurrentTeam.MoveUnit(indexBackupUnit, indexStarterUnit);

        _view.InvokeAnUnit(unitReplaced);
        if (ability != null)
        {
            game.CurrentTeam.GetUnitInTurn().Attributes.CurrentMp -= ability.Cost;
        }
        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(AbilityType.Special, unitReplaced, game.CurrentTeam);

        
        _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        game.CurrentTeam.ChangeOrder();
        
        
        
        // MEJORAR MUCHO POR FFAVOR
        for (int i = 0; i < blinkingTurnLoss; i++)
        {
            game.CurrentTeam.DestroyTurn(TurnType.Blinking);
        }
        for (int i = 0; i < fullTurnLoss; i++)
        {
            game.CurrentTeam.DestroyTurn(TurnType.Full);
        }

        for (int i = 0; i < blinkingTurnWon; i++)
        {
            game.CurrentTeam.AddTurn(TurnType.Blinking);
        }

    }
    
}
