using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easter2016
{
    class SimpleSpriteManager: DrawableGameComponent
    {
        public enum GameState { WON, LOST, PLAYING, OVER}

        GameState currentGameState = GameState.PLAYING;

        SoundEffectInstance _audioPlayer;
        List<SimpleSprite> _blackKnights = new List<SimpleSprite>();
        LinkedList<TimedSprite> timed = new LinkedList<TimedSprite>();

        // Solution to Final Assessment setup Queue
        Queue<SimpleSprite> Icons = new Queue<SimpleSprite>();
        SimpleSprite CurrentIcon = null;
        SimpleSprite Background = null;
        SimpleSprite WinnerScreen = null;
        SimpleSprite LooserScreen = null;

        TimeSpan TimeOut = new TimeSpan(0, 0, 2);
        TimeSpan zeroTime = new TimeSpan();

        // Icon Variables
        Vector2 IconBasePosition = new Vector2(20, 20);
        int targetCount = 2;
        bool TargetReached = false;
        
        // Game over State and Winner state
        bool GameOver = false;
        bool Winner = false;

        TimeSpan TimePassed;
        Player player;
        Tower startTower;
        Tower playerTower;

        public SimpleSpriteManager(Game g) : base(g)
        {
            g.Components.Add(this);
        }

        protected override void LoadContent()
        {
            LoadAssets();
            setupObjects();
            //for (int i = 0; i < 5; i++)
            //{
            //    TimedSprite next = new TimedSprite(Game, "cannonball", new Vector2(Utilities.Utility.NextRandom(200), Utilities.Utility.NextRandom(200)));
            //    addtoTimes(next);
            //}
            
            base.LoadContent();
        }

        private void addtoTimes(TimedSprite next)
        {
            if (timed.Count == 0)
                timed.AddFirst(next);
            else
            {
                LinkedListNode<TimedSprite> current = timed.First;
                while (current != timed.Last && next.Activate >= current.Value.Activate)
                    current = current.Next;
                if (current == timed.Last && next.Activate >= current.Value.Activate)
                    timed.AddAfter(timed.First, next);
                else
                    timed.AddBefore(current, next);
            }

        }

        private void removeSimpleSpriteComponents()
        {
            var removalList = Game.Components.OfType<SimpleSprite>().Where(s => s.Alive == false).ToList();
            if (removalList.Count() > 0)
                foreach (SimpleSprite deadCharacter in removalList)
                    Game.Components.Remove(deadCharacter);
        }

        private void setupObjects()
        {

            // Players Tower is bottom left
            Vector2 PlayerTowerPos = new Vector2(0,
            GraphicsDevice.Viewport.Height - LoadedGameContent.Textures["End Tower"].Height
            
            );

            // Player is placed bottom left of the Viewport
            Vector2 playerPosition = PlayerTowerPos + new Vector2(LoadedGameContent.Textures["Player"].Width,
                        -LoadedGameContent.Textures["Player"].Height
                        ) ;

            Vector2 startTowerPos = new Vector2(GraphicsDevice.Viewport.Width - LoadedGameContent.Textures["Start Tower"].Width,
            0
            );


            player = new Player(Game, "Player", playerPosition);
            playerTower = new Tower(Game, "End Tower", PlayerTowerPos );
            startTower = new Tower(Game, "Start Tower", startTowerPos);

            for (int i = 0; i < 5; i++)
            {
                Stack<Vector2> path = new Stack<Vector2>();
                path.Push(PlayerTowerPos);
                path.Push(new Vector2(Utilities.Utility.NextRandom(200), Utilities.Utility.NextRandom(400)));
                SimpleSprite s = new SimpleSprite(Game, "Black Knight", startTowerPos, path);
                _blackKnights.Add(s);
                
            }
            _blackKnights.First().Active = true;
            _blackKnights.First().followPath();

            // Setup Background and make sure it draws behind the other objects
            // by settings its draw order to -1
            Background = new SimpleSprite(Game, "Background", Vector2.Zero);
            Background.DrawOrder = -1;
            Background.Active = true;

        }

        public  void monitorKnights()
        {
            // if they are not all stopped then there is at least one active
            var _activeKnights = _blackKnights.Where(k => !k.Stopped() && k.Active);
            if (_activeKnights.Count() < 1)
            {
                // then the inactive one has been deleted so activate the next one and add
                // a new one
                Vector2 startTowerPos = new Vector2(GraphicsDevice.Viewport.Width - LoadedGameContent.Textures["Start Tower"].Width,
                            0);
                Vector2 target = new Vector2(0,
                                    GraphicsDevice.Viewport.Height - LoadedGameContent.Textures["End Tower"].Height
                                    );
                // Add a new one
                Stack<Vector2> path = new Stack<Vector2>();
                path.Push(target);
                path.Push(new Vector2(Utilities.Utility.NextRandom(200), Utilities.Utility.NextRandom(400)));
                SimpleSprite s = new SimpleSprite(Game, "Black Knight", startTowerPos, path);
                _blackKnights.Add(s);
                // acticate the next one at the head of the list
                _blackKnights.First().Active = true;
                _blackKnights.First().followPath();
            }
            else 
            {
                // Check for collision with the tower 
                // NOTE: we only delete the first one.
                // Subsequent updates will call this again and delete others one at a time
                // otherwise we get a iteration error over active knights as the referennce to the object disappears 
                // When removed from the _blackknoghts collection and the Game Component collection
                foreach (var enemy in _activeKnights)
                {
                    if (playerTower.Collision(enemy))
                    {
                        LoadedGameContent.Sounds["Impact"].Play();
                        Game.Components.Remove(enemy);
                        _blackKnights.Remove(enemy);
                        // update the health of the player and the player healthbar
                        playerTower.Health -= 20;
                        if (playerTower.Health < 1)
                        {
                            Winner = false;
                            currentGameState = GameState.LOST;
                        }
                        break;
                   }
                }

            }
        }

        private void LoadAssets()
        {
            // load sounds
            LoadedGameContent.Sounds.Add("backing", Game.Content.Load<SoundEffect>("Backing Track wav"));
            LoadedGameContent.Sounds.Add("cannon fire", Game.Content.Load<SoundEffect>("cannon fire"));
            LoadedGameContent.Sounds.Add("Impact", Game.Content.Load<SoundEffect>("Impact"));
            LoadedGameContent.Sounds.Add("Winning Track", Game.Content.Load<SoundEffect>("Winning Track"));
            LoadedGameContent.Sounds.Add("Losing Track", Game.Content.Load<SoundEffect>("Lose"));

            // load Textures
            LoadedGameContent.Textures.Add("Black Knight", Game.Content.Load<Texture2D>("Black Knight"));
            LoadedGameContent.Textures.Add("cannonball", Game.Content.Load<Texture2D>("cannonball"));
            LoadedGameContent.Textures.Add("Start Tower", Game.Content.Load<Texture2D>("Start Tower"));
            LoadedGameContent.Textures.Add("End Tower", Game.Content.Load<Texture2D>("End Tower"));
            LoadedGameContent.Textures.Add("Player", Game.Content.Load<Texture2D>("Player"));
            LoadedGameContent.Textures.Add("Icon", Game.Content.Load<Texture2D>("mini Black Knight"));
            LoadedGameContent.Textures.Add("Background", Game.Content.Load<Texture2D>("Background scene2"));
            LoadedGameContent.Textures.Add("Winner", Game.Content.Load<Texture2D>("Winner"));

            LoadedGameContent.Fonts.Add("SimpleSpriteFont", Game.Content.Load<SpriteFont>("SimpleSpriteFont"));
            _audioPlayer = LoadedGameContent.Sounds["backing"].CreateInstance();
            _audioPlayer.Volume = 0.2f;
            _audioPlayer.IsLooped = true;
            _audioPlayer.Play();

        }

        public void MonitorCannonBalls()
        {
            // remove any cannon all that is not moving
            var removalList = Game.Components.OfType<SimpleSprite>()
                .Where(s => s.Stopped() && s.Name == "cannonball").ToList();
            if(removalList.Count() > 0)
                LoadedGameContent.Sounds["Impact"].Play();
            foreach (var item in removalList)
                Game.Components.Remove(item);
            // get the active cannon balls
            var activeCannonBalls = Game.Components.OfType<SimpleSprite>()
                .Where(s => !s.Stopped() && s.Name == "cannonball").ToList();
            // Get the active enemies
            var enemies = Game.Components.OfType<SimpleSprite>()
                .Where(s => !s.Stopped() && s.Name == "Black Knight").ToList();

            // check collisions between cannon balls and enemies
            foreach (var b in activeCannonBalls)
            {
                foreach (var enemy in enemies)
                {
                    if (b.Collision(enemy))
                    {
                        LoadedGameContent.Sounds["Impact"].Play();
                        Game.Components.Remove(b);
                        Game.Components.Remove(enemy);
                        _blackKnights.Remove(enemy);
                        addIcon(enemy);
                    }
                }
            }
        }

        private void addIcon(SimpleSprite enemy)
        {
            if(Icons.Count < 1)
            {
                SimpleSprite Icon = new SimpleSprite(Game, "Icon", enemy.Currentposition);
                Icon.Active = true;
                Icon.moveTo(IconBasePosition);
                Icons.Enqueue(Icon);
            }
            else
            {
                Vector2 nextposition = IconBasePosition + new Vector2((LoadedGameContent.Textures["Icon"].Width * Icons.Count ) + 10, 0);
                SimpleSprite Icon = new SimpleSprite(Game, "Icon", enemy.Currentposition);
                Icon.Active = true;
                Icon.moveTo(nextposition);
                Icons.Enqueue(Icon);
            }

            if(Icons.Count >= targetCount)
            {
                TargetReached = true;
                currentGameState = GameState.WON;
            }
        }

        private void loose()
        {
            if(LooserScreen == null)
            {
                // turn off the active background
                if(Background != null)
                {
                    Background.Active = false;
                }
                // Activate the Looser screen
                // Note all the other game objects are active
                LooserScreen = new SimpleSprite(Game, "Looser", Vector2.Zero);
                LooserScreen.DrawOrder = -1;
                LooserScreen.Active = true;
                _audioPlayer.Stop();
                _audioPlayer = LoadedGameContent.Sounds["Losing Track"].CreateInstance();
                _audioPlayer.Play();
            }
            // The end of the track marks the end of the game 
            else if(_audioPlayer.State == SoundState.Stopped)
            { currentGameState = GameState.OVER; }
        }
        public override void Update(GameTime gameTime)
        {
            switch (currentGameState)
            {
                case GameState.PLAYING:
                if (!TargetReached)
                    {
                        TimePassed = gameTime.TotalGameTime;
                        //checkTimedObjects();
                        MonitorCannonBalls();
                        monitorKnights();
                        currentGameState = GameState.PLAYING;
                    }
                    break;
                case GameState.LOST:
                    loose();
                    break;
                case GameState.WON:
                    playTargetReached();
                    break;
                case GameState.OVER:
                    Game.Exit();
                    break;
                default:
                    break;
            }
            base.Update(gameTime);
        }

        private void playTargetReached()
        {
            // If we have reached the target then display the 
            if(WinnerScreen == null)
            {
                Background.Active = false;
                WinnerScreen = new SimpleSprite(Game, "Winner",Vector2.Zero);
                WinnerScreen.DrawOrder = -1;
                WinnerScreen.Active = true;
                _audioPlayer.Stop();
                _audioPlayer = LoadedGameContent.Sounds["Winning Track"].CreateInstance();
                _audioPlayer.Play();

            }
            // Remove any active Blacknights and Cannon Balls
            List<SimpleSprite> remaining = Game.Components.OfType<SimpleSprite>()
                            .Where(s => s.Name == "Black Knight" || s.Name == "cannonball").ToList();
            foreach(SimpleSprite s in remaining)
                Game.Components.Remove(s);

            // No Icon activated and Icons not dealth with
            if (CurrentIcon == null && Icons.Count > 0)
            {
                CurrentIcon = (SimpleSprite)Icons.Dequeue();
                Vector2 target = new Vector2(0,
                                    GraphicsDevice.Viewport.Height
                                            - LoadedGameContent.Textures["End Tower"].Height
                                    );
                CurrentIcon.Path.Push(target);
                CurrentIcon.Path.Push(new Vector2(200, 400));
                CurrentIcon.followPath();
            }
            else if (CurrentIcon != null && CurrentIcon.Stopped())
            {
                SimpleSprite removalitem = Game.Components.OfType<SimpleSprite>()
                    .Where(s => s.Id == CurrentIcon.Id).FirstOrDefault();
                if (removalitem != null) { Game.Components.Remove(removalitem); }
                CurrentIcon = null;
            }
            // Check for End Game State when all the Icons have made it home and the music has finished
            else if (Icons.Count < 1 && _audioPlayer.State == SoundState.Stopped)
            {
                // end of procession marks and end of Audio play marks
                currentGameState = GameState.OVER;
                GameOver = true;
            }
        }

        private void checkTimedObjects()
        {
            var deadTimed = Game.Components.OfType<TimedSprite>().Where(t => !t.Alive).ToList();
            foreach (TimedSprite t in deadTimed)
            {
                timed.Remove(t);
                Game.Components.Remove(t);
            }
            if(timed.Count < 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    TimedSprite next = new TimedSprite(Game, "cannonball", new Vector2(Utilities.Utility.NextRandom(200), Utilities.Utility.NextRandom(200)));
                    next.Activate += TimePassed;
                    next.Survival += TimePassed;
                    addtoTimes(next);
                }

            }

        }

        public override void Draw(GameTime gameTime)
        {
            // Just drawing the Game time as an example
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            sp.Begin();
            sp.DrawString(LoadedGameContent.Fonts["SimpleSpriteFont"], TimePassed.TotalSeconds.ToString(),new Vector2(10,10), Color.White);
            sp.End();
            base.Draw(gameTime);
        }
    }
}
