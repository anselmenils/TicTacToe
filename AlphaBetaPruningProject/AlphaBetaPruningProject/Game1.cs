using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AlphaBetaPruningProject
{
    //Position for the mouse
    public class Position
    {
        public int x;
        public int y;
        public Position(int xPos, int yPos)
        {
            x = xPos;
            y = yPos;
        }
    }

    //Board configuration stored
    public class TicTacToeBoard
    {
        //int 0 = empty, 1 for Circle, 2 for X
        public int[,] boardConfig = new int[5, 4];
        public int alpha = -1000;
        public int beta = 1000;
        public int cost = -10000;

        public TicTacToeBoard( )
        {
            for (int i = 0; i < boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < boardConfig.GetLength(1); j++)
                {
                    boardConfig[i, j] = 0;
                }
            }
        }

        public TicTacToeBoard copy( )
        {
            TicTacToeBoard copyBoard = new TicTacToeBoard( );
            copyBoard.alpha = this.alpha;
            copyBoard.beta = this.beta;
            for (int i = 0; i < boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < boardConfig.GetLength(1); j++)
                {
                    copyBoard.boardConfig[i, j] = this.boardConfig[i, j];
                }
            }
            return copyBoard;
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Sprites
        Texture2D boardSprite;
        Texture2D circleSprite;
        Texture2D crossSprite;
        Texture2D firstButton;
        Texture2D secondButton;
        Texture2D xWin;
        Texture2D oWin;
        Texture2D draw;

        //Initial size of board
        const int BOARD_WIDTH = 500;
        const int BOARD_HEIGHT = 400;

        const int boxHeight = 100;
        const int boxWidth = 100;


        TicTacToeBoard newBoard = new TicTacToeBoard( );

        //Determines if player wants to go first
        bool playerSelected = false;
        //Determine if its the player's turn
        bool playerTurn = true;
        //determine if its the O or the X turn
        bool oTurn = true;
        //Did we win? 
        Position[] won = null;  // Positions that are part of the win.  
        bool aDraw = false;
        bool xWins = false;
        bool oWins = false;

        List<TicTacToeBoard> aiChoices = new List<TicTacToeBoard>();

        //save the last mouse state to ensure that when we click it does not alternate between click and non-click
        MouseState lastState = Mouse.GetState();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // game board area 500 x 400
            graphics.PreferredBackBufferWidth = BOARD_WIDTH;
            graphics.PreferredBackBufferHeight = BOARD_HEIGHT;

            IsMouseVisible = true;  // We want to see the mouse OR DO WE?
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //Load the sprite images
            boardSprite = Content.Load<Texture2D>(@"Sprites\Board");
            circleSprite = Content.Load<Texture2D>(@"Sprites\Circle");
            crossSprite = Content.Load<Texture2D>(@"Sprites\Cross");
            firstButton = Content.Load<Texture2D>(@"Sprites\FirstButton");
            secondButton = Content.Load<Texture2D>(@"Sprites\SecondButton");
            xWin = Content.Load<Texture2D>(@"Sprites\XWins");
            oWin = Content.Load<Texture2D>(@"Sprites\OWins");
            draw = Content.Load<Texture2D>(@"Sprites\Draw");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            //Press R reset the game
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                Reset();
            }

            //Win state - check if reached
            bool terminalState = TerminalTest(newBoard);
            if (terminalState)
            {
                won = WinState(newBoard);
                if (won != null)
                {
                    if (newBoard.boardConfig[won[0].x, won[0].y] == 1)
                    {
                        oWins = true;
                    }
                    else if (newBoard.boardConfig[won[0].x, won[0].y] == 2)
                    {
                        xWins = true;
                    }
                }
                aDraw = true;
            }

            if (!playerTurn && !terminalState)
            {
                AlphaBetaPruning();
            }


            //handles mouse interaction with Screen
            HandleMouse();


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>    
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (xWins)
                spriteBatch.Draw(xWin, Vector2.Zero, Color.White);
            else if (oWins)
                spriteBatch.Draw(oWin, Vector2.Zero, Color.White);
            else if (aDraw)
                spriteBatch.Draw(draw, Vector2.Zero, Color.White);
            //if player did select to play
            else if ( playerSelected )
            {
                //draw the board
                spriteBatch.Draw(boardSprite, Vector2.Zero, Color.White);
                //draw the X and Y
                for (int i = 0; i < newBoard.boardConfig.GetLength(0); i++)
                {
                    for (int j = 0; j < newBoard.boardConfig.GetLength(1); j++)
                    {
                        if (newBoard.boardConfig[i, j] == 1)
                        {
                            spriteBatch.Draw(circleSprite, new Vector2(i * (boxWidth), j * (boxHeight)), Color.White);
                        }
                        else if( newBoard.boardConfig[i, j] == 2 )
                        {
                            spriteBatch.Draw(crossSprite, new Vector2(i * (boxWidth), j * (boxHeight)), Color.White);
                        }
                    }
                }
            }
            //player did not select - draw the buttons
            else
            {
                spriteBatch.Draw(firstButton, new Vector2(150, 50), Color.White);
                spriteBatch.Draw(secondButton, new Vector2(150, 250), Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        //is the mouse over the second button
        private bool OverFirstButton()
        {
            float xPos = Mouse.GetState().X;
            float yPos = Mouse.GetState().Y;
            return xPos > 150 && xPos < 150 + firstButton.Width && yPos > 50 && yPos < 50 + firstButton.Height;
        }

        //Is the mouse over the first button
        private bool OverSecondButton()
        {
            float xPos = Mouse.GetState().X;
            float yPos = Mouse.GetState().Y;
            //BOARD_WIDTH - secondButton.Width, (BOARD_HEIGHT - secondButton.Height) / 2
            return xPos > 150 && xPos < 150 + secondButton.Width && yPos > 250 && yPos < 250 + secondButton.Height;
        }

        //Compute position of the Mouse
        private Position ComputePosition()
        {
            //current position of x and y coordinates of mouse click
            float xPos = Mouse.GetState().X;
            float yPos = Mouse.GetState().Y;
            //if you clicked outside the game - returns a null
            if (xPos < 0 || xPos > 5 * boxWidth ||
                yPos < 0 || yPos > 4 * boxHeight)
                return null;
            int x = (int)(xPos / (boxWidth));
            int y = (int)(yPos / boxHeight);

            //return the position (with x and y coordinates) of the mouse
            return new Position(x, y);
        }

        //Mouse stuff if we click here is the code
        private void HandleMouse()
        {
            //get the current state of the mouse - what button is clicked, etc.
            MouseState mouseState = Mouse.GetState();
            if (IsActive // Are we the active application?
                && mouseState.LeftButton == ButtonState.Pressed // Is the left button pressed
                && lastState.LeftButton == ButtonState.Released  // And was it not pressed last time around?
                && playerTurn) //only player can click
            {
                //if we didn't win
                if (won == null)
                {
                    //get the current position of the board
                    Position pos = ComputePosition();
                    if (pos != null && newBoard.boardConfig[pos.x, pos.y] == 0 && playerSelected)
                    {
                        // Click was in a valid posistion that wasn't occupied
                        // so set the board position to the current player's sprite
                        newBoard.boardConfig[pos.x, pos.y] = (oTurn ? 1:2); //draw based on circle or cross's turn
                        oTurn = !oTurn;  // Toggle whose turn it is
                        playerTurn = !playerTurn; //Toggle Player or AI
                    }
                    //if a choice wasn't selected - check to see if player clicked over one of the two buttoms
                    else if (pos != null && !playerSelected)
                    {
                        //First button choice - set to player = first
                        if (OverFirstButton())
                        {
                            playerTurn = true;
                            playerSelected = true;
                        }
                        //second button choice-  set to player = second
                        else if (OverSecondButton())
                        {
                            playerSelected = true;
                            playerTurn = false;
                        }
                    }
                }
            }
            //useful to stop the alternating click problem
            lastState = mouseState;
        }
    
        //reset instead of closing and opening game
        private void Reset()
        {
            //reset all instances of globals
            oTurn = true;
            playerTurn = true;
            playerSelected = false;

            newBoard = new TicTacToeBoard( );
            aiChoices = new List<TicTacToeBoard>();

            won = null;
            aDraw = false;
            xWins = false;
            oWins = false;
        }

        //test if we have  won
        private Position[] WinState(TicTacToeBoard state)
        {
            //loop through everything in the game board - check for win states
            for (int i = 0; i < state.boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < state.boardConfig.GetLength(1); j++)
                {
                    //check if current position in empty
                    if (state.boardConfig[i, j] != 0)
                    {
                        if (i - 2 >= 0 && j - 2 >= 0)
                        {
                            if (state.boardConfig[i - 2, j - 2] == state.boardConfig[i - 1, j - 1] &&
                                state.boardConfig[i - 1, j - 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i - 2, j - 2), new Position(i - 1, j - 1), new Position(i, j) };
                            }
                        }
                        if (j - 2 >= 0)
                        {

                            if (state.boardConfig[i, j - 2] == state.boardConfig[i, j - 1] &&
                                state.boardConfig[i, j - 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i - 2, j), new Position(i - 1, j), new Position(i, j) };
                            }
                        }
                        if (i + 2 < state.boardConfig.GetLength(0) && j - 2 >= 0)
                        {

                            if (state.boardConfig[i + 2, j - 2] == state.boardConfig[i + 1, j - 1] &&
                                state.boardConfig[i + 1, j - 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i + 2, j - 2), new Position(i + 1, j - 1), new Position(i, j) };
                            }
                        }
                        if (i - 2 >= 0)
                        {

                            if (state.boardConfig[i - 2, j] == state.boardConfig[i - 1, j] &&
                                state.boardConfig[i - 1, j] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i - 2, j), new Position(i - 1, j), new Position(i, j) };

                            }
                        }
                        if (i + 2 < state.boardConfig.GetLength(0))
                        {

                            if (state.boardConfig[i + 2, j] == state.boardConfig[i + 1, j] &&
                                state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i + 2, j), new Position(i + 1, j), new Position(i, j) };
                            }
                        }
                        if (i - 2 >= 0 && j + 2 < state.boardConfig.GetLength(1))
                        {

                            if (state.boardConfig[i - 2, j + 2] == state.boardConfig[i - 1, j + 1] &&
                                state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i - 2, j + 2), new Position(i - 1, j + 1), new Position(i, j) };
                            }
                        }
                        if (j + 2 < state.boardConfig.GetLength(1))
                        {
                            if (state.boardConfig[i, j + 2] == state.boardConfig[i, j + 1] &&
                                state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i, j + 2), new Position(i, j + 1), new Position(i, j) };
                            }
                        }
                        if (i + 2 < state.boardConfig.GetLength(0) && j + 2 < state.boardConfig.GetLength(1))
                        {

                            if (state.boardConfig[i + 2, j + 2] == state.boardConfig[i + 1, j + 1] &&
                                state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                            {
                                return new Position[] { new Position(i + 2, j + 2), new Position(i + 1, j + 1), new Position(i, j) };
                            }
                        }
                    }
                }
            }
            //did not reach a win state 
            return null;
        }

        //test if we are in a terminal state
        private bool TerminalTest(TicTacToeBoard state)
        {
            //Terminal State if we win
            Position[] winStates = WinState(state);
            if (winStates != null)
            {
                return true;
            }
            //if not win, if the parts of the board is  empty, game doesn't end
            for (int i = 0; i < state.boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < state.boardConfig.GetLength(1); j++)
                {
                    if (state.boardConfig[i, j] == 0)
                        return false;
                }
            }
            //board is full, terminal state and we didn't win 
            return true;
        }

        //Search for the AI
        private void AlphaBetaPruning()
        {
            bool tempOTurn = oTurn;
            int counter = 0;
            aiChoices = new List<TicTacToeBoard>();
            newBoard.alpha = -1000;
            newBoard.beta = 1000;
            newBoard.cost = -10000;
            int v = Max_value(ref newBoard, ref tempOTurn, ref counter, 1);
            Position theMove = new Position(-1, -1);
            for (int i = 0; i < aiChoices.Count; i++)
            {
                if (aiChoices[i].cost == v)
                {
                    theMove = determineMove(newBoard, aiChoices[i]);
                    break;
                }
            }
            if (theMove.x == -1 || theMove.y == -1)
            {
                theMove = determineMove(newBoard, aiChoices[0]);
            }
            newBoard.boardConfig[theMove.x, theMove.y] = (oTurn ? 1 : 2);
            oTurn = !oTurn;  // Toggle whose turn it is
            playerTurn = !playerTurn; //Toggle Player or AI
        }

        private int Max_value(ref TicTacToeBoard state, ref bool tempOTurn, ref int counter, int depth)
        {
            if (( depth == 4) || TerminalTest(state))
                return utility(ref state, depth, ref tempOTurn);
            int v = -1000;
            Queue<TicTacToeBoard> children = GenerateSuccessors(state, ref tempOTurn, ref counter);
            while (children.Count != 0)
            {
                TicTacToeBoard next = children.Dequeue();
                tempOTurn = !tempOTurn;
                v = Math.Max(v, Min_value(ref next, ref tempOTurn, ref counter,  depth + 1));
                state.cost = v;
                if (v >= state.beta)
                    return v;
                state.alpha = Math.Max(state.alpha, v);
            }
            return v;
        }

        private int Min_value(ref TicTacToeBoard state, ref bool tempOTurn, ref int counter, int depth)
        {
            if ((depth == 4) || TerminalTest(state) )
                return utility(ref state, depth, ref tempOTurn);
            int v = 1000;
            Queue<TicTacToeBoard> children = GenerateSuccessors(state, ref tempOTurn, ref counter);
            while (children.Count != 0)
            {
                TicTacToeBoard next = children.Dequeue();
                tempOTurn = !tempOTurn;
                v = Math.Min(v, Max_value(ref next, ref tempOTurn, ref counter, depth));
                state.cost = v;
                if (v <= state.alpha)
                    return v;
                state.beta = Math.Min(v, state.beta);
            }
            return v;
        }


        //get the children from current state
        private Queue<TicTacToeBoard> GenerateSuccessors(TicTacToeBoard state, ref bool tempOTurn, ref int counter)
        {
            Queue<TicTacToeBoard> children = new Queue<TicTacToeBoard>();
            for (int i = 0; i < state.boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < state.boardConfig.GetLength(1); j++)
                {
                    TicTacToeBoard copy = state.copy();
                    if (copy.boardConfig[i, j] == 0)
                    {
                        if( counter == 0 )
                            aiChoices.Add(copy);
                        copy.boardConfig[i, j] = (tempOTurn ? 1 : 2);
                        children.Enqueue(copy);
                    }
                }
            }
            counter++;
            return children;
        }

        //utility function of Alpha beta pruning - will only enter is terminal state
        private int utility(ref TicTacToeBoard state, int depth, ref bool tempOTurn) //replace this with file input of all possible states - and weights
        {
            int cost = 0;
            for (int i = 0; i < state.boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < state.boardConfig.GetLength(1); j++)
                {
                    if (oTurn)
                    {
                        if (state.boardConfig[i, j] == 1)
                        {
                            if (i + 1 < state.boardConfig.GetLength(0))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0))
                                {
                                    if (state.boardConfig[i + 2, j] == state.boardConfig[i + 1, j] &&
                                        state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && state.boardConfig[i - 1, j] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i, j + 2] == state.boardConfig[i, j + 1] &&
                                        state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        if (j - 1 >= 0 && state.boardConfig[i, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (i + 1 < state.boardConfig.GetLength(0) && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0) && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i + 2, j + 2] == state.boardConfig[i + 1, j + 1] &&
                                        state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && j - 1 >= 0 && state.boardConfig[i - 1, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (i - 1 >= 0 && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i - 2 >= 0 && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i - 2, j + 2] == state.boardConfig[i - 1, j + 1] &&
                                        state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        if (i + 1 < state.boardConfig.GetLength(0) && j - 1 >= 0 &&
                                            state.boardConfig[i + 1, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                        }
                        else if (state.boardConfig[i, j] == 2)
                        {
                            if (i + 1 < state.boardConfig.GetLength(0))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0))
                                {
                                    if (state.boardConfig[i + 2, j] == state.boardConfig[i + 1, j] &&
                                        state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && state.boardConfig[i - 1, j] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i, j + 2] == state.boardConfig[i, j + 1] &&
                                        state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        if (j - 1 >= 0 && state.boardConfig[i, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (i + 1 < state.boardConfig.GetLength(0) && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0) && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i + 2, j + 2] == state.boardConfig[i + 1, j + 1] &&
                                        state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && j - 1 >= 0 && state.boardConfig[i - 1, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (i - 1 >= 0 && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i - 2 >= 0 && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i - 2, j + 2] == state.boardConfig[i - 1, j + 1] &&
                                        state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        if (i + 1 < state.boardConfig.GetLength(0) && j - 1 >= 0 &&
                                            state.boardConfig[i + 1, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                        }
                    }
                    else if (!oTurn)
                    {
                        if (state.boardConfig[i, j] == 2)
                        {
                            if (i + 1 < state.boardConfig.GetLength(0))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0))
                                {
                                    if (state.boardConfig[i + 2, j] == state.boardConfig[i + 1, j] &&
                                        state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && state.boardConfig[i - 1, j] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i, j + 2] == state.boardConfig[i, j + 1] &&
                                        state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        if (j - 1 >= 0 && state.boardConfig[i, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (i + 1 < state.boardConfig.GetLength(0) && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0) && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i + 2, j + 2] == state.boardConfig[i + 1, j + 1] &&
                                        state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && j - 1 >= 0 && state.boardConfig[i - 1, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                            if (i - 1 >= 0 && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i - 2 >= 0 && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i - 2, j + 2] == state.boardConfig[i - 1, j + 1] &&
                                        state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        cost += 100;
                                }
                                else
                                    if (state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        if (i + 1 < state.boardConfig.GetLength(0) && j - 1 >= 0 &&
                                            state.boardConfig[i + 1, j - 1] != state.boardConfig[i, j])
                                            cost += 10;
                            }
                        }
                        else if (state.boardConfig[i, j] == 1)
                        {
                            if (i + 1 < state.boardConfig.GetLength(0))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0))
                                {
                                    if (state.boardConfig[i + 2, j] == state.boardConfig[i + 1, j] &&
                                        state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && state.boardConfig[i - 1, j] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i, j + 2] == state.boardConfig[i, j + 1] &&
                                        state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i, j + 1] == state.boardConfig[i, j])
                                        if (j - 1 >= 0 && state.boardConfig[i, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (i + 1 < state.boardConfig.GetLength(0) && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i + 2 < state.boardConfig.GetLength(0) && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i + 2, j + 2] == state.boardConfig[i + 1, j + 1] &&
                                        state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i + 1, j + 1] == state.boardConfig[i, j])
                                        if (i - 1 >= 0 && j - 1 >= 0 && state.boardConfig[i - 1, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                            if (i - 1 >= 0 && j + 1 < state.boardConfig.GetLength(1))
                            {
                                if (i - 2 >= 0 && j + 2 < state.boardConfig.GetLength(1))
                                {
                                    if (state.boardConfig[i - 2, j + 2] == state.boardConfig[i - 1, j + 1] &&
                                        state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        cost -= 100;
                                }
                                else
                                    if (state.boardConfig[i - 1, j + 1] == state.boardConfig[i, j])
                                        if (i + 1 < state.boardConfig.GetLength(0) && j - 1 >= 0 &&
                                            state.boardConfig[i + 1, j - 1] != state.boardConfig[i, j])
                                            cost -= 10;
                            }
                        }
                    }

                }
            }

            state.cost = cost;
            return cost;
        }

        public Position determineMove(TicTacToeBoard state, TicTacToeBoard otherState )
        {
            for (int i = 0; i < state.boardConfig.GetLength(0); i++)
            {
                for (int j = 0; j < state.boardConfig.GetLength(1); j++)
                {
                    if (state.boardConfig[i, j] != otherState.boardConfig[i, j])
                    {
                        Position newPos = new Position( i, j );
                        return newPos;
                    }
                }
            }
            return null;
        }
        
    }
 }
