using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace BattleShip
{
    class Program
    {
        static void Main(string[] args)
        {
            GameEngine MiMotorDeJuego = new GameEngine();

            while (true)
            {
                MiMotorDeJuego.Engine();
            }
        }
    }

    class GameEngine
    {
        public Ships ships;
        public Ships playerShips;
        public int Pista { get; set; }
        List<char> _MiIsla = new List<char>();
        List<char> _IslaEnemiga = new List<char>();
        List<char> _CubrirIslaEnemiga = new List<char>();
        bool EstaGenerando;
        int cubePosition;
        char saveCharacter;
        public int TusBarcos { get; set; }

        enum Direction
        {
            Up = -10,
            Left = -1,
            Right = 1,
            Down = 10
        }

        public GameEngine()
        {
            playerShips = new Ships(this, 5);
            Pista = 1;
        }

        public void DrawIslands()
        {
            Console.Clear();

            Console.Write("  ");
            for (int i = 1; i <= 10; i++)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();

            int Counter = 0;
            int HowManyTime = 0;

            for (int i = 0; i < 10; i++)
            {
                Console.Write(((char)('A' + i)) + " ");

                for (int j = 0; j < 10; j++)
                {
                    Console.Write("{0} ", _MiIsla[i * 10 + j]);
                }

                Console.Write("║ ");

                for (int j = 0; j < 10; j++)
                {
                    Console.Write("{0} ", _CubrirIslaEnemiga[i * 10 + j]);
                }

                Console.WriteLine();

                if (HowManyTime == 100)
                {
                    break;
                }
            }
        }

        private void ShowInformation()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("░ TUS BARCOS: {0} \t BARCOS ENEMIGOS: {1} \t PISTA: {2}", playerShips.TusBarcos, playerShips.BarcosEnemigos, Pista);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("░ 1. ATACAR \t 2. PISTA \t 3. SALIR");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Engine()
        {
            if (!EstaGenerando)
            {
                FirstTimeGenerate();
            }
            DrawIslands();
            ShowInformation();
            string userInput = GetString("░ Elige un número > ");

            switch (userInput)
            {
                case "1":
                    var (fila, columna) = SolicitarUbicacionDeAtaque();
                    bool resultadoAtaque = playerShips.Attack(_IslaEnemiga, _CubrirIslaEnemiga, fila * 10 + columna);

                    if (resultadoAtaque)
                    {
                        _IslaEnemiga[fila * 10 + columna] = 'X';
                    }
                    else
                    {
                        _IslaEnemiga[fila * 10 + columna] = 'O';
                    }
                    cubePosition = 0;
                    saveCharacter = _CubrirIslaEnemiga[cubePosition];
                    _CubrirIslaEnemiga[cubePosition] = '■';
                    DrawIslands();
                    bool isEnter = false;

                    while (!isEnter)
                    {
                        ConsoleKeyInfo readKey = Console.ReadKey();

                        switch (readKey.Key)
                        {
                            case ConsoleKey.UpArrow:
                                Move(Direction.Up);
                                break;

                            case ConsoleKey.DownArrow:
                                Move(Direction.Down);
                                break;

                            case ConsoleKey.RightArrow:
                                Move(Direction.Right);
                                break;

                            case ConsoleKey.LeftArrow:
                                Move(Direction.Left);
                                break;

                            case ConsoleKey.Enter:
                                isEnter = true;
                                bool result = playerShips.Attack(_IslaEnemiga, _CubrirIslaEnemiga, cubePosition);

                                if (result)
                                {
                                    GetSucces("¡Hundiste uno de los barcos enemigos! ¡PODER!");
                                    if (Pista == 0)
                                    {
                                        Pista++;
                                    }
                                    HasAnyoneWon();
                                }
                                else
                                {
                                    GetError("¡Maldición, Almirante!");
                                }
                                break;
                        }
                    }

                    playerShips.AttackEnemy(_MiIsla);
                    HasAnyoneWon();
                    break;

                case "2":
                    if (Pista == 1)
                    {
                        string userChoice = GetString("¿Estás seguro de usar la pista [PRESIONA Y]? (si no hay ningún barco en la fila, uno de tus barcos se hundirá)");
                        if (userChoice == "y")
                        {
                            bool result = playerShips.Hint(_IslaEnemiga, _CubrirIslaEnemiga);
                            Pista = 0;
                            if (!result)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    int index = _MiIsla.LastIndexOf('@');
                                    _MiIsla.RemoveAt(index);
                                    _MiIsla.Insert(index, '░');
                                }
                                DrawIslands();
                                HasAnyoneWon();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        GetError("No tienes ninguna pista.");
                    }
                    break;

                case "3":
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Gracias por elegir mi juego. Nos vemos más tarde.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(0);
                    break;

                default:
                    GetError("¡Has introducido un valor incorrecto!");
                    break;
            }
        }

        public static (int, int) SolicitarUbicacionDeAtaque()
        {
            Console.WriteLine("Ingresa la ubicación para atacar (ejemplo: A5):");
            string input = Console.ReadLine().ToUpper();
            while (!Regex.IsMatch(input, "^[A-J](10|[1-9])$"))
            {
                Console.WriteLine("Entrada inválida. Asegúrate de ingresar una letra de A a J seguida de un número de 1 a 10 (ejemplo: A5):");
                input = Console.ReadLine().ToUpper();
            }

            int fila = input[0] - 'A';
            int columna = (input.Length == 3) ? 9 : int.Parse(input[1].ToString()) - 1;

            return (fila, columna);
        }

        private void FirstTimeGenerate()
        {
            for (int i = 0; i < 100; i++)
            {
                _MiIsla.Add('■');
                _IslaEnemiga.Add('■');
                _CubrirIslaEnemiga.Add('·');
            }
            playerShips.GenerateShips(_MiIsla);
            playerShips.GenerateShips(_IslaEnemiga);
            EstaGenerando = true;
        }

        private void Move(Direction direction)
        {
            _CubrirIslaEnemiga[cubePosition] = saveCharacter;
            cubePosition += (int)direction;

            if (cubePosition > 0 && cubePosition < 100)
            {
                saveCharacter = _CubrirIslaEnemiga[cubePosition];
                _CubrirIslaEnemiga[cubePosition] = '■';
                DrawIslands();
            }
            else
            {
                GetError("No puedes moverte fuera del campo");
                cubePosition = 0;
            }
        }

        private void HasAnyoneWon()
        {
            if (playerShips.BarcosEnemigos == 0 || playerShips.TusBarcos == 0)
            {
                int Counter = 0;
                int HowManyTime = 0;
                Console.Clear();

                while (true)
                {
                    HowManyTime += 10;
                    while (Counter < HowManyTime)
                    {
                        Console.Write("{0} ", _MiIsla[Counter]);
                        Counter++;
                    }

                    Console.Write("║ ");
                    Counter -= 10;

                    while (Counter < HowManyTime)
                    {
                        Console.Write("{0} ", _IslaEnemiga[Counter]);
                        Counter++;
                    }

                    Console.WriteLine();

                    if (HowManyTime == 100)
                    {
                        break;
                    }
                }

                if (playerShips.BarcosEnemigos == 0)
                {
                    GetSucces("¡Ganaste, Almirante! ¡Regresemos al país y contemos a la gente sobre tu valentía! ¡HURRA!");
                }
                else
                {
                    GetError("¡Perdiste, Almirante! ¡No te desanimes, juega de nuevo!");
                }

                Environment.Exit(0);
            }
        }

        public static string GetString(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message.ToLower());
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadLine();
        }

        public static void GetError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(3000);
        }

        public static void GetSucces(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(2000);
        }
    }

    class Ships
    {
        public int TusBarcos { get; set; }
        public int BarcosEnemigos { get; set; }
        private GameEngine motorDeJuego;

        public Ships(GameEngine motorJuego, int cuantosBarcos)
        {
            this.motorDeJuego = motorJuego;
            TusBarcos = cuantosBarcos;
            BarcosEnemigos = cuantosBarcos;
        }

        public void GenerateShips(List<char> isla)
        {
            int counter = 0;
            Random randomGenerator = new Random();
            while (true)
            {
                if (counter == TusBarcos)
                {
                    break;
                }

                int index = randomGenerator.Next(0, isla.Count);

                if (FindBestLocation(index, isla))
                {
                    isla[index] = '@';
                    isla[index - 1] = '@';
                    isla[index + 1] = '@';

                    counter++;
                }
            }
        }

        private bool FindBestLocation(int index, List<char> isla)
        {
            try
            {
                int Counter = index - (index % 10);
                for (int i = Counter; i < Counter + 10; i++)
                {
                    if (isla[i] != '■')
                    {
                        return false;
                    }
                }
                if (index % 10 > 2 && index % 10 < 9)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Hint(List<char> enemyIsla, List<char> enemyIslaCover)
        {
            for (int i = 0; i < enemyIsla.Count; i++)
            {
                if (enemyIsla[i] == '@')
                {
                    motorDeJuego.Pista--;
                    enemyIslaCover[i] = '@';
                    return true;
                }
            }
            return false;
        }

        public bool Attack(List<char> enemyIsla, List<char> enemyIslaCover, int index)
        {
            try
            {
                if (enemyIsla[index] == '@')
                {
                    enemyIsla[index] = 'X';
                    BarcosEnemigos--;
                    return true;
                }
                else
                {
                    enemyIslaCover[index] = 'O';
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool AttackEnemy(List<char> enemyIsla)
        {
            try
            {
                Random randomGenerator = new Random();
                int index = randomGenerator.Next(0, enemyIsla.Count);

                if (enemyIsla[index] == '@')
                {
                    motorDeJuego.TusBarcos--;
                    enemyIsla[index] = 'X';
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
