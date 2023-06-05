using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace jogo_da_velha
{
    public class Game
    {
        List<Field> fields = new List<Field>();
        List<WinProbability> winProbabilities = new List<WinProbability>();

        private int userWins = 0;
        private int computerWins = 0;

        private bool hasWin = false;
        private bool hasDraw = false;
        private bool ocurredProblem = false;

        List<Action> actions;

        public Game()
        {
            actions = new List<Action>()
            {
                startUserPlay,
                startComputerPlay,
            };

            generateProbabilities();
        }

        public void start()
        {
            while (true)
            {
                hasDraw = false;
                hasWin = false;

                createBoard();

                foreach (var action in actions.ToList())
                {
                    action.Invoke();
                }
            }
        }

        private void stop()
        {
            fields.Clear();

            actions.Reverse();
        }

        private void startUserPlay()
        {
            if (hasDraw || hasWin)
            {
                return;
            }
            ocurredProblem = false;
            userPlay();
            checkWinAndDraw(EPlayer.user);
        }

        private void startComputerPlay()
        {
            if (ocurredProblem)
            {
                ocurredProblem = false;
                return;
            }
            else if (hasDraw || hasWin)
            {
                return;
            }

            computerPlay();
            createBoard();

            checkWinAndDraw(EPlayer.computer);
        }

        private void checkWinAndDraw(EPlayer player)
        {
            checkWin(player);

            if (hasWin)
            {
                displayWin(player);
                hasWin = true;
            }
            else if (checkDraw())
            {
                displayDraw();
                hasDraw = true;
            }
        }

        private void displayWin(EPlayer player)
        {
            bool userWinned = player == EPlayer.user;

            if (userWinned)
            {
                userWins++;
            }
            else
            {
                computerWins++;
            }
            Console.Clear();
            createBoard();
            Console.WriteLine();
            Console.WriteLine(
                (userWinned ? "Você ganhou!" : "Você perdeu!")
                + "[Aperte qualquer tecla para ir para próxima rodada]");
            stop();
            Console.ReadKey();
            Console.Clear();
        }

        private void displayDraw()
        {
            Console.Clear();
            createBoard();
            Console.WriteLine();
            Console.WriteLine(
                ("Empate")
                + "[Aperte qualquer tecla para ir para próxima rodada]");
            stop();
            Console.ReadKey();
            Console.Clear();
        }

        private bool checkDraw()
        {
            var freeField = fields.FirstOrDefault((e) => e.owner is null);
            return freeField == null;
        }

        private void createBoard()
        {

            Console.Clear();
            Console.WriteLine("Jogo da Velha");
            Console.WriteLine($"Jogador {userWins} x {computerWins} Computador");
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (fields.Count == 9)
                    {
                        var field = fields.FirstOrDefault(e => e.column == c && e.row == r);
                        Console.Write(
                            field?.owner == null ? " . " : field.owner == EPlayer.user ? " X " : " O ");
                    }
                    else
                    {
                        fields.Add(new Field(c, r, null));
                        Console.Write(" . ");
                    }
                }
                Console.WriteLine("");
            }
        }

        private void userPlay()
        {
            Console.WriteLine("");
            Console.Write("Escolha uma coluna: ");
            int col = int.Parse(Console.ReadLine() ?? throw new Exception());

            Console.Write("Escolha uma linha: ");
            int row = int.Parse(Console.ReadLine() ?? throw new Exception());

            var field = fields.FirstOrDefault((e) => e.column == col - 1 && e.row == row - 1);


            if (field != null)
            {
                if (field.owner != null)
                {
                    Console.WriteLine("Campo já selecionado! [Aperte qualquer botão]");
                    Console.ReadKey();
                    Console.Clear();
                    ocurredProblem = true;

                    return;
                }

                field.owner = EPlayer.user;
            }
            else
            {
                Console.WriteLine("Escolha um campo válido! [Aperte qualquer botão]");
                Console.ReadKey();
                Console.Clear();
                ocurredProblem = true;
            }
        }

        private void computerPlay()
        {
            if (checkMomentWin()) return;

            var userProbability = checkUserWinProbability();

            if (userProbability != null)
            {
                blockUserMove(userProbability);
                return;
            }
            else
            {
                if (tryMarkCenter()) return;

                if (initNewProbabilityPlay()) return;

                fields.First((e) => e.owner is null).owner = EPlayer.computer;
            }
        }

        private bool initNewProbabilityPlay()
        {
            foreach (var winProbability in winProbabilities)
            {
                var probabilityFields = new List<Field>();

                foreach (var probField in winProbability.fields)
                {
                    var field = fields.First(
                        (e) => e.column == probField.Item1 && e.row == probField.Item2);

                    if (field.owner == null)
                    {
                        probabilityFields.Add(field);

                        probabilityFields.First().owner = EPlayer.computer;
                        return true;
                    }
                    else if (field.owner == EPlayer.user)
                    {
                        break;
                    }
                }
            }
            return false;
        }

        private bool tryMarkCenter()
        {
            var centerField = fields.First((e) => e.column == 1 && e.row == 1);
            if (centerField.owner == null)
            {
                centerField.owner = EPlayer.computer;
                return true;
            }
            return false;
        }

        private bool checkMomentWin()
        {
            foreach (var winProbability in winProbabilities)
            {
                int hits = 0;
                var winFields = new List<Field>();

                foreach (var probField in winProbability.fields)
                {
                    var field = fields.First((e) => e.column == probField.Item1 && e.row == probField.Item2);
                    if (field.owner == EPlayer.computer)
                    {
                        hits++;
                    }
                    else if (field.owner == EPlayer.user)
                    {
                        break;
                    }
                    winFields.Add(field);
                }
                if (hits == 2)
                {
                    var winField = winFields.FirstOrDefault((e) => e.owner is null);
                    if (winField != null)
                    {
                        winField.owner = EPlayer.computer;
                        return true;
                    }
                }
            }

            return false;
        }

        private WinProbability? checkUserWinProbability()
        {
            WinProbability? userProbability = null;
            var userFields = fields.Where((e) => e.owner == EPlayer.user).ToList();

            if (userFields.Count() >= 2)
            {
                int userHits = 0;

                foreach (var winProbability in winProbabilities)
                {
                    userHits = 0;

                    foreach (var field in userFields)
                    {
                        var hittedProbability = winProbability.fields.FirstOrDefault(
                            (e) => e.Item1 == field.column && e.Item2 == field.row, (-1, -1));

                        if (hittedProbability != (-1, -1))
                        {
                            userHits++;

                            if (userHits == 2)
                            {
                                if (!computerAlreadyStoppedProbability(winProbability))
                                {
                                    userProbability = winProbability;
                                    break;
                                }
                            }
                        }
                    }
                    if (userProbability != null)
                    {
                        break;
                    }
                }
            }
            return userProbability;
        }

        private bool computerAlreadyStoppedProbability(WinProbability probability)
        {
            foreach (var probabilityField in probability.fields)
            {
                var field = fields.FirstOrDefault(
                    (e) => e.column == probabilityField.Item1 && e.row == probabilityField.Item2);
                if (field != null && field.owner == EPlayer.computer)
                {
                    return true;
                }
            }
            return false;
        }

        private void blockUserMove(WinProbability userProbability)
        {
            foreach (var item in userProbability.fields)
            {
                var field = fields.FirstOrDefault((e) => e.column == item.Item1 && e.row == item.Item2);
                if (field != null && field.owner == null)
                {
                    field.owner = EPlayer.computer;
                    return;
                }
            }
        }

        private void checkWin(EPlayer player)
        {
            var selectedFields = fields.Where((e) => e.owner == player).ToList();

            if (selectedFields.Count() >= 3)
            {
                int hits = 0;

                foreach (var winProbability in winProbabilities)
                {
                    hits = 0;

                    foreach (var field in selectedFields)
                    {
                        var hittedProbability = winProbability.fields.FirstOrDefault(
                            (e) => e.Item1 == field.column && e.Item2 == field.row, (-1, -1));

                        if (hittedProbability != (-1, -1))
                        {
                            hits++;

                            if (hits == 3)
                            {
                                hasWin = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void generateProbabilities()
        {
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (0, 0),
                        (0, 1),
                        (0, 2),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (1, 0),
                        (1, 1),
                        (1, 2),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (2, 0),
                        (2, 1),
                        (2, 2),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (0, 0),
                        (1, 0),
                        (2, 0),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (0, 1),
                        (1, 1),
                        (2, 1),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (0, 2),
                        (1, 2),
                        (2, 2),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (0, 0),
                        (1, 1),
                        (2, 2),
                    }));
            winProbabilities.Add(new WinProbability(new List<(int, int)>()
                    {
                        (2, 0),
                        (1, 1),
                        (0, 2),
                    }));


        }
    }
}
