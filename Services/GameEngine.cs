﻿using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;

    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        // TODO Update this method to allow for attacking a selected monster in the room.
        // TODO e.g. "Which monster would you like to attack?"
        // TODO Right now it just attacks the first monster in the room.
        // TODO It is ok to leave this functionality if there is only one monster in the room.

        // changed the target to a list instead of one monster
        var targetlist = _player.CurrentRoom.Characters.Where(c => c != _player).ToList();
        
        if (targetlist != null)
        {
            for (int i = 0; i < targetlist.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {targetlist[i].Name}");
                
            }
            Console.WriteLine("");
            Console.Write("Select a target: ");
            Console.WriteLine("");
            var input = Console.ReadLine();
            var target = targetlist[int.Parse(input) - 1];
            _player.Attack(target);
            
            _player.CurrentRoom.RemoveCharacter(target);
            
        }
        else
        {
            _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _mapManager.DisplayMap();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Move North");
            _outputManager.WriteLine("2. Move South");
            _outputManager.WriteLine("3. Move East");
            _outputManager.WriteLine("4. Move West");

            // Check if there are characters in the current room to attack
            if (_player.CurrentRoom.Characters.Any(c => c != _player))
            {
                _outputManager.WriteLine("5. Attack");
            }

            _outputManager.WriteLine("6. Exit Game");

            _outputManager.Display();

            var input = Console.ReadLine();

            string? direction = null;
            switch (input)
            {
                case "1":
                    direction = "north";
                    break;
                case "2":
                    direction = "south";
                    break;
                case "3":
                    direction = "east";
                    break;
                case "4":
                    direction = "west";
                    break;
                case "5":
                    if (_player.CurrentRoom.Characters.Any(c => c != _player))
                    {
                        AttackCharacter();
                    }
                    else
                    {
                        _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                    }

                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                    break;
            }

            // Update map manager with the current room after movement
            if (!string.IsNullOrEmpty(direction))
            {
                _outputManager.Clear();
                _player.Move(direction);
                _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();


        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        // TODO Load your two new monsters here into the same room
        random = new Random();
        randomRoom = _rooms[random.Next(_rooms.Count)];
        var dragon = _context.Characters.OfType<Dragon>().FirstOrDefault();
        var bunny = _context.Characters.OfType<Bunny>().FirstOrDefault();
        randomRoom.AddCharacter(dragon);
        randomRoom.AddCharacter(bunny);
    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        // TODO Update this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var bossRoom = _roomFactory.CreateRoom("boss", _outputManager);
        var potatoRoom = _roomFactory.CreateRoom("potato", _outputManager);

        entrance.North = treasureRoom;
        entrance.West = library;
        entrance.East = garden;

        treasureRoom.South = entrance;
        treasureRoom.West = dungeonRoom;
        treasureRoom.East = bossRoom;

        dungeonRoom.East = treasureRoom;

        library.East = entrance;
        library.South = armory;

        armory.North = library;

        garden.West = entrance;

        bossRoom.West = treasureRoom;
        bossRoom.North = potatoRoom;

        potatoRoom.South = bossRoom;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden, bossRoom, potatoRoom };

        return entrance;
    }
}
