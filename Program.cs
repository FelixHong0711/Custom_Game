using System;
using SplashKitSDK;
#nullable disable

public class Program
{
    public static void Main()
    {
        Window newWin = new Window("My Game", 1200, 850);
        GamePlay GamePlay = new GamePlay(newWin);
        // As long as there is no quit request then the program will run infinitely
        do
        {
            SplashKit.ProcessEvents();
            GamePlay.HandleInput();
            GamePlay.Update();
            GamePlay.Draw();
        } while (!newWin.CloseRequested && !GamePlay.Quit);
        newWin.Close();
        newWin = null;
    }
}

// Create ICollidable interface
public interface ICollidable
{
    // Create the common CollisionCircle with public get
    Circle CollisionCircle { get; }
}

public class Player
{
    // Declare _PlayerBitmap
    private Bitmap _PlayerBitmap;
    // Declare _LiveBitmap
    private Bitmap _LiveBitmap;
    // Declare _GameWindow
    private Window _GameWindow;
    // Declare player X position
    public double X { get; private set; }
    // Declare player Y position
    public double Y { get; private set; }
    // Declare Live and set it to 5
    public int Live { get; set; } = 5;
    // Declare live icon X position
    public double X_Live { get; private set; }
    // Declare live icon Y position
    public double Y_Live { get; private set; }
    // Declare UpKeyDownTime
    public DateTime UpKeyDownTime { get; private set; }
    // Declare X position of mouse
    public double MouseX { get; private set; }
    // Declare Y position of mouse
    public double MouseY { get; private set; }
    // Declare Quit and set to false
    public bool Quit { get; private set; } = false;
    // Declare Shoot and set to false
    public bool Shoot { get; private set; } = false;
    // Declare GAP and set it to 10
    const int GAP = 10;
    // Declare Width and make it read width of player bitmap
    public int Width
    {
        get
        {
            return _PlayerBitmap.Width;
        }
    }
    // Declare Height and make it read height of player bitmap
    public int Height
    {
        get
        {
            return _PlayerBitmap.Height;
        }
    }

    // Create constructor
    public Player(Window gameWindow)
    {
        _PlayerBitmap = new Bitmap("Player", "Player.png");
        _LiveBitmap = new Bitmap("Live", "live.png");
        _GameWindow = gameWindow;
        // Create initial location of player
        X = GAP;
        Y = _GameWindow.Height - GAP - _PlayerBitmap.Height;

        // Assign X_Live, Y_Live with appropriate value for displaying
        X_Live = _GameWindow.Width - _LiveBitmap.Width - 100;
        Y_Live = _LiveBitmap.Height + 10;
    }

    // Create Draw method
    public void Draw(Window gameWindow)
    {
        _PlayerBitmap.Draw(X, Y);
        _LiveBitmap.Draw(X_Live, Y_Live);

        // Display information about live and score
        gameWindow.DrawText($"x{Live}", Color.Black, X_Live + 50, Y_Live + 15);
    }
    // Create HandleInput method
    public void HandleInput(List<Bar> bars)
    {
        // Declare SPEED
        const int SPEED = 5;
        // Declare GRAVITY
        const int GRAVITY = 3;

        // Check duration of holding key
        TimeSpan keyHoldDuration = TimeSpan.FromSeconds(1.8);

        // Always apply gravity unless the player is on a bar
        Y += GRAVITY;

        // Create logic for up movement (so it can work in iteration when program is run)
        if (SplashKit.KeyDown(KeyCode.UpKey) || SplashKit.KeyDown(KeyCode.WKey))
        {
            // If UpKeyDownTime is moment of midnight at the beginning of January 1 of the year 1 in the Gregorian calendar then set it to now
            if (UpKeyDownTime == DateTime.MinValue)
            {
                UpKeyDownTime = DateTime.Now;
            }

            // Check that if player hold up key more than keyHoldDuration then the player will move in other direction until unhold the key
            if (DateTime.Now - UpKeyDownTime < keyHoldDuration)
            {
                Y -= SPEED;
            }
            return;
        }
        else
        {
            // If upkey is not press then set UpKeyDownTime to the midnight at the beginning of January 1 of the year 1 in the Gregorian calendar
            UpKeyDownTime = DateTime.MinValue;
        }

        // Check if player collide with bar, if yes then the location of player will be reset to appear on the bar
        foreach (Bar bar in bars)
        {
            if (CollideWith(bar))
            {
                // If the location of player is < than Y location of bar, then player will not standing on bar
                if (Y < bar.Y)
                {
                    Y = bar.Y - Height;
                }
                // If location of player is above bar then it will stand on bar
                else if (Y > bar.Y + bar.Height)
                {
                    Y = bar.Y + bar.Height;
                }
                // The program will end for loop here after checking already the appropriate bar
                break;
            }
        }

        if (SplashKit.KeyDown(KeyCode.RightKey) || SplashKit.KeyDown(KeyCode.DKey)) X += 4;
        if (SplashKit.KeyDown(KeyCode.LeftKey) || SplashKit.KeyDown(KeyCode.AKey)) X -= 4;
        if (SplashKit.KeyDown(KeyCode.EscapeKey)) Quit = true;

        // If click left mouse then assign Shoot = true and identify mouse clicked position
        if (SplashKit.MouseClicked(MouseButton.LeftButton))
        {
            Shoot = true;
            ShootLocation();
        }

    }
    // Create StayOnWindow method
    public void StayOnWindow(Window windowBounder)
    {
        if (X < GAP) X = GAP;

        if (GAP + X + _PlayerBitmap.Width > windowBounder.Width)
        {
            X = windowBounder.Width - _PlayerBitmap.Width - GAP;
        }

        if (Y < GAP) Y = GAP;

        if (GAP + Y + _PlayerBitmap.Height > windowBounder.Height)
        {
            Y = windowBounder.Height - _PlayerBitmap.Height - GAP;
        }
    }

    // Create CollideWith method
    public bool CollideWith(ICollidable other)
    {
        return _PlayerBitmap.CircleCollision(X, Y, other.CollisionCircle);
    }

    // Create CollideWith method for Bar
    public bool CollideWith(Bar other)
    {
        return _PlayerBitmap.RectangleCollision(X, Y, other.CollisionRectangle);
    }

    // Create ShootLocation method
    public void ShootLocation()
    {
        MouseX = SplashKit.MousePosition().X;
        MouseY = SplashKit.MousePosition().Y;
    }
}
public class Food : ICollidable
{
    // Declare _FoodBitmap
    private Bitmap _FoodBitmap;
    // Declare Bar
    private Bar _Bar;
    // Declare Food X location
    public double X
    {
        get
        {
            return _Bar.X + _relativeX;
        }
    }
    // Declare Food Y location
    public double Y
    {
        get
        {
            return _Bar.Y - _FoodBitmap.Height + 5; ;
        }
    }
    // Declare height
    public double Height
    {
        get
        {
            return _FoodBitmap.Height;
        }
    }
    // Declare CollisionCircle
    public Circle CollisionCircle { get; private set; }
    // Declare _relativeX        
    private double _relativeX;

    // Create constructor
    public Food(Bar bar, List<Bar> bars)
    {
        _Bar = bar;

        _FoodBitmap = new Bitmap("Food", "food.jpg"); ;

        // Find lowest bar (lowest Y)
        Bar lowestBar = bars[0];


        foreach (Bar eachBar in bars)
        {
            if (eachBar.Y < lowestBar.Y)
            {
                lowestBar = eachBar;
            }
        }

        // If it is lowest bar, then food will be in the middle of bar else it will be at the corner of bar
        if (_Bar == lowestBar)
        {
            _relativeX = (_Bar.Width - _FoodBitmap.Width) / 2;
        }
        else
        {
            _relativeX = 0;
        }
    }

    // Create Draw method
    public void Draw()
    {
        SplashKit.DrawBitmap(_FoodBitmap, X, Y);
    }

    // Create Update method
    public void Update()
    {
        CollisionCircle = SplashKit.CircleAt(X, Y, 20);
    }
}
public abstract class Enemy : ICollidable
{
    // Declare enemy X position
    protected double X;
    // Declare enemy X position
    protected double Y;
    // Declare Velocity
    public Vector2D Velocity { get; set; }
    // Declare Width
    public int Width
    {
        get { return 50; }
    }
    // Declare Height
    public int Height
    {
        get { return 50; }
    }
    // Declare CollisionCircle
    public Circle CollisionCircle { get; private set; }

    // Create constructor
    public Enemy(Window gameWindow, Player player)
    {
        if (SplashKit.Rnd() < 0.5)
        {
            X = SplashKit.Rnd(gameWindow.Width);
            if (SplashKit.Rnd() < 0.5)
            {
                Y = -Height;
            }
            else
            {
                Y = gameWindow.Height;
            }
        }
        else
        {
            Y = SplashKit.Rnd(gameWindow.Height);
            if (SplashKit.Rnd() < 0.5)
            {
                X = -Width;
            }
            else
            {
                X = gameWindow.Width;
            }
        }
        const int SPEED = 5;
        Point2D fromPt = new Point2D()
        {
            X = X,
            Y = Y
        };
        Point2D toPt = new Point2D()
        {
            X = player.X,
            Y = player.Y
        };
        // Calculate movement speed and direction of the enemy
        Vector2D dir;
        dir = SplashKit.UnitVector(SplashKit.VectorPointToPoint(fromPt, toPt));
        Velocity = SplashKit.VectorMultiply(dir, SPEED);
    }

    // Create abstract Draw method
    public abstract void Draw();

    // Create Update method
    public void Update()
    {
        // Make enemy move on scene
        X += Velocity.X;
        Y += Velocity.Y;
        // Create circle collision
        CollisionCircle = SplashKit.CircleAt(X + Width / 2, Y + Height / 2, 20);
    }

    // Create IsOffscreen method
    public bool IsOffscreen(Window screen)
    {
        if ((X < -Width) || (X > screen.Width) || (Y < -Height) || (Y > screen.Height))
        {
            return true;
        }
        return false;
    }
}


// Create Ghost
public class Ghost : Enemy
{
    public Ghost(Window gameWindow, Player player) : base(gameWindow, player) { }
    public override void Draw()
    {
        Bitmap enemy = new Bitmap("ghost", "ghost.webp");
        enemy.Draw(X, Y);
    }
}

// Create Pig
public class Pig : Enemy
{
    public Pig(Window gameWindow, Player player) : base(gameWindow, player) { }
    public override void Draw()
    {
        Bitmap enemy = new Bitmap("pig", "pig.png");
        enemy.Draw(X, Y);
    }
}

// Create Bat
public class Bat : Enemy
{
    public Bat(Window gameWindow, Player player) : base(gameWindow, player) { }
    public override void Draw()
    {
        Bitmap enemy = new Bitmap("bat", "bat.webp");
        enemy.Draw(X, Y);
    }
}
public class Door : ICollidable
{
    // Declare door X position
    public double X { get; set; }
    // Declare door Y position
    public double Y { get; set; }
    // Declare CollisionCircle
    public Circle CollisionCircle { get; set; }
    // Declare _DoorBitmap
    public Bitmap _DoorBitmap { get; private set; }
    // Declare door Width
    public int Width
    {
        get
        {
            return _DoorBitmap.Width;
        }
    }
    // Declare door Height
    public int Height
    {
        get
        {
            return _DoorBitmap.Height;
        }
    }
    // Create constructor
    public Door()
    {
        _DoorBitmap = new Bitmap("door", "door.jpeg");
    }

    // Create Draw method
    public void Draw()
    {
        _DoorBitmap.Draw(X, Y);
    }

    // Create SpawnOnLowestBar for door to appear on the lowest bar (lowest Y)
    public void SpawnOnLowestBar(List<Bar> bars)
    {
        // Compare each bar in list
        if (bars.Count > 0)
        {
            Bar lowestBar = bars[0];
            foreach (var bar in bars)
            {
                if (bar.Y < lowestBar.Y)
                {
                    lowestBar = bar;
                }
            }

            // Position for draw door
            X = lowestBar.X;
            // +12 because I want door to have some gap with border of window
            Y = lowestBar.Y - Height + 12;
            // Declare CollisionCircle
            CollisionCircle = SplashKit.CircleAt(X + (Width / 2), Y + (Height / 2), 20);
        }
    }
}
public class Bullet
{
    // Declare bullet X position
    public double X { get; private set; }
    // Declare bullet Y position
    public double Y { get; private set; }
    // Declare Velocity
    private Vector2D Velocity { get; set; }
    // Declare Radius and set it to 20
    public double Radius { get; private set; } = 20;
    // Declare Radius and set it to 20
    private bool IsShot = false;

    // Create constructor
    public Bullet(Player player)
    {
        // Bullet location will start from X, Y of player
        X = player.X;
        Y = player.Y;
    }

    // Create Shoot method
    public void Shoot(Player player)
    {
        // if bullet is already shot and player click then calculate direction of bullet
        if (!IsShot && player.Shoot)
        {
            const int SPEED = 5;
            Point2D fromPt = new Point2D()
            {
                X = player.X,
                Y = player.Y
            };
            Point2D toPt = new Point2D()
            {
                X = player.MouseX,
                Y = player.MouseY
            };

            // Calculate movement speed and direction of the bullet
            Vector2D dir;
            dir = SplashKit.UnitVector(SplashKit.VectorPointToPoint(fromPt, toPt));
            Velocity = SplashKit.VectorMultiply(dir, SPEED);
            // Set IsShot to true so that when player click again the bullet that has been already shot won't change its direction
            IsShot = true;
        }
    }

    // Create Update method
    public void Update()
    {
        // if the bullet is shot then create movement scene for bullet
        if (IsShot)
        {
            X += Velocity.X;
            Y += Velocity.Y;
        }
    }

    // Create Draw method
    public void Draw()
    {
        // if the bullet is shot then draw bullet
        if (IsShot)
        {
            SplashKit.FillCircle(Color.DarkRed, X, Y, Radius);
        }
    }

    // Create CollideWith method
    public bool CollideWith(Enemy other)
    {
        return IsShot && SplashKit.CirclesIntersect(SplashKit.CircleAt(X, Y, Radius), other.CollisionCircle);
    }

    // Create IsOffscreen method
    public bool IsOffscreen(Window screen)
    {
        if (X > screen.Width || Y > screen.Height || X < 0 || Y < 0)
        {
            return true;
        }
        return false;
    }
}
public class Bar
{
    // Declare X position of bar
    public double X { get; private set; }
    // Declare Y position of bar
    public double Y { get; private set; }
    // Declare length of bar
    public double Width { get; private set; } = 200;
    // Declare height of bar
    public double Height { get; private set; } = 20;
    // Declare HasFood, set it to false
    public bool HasFood = false;
    // Declare Velocity
    private Vector2D Velocity;
    // Declare MainColor
    private Color MainColor;
    // Declare CollisionRectangle
    public Rectangle CollisionRectangle;
    // Declare _GameWindow
    private Window _GameWindow;

    // Create constructor
    public Bar(double x, double y, Window gameWindow)
    {
        X = x;
        Y = y;
        _GameWindow = gameWindow;

        // Declare SPEED = 3
        const int SPEED = 3;

        // Calculate movement speed and direction along x-axis of bar
        Velocity = SplashKit.VectorMultiply(new Vector2D() { X = 1 }, SPEED);
        // Randomize color of bar
        MainColor = Color.RandomRGB(200);
    }

    // Create Draw method
    public void Draw()
    {
        SplashKit.FillRectangle(MainColor, X, Y, Width, Height);
    }

    // Create Update method
    public void Update()
    {
        X += Velocity.X;

        // If the bar hit screen bound then it will change direction
        if (IsHitScreenBound(_GameWindow))
        {
            ChangeDirection();
        }
        // Create CollisionRectangle
        CollisionRectangle = new Rectangle() { X = X, Y = Y, Width = Width, Height = Height };
    }

    // Create IsHitScreenBound method
    public bool IsHitScreenBound(Window screen)
    {
        return X <= 0 || X + Width >= screen.Width;
    }
    // Create ChangeDirection method
    public void ChangeDirection()
    {
        Velocity.X = -Velocity.X;
    }
}
public class GamePlay
{
    // Declare list of Food
    private List<Food> _Foods;
    // Declare _Player
    private Player _Player;
    // Declare _GameWindow
    private Window _GameWindow;
    // Declare list of Enemy
    private List<Enemy> Enemies;
    // Declare list of Bullet
    private List<Bullet> _Bullets;
    // Declare MaxBullet
    private int MaxBullet = 5;
    // Declare list of Bar
    private List<Bar> _Bars;
    // Declare GlobalFoodSpawn
    public int GlobalFoodSpawn = 5;
    // Declare _Door
    private Door _Door;
    // Declare _HasWon
    private bool _HasWon = false;
    // Declare _HasLose
    private bool _HasLose = false;
    // Declare Shoot
    public bool Shoot { get; private set; }
    // Declare Quit, it will be true when player chooses Quit
    public bool Quit
    {
        get
        {
            return _Player.Quit;
        }
    }

    // Create constructor
    public GamePlay(Window gameWindow)
    {
        _GameWindow = gameWindow;
        _Player = new Player(_GameWindow);
        Enemies = new List<Enemy>();
        _Bullets = new List<Bullet>();
        _Bars = new List<Bar>();
        _Foods = new List<Food>();
        _Door = new Door();
        Shoot = _Player.Shoot;
    }

    // Create HandleInput method
    public void HandleInput()
    {
        _Player.HandleInput(_Bars);
        // Call Shoot method on each bullet
        foreach (Bullet bullet in _Bullets)
        {
            bullet.Shoot(_Player);
        }
        // Make sure player stay on window
        _Player.StayOnWindow(_GameWindow);
    }

    // Create Draw method
    public void Draw()
    {
        _GameWindow.Clear(Color.White);
        // Spawn door
        _Door.SpawnOnLowestBar(_Bars);
        _Door.Draw();
        // Spawn food
        foreach (Food food in _Foods)
        {
            food.Draw();
        }
        // Spawn bar
        foreach (Bar bar in _Bars)
        {
            bar.Draw();
        }
        // Call Draw method on each enemy
        foreach (Enemy enemy in Enemies)
        {
            enemy.Draw();
        }
        // Call Draw method on each bullet
        foreach (Bullet bullet in _Bullets)
        {
            bullet.Draw();
        }

        _Player.Draw(_GameWindow);
        // Indicate number of bullet left (cannot exceed MaxBullet)
        _GameWindow.DrawText($"Bullets remaining: {MaxBullet - _Bullets.Count}", Color.Black, 1000, 100);
        // Lose & Win display on screen
        if (_HasLose)
        {
            _GameWindow.DrawText("Sorry! You have lost! Press Escape to quit", Color.Red, _GameWindow.Width / 2 - 150, _GameWindow.Height / 2 - 20);
        }
        else
        {
            if (_HasWon)
            {
                _GameWindow.DrawText("Congratulations! You have won! Press Escape to quit", Color.Green, _GameWindow.Width / 2 - 150, _GameWindow.Height / 2 - 20);
            }
        }
        _GameWindow.Refresh(60);
    }

    // Create Update method
    public void Update()
    {
        // Call Update method on each bullet
        foreach (Bullet bullet in _Bullets)
        {
            bullet.Update();
        }

        SpawnBars();
        foreach (Bar bar in _Bars)
        {
            bar.Update();
        }

        SpawnFoodOnBars();
        foreach (Food food in _Foods)
        {
            food.Update();
        }
        // If mouse is clicked then add a bullet to _Bullets list
        if (SplashKit.MouseClicked(MouseButton.LeftButton))
        {
            if (_Bullets.Count < MaxBullet)
            {
                _Bullets.Add(BulletCreation());
            }
        }
        // Create random enemy
        if (SplashKit.Rnd() < 0.015)
        {
            Enemies.Add(RandomEnemy());
        }
        // Call Update method on each enemy
        foreach (Enemy enemy in Enemies)
        {
            enemy.Update();
        }
        // Call CheckCollision method
        CheckCollision();
    }

    // Create RandomEnemy method
    public Enemy RandomEnemy()
    {
        Enemy randomEnemy;
        if (SplashKit.Rnd() < 0.3)
        {
            randomEnemy = new Ghost(_GameWindow, _Player);
        }
        else if (SplashKit.Rnd() < 0.6)
        {
            randomEnemy = new Pig(_GameWindow, _Player);
        }
        else
        {
            randomEnemy = new Bat(_GameWindow, _Player);
        }

        return randomEnemy;
    }

    // Create SpawnBars method
    public void SpawnBars()
    {
        // ensure that numberOfBars = 5
        int numberOfBars = 5;
        int currentBarCount = _Bars.Count;
        int barsToSpawn = numberOfBars - currentBarCount;

        if (barsToSpawn > 0)
        {
            // Distance between bar (Y-axis)
            double totalDistance = _GameWindow.Height - _Player.Height - 10;
            double distanceBetweenBars = totalDistance / numberOfBars;
            double barWidth = 200;

            for (int i = 0; i < numberOfBars; i++)
            {
                // Make bar appear randomly along X-axis and have equal distance between each
                double x = SplashKit.Rnd() * (_GameWindow.Width - barWidth - 10);
                double y = _Player.Height + 10 + (i * distanceBetweenBars);

                Bar newBar = new Bar(x, y, _GameWindow);

                _Bars.Add(newBar);
            }
        }
    }

    // Create SpawnFoodOnBars method
    public void SpawnFoodOnBars()
    {
        // This is to control the number of food spawn on each bar is = 1
        foreach (Bar bar in _Bars)
        {
            // If bar not has food then create food and make bar has food
            if (!bar.HasFood)
            {
                Food newFood = new Food(bar, _Bars);
                _Foods.Add(newFood);
                bar.HasFood = true;
            }
        }
    }

    // Create BulletCreation method
    public Bullet BulletCreation()
    {
        Bullet bulletCreation = new Bullet(_Player);
        return bulletCreation;
    }

    // Create CheckCollision method
    private void CheckCollision()
    {
        List<Enemy> _removeEnemies = new List<Enemy>();
        List<Bullet> _removeBullets = new List<Bullet>();
        List<Food> _removeFoods = new List<Food>();

        // If enemy hit bullet then add these enemy, bullet to remove list
        foreach (Enemy enemy in Enemies)
        {
            {
                foreach (Bullet bullet in _Bullets)
                {
                    if (_Player.CollideWith(enemy) || bullet.CollideWith(enemy))
                    {
                        _removeEnemies.Add(enemy);
                        _removeBullets.Add(bullet);
                    }
                }
            }
        }

        // If bullet is offscreen then add to remove list
        foreach (Bullet bullet in _Bullets)
        {
            if (bullet.IsOffscreen(_GameWindow))
            {
                _removeBullets.Add(bullet);
            }
        }

        // If enemy is offscreen then add to remove list
        foreach (Enemy enemy in Enemies)
        {
            if (enemy.IsOffscreen(_GameWindow))
            {
                _removeEnemies.Add(enemy);
            }
        }

        // If player collide food then add it to remove list
        foreach (Food food in _Foods)
        {
            // Checking if the player collides with food
            if (_Player.CollideWith(food))
            {
                // Add food to remove list
                _removeFoods.Add(food);

                // Decreasing the global food spawn
                GlobalFoodSpawn -= 1;
            }
        }
        // If GlobalFoodSpawn = 0 & player collide with door then _HasWon = true
        if (GlobalFoodSpawn == 0 && _Player.CollideWith(_Door))
        {
            _HasWon = true;
        }

        // If enemy hit player, then player will lose 1 live, if all lives are losed then _HasLose = true
        foreach (Enemy enemy in Enemies)
        {
            if (_Player.CollideWith(enemy))
            {
                _removeEnemies.Add(enemy);
                if (_HasWon == false && _HasLose == false)
                {
                    _Player.Live -= 1;
                }
                else
                {
                    _Player.Live -= 0;
                }

                if (_Player.Live == 0)
                {
                    _HasLose = true;
                }
            }
        }

        // Remove enemy from remove list
        foreach (Enemy enemy in _removeEnemies)
        {
            Enemies.Remove(enemy);
        }

        // Remove bullet from remove list
        foreach (Bullet bullet in _removeBullets)
        {
            _Bullets.Remove(bullet);
        }

        // Remove food from remove list
        foreach (Food food in _removeFoods)
        {
            _Foods.Remove(food);
        }
    }
}



