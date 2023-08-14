using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TicTacToeClientSide1225
{
    public partial class MainWindow : Window
    {
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int port = 27001;

        private bool isPlayerTurn = true;
        private char[] gameBoard = new char[9]; 

        public MainWindow()
        {
            InitializeComponent();
            InitializeGameBoard();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            RequestLoop();
        }

        private void RequestLoop()
        {
            var receiver = Task.Run(() =>
            {
                while (true)
                {
                    ReceiveResponse();
                }
            });
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            IntegrateToView(text);
        }

        private void IntegrateToView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var data = text.Split('\n');
                var row1 = data[0].Split('\t');
                var row2 = data[1].Split('\t');
                var row3 = data[2].Split('\t');

                b1.Content = row1[0];
                b2.Content = row1[1];
                b3.Content = row1[2];

                b4.Content = row2[0];
                b5.Content = row2[1];
                b6.Content = row2[2];

                b7.Content = row3[0];
                b8.Content = row3[1];
                b9.Content = row3[2];
            });
        }

        private void ConnectToServer()
        {
            while (!ClientSocket.Connected)
            {
                try
                {
                    ClientSocket.Connect(IPAddress.Parse("192.168.1.4"), port);
                }
                catch (Exception)
                {
                }
            }

            MessageBox.Show("Connected to game");

            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;

            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text = Encoding.ASCII.GetString(data);
            this.Title = "Player : " + text;
            this.player.Text = this.Title;
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn) return;

            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    var bt = sender as Button;
                    string request = bt.Content.ToString() + player.Text.Split(' ')[2];
                    int buttonIndex = int.Parse(bt.Content.ToString()) - 1;

                    if (gameBoard[buttonIndex] == ' ')
                    {
                        SendString(request);
                        UpdateGameState(buttonIndex, isPlayerTurn ? 'X' : 'O'); 
                        DisableButtons(); 
                    }
                });
            });
        }

        private void SendString(string request)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(request);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void InitializeGameBoard()
        {
            for (int i = 0; i < gameBoard.Length; i++)
            {
                gameBoard[i] = ' ';
            }
        }

        private bool CheckForWin(char player)
        {
            return (gameBoard[0] == player && gameBoard[1] == player && gameBoard[2] == player) ||
                   (gameBoard[3] == player && gameBoard[4] == player && gameBoard[5] == player) ||
                   (gameBoard[6] == player && gameBoard[7] == player && gameBoard[8] == player) ||
                   (gameBoard[0] == player && gameBoard[3] == player && gameBoard[6] == player) ||
                   (gameBoard[1] == player && gameBoard[4] == player && gameBoard[7] == player) ||
                   (gameBoard[2] == player && gameBoard[5] == player && gameBoard[8] == player) ||
                   (gameBoard[0] == player && gameBoard[4] == player && gameBoard[8] == player) ||
                   (gameBoard[2] == player && gameBoard[4] == player && gameBoard[6] == player);
        }

        private bool IsBoardFull()
        {
            return gameBoard.All(cell => cell != ' ');
        }

        private void UpdateGameState(int index, char playerSymbol)
        {
            gameBoard[index] = playerSymbol;

            if (CheckForWin(playerSymbol))
            {
                string winner = playerSymbol == 'X' ? "Player X" : "Player O";
                MessageBox.Show($"{winner} wins!");
                ResetGame();
                return;
            }

            if (IsBoardFull())
            {
                MessageBox.Show("It's a draw!");
                ResetGame();
                return;
            }

            isPlayerTurn = !isPlayerTurn;
            if (isPlayerTurn)
            {
                EnableButtons();
            }
        }

        private void ResetGame()
        {
            InitializeGameBoard();
            EnableButtons();
        }

        private void DisableButtons()
        {
            isPlayerTurn = false;
            b1.IsEnabled = false;
            b2.IsEnabled = false;
            b3.IsEnabled = false;
            b4.IsEnabled = false;
            b5.IsEnabled = false;
            b6.IsEnabled = false;
            b7.IsEnabled = false;
            b8.IsEnabled = false;
            b9.IsEnabled = false;
        }

        private void EnableButtons()
        {
            isPlayerTurn = true;
            b1.IsEnabled = true;
            b2.IsEnabled = true;
            b3.IsEnabled = true;
            b4.IsEnabled = true;
            b5.IsEnabled = true;
            b6.IsEnabled = true;
            b7.IsEnabled = true;
            b8.IsEnabled = true;
            b9.IsEnabled = true;
        }
    }
}
